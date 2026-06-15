using Mapster;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
using MotoRevApi.Hubs;
using MotoRevApi.Model;

namespace MotoRevApi.Services;

public class AlertaService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public AlertaService(AppDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    // Construtor padrão para o Moq
    public AlertaService() { }

    private async Task CriarAlertaAsync(
        TipoAlerta tipo,
        string usuarioId,
        int? motoId = null,
        int? agendamentoId = null,
        int? quilometragem = null,
        int? ordemRevisao = null,
        string? modeloMotoNome = null,
        string? marcaMoto = null)
    {
        var alerta = new Alerta
        {
            Tipo = tipo,
            UsuarioId = usuarioId,
            MotoId = motoId,
            AgendamentoId = agendamentoId,
            Quilometragem = quilometragem,
            OrdemRevisao = ordemRevisao,
            ModeloMotoNome = modeloMotoNome,
            MarcaMoto = marcaMoto,
            Lido = false,
            CriadoEm = DateTime.UtcNow
        };

        _context.Alertas.Add(alerta);
        await _context.SaveChangesAsync();

        // Notificar via SignalR
        var response = alerta.Adapt<AlertaResponse>();
        await _hubContext.Clients.User(usuarioId).SendAsync("ReceberAlerta", response);
    }

    public virtual async Task GerarAlertaRevisaoProximaAsync(
        string usuarioId,
        int motoId,
        int quilometragemRevisao)
    {
        var moto = await _context.Motos
            .Include(m => m.ModeloMoto)
            .Include(m => m.RevisoesPlanejadas)
            .FirstOrDefaultAsync(m => m.Id == motoId);

        var ordem = moto?.RevisoesPlanejadas
            .FirstOrDefault(r => r.Quilometragem == quilometragemRevisao)?.Ordem;

        await CriarAlertaAsync(
            TipoAlerta.RevisaoProxima,
            usuarioId,
            motoId: motoId,
            quilometragem: quilometragemRevisao,
            ordemRevisao: ordem,
            modeloMotoNome: moto?.ModeloMoto?.NomeModelo,
            marcaMoto: moto?.ModeloMoto?.Marca);
    }

    public virtual async Task GerarAlertaRevisaoAtrasadaAsync(
        string usuarioId,
        int motoId,
        int quilometragemAtrasada)
    {
        var moto = await _context.Motos
            .Include(m => m.ModeloMoto)
            .Include(m => m.Concessionaria)
            .Include(m => m.RevisoesPlanejadas)
            .FirstOrDefaultAsync(m => m.Id == motoId);

        int? ordem = moto?.RevisoesPlanejadas
            .FirstOrDefault(r => r.Quilometragem == quilometragemAtrasada)?.Ordem;

        // Alerta para o Cliente
        await CriarAlertaAsync(
            TipoAlerta.RevisaoAtrasada,
            usuarioId,
            motoId: motoId,
            quilometragem: quilometragemAtrasada,
            ordemRevisao: ordem,
            modeloMotoNome: moto?.ModeloMoto?.NomeModelo,
            marcaMoto: moto?.ModeloMoto?.Marca);

        // Alerta para a Concessionária
        if (moto?.Concessionaria != null)
        {
            await CriarAlertaAsync(
                TipoAlerta.RevisaoAtrasada,
                moto.Concessionaria.UsuarioId,
                motoId: motoId,
                quilometragem: quilometragemAtrasada,
                ordemRevisao: ordem,
                modeloMotoNome: moto?.ModeloMoto?.NomeModelo,
                marcaMoto: moto?.ModeloMoto?.Marca);
        }
    }

    public virtual async Task GerarAlertaAgendamentoCriadoAsync(
        int agendamentoId,
        string clienteUsuarioId,
        string concessionariaUsuarioId,
        int motoId)
    {
        await CriarAlertaAsync(TipoAlerta.AgendamentoCriado, clienteUsuarioId, motoId: motoId, agendamentoId: agendamentoId);
        await CriarAlertaAsync(TipoAlerta.NovaSolicitacao, concessionariaUsuarioId, motoId: motoId, agendamentoId: agendamentoId);
    }

    public virtual async Task GerarAlertaAgendamentoAlteradoAsync(
        int agendamentoId,
        string clienteUsuarioId,
        string concessionariaUsuarioId,
        int motoId)
    {
        await CriarAlertaAsync(TipoAlerta.AgendamentoAlterado, clienteUsuarioId, motoId: motoId, agendamentoId: agendamentoId);
        await CriarAlertaAsync(TipoAlerta.Reagendamento, concessionariaUsuarioId, motoId: motoId, agendamentoId: agendamentoId);
    }

    public virtual async Task GerarAlertaAgendamentoCanceladoAsync(
        int agendamentoId,
        string clienteUsuarioId,
        string concessionariaUsuarioId,
        int motoId)
    {
        await CriarAlertaAsync(TipoAlerta.Cancelamento, clienteUsuarioId, motoId: motoId, agendamentoId: agendamentoId);
        await CriarAlertaAsync(TipoAlerta.Cancelamento, concessionariaUsuarioId, motoId: motoId, agendamentoId: agendamentoId);
    }

    public virtual async Task GerarAlertaAgendamentoAprovadoAsync(
        int agendamentoId,
        string clienteUsuarioId,
        string concessionariaUsuarioId,
        int motoId)
    {
        await CriarAlertaAsync(TipoAlerta.AgendamentoAprovado, clienteUsuarioId, motoId: motoId, agendamentoId: agendamentoId);
    }

    public virtual async Task GerarAlertaAgendamentoRecusadoAsync(
        int agendamentoId,
        string clienteUsuarioId,
        string concessionariaUsuarioId,
        int motoId)
    {
        await CriarAlertaAsync(TipoAlerta.AgendamentoRecusado, clienteUsuarioId, motoId: motoId, agendamentoId: agendamentoId);
    }

    public virtual async Task GerarAlertaRevisaoConcluidaAsync(
        string clienteUsuarioId,
        int motoId)
    {
        await CriarAlertaAsync(TipoAlerta.RevisaoConcluida, clienteUsuarioId, motoId: motoId);
    }

    // Métodos de consulta para o Controller
    public virtual async Task<List<AlertaResponse>> ListarAlertasAsync(string usuarioId, bool? lido = null, TipoAlerta? tipo = null)
    {
        var query = _context.Alertas.Where(a => a.UsuarioId == usuarioId);

        if (lido.HasValue)
            query = query.Where(a => a.Lido == lido.Value);

        if (tipo.HasValue)
            query = query.Where(a => a.Tipo == tipo.Value);

        var alertas = await query.OrderByDescending(a => a.CriadoEm).ToListAsync();
        return alertas.Adapt<List<AlertaResponse>>();
    }

    public virtual async Task<int> ContarNaoLidosAsync(string usuarioId)
    {
        return await _context.Alertas.CountAsync(a => a.UsuarioId == usuarioId && !a.Lido);
    }

    public virtual async Task<AlertaResponse?> MarcarComoLidoAsync(int id, string usuarioId)
    {
        var alerta = await _context.Alertas.FirstOrDefaultAsync(a => a.Id == id && a.UsuarioId == usuarioId);
        if (alerta == null) return null;

        if (!alerta.Lido)
        {
            alerta.Lido = true;
            await _context.SaveChangesAsync();
        }

        return alerta.Adapt<AlertaResponse>();
    }

    public virtual async Task MarcarTodosComoLidosAsync(string usuarioId)
    {
        var alertas = await _context.Alertas
            .Where(a => a.UsuarioId == usuarioId && !a.Lido)
            .ToListAsync();

        foreach (var alerta in alertas)
        {
            alerta.Lido = true;
        }

        await _context.SaveChangesAsync();
    }
}
