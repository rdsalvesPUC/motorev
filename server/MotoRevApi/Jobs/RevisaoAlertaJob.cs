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

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
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
            .Include(m => m.RevisoesPlanejadas)
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
        // 1. Determinar a próxima revisão baseada nas revisões planejadas da moto
        // Buscamos a primeira que ainda está com status "Planejada" (não foi concluída ou cancelada)
        var proximaRevisao = moto.RevisoesPlanejadas
            .Where(r => r.Status == "Planejada")
            .OrderBy(r => r.Ordem)
            .FirstOrDefault();

        if (proximaRevisao == null) return;

        // 2. Determinar a revisão anterior para calcular o intervalo (se houver)
        var revisaoAnterior = moto.RevisoesPlanejadas
            .Where(r => r.Ordem < proximaRevisao.Ordem)
            .OrderByDescending(r => r.Ordem)
            .FirstOrDefault();

        // 3. Lógica de Revisão Próxima (80% do intervalo de KM ou Próximo da Data)
        var kmAnterior = revisaoAnterior?.Quilometragem ?? 0;
        var intervalKm = proximaRevisao.Quilometragem - kmAnterior;
        var limiarKm = kmAnterior + (intervalKm * 0.80);

        // Alerta Próximo por Quilometragem
        var deveGerarAlertaProximo = moto.KilometragemAtual >= limiarKm;

        // Alerta Próximo por Data (ex: faltando 30 dias para a data prevista)
        var limiarData = proximaRevisao.DataPrevista.AddDays(-30);
        if (DateTime.UtcNow >= limiarData)
        {
            deveGerarAlertaProximo = true;
        }

        if (deveGerarAlertaProximo)
        {
            // Deduplicação
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

        // 4. Lógica de Revisão Atrasada (Passou da KM ou Passou da Data)
        var estaAtrasada = moto.KilometragemAtual > proximaRevisao.Quilometragem || DateTime.UtcNow > proximaRevisao.DataPrevista;

        if (estaAtrasada)
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
