using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;

namespace MotoRevApi.Services;

public class AgendamentoService
{
    private static readonly HashSet<string> StatusesVisiveis = new()
    {
        "aguardando_agendamento",
        "aguardando_confirmacao",
        "agendada",
        "em_execucao",
        "atrasada",
    };

    private static readonly StatusAgendamento[] StatusesVisiveisConcessionaria =
    [
        StatusAgendamento.AguardandoConfirmacao,
        StatusAgendamento.Agendada,
        StatusAgendamento.Recusada,
        StatusAgendamento.EmExecucao,
        StatusAgendamento.Concluida,
    ];

    private readonly AppDbContext _context;
    private readonly Func<DateTime> _todayProvider;

    public AgendamentoService(AppDbContext context) : this(context, () => DateTime.Today)
    {
    }

    public AgendamentoService(AppDbContext context, Func<DateTime> todayProvider)
    {
        _context = context;
        _todayProvider = todayProvider;
    }

    public virtual async Task<List<AgendamentoClienteResponse>> ListarAgendamentosClienteAsync(string userId)
    {
        var cliente = await _context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UsuarioId == userId);

        if (cliente == null)
        {
            throw new NotFoundException("Cliente não encontrado.");
        }

        var motos = await _context.Motos
            .AsNoTracking()
            .Include(m => m.ModeloMoto)
            .Include(m => m.RevisoesPlanejadas)
                .ThenInclude(rm => rm.RevisaoPadrao)
                    .ThenInclude(rp => rp.Servicos)
            .Include(m => m.RevisoesPlanejadas)
                .ThenInclude(rm => rm.RevisaoPadrao)
                    .ThenInclude(rp => rp.Pecas)
            .AsSplitQuery()
            .Where(m => m.ClienteId == cliente.Id && m.Ativo)
            .ToListAsync();

        var revisaoIds = motos
            .SelectMany(m => m.RevisoesPlanejadas)
            .Select(r => r.Id)
            .ToList();

        var agendamentos = await _context.Agendamentos
            .AsNoTracking()
            .Include(a => a.Loja)
            .Where(a => revisaoIds.Contains(a.RevisaoMotoId))
            .ToListAsync();

        var ultimoAgendamentoPorRevisao = agendamentos
            .GroupBy(a => a.RevisaoMotoId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(a => a.CriadoEm).ThenByDescending(a => a.Id).First());

        var hoje = _todayProvider().Date;
        var itens = new List<AgendamentoClienteResponse>();

        foreach (var moto in motos)
        {
            foreach (var revisao in moto.RevisoesPlanejadas.OrderBy(r => r.Ordem))
            {
                ultimoAgendamentoPorRevisao.TryGetValue(revisao.Id, out var agendamento);
                var item = CriarResponse(moto, revisao, agendamento, hoje);

                if (StatusesVisiveis.Contains(item.Status))
                {
                    itens.Add(item);
                }
            }
        }

