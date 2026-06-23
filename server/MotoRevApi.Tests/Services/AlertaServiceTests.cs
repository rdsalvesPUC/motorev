using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using MotoRevApi.Data;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
using MotoRevApi.Hubs;
using MotoRevApi.Model;
using MotoRevApi.Profiles;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Services;

public class AlertaServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;
    private readonly Mock<IHubContext<NotificationHub>> _hubContextMock;
    private readonly Mock<IHubClients> _hubClientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;

    public AlertaServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        MapsterConfig.RegisterMapsterConfiguration();

        _hubContextMock = new Mock<IHubContext<NotificationHub>>();
        _hubClientsMock = new Mock<IHubClients>();
        _clientProxyMock = new Mock<IClientProxy>();

        _hubContextMock.Setup(h => h.Clients).Returns(_hubClientsMock.Object);
        _hubClientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(_clientProxyMock.Object);
    }

    private AppDbContext CreateContext() => new AppDbContext(_dbContextOptions);

    [Fact]
    public async Task GerarAlertaRevisaoProximaAsync_DeveAdicionarAlertaENotificarUsuario()
    {
        // Arrange
        using var context = CreateContext();
        
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "F 750 GS", Marca = "BMW", Linha = new Linha { Nome = "Adventure" } };
        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user-123" };
        var moto = new Moto
        {
            Id = 10,
            Placa = "ABC1234",
            Chassi = "CHASSI123",
            Cor = "Azul",
            Cliente = cliente,
            ModeloMoto = modelo,
            RevisoesPlanejadas = new List<RevisaoMoto>
            {
                new RevisaoMoto { Id = 100, Quilometragem = 10000, Ordem = 1, Nome = "1ª Revisão", Status = "Planejada", DataPrevista = DateTime.UtcNow }
            }
        };

        context.Clientes.Add(cliente);
        context.Motos.Add(moto);
        await context.SaveChangesAsync();

        var service = new AlertaService(context, _hubContextMock.Object);

        // Act
        await service.GerarAlertaRevisaoProximaAsync("user-123", 10, 10000);

        // Assert
        var alerta = await context.Alertas.FirstOrDefaultAsync(a => a.UsuarioId == "user-123" && a.Tipo == TipoAlerta.RevisaoProxima);
        Assert.NotNull(alerta);
        Assert.Equal(10, alerta.MotoId);
        Assert.Equal(10000, alerta.Quilometragem);
        Assert.Equal(1, alerta.OrdemRevisao);
        Assert.Equal("F 750 GS", alerta.ModeloMotoNome);
        Assert.Equal("BMW", alerta.MarcaMoto);
        Assert.False(alerta.Lido);

        _hubClientsMock.Verify(c => c.User("user-123"), Times.Once);
        _clientProxyMock.Verify(
            c => c.SendCoreAsync(
                "ReceberAlerta",
                It.Is<object?[]>(args => args.Length == 1 && args[0] is AlertaResponse && ((AlertaResponse)args[0]!).Tipo == TipoAlerta.RevisaoProxima),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GerarAlertaRevisaoAtrasadaAsync_SemConcessionaria_DeveAdicionarAlertaSomenteAoCliente()
    {
        // Arrange
        using var context = CreateContext();

        var modelo = new ModeloMoto { Id = 2, NomeModelo = "CG 160", Marca = "Honda", Linha = new Linha { Nome = "Street" } };
        var cliente = new Cliente { Id = 2, Nome = "Cliente Teste 2", UsuarioId = "user-456" };
        var moto = new Moto
        {
            Id = 20,
            Placa = "XYZ9876",
            Chassi = "CHASSI456",
            Cor = "Vermelha",
            Cliente = cliente,
            ModeloMoto = modelo,
            RevisoesPlanejadas = new List<RevisaoMoto>
            {
                new RevisaoMoto { Id = 200, Quilometragem = 6000, Ordem = 2, Nome = "2ª Revisão", Status = "Planejada", DataPrevista = DateTime.UtcNow }
            }
        };

        context.Clientes.Add(cliente);
        context.Motos.Add(moto);
        await context.SaveChangesAsync();

        var service = new AlertaService(context, _hubContextMock.Object);

        // Act
        await service.GerarAlertaRevisaoAtrasadaAsync("user-456", 20, 6000);

        // Assert
        var alertas = await context.Alertas.Where(a => a.MotoId == 20).ToListAsync();
        Assert.Single(alertas);
        var alerta = alertas[0];
        Assert.Equal("user-456", alerta.UsuarioId);
        Assert.Equal(TipoAlerta.RevisaoAtrasada, alerta.Tipo);

        _hubClientsMock.Verify(c => c.User("user-456"), Times.Once);
        _clientProxyMock.Verify(
            c => c.SendCoreAsync(
                "ReceberAlerta",
                It.Is<object?[]>(args => args.Length == 1 && args[0] is AlertaResponse && ((AlertaResponse)args[0]!).Tipo == TipoAlerta.RevisaoAtrasada),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GerarAlertaRevisaoAtrasadaAsync_ComConcessionaria_DeveAdicionarAlertaParaClienteEConcessionaria()
    {
        // Arrange
        using var context = CreateContext();

        var modelo = new ModeloMoto { Id = 3, NomeModelo = "Ninja 400", Marca = "Kawasaki", Linha = new Linha { Nome = "Sport" } };
        var cliente = new Cliente { Id = 3, Nome = "Cliente 3", UsuarioId = "user-789" };
        var concessionaria = new Concessionaria { Id = 1, Nome = "Kawa SP", Cnpj = "12.345.678/0001-99", Telefone = "11999999999", UsuarioId = "concess-123", Usuario = new Usuario { Id = "concess-123", Email = "kawa@test.com", UserName = "Kawa" } };
        var moto = new Moto
        {
            Id = 30,
            Placa = "KWA4000",
            Chassi = "CHASSI789",
            Cor = "Verde",
            Cliente = cliente,
            ModeloMoto = modelo,
            Concessionaria = concessionaria,
            RevisoesPlanejadas = new List<RevisaoMoto>
            {
                new RevisaoMoto { Id = 300, Quilometragem = 12000, Ordem = 3, Nome = "3ª Revisão", Status = "Planejada", DataPrevista = DateTime.UtcNow }
            }
        };

        context.Clientes.Add(cliente);
        context.Concessionarias.Add(concessionaria);
        context.Motos.Add(moto);
        await context.SaveChangesAsync();

        var service = new AlertaService(context, _hubContextMock.Object);

        // Act
        await service.GerarAlertaRevisaoAtrasadaAsync("user-789", 30, 12000);

        // Assert
        var alertas = await context.Alertas.Where(a => a.MotoId == 30).ToListAsync();
        Assert.Equal(2, alertas.Count);

        var alertaCliente = alertas.FirstOrDefault(a => a.UsuarioId == "user-789");
        var alertaConcess = alertas.FirstOrDefault(a => a.UsuarioId == "concess-123");

        Assert.NotNull(alertaCliente);
        Assert.Equal(TipoAlerta.RevisaoAtrasada, alertaCliente.Tipo);

        Assert.NotNull(alertaConcess);
        Assert.Equal(TipoAlerta.RevisaoAtrasada, alertaConcess.Tipo);

        _hubClientsMock.Verify(c => c.User("user-789"), Times.Once);
        _hubClientsMock.Verify(c => c.User("concess-123"), Times.Once);
    }

    [Fact]
    public async Task GerarAlertaAgendamentoCriadoAsync_DeveCriarDoisAlertas()
    {
        // Arrange
        using var context = CreateContext();
        var service = new AlertaService(context, _hubContextMock.Object);

        // Act
        await service.GerarAlertaAgendamentoCriadoAsync(1, "cliente-1", "concess-1", 100);

        // Assert
        var alertas = await context.Alertas.Where(a => a.AgendamentoId == 1).ToListAsync();
        Assert.Equal(2, alertas.Count);

        var cAlerta = alertas.FirstOrDefault(a => a.UsuarioId == "cliente-1");
        var pAlerta = alertas.FirstOrDefault(a => a.UsuarioId == "concess-1");

        Assert.NotNull(cAlerta);
        Assert.Equal(TipoAlerta.AgendamentoCriado, cAlerta.Tipo);

        Assert.NotNull(pAlerta);
        Assert.Equal(TipoAlerta.NovaSolicitacao, pAlerta.Tipo);
    }

    [Fact]
    public async Task GerarAlertaAgendamentoAlteradoAsync_DeveCriarDoisAlertas()
    {
        // Arrange
        using var context = CreateContext();
        var service = new AlertaService(context, _hubContextMock.Object);

        // Act
        await service.GerarAlertaAgendamentoAlteradoAsync(2, "cliente-2", "concess-2", 200);

        // Assert
        var alertas = await context.Alertas.Where(a => a.AgendamentoId == 2).ToListAsync();
        Assert.Equal(2, alertas.Count);

        var cAlerta = alertas.FirstOrDefault(a => a.UsuarioId == "cliente-2");
        var pAlerta = alertas.FirstOrDefault(a => a.UsuarioId == "concess-2");

        Assert.NotNull(cAlerta);
        Assert.Equal(TipoAlerta.AgendamentoAlterado, cAlerta.Tipo);

        Assert.NotNull(pAlerta);
        Assert.Equal(TipoAlerta.Reagendamento, pAlerta.Tipo);
    }

    [Fact]
    public async Task GerarAlertaAgendamentoCanceladoAsync_DeveCriarDoisAlertasDeCancelamento()
    {
        // Arrange
        using var context = CreateContext();
        var service = new AlertaService(context, _hubContextMock.Object);

        // Act
        await service.GerarAlertaAgendamentoCanceladoAsync(3, "cliente-3", "concess-3", 300);

        // Assert
        var alertas = await context.Alertas.Where(a => a.AgendamentoId == 3).ToListAsync();
        Assert.Equal(2, alertas.Count);

        var cAlerta = alertas.FirstOrDefault(a => a.UsuarioId == "cliente-3");
        var pAlerta = alertas.FirstOrDefault(a => a.UsuarioId == "concess-3");

        Assert.NotNull(cAlerta);
        Assert.Equal(TipoAlerta.Cancelamento, cAlerta.Tipo);

        Assert.NotNull(pAlerta);
        Assert.Equal(TipoAlerta.Cancelamento, pAlerta.Tipo);
    }

    [Fact]
    public async Task GerarAlertaAgendamentoAprovadoAsync_DeveCriarUmAlerta()
    {
        // Arrange
        using var context = CreateContext();
        var service = new AlertaService(context, _hubContextMock.Object);

        // Act
        await service.GerarAlertaAgendamentoAprovadoAsync(4, "cliente-4", "concess-4", 400);

        // Assert
        var alertas = await context.Alertas.Where(a => a.AgendamentoId == 4).ToListAsync();
        Assert.Single(alertas);
        Assert.Equal("cliente-4", alertas[0].UsuarioId);
        Assert.Equal(TipoAlerta.AgendamentoAprovado, alertas[0].Tipo);
    }

    [Fact]
    public async Task GerarAlertaAgendamentoRecusadoAsync_DeveCriarUmAlerta()
    {
        // Arrange
        using var context = CreateContext();
        var service = new AlertaService(context, _hubContextMock.Object);

        // Act
        await service.GerarAlertaAgendamentoRecusadoAsync(5, "cliente-5", "concess-5", 500);

        // Assert
        var alertas = await context.Alertas.Where(a => a.AgendamentoId == 5).ToListAsync();
        Assert.Single(alertas);
        Assert.Equal("cliente-5", alertas[0].UsuarioId);
        Assert.Equal(TipoAlerta.AgendamentoRecusado, alertas[0].Tipo);
    }

    [Fact]
    public async Task GerarAlertaRevisaoConcluidaAsync_DeveCriarUmAlerta()
    {
        // Arrange
        using var context = CreateContext();
        var service = new AlertaService(context, _hubContextMock.Object);

        // Act
        await service.GerarAlertaRevisaoConcluidaAsync("cliente-6", 600);

        // Assert
        var alertas = await context.Alertas.Where(a => a.MotoId == 600).ToListAsync();
        Assert.Single(alertas);
        Assert.Equal("cliente-6", alertas[0].UsuarioId);
        Assert.Equal(TipoAlerta.RevisaoConcluida, alertas[0].Tipo);
    }

    [Fact]
    public async Task ListarAlertasAsync_ComFiltros_DeveRetornarAlertasCorretos()
    {
        // Arrange
        using var context = CreateContext();
        var dataRef = DateTime.UtcNow;
        context.Alertas.AddRange(
            new Alerta { Id = 1, UsuarioId = "user-1", Tipo = TipoAlerta.RevisaoProxima, Lido = false, CriadoEm = dataRef.AddMinutes(-5) },
            new Alerta { Id = 2, UsuarioId = "user-1", Tipo = TipoAlerta.AgendamentoCriado, Lido = true, CriadoEm = dataRef.AddMinutes(-2) },
            new Alerta { Id = 3, UsuarioId = "user-1", Tipo = TipoAlerta.RevisaoProxima, Lido = true, CriadoEm = dataRef },
            new Alerta { Id = 4, UsuarioId = "user-2", Tipo = TipoAlerta.RevisaoProxima, Lido = false, CriadoEm = dataRef }
        );
        await context.SaveChangesAsync();

        var service = new AlertaService(context, _hubContextMock.Object);

        // Act & Assert 1: Sem filtros
        var result1 = await service.ListarAlertasAsync("user-1");
        Assert.Equal(3, result1.Count);
        Assert.Equal(3, result1[0].Id); // Ordenado por decrescente CriadoEm
        Assert.Equal(2, result1[1].Id);
        Assert.Equal(1, result1[2].Id);

        // Act & Assert 2: Filtrar por Lido = true
        var result2 = await service.ListarAlertasAsync("user-1", lido: true);
        Assert.Equal(2, result2.Count);
        Assert.All(result2, a => Assert.True(a.Lido));

        // Act & Assert 3: Filtrar por Lido = false
        var result3 = await service.ListarAlertasAsync("user-1", lido: false);
        Assert.Single(result3);
        Assert.Equal(1, result3[0].Id);

        // Act & Assert 4: Filtrar por Tipo = RevisaoProxima
        var result4 = await service.ListarAlertasAsync("user-1", tipo: TipoAlerta.RevisaoProxima);
        Assert.Equal(2, result4.Count);
        Assert.All(result4, a => Assert.Equal(TipoAlerta.RevisaoProxima, a.Tipo));

        // Act & Assert 5: Filtrar por Tipo = RevisaoProxima e Lido = true
        var result5 = await service.ListarAlertasAsync("user-1", lido: true, tipo: TipoAlerta.RevisaoProxima);
        Assert.Single(result5);
        Assert.Equal(3, result5[0].Id);
    }

    [Fact]
    public async Task ContarNaoLidosAsync_DeveRetornarContagemCorreta()
    {
        // Arrange
        using var context = CreateContext();
        context.Alertas.AddRange(
            new Alerta { Id = 1, UsuarioId = "user-1", Lido = false },
            new Alerta { Id = 2, UsuarioId = "user-1", Lido = true },
            new Alerta { Id = 3, UsuarioId = "user-1", Lido = false },
            new Alerta { Id = 4, UsuarioId = "user-2", Lido = false }
        );
        await context.SaveChangesAsync();

        var service = new AlertaService(context, _hubContextMock.Object);

        // Act
        var count = await service.ContarNaoLidosAsync("user-1");

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task MarcarComoLidoAsync_QuandoInexistenteOuOutroUsuario_DeveRetornarNull()
    {
        // Arrange
        using var context = CreateContext();
        context.Alertas.Add(new Alerta { Id = 1, UsuarioId = "user-1", Lido = false });
        await context.SaveChangesAsync();

        var service = new AlertaService(context, _hubContextMock.Object);

        // Act
        var resultNonExistent = await service.MarcarComoLidoAsync(999, "user-1");
        var resultWrongUser = await service.MarcarComoLidoAsync(1, "user-2");

        // Assert
        Assert.Null(resultNonExistent);
        Assert.Null(resultWrongUser);
    }

    [Fact]
    public async Task MarcarComoLidoAsync_QuandoNaoLido_DeveMarcarSalvarERetornar()
    {
        // Arrange
        using var context = CreateContext();
        var alerta = new Alerta { Id = 1, UsuarioId = "user-1", Lido = false };
        context.Alertas.Add(alerta);
        await context.SaveChangesAsync();

        var service = new AlertaService(context, _hubContextMock.Object);

        // Act
        var result = await service.MarcarComoLidoAsync(1, "user-1");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Lido);

        var dbAlerta = await context.Alertas.FindAsync(1);
        Assert.NotNull(dbAlerta);
        Assert.True(dbAlerta.Lido);
    }

    [Fact]
    public async Task MarcarComoLidoAsync_QuandoJaLido_DeveApenasRetornarSemSalvarNovamente()
    {
        // Arrange
        using var context = CreateContext();
        var alerta = new Alerta { Id = 1, UsuarioId = "user-1", Lido = true };
        context.Alertas.Add(alerta);
        await context.SaveChangesAsync();

        var service = new AlertaService(context, _hubContextMock.Object);

        // Act
        var result = await service.MarcarComoLidoAsync(1, "user-1");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Lido);
    }

    [Fact]
    public async Task MarcarTodosComoLidosAsync_DeveMarcarTodosNaoLidosDoUsuario()
    {
        // Arrange
        using var context = CreateContext();
        context.Alertas.AddRange(
            new Alerta { Id = 1, UsuarioId = "user-1", Lido = false },
            new Alerta { Id = 2, UsuarioId = "user-1", Lido = true },
            new Alerta { Id = 3, UsuarioId = "user-1", Lido = false },
            new Alerta { Id = 4, UsuarioId = "user-2", Lido = false }
        );
        await context.SaveChangesAsync();

        var service = new AlertaService(context, _hubContextMock.Object);

        // Act
        await service.MarcarTodosComoLidosAsync("user-1");

        // Assert
        var alertasUser1 = await context.Alertas.Where(a => a.UsuarioId == "user-1").ToListAsync();
        Assert.All(alertasUser1, a => Assert.True(a.Lido));

        var alertaUser2 = await context.Alertas.FindAsync(4);
        Assert.NotNull(alertaUser2);
        Assert.False(alertaUser2.Lido);
    }
}
