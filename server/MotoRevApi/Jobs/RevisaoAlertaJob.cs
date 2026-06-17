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

    public async Task VerificarMotoAsync(Moto moto, AppDbContext context, AlertaService alertaService, CancellationToken stoppingToken)
    {
        // 1. Determinar a próxima revisão baseada nas revisões planejadas da moto
        // Buscamos a primeira que ainda está com status "Planejada" (não foi concluída ou cancelada)
        var proximaRevisao = moto.RevisoesPlanejadas
            .Where(r => r.Status == "Planejada")
            .OrderBy(r => r.Ordem)
            .FirstOrDefault();

        if (proximaRevisao == null) return;

        // 2. Lógica de Revisão Próxima e Atrasada baseada na janela de datas (conforme lógica do frontend)
        var today = DateTime.UtcNow.Date;
        var dataPrevista = proximaRevisao.DataPrevista.Date;
        var dataMinima = dataPrevista.AddDays(-15);
        var dataLimite = dataPrevista.AddDays(15);

        // Revisão Próxima: Hoje está dentro da janela ideal de agendamento (entre 15 dias antes e 15 dias depois da data prevista)
        var deveGerarAlertaProximo = today >= dataMinima && today <= dataLimite;

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

        // Revisão Atrasada: Hoje passou da data limite (mais de 15 dias após a data prevista)
        var estaAtrasada = today > dataLimite;

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
