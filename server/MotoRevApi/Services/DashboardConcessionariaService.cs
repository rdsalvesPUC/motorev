using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;

namespace MotoRevApi.Services;

public class DashboardConcessionariaService
{
    private static readonly MecanicoMock[] Mecanicos =
    [
        new("1", "Carlos Silva", "Trail", 8),
        new("2", "Ana Paula Oliveira", "Passeio", 8),
        new("3", "Roberto Santos", "Scooter", 8),
        new("4", "Juliana Costa", "Geral", 8)
    ];

    private static readonly StatusAgendamento[] StatusesDaFila =
    [
        StatusAgendamento.Agendada,
        StatusAgendamento.EmExecucao,
        StatusAgendamento.Concluida
    ];

    private readonly AppDbContext _context;
    private readonly Func<DateTime> _todayProvider;

    public DashboardConcessionariaService(AppDbContext context) : this(context, () => DateTime.Today)
    {
    }

    public DashboardConcessionariaService(AppDbContext context, Func<DateTime> todayProvider)
    {
        _context = context;
        _todayProvider = todayProvider;
    }

    public virtual async Task<DashboardConcessionariaResponse> ObterDashboardRevisoesAsync(
        string userId,
        DateTime? data = null)
    {
        var concessionaria = await _context.Concessionarias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UsuarioId == userId);

        if (concessionaria == null)
        {
            throw new NotFoundException("Concessionária não encontrada.");
        }

        var dia = (data ?? _todayProvider()).Date;
        var proximoDia = dia.AddDays(1);

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
                a.DataAgendada >= dia &&
                a.DataAgendada < proximoDia &&
                StatusesDaFila.Contains(a.Status))
            .OrderBy(a => a.DataAgendada)
            .ThenBy(a => a.CriadoEm)
            .ThenBy(a => a.Id)
            .ToListAsync();

        var filas = Mecanicos
            .Select(m => new FilaMecanicoBuilder(m))
            .ToList();

        for (var index = 0; index < agendamentos.Count; index++)
        {
            var fila = filas[index % filas.Count];
            fila.Itens.Add(CriarItemFila(agendamentos[index], fila.Itens.Count + 1));
        }

        var filasResponse = filas
            .Select(f => new FilaMecanicoResponse(
                f.Mecanico.Id,
                f.Mecanico.Nome,
                f.Mecanico.Especialidade,
                f.Mecanico.CapacidadeDiaria,
                f.Itens))
            .ToList();

        return new DashboardConcessionariaResponse(
            dia,
            Mecanicos.Length,
            agendamentos.Count,
            agendamentos.Count(a => a.Status == StatusAgendamento.Agendada),
            agendamentos.Count(a => a.Status == StatusAgendamento.EmExecucao),
            agendamentos.Count(a => a.Status == StatusAgendamento.Concluida),
            filasResponse);
    }

    private static ItemFilaRevisaoResponse CriarItemFila(Agendamento agendamento, int posicaoFila)
    {
        var revisao = agendamento.RevisaoMoto;
        var moto = revisao.Moto;
        var modelo = moto.ModeloMoto;
        var quantidadePecas = revisao.RevisaoPadrao.Pecas.Count;
        var quantidadeServicos = revisao.RevisaoPadrao.Servicos.Count;
        var totalItens = quantidadePecas + quantidadeServicos;
        var itensConcluidos = CalcularItensConcluidos(agendamento.Status, totalItens);
        var progresso = totalItens == 0
            ? 0
            : (int)Math.Round((decimal)itensConcluidos / totalItens * 100, MidpointRounding.AwayFromZero);

        return new ItemFilaRevisaoResponse(
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
            agendamento.DataAgendada,
            posicaoFila,
            revisao.Quilometragem,
            quantidadePecas,
            quantidadeServicos,
            totalItens,
            itensConcluidos,
            progresso);
    }

    private static int CalcularItensConcluidos(StatusAgendamento status, int totalItens)
    {
        if (totalItens <= 0)
        {
            return 0;
        }

        return status switch
        {
            StatusAgendamento.Concluida => totalItens,
            StatusAgendamento.EmExecucao => Math.Max(1, (int)Math.Ceiling(totalItens / 2m)),
            _ => 0
        };
    }

    private static string NormalizarStatusAgendamento(StatusAgendamento status) => status switch
    {
        StatusAgendamento.Agendada => "agendada",
        StatusAgendamento.EmExecucao => "em_execucao",
        StatusAgendamento.Concluida => "concluida",
        _ => status.ToString()
    };

    private record MecanicoMock(string Id, string Nome, string Especialidade, int CapacidadeDiaria);

    private sealed class FilaMecanicoBuilder
    {
        public FilaMecanicoBuilder(MecanicoMock mecanico)
        {
            Mecanico = mecanico;
        }

        public MecanicoMock Mecanico { get; }
        public List<ItemFilaRevisaoResponse> Itens { get; } = [];
    }
}