        return itens
            .OrderBy(item => ObterPrioridadeStatus(item.Status))
            .ThenBy(item => item.DataAgendada ?? item.DataIdeal)
            .ThenBy(item => item.NumeroRevisao)
            .ToList();
    }

    public virtual async Task<List<AgendamentoConcessionariaResponse>> ListarAgendamentosConcessionariaAsync(string userId)
    {
        var concessionaria = await _context.Concessionarias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UsuarioId == userId);

        if (concessionaria == null)
        {
            throw new NotFoundException("Concessionária não encontrada.");
        }

        var agendamentos = await _context.Agendamentos
            .AsNoTracking()
            .Include(a => a.Loja)
            .Include(a => a.RevisaoMoto)
                .ThenInclude(r => r.Moto)
                    .ThenInclude(m => m.Cliente)
            .Include(a => a.RevisaoMoto)
                .ThenInclude(r => r.Moto)
                    .ThenInclude(m => m.ModeloMoto)
            .Include(a => a.RevisaoMoto)
                .ThenInclude(r => r.RevisaoPadrao)
                    .ThenInclude(rp => rp.Servicos)
            .Include(a => a.RevisaoMoto)
                .ThenInclude(r => r.RevisaoPadrao)
                    .ThenInclude(rp => rp.Pecas)
            .AsSplitQuery()
            .Where(a =>
                a.Loja.ConcessionariaId == concessionaria.Id &&
                StatusesVisiveisConcessionaria.Contains(a.Status))
            .ToListAsync();

        return agendamentos
            .OrderBy(a => ObterPrioridadeConcessionaria(a.Status))
            .ThenBy(a => a.DataAgendada)
            .Select(CriarResponseConcessionaria)
            .ToList();
    }

    public virtual async Task CancelarAgendamentoClienteAsync(int agendamentoId, string userId)
    {
        var agendamento = await BuscarAgendamentoDoClienteAsync(agendamentoId, userId);

        if (agendamento.Status != StatusAgendamento.Agendada)
        {
            throw new BusinessRuleException("Somente agendamentos confirmados podem ser cancelados.");
        }

        agendamento.Status = StatusAgendamento.Cancelada;
        agendamento.AtualizadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public virtual async Task RemarcarAgendamentoClienteAsync(
        int agendamentoId,
        string userId,
        RemarcarAgendamentoRequest request)
    {
        var agendamento = await BuscarAgendamentoDoClienteAsync(agendamentoId, userId);

        if (agendamento.Status != StatusAgendamento.Agendada)
        {
            throw new BusinessRuleException("Somente agendamentos confirmados podem ser remarcados.");
        }

        var novaData = request.NovaData.Date;
        var hoje = _todayProvider().Date;
        var dataIdeal = agendamento.RevisaoMoto.DataPrevista.Date;
        var dataMinima = dataIdeal.AddDays(-15);
        var dataLimite = dataIdeal.AddDays(15);

        if (novaData < hoje)
        {
            throw new BusinessRuleException("A nova data do agendamento não pode ser anterior a hoje.");
        }

        if (novaData < dataMinima || novaData > dataLimite)
        {
            throw new BusinessRuleException("A nova data deve estar dentro da janela de tolerância da revisão.");
        }

        var agora = DateTime.UtcNow;
        agendamento.Status = StatusAgendamento.Cancelada;
        agendamento.AtualizadoEm = agora;

        _context.Agendamentos.Add(new Agendamento
        {
            RevisaoMotoId = agendamento.RevisaoMotoId,
            LojaId = agendamento.LojaId,
            DataAgendada = novaData,
            Status = StatusAgendamento.AguardandoConfirmacao,
            CriadoEm = agora,
            AtualizadoEm = agora
        });

        await _context.SaveChangesAsync();
    }

    public virtual async Task AgendarRevisaoClienteAsync(
        int revisaoMotoId,
        string userId,
        AgendarRevisaoRequest request)
    {
        var revisao = await BuscarRevisaoDoClienteAsync(revisaoMotoId, userId);
        var lojaExiste = await _context.Lojas
            .AsNoTracking()
            .AnyAsync(l => l.Id == request.LojaId && l.Ativo);

        if (!lojaExiste)
        {
            throw new NotFoundException("Loja não encontrada.");
        }

        var ultimoAgendamento = await _context.Agendamentos
            .Where(a => a.RevisaoMotoId == revisaoMotoId)
            .OrderByDescending(a => a.CriadoEm)
            .ThenByDescending(a => a.Id)
            .FirstOrDefaultAsync();

        var hoje = _todayProvider().Date;
        var dataIdeal = revisao.DataPrevista.Date;
        var dataMinima = dataIdeal.AddDays(-15);
        var dataLimite = dataIdeal.AddDays(15);
        var statusAtual = CalcularStatus(revisao, ultimoAgendamento, hoje, dataMinima, dataLimite);

        if (statusAtual is not ("aguardando_agendamento" or "atrasada"))
        {
            throw new BusinessRuleException("Esta revisão não está disponível para agendamento.");
        }

        var dataAgendada = request.DataAgendada.Date;
        ValidarDataDentroDaJanela(dataAgendada, hoje, dataMinima, dataLimite);

        var agora = DateTime.UtcNow;
        if (ultimoAgendamento is { Status: StatusAgendamento.Agendada } &&
            hoje > ultimoAgendamento.DataAgendada.Date)
        {
            ultimoAgendamento.Status = StatusAgendamento.Cancelada;
            ultimoAgendamento.AtualizadoEm = agora;
        }

        _context.Agendamentos.Add(new Agendamento
        {
            RevisaoMotoId = revisaoMotoId,
            LojaId = request.LojaId,
            DataAgendada = dataAgendada,
            Status = StatusAgendamento.AguardandoConfirmacao,
            CriadoEm = agora,
            AtualizadoEm = agora
        });

        await _context.SaveChangesAsync();
    }

    public virtual async Task VisualizarRecusaClienteAsync(int agendamentoId, string userId)
    {
        var agendamento = await BuscarAgendamentoDoClienteAsync(agendamentoId, userId);

        if (agendamento.Status != StatusAgendamento.Recusada)
        {
            throw new BusinessRuleException("Somente recusas podem ser marcadas como visualizadas.");
        }

        agendamento.RecusaVisualizadaCliente = true;
        agendamento.AtualizadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public virtual async Task AceitarSolicitacaoConcessionariaAsync(int agendamentoId, string userId)
    {
        var agendamento = await BuscarAgendamentoDaConcessionariaAsync(agendamentoId, userId);

        if (agendamento.Status != StatusAgendamento.AguardandoConfirmacao)
        {
            throw new BusinessRuleException("Somente solicitações aguardando confirmação podem ser aceitas.");
        }

        agendamento.Status = StatusAgendamento.Agendada;
        agendamento.MensagemRecusa = null;
        agendamento.DataRecusa = null;
        agendamento.RecusaVisualizadaCliente = false;
        agendamento.AtualizadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public virtual async Task RecusarSolicitacaoConcessionariaAsync(
        int agendamentoId,
        string userId,
        RecusarAgendamentoRequest request)
    {
        var agendamento = await BuscarAgendamentoDaConcessionariaAsync(agendamentoId, userId);

        if (agendamento.Status != StatusAgendamento.AguardandoConfirmacao)
        {
            throw new BusinessRuleException("Somente solicitações aguardando confirmação podem ser recusadas.");
        }

        agendamento.Status = StatusAgendamento.Recusada;
        agendamento.MensagemRecusa = string.IsNullOrWhiteSpace(request.Motivo)
            ? "Solicitação recusada pela concessionária."
            : request.Motivo.Trim();
        agendamento.DataRecusa = _todayProvider().Date;
        agendamento.RecusaVisualizadaCliente = false;
        agendamento.AtualizadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private async Task<Agendamento> BuscarAgendamentoDoClienteAsync(int agendamentoId, string userId)
    {
        var agendamento = await _context.Agendamentos
            .Include(a => a.RevisaoMoto)
                .ThenInclude(r => r.Moto)
                    .ThenInclude(m => m.Cliente)
            .FirstOrDefaultAsync(a =>
                a.Id == agendamentoId &&
                a.RevisaoMoto.Moto.Cliente.UsuarioId == userId &&
                a.RevisaoMoto.Moto.Ativo);

        return agendamento ?? throw new NotFoundException("Agendamento não encontrado.");
    }

    private async Task<Agendamento> BuscarAgendamentoDaConcessionariaAsync(int agendamentoId, string userId)
    {
        var agendamento = await _context.Agendamentos
            .Include(a => a.Loja)
                .ThenInclude(l => l.Concessionaria)
            .FirstOrDefaultAsync(a =>
                a.Id == agendamentoId &&
                a.Loja.Concessionaria.UsuarioId == userId);

        return agendamento ?? throw new NotFoundException("Agendamento não encontrado.");
    }

    private async Task<RevisaoMoto> BuscarRevisaoDoClienteAsync(int revisaoMotoId, string userId)
    {
        var revisao = await _context.RevisoesMotos
            .Include(r => r.Moto)
                .ThenInclude(m => m.Cliente)
            .FirstOrDefaultAsync(r =>
                r.Id == revisaoMotoId &&
                r.Moto.Cliente.UsuarioId == userId &&
                r.Moto.Ativo);

        return revisao ?? throw new NotFoundException("Revisão não encontrada.");
    }

    private static void ValidarDataDentroDaJanela(
        DateTime dataAgendada,
        DateTime hoje,
        DateTime dataMinima,
        DateTime dataLimite)
    {
        if (dataAgendada < hoje)
        {
            throw new BusinessRuleException("A data do agendamento não pode ser anterior a hoje.");
        }

        if (dataAgendada < dataMinima || dataAgendada > dataLimite)
        {
            throw new BusinessRuleException("A data deve estar dentro da janela de tolerância da revisão.");
        }
    }

    private static AgendamentoClienteResponse CriarResponse(
        Moto moto,
        RevisaoMoto revisao,
        Agendamento? agendamento,
        DateTime hoje)
    {
        if (agendamento?.Status == StatusAgendamento.Cancelada)
        {
            agendamento = null;
        }

        var dataIdeal = revisao.DataPrevista.Date;
        var dataMinima = dataIdeal.AddDays(-15);
        var dataLimite = dataIdeal.AddDays(15);
        var status = CalcularStatus(revisao, agendamento, hoje, dataMinima, dataLimite);

        return new AgendamentoClienteResponse(
            agendamento?.Id,
            revisao.Id,
            moto.Id,
            moto.ModeloMoto.Marca,
            moto.ModeloMoto.NomeModelo,
            moto.Placa,
            revisao.Ordem,
            revisao.Nome,
            status,
            dataIdeal,
            dataMinima,
            dataLimite,
            agendamento?.DataAgendada.Date,
            agendamento?.LojaId,
            agendamento?.Loja.Nome,
            agendamento?.Loja.Cidade,
            revisao.RevisaoPadrao.Pecas.Count,
            revisao.RevisaoPadrao.Servicos.Count,
            CriarPrazoTexto(status, hoje, dataIdeal, dataLimite),
            agendamento?.Status == StatusAgendamento.Recusada && !agendamento.RecusaVisualizadaCliente
                ? agendamento.MensagemRecusa
                : null,
            agendamento?.Status == StatusAgendamento.Recusada && !agendamento.RecusaVisualizadaCliente
                ? agendamento.DataRecusa
                : null
        );
    }

    private static AgendamentoConcessionariaResponse CriarResponseConcessionaria(Agendamento agendamento)
    {
        var revisao = agendamento.RevisaoMoto;
        var moto = revisao.Moto;
        var modelo = moto.ModeloMoto;

        return new AgendamentoConcessionariaResponse(
            agendamento.Id,
            revisao.Id,
            moto.Id,
            agendamento.LojaId,
            agendamento.Loja.Nome,
            moto.Cliente.Nome,
            modelo.Marca,
            modelo.NomeModelo,
            moto.Placa,
            revisao.Ordem,
            revisao.Nome,
            NormalizarStatusAgendamento(agendamento.Status),
            revisao.DataPrevista.Date,
            agendamento.DataAgendada.Date,
            revisao.Quilometragem,
            revisao.RevisaoPadrao.Pecas.Count,
            revisao.RevisaoPadrao.Servicos.Count,
            agendamento.MensagemRecusa,
            agendamento.DataRecusa);
    }

    private static string CalcularStatus(
        RevisaoMoto revisao,
        Agendamento? agendamento,
        DateTime hoje,
        DateTime dataMinima,
        DateTime dataLimite)
    {
        var statusRevisao = NormalizarStatus(revisao.Status);
        if (statusRevisao.Contains("concluida"))
        {
            return "concluida";
        }

        if (agendamento?.Status == StatusAgendamento.Concluida)
        {
            return "concluida";
        }

        if (agendamento?.Status == StatusAgendamento.EmExecucao)
        {
            return "em_execucao";
        }

        if (agendamento?.Status == StatusAgendamento.AguardandoConfirmacao)
        {
            return "aguardando_confirmacao";
        }

        if (hoje > dataLimite)
        {
            return "perdida";
        }

        if (agendamento?.Status == StatusAgendamento.Agendada)
        {
            return hoje > agendamento.DataAgendada.Date ? "atrasada" : "agendada";
        }

        if (agendamento?.Status == StatusAgendamento.Recusada)
        {
            return "aguardando_agendamento";
        }

        if (agendamento?.Status == StatusAgendamento.Cancelada)
        {
            agendamento = null;
        }

        if (hoje < dataMinima)
        {
            return "planejada";
        }

        return "aguardando_agendamento";
    }

    private static string CriarPrazoTexto(string status, DateTime hoje, DateTime dataIdeal, DateTime dataLimite)
    {
        return status switch
        {
            "em_execucao" => "Em execução",
            "aguardando_confirmacao" => "Aguardando confirmação da concessionária",
            "agendada" => CriarTextoAgendada(hoje, dataIdeal),
            "atrasada" => $"Atrasada há {Math.Abs((dataIdeal - hoje).Days)} dia(s)",
            "aguardando_agendamento" => CriarTextoAguardandoAgendamento(hoje, dataLimite),
            _ => string.Empty
        };
    }

    private static string CriarTextoAgendada(DateTime hoje, DateTime dataIdeal)
    {
        var diasParaIdeal = (dataIdeal - hoje).Days;
        if (diasParaIdeal > 0)
        {
            return $"Faltam {diasParaIdeal} dia(s) para a data ideal";
        }

        if (diasParaIdeal == 0)
        {
            return "Data ideal é hoje";
        }

        return $"{Math.Abs(diasParaIdeal)} dia(s) após data ideal";
    }

    private static string CriarTextoAguardandoAgendamento(DateTime hoje, DateTime dataLimite)
    {
        var diasParaLimite = (dataLimite - hoje).Days;
        if (diasParaLimite == 0)
        {
            return "Último dia da janela";
        }

        return $"Faltam {diasParaLimite} dia(s) para o limite";
    }

    private static int ObterPrioridadeStatus(string status) => status switch
    {
        "em_execucao" => 0,
        "aguardando_confirmacao" => 1,
        "agendada" => 2,
        "atrasada" => 3,
        "aguardando_agendamento" => 4,
        _ => 5
    };

    private static int ObterPrioridadeConcessionaria(StatusAgendamento status) => status switch
    {
        StatusAgendamento.AguardandoConfirmacao => 0,
        StatusAgendamento.Agendada => 1,
        StatusAgendamento.Recusada => 2,
        StatusAgendamento.EmExecucao => 3,
        StatusAgendamento.Concluida => 4,
        _ => 5
    };

    private static string NormalizarStatusAgendamento(StatusAgendamento status) => status switch
    {
        StatusAgendamento.AguardandoConfirmacao => "aguardando_confirmacao",
        StatusAgendamento.Agendada => "agendada",
        StatusAgendamento.EmExecucao => "em_execucao",
        StatusAgendamento.Concluida => "concluida",
        StatusAgendamento.Recusada => "recusada",
        StatusAgendamento.Cancelada => "cancelada",
        _ => NormalizarStatus(status.ToString())
    };

    private static string NormalizarStatus(string status)
    {
        return string.Concat(status.Normalize(System.Text.NormalizationForm.FormD)
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark))
            .ToLowerInvariant()
            .Replace(" ", "_");
    }
}
