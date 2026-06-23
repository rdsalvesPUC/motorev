using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MotoRevApi.Data;
using MotoRevApi.Enums;
using MotoRevApi.Jobs;
using MotoRevApi.Model;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Jobs;

public class RevisaoAlertaJobTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public RevisaoAlertaJobTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    private AppDbContext CreateContext() => new AppDbContext(_dbContextOptions);

    [Fact]
    public async Task VerificarMotoAsync_DeveGerarAlertaProximo_QuandoHojeEstaNaJanelaDe15DiasAntes()
    {
        // Arrange
        using var context = CreateContext();

        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user-123" };
        var moto = new Moto
        {
            Id = 1,
            Placa = "ABC1234",
            Chassi = "CHASSI123",
            Cor = "Azul",
            Ativo = true,
            Cliente = cliente,
            RevisoesPlanejadas = new List<RevisaoMoto>
            {
                new RevisaoMoto
                {
                    Id = 10,
                    Nome = "10k Revision",
                    Ordem = 1,
                    Quilometragem = 10000,
                    Status = "Planejada",
                    DataPrevista = DateTime.UtcNow.AddDays(10) // 10 days in the future, which is in the [-15, +15] window
                }
            }
        };

        context.Clientes.Add(cliente);
        context.Motos.Add(moto);
        await context.SaveChangesAsync();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var loggerMock = new Mock<ILogger<RevisaoAlertaJob>>();
        var configMock = new Mock<IConfiguration>();
        var alertaServiceMock = new Mock<AlertaService>();

        var job = new RevisaoAlertaJob(scopeFactoryMock.Object, loggerMock.Object, configMock.Object);

        // Act
        await job.VerificarMotoAsync(moto, context, alertaServiceMock.Object, CancellationToken.None);

        // Assert
        alertaServiceMock.Verify(s => s.GerarAlertaRevisaoProximaAsync(
            "user-123", 
            moto.Id, 
            10000
        ), Times.Once);

        alertaServiceMock.Verify(s => s.GerarAlertaRevisaoAtrasadaAsync(
            It.IsAny<string>(), 
            It.IsAny<int>(), 
            It.IsAny<int>()
        ), Times.Never);
    }

    [Fact]
    public async Task VerificarMotoAsync_DeveGerarAlertaAtrasado_QuandoHojePassouMaisDe15Dias()
    {
        // Arrange
        using var context = CreateContext();

        var cliente = new Cliente { Id = 2, Nome = "Cliente Teste 2", UsuarioId = "user-456" };
        var moto = new Moto
        {
            Id = 2,
            Placa = "DEF5678",
            Chassi = "CHASSI456",
            Cor = "Preto",
            Ativo = true,
            Cliente = cliente,
            RevisoesPlanejadas = new List<RevisaoMoto>
            {
                new RevisaoMoto
                {
                    Id = 11,
                    Nome = "20k Revision",
                    Ordem = 1,
                    Quilometragem = 20000,
                    Status = "Planejada",
                    DataPrevista = DateTime.UtcNow.AddDays(-16) // 16 days in the past, past the +15 threshold
                }
            }
        };

        context.Clientes.Add(cliente);
        context.Motos.Add(moto);
        await context.SaveChangesAsync();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var loggerMock = new Mock<ILogger<RevisaoAlertaJob>>();
        var configMock = new Mock<IConfiguration>();
        var alertaServiceMock = new Mock<AlertaService>();

        var job = new RevisaoAlertaJob(scopeFactoryMock.Object, loggerMock.Object, configMock.Object);

        // Act
        await job.VerificarMotoAsync(moto, context, alertaServiceMock.Object, CancellationToken.None);

        // Assert
        alertaServiceMock.Verify(s => s.GerarAlertaRevisaoAtrasadaAsync(
            "user-456", 
            moto.Id, 
            20000
        ), Times.Once);

        alertaServiceMock.Verify(s => s.GerarAlertaRevisaoProximaAsync(
            It.IsAny<string>(), 
            It.IsAny<int>(), 
            It.IsAny<int>()
        ), Times.Never);
    }

    [Fact]
    public async Task VerificarMotoAsync_NaoDeveGerarAlerta_QuandoHojeEstaForaDasJanelas()
    {
        // Arrange
        using var context = CreateContext();

        var cliente = new Cliente { Id = 3, Nome = "Cliente Teste 3", UsuarioId = "user-789" };
        var moto = new Moto
        {
            Id = 3,
            Placa = "GHI9012",
            Chassi = "CHASSI789",
            Cor = "Branco",
            Ativo = true,
            Cliente = cliente,
            RevisoesPlanejadas = new List<RevisaoMoto>
            {
                new RevisaoMoto
                {
                    Id = 12,
                    Nome = "30k Revision",
                    Ordem = 1,
                    Quilometragem = 30000,
                    Status = "Planejada",
                    DataPrevista = DateTime.UtcNow.AddDays(30) // 30 days in the future, way before the -15 threshold
                }
            }
        };

        context.Clientes.Add(cliente);
        context.Motos.Add(moto);
        await context.SaveChangesAsync();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var loggerMock = new Mock<ILogger<RevisaoAlertaJob>>();
        var configMock = new Mock<IConfiguration>();
        var alertaServiceMock = new Mock<AlertaService>();

        var job = new RevisaoAlertaJob(scopeFactoryMock.Object, loggerMock.Object, configMock.Object);

        // Act
        await job.VerificarMotoAsync(moto, context, alertaServiceMock.Object, CancellationToken.None);

        // Assert
        alertaServiceMock.Verify(s => s.GerarAlertaRevisaoProximaAsync(
            It.IsAny<string>(), 
            It.IsAny<int>(), 
            It.IsAny<int>()
        ), Times.Never);

        alertaServiceMock.Verify(s => s.GerarAlertaRevisaoAtrasadaAsync(
            It.IsAny<string>(), 
            It.IsAny<int>(), 
            It.IsAny<int>()
        ), Times.Never);
    }

    [Fact]
    public async Task VerificarMotoAsync_NaoDeveGerarAlertaProximo_QuandoAlertaJaExisteNoBanco()
    {
        // Arrange
        using var context = CreateContext();

        var cliente = new Cliente { Id = 4, Nome = "Cliente Teste 4", UsuarioId = "user-101" };
        var moto = new Moto
        {
            Id = 4,
            Placa = "JKL3456",
            Chassi = "CHASSI101",
            Cor = "Azul",
            Ativo = true,
            Cliente = cliente,
            RevisoesPlanejadas = new List<RevisaoMoto>
            {
                new RevisaoMoto
                {
                    Id = 13,
                    Nome = "10k Revision",
                    Ordem = 1,
                    Quilometragem = 10000,
                    Status = "Planejada",
                    DataPrevista = DateTime.UtcNow.AddDays(10) // Window is correct
                }
            }
        };

        // Existing alert in database to trigger deduplication
        var alertaExistente = new Alerta
        {
            UsuarioId = "user-101",
            Tipo = TipoAlerta.RevisaoProxima,
            MotoId = 4,
            Quilometragem = 10000
        };

        context.Clientes.Add(cliente);
        context.Motos.Add(moto);
        context.Alertas.Add(alertaExistente);
        await context.SaveChangesAsync();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var loggerMock = new Mock<ILogger<RevisaoAlertaJob>>();
        var configMock = new Mock<IConfiguration>();
        var alertaServiceMock = new Mock<AlertaService>();

        var job = new RevisaoAlertaJob(scopeFactoryMock.Object, loggerMock.Object, configMock.Object);

        // Act
        await job.VerificarMotoAsync(moto, context, alertaServiceMock.Object, CancellationToken.None);

        // Assert
        alertaServiceMock.Verify(s => s.GerarAlertaRevisaoProximaAsync(
            It.IsAny<string>(), 
            It.IsAny<int>(), 
            It.IsAny<int>()
        ), Times.Never);
    }

    [Fact]
    public async Task VerificarMotoAsync_NaoDeveGerarAlertaAtrasado_QuandoAlertaJaExisteNoBanco()
    {
        // Arrange
        using var context = CreateContext();

        var cliente = new Cliente { Id = 5, Nome = "Cliente Teste 5", UsuarioId = "user-202" };
        var moto = new Moto
        {
            Id = 5,
            Placa = "MNO7890",
            Chassi = "CHASSI202",
            Cor = "Branco",
            Ativo = true,
            Cliente = cliente,
            RevisoesPlanejadas = new List<RevisaoMoto>
            {
                new RevisaoMoto
                {
                    Id = 14,
                    Nome = "20k Revision",
                    Ordem = 1,
                    Quilometragem = 20000,
                    Status = "Planejada",
                    DataPrevista = DateTime.UtcNow.AddDays(-16) // Atrasada window
                }
            }
        };

        // Existing alert in database to trigger deduplication
        var alertaExistente = new Alerta
        {
            UsuarioId = "user-202",
            Tipo = TipoAlerta.RevisaoAtrasada,
            MotoId = 5,
            Quilometragem = 20000
        };

        context.Clientes.Add(cliente);
        context.Motos.Add(moto);
        context.Alertas.Add(alertaExistente);
        await context.SaveChangesAsync();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var loggerMock = new Mock<ILogger<RevisaoAlertaJob>>();
        var configMock = new Mock<IConfiguration>();
        var alertaServiceMock = new Mock<AlertaService>();

        var job = new RevisaoAlertaJob(scopeFactoryMock.Object, loggerMock.Object, configMock.Object);

        // Act
        await job.VerificarMotoAsync(moto, context, alertaServiceMock.Object, CancellationToken.None);

        // Assert
        alertaServiceMock.Verify(s => s.GerarAlertaRevisaoAtrasadaAsync(
            It.IsAny<string>(), 
            It.IsAny<int>(), 
            It.IsAny<int>()
        ), Times.Never);
    }

    [Fact]
    public async Task VerificarMotoAsync_NaoDeveGerarAlerta_QuandoNaoHaRevisaoPlanejada()
    {
        // Arrange
        using var context = CreateContext();

        var cliente = new Cliente { Id = 6, Nome = "Cliente Teste 6", UsuarioId = "user-303" };
        var moto = new Moto
        {
            Id = 6,
            Placa = "PQR1234",
            Chassi = "CHASSI303",
            Cor = "Vermelha",
            Ativo = true,
            Cliente = cliente,
            RevisoesPlanejadas = new List<RevisaoMoto>
            {
                new RevisaoMoto
                {
                    Id = 15,
                    Nome = "10k Revision",
                    Ordem = 1,
                    Quilometragem = 10000,
                    Status = "Concluida", // Not planned!
                    DataPrevista = DateTime.UtcNow.AddDays(10)
                }
            }
        };

        context.Clientes.Add(cliente);
        context.Motos.Add(moto);
        await context.SaveChangesAsync();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var loggerMock = new Mock<ILogger<RevisaoAlertaJob>>();
        var configMock = new Mock<IConfiguration>();
        var alertaServiceMock = new Mock<AlertaService>();

        var job = new RevisaoAlertaJob(scopeFactoryMock.Object, loggerMock.Object, configMock.Object);

        // Act
        await job.VerificarMotoAsync(moto, context, alertaServiceMock.Object, CancellationToken.None);

        // Assert
        alertaServiceMock.Verify(s => s.GerarAlertaRevisaoProximaAsync(
            It.IsAny<string>(), 
            It.IsAny<int>(), 
            It.IsAny<int>()
        ), Times.Never);

        alertaServiceMock.Verify(s => s.GerarAlertaRevisaoAtrasadaAsync(
            It.IsAny<string>(), 
            It.IsAny<int>(), 
            It.IsAny<int>()
        ), Times.Never);
    }
}
