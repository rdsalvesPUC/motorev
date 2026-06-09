using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
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
        int? quilometragem = null)
    {
        var alerta = new Alerta
        {
            Tipo = tipo,
            UsuarioId = usuarioId,
            MotoId = motoId,
            AgendamentoId = agendamentoId,
            Quilometragem = quilometragem,
            Lido = false,
            CriadoEm = DateTime.UtcNow
        };

        _context.Alertas.Add(alerta);
        await _context.SaveChangesAsync();

        // Notificar via SignalR
        await _hubContext.Clients.User(usuarioId).SendAsync("ReceberAlerta", alerta);
    }

    public virtual async Task GerarAlertaRevisaoProximaAsync(
        string usuarioId,
        int motoId,
        int quilometragemRevisao)
    {
        await CriarAlertaAsync(TipoAlerta.RevisaoProxima, usuarioId, motoId: motoId, quilometragem: quilometragemRevisao);
    }

    public virtual async Task GerarAlertaRevisaoAtrasadaAsync(
        string usuarioId,
        int motoId,
        int quilometragemAtrasada)
    {
        // Alerta para o Cliente
        await CriarAlertaAsync(TipoAlerta.RevisaoAtrasada, usuarioId, motoId: motoId, quilometragem: quilometragemAtrasada);

        // Alerta para a Concessionária
        var moto = await _context.Motos
            .Include(m => m.Concessionaria)
            .FirstOrDefaultAsync(m => m.Id == motoId);

        if (moto?.Concessionaria != null)
        {
            await CriarAlertaAsync(TipoAlerta.RevisaoAtrasada, moto.Concessionaria.UsuarioId, motoId: motoId, quilometragem: quilometragemAtrasada);
        }
    }

    public virtual async Task GerarAlertaAgendamentoCriadoAsync(
        int agendamentoId,
        string clienteUsuarioId,
        string concessionariaUsuarioId,
        int motoId)
    {
        await CriarAlertaAsync(TipoAlerta.AgendamentoCriado, clienteUsuarioId, motoId: motoId, agendamentoId: agendamentoId);
        await CriarAlertaAsync(TipoAlerta.AgendamentoCriado, concessionariaUsuarioId, motoId: motoId, agendamentoId: agendamentoId);
    }

    public virtual async Task GerarAlertaAgendamentoAlteradoAsync(
        int agendamentoId,
        string clienteUsuarioId,
        string concessionariaUsuarioId,
        int motoId)
    {
        await CriarAlertaAsync(TipoAlerta.AgendamentoAlterado, clienteUsuarioId, motoId: motoId, agendamentoId: agendamentoId);
        await CriarAlertaAsync(TipoAlerta.AgendamentoAlterado, concessionariaUsuarioId, motoId: motoId, agendamentoId: agendamentoId);
    }

    public virtual async Task GerarAlertaRevisaoConcluidaAsync(
        string clienteUsuarioId,
        int motoId)
    {
        await CriarAlertaAsync(TipoAlerta.RevisaoConcluida, clienteUsuarioId, motoId: motoId);
    }

    // Métodos de consulta para o Controller
    public virtual async Task<List<Alerta>> ListarAlertasAsync(string usuarioId, bool? lido = null, TipoAlerta? tipo = null)
    {
        var query = _context.Alertas.Where(a => a.UsuarioId == usuarioId);

        if (lido.HasValue)
            query = query.Where(a => a.Lido == lido.Value);

        if (tipo.HasValue)
            query = query.Where(a => a.Tipo == tipo.Value);

        return await query.OrderByDescending(a => a.CriadoEm).ToListAsync();
    }

    public virtual async Task<int> ContarNaoLidosAsync(string usuarioId)
    {
        return await _context.Alertas.CountAsync(a => a.UsuarioId == usuarioId && !a.Lido);
    }

    public virtual async Task<Alerta?> MarcarComoLidoAsync(int id, string usuarioId)
    {
        var alerta = await _context.Alertas.FirstOrDefaultAsync(a => a.Id == id && a.UsuarioId == usuarioId);
        if (alerta == null) return null;

        if (!alerta.Lido)
        {
            alerta.Lido = true;
            await _context.SaveChangesAsync();
        }

        return alerta;
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
