using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Enums;
using MotoRevApi.Model;
using MotoRevApi.Services;

namespace MotoRevApi.Jobs;

public class RevisaoAlertaJob : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RevisaoAlertaJob> _logger;
    private readonly IConfiguration _configuration;

    public RevisaoAlertaJob(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<RevisaoAlertaJob> logger,
        IConfiguration configuration)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalHoras = _configuration.GetValue<int>("RevisaoAlertaJob:IntervalHoras", 24);
        _logger.LogInformation("RevisaoAlertaJob iniciado com intervalo de {Intervalo} horas.", intervalHoras);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarMotosAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante a execução do RevisaoAlertaJob.");
            }

            await Task.Delay(TimeSpan.FromHours(intervalHoras), stoppingToken);
        }
    }

    private async Task ProcessarMotosAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var alertaService = scope.ServiceProvider.GetRequiredService<AlertaService>();

        var motos = await context.Motos
            .Include(m => m.ModeloMoto)
            .Include(m => m.Cliente)
            .Where(m => m.Ativo)
            .ToListAsync(stoppingToken);

        _logger.LogInformation("Verificando {Total} motos para alertas de revisão.", motos.Count);

        foreach (var moto in motos)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                await VerificarMotoAsync(moto, context, alertaService, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar moto ID {MotoId}.", moto.Id);
            }
        }
    }

    private async Task VerificarMotoAsync(Moto moto, AppDbContext context, AlertaService alertaService, CancellationToken stoppingToken)
    {
        // 1. Buscar a linha da moto
        var linhaId = moto.ModeloMoto.LinhaId;

        // 2. Buscar revisões padrão para esta linha
        var revisoesPadrao = await context.RevisoesPadrao
            .Where(r => r.LinhaId == linhaId && r.Ativo)
            .OrderBy(r => r.Ordem)
            .ToListAsync(stoppingToken);

        if (!revisoesPadrao.Any()) return;

        // 3. Determinar a próxima revisão. 
        // Como o histórico de revisões não está implementado, vamos assumir a primeira revisão baseada na KM
        // ou a revisão que mais se aproxima da KM atual.
        // TODO: Quando o histórico de revisões estiver pronto, buscar a primeira que não consta no histórico.
        
        RevisaoPadrao? proximaRevisao = null;
        RevisaoPadrao? revisaoAnterior = null;

        foreach (var rev in revisoesPadrao)
        {
            if (moto.KilometragemAtual < rev.Quilometragem)
            {
                proximaRevisao = rev;
                break;
            }
            revisaoAnterior = rev;
        }

        // Se não houver próxima revisão (já fez todas), encerra
        if (proximaRevisao == null) return;

        // 4. Lógica de Revisão Próxima (80% do intervalo)
        var kmAnterior = revisaoAnterior?.Quilometragem ?? 0;
        var intervalKm = proximaRevisao.Quilometragem - kmAnterior;
        var limiarKm = kmAnterior + (intervalKm * 0.80);

        // Alerta Próximo
        if (moto.KilometragemAtual >= limiarKm)
        {
            // Deduplicação (ver plano 5.6)
            var jaExiste = await context.Alertas.AnyAsync(a => 
                a.UsuarioId == moto.Cliente.UsuarioId && 
                a.Tipo == TipoAlerta.RevisaoProxima && 
                a.MotoId == moto.Id &&
                a.Quilometragem == proximaRevisao.Quilometragem, stoppingToken);

            if (!jaExiste)
            {
                await alertaService.GerarAlertaRevisaoProximaAsync(moto.Cliente.UsuarioId, moto.Id, proximaRevisao.Quilometragem);
                _logger.LogInformation("Alerta de Revisão Próxima gerado para Moto {MotoId}, Cliente {UsuarioId}.", moto.Id, moto.Cliente.UsuarioId);
            }
        }

        // 5. Lógica de Revisão Atrasada
        if (moto.KilometragemAtual > proximaRevisao.Quilometragem)
        {
            var jaExisteAtrasada = await context.Alertas.AnyAsync(a => 
                a.UsuarioId == moto.Cliente.UsuarioId && 
                a.Tipo == TipoAlerta.RevisaoAtrasada && 
                a.MotoId == moto.Id &&
                a.Quilometragem == proximaRevisao.Quilometragem, stoppingToken);

            if (!jaExisteAtrasada)
            {
                await alertaService.GerarAlertaRevisaoAtrasadaAsync(moto.Cliente.UsuarioId, moto.Id, proximaRevisao.Quilometragem);
                _logger.LogInformation("Alerta de Revisão Atrasada gerado para Moto {MotoId}, Cliente {UsuarioId}.", moto.Id, moto.Cliente.UsuarioId);
            }
        }
    }
}
