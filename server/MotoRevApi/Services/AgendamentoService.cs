using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
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
            CriarPrazoTexto(status, hoje, dataIdeal, dataLimite)
        );
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

    private static string NormalizarStatus(string status)
    {
        return string.Concat(status.Normalize(System.Text.NormalizationForm.FormD)
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark))
            .ToLowerInvariant()
            .Replace(" ", "_");
    }
}
