using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using MotoRevApi.Services;
using Moq;
using Xunit;

namespace MotoRevApi.Tests.Services;

public class MotoServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public MotoServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
            
        // Garantir que o Mapster esteja configurado uma vez para os testes
        MotoRevApi.Profiles.MapsterConfig.RegisterMapsterConfiguration();
    }

    private AppDbContext CreateContext() => new AppDbContext(_dbContextOptions);

    private static void SeedRevisaoPadrao(AppDbContext context, ModeloMoto modelo)
    {
        context.RevisoesPadrao.Add(new RevisaoPadrao
        {
            Nome = "Primeira revisão",
            Ordem = 1,
            Quilometragem = 1000,
            TempoMeses = 6,
            LinhaId = modelo.LinhaId,
            Ativo = true
        });
        context.SaveChanges();
    }

    [Fact]
    public async Task CadastrarMotoAsync_DeveCriarMotoComSucesso()
    {
        // Arrange
        using var context = CreateContext();
        
        // Seed Modelo
        var modelo = new ModeloMoto 
        { 
            Id = 1, 
            NomeModelo = "CB 500F", 
            Marca = "Honda", 
            Ativo = true,
            Linha = new Linha { Nome = "CB" },
            Cilindrada = "500cc",
            Ano = 2023
        };
        context.ModelosMotos.Add(modelo);
        
        // Seed Cliente
        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        
        await context.SaveChangesAsync();
        SeedRevisaoPadrao(context, modelo);

        var service = new MotoService(context);
        var request = new MotoRequest(
            "ABC-1234", 
            "CHASSI12345678901", 
            1, 
            "Vermelha", 
            1500, 
            DateTime.Now.AddMonths(-6)
        );

        // Act
        var response = await service.CadastrarMotoAsync(request, "user123");

        // Assert
        Assert.NotNull(response);
        Assert.Equal("ABC1234", response.Placa); // Sem hífen e em caixa alta
        Assert.Equal("CHASSI12345678901", response.Chassi);
        Assert.Equal(1, response.ModeloMotoId);
        Assert.Equal("CB 500F", response.NomeModelo);
        Assert.Equal("Honda", response.Marca);
        Assert.Equal("CB", response.Linha);
        Assert.Equal("500cc", response.Cilindrada);
        Assert.Equal(2023, response.Ano);
    }

    [Fact]
    public async Task CadastrarMotoAsync_DeveInstanciarRevisoesPlanejadasDaLinhaDoModelo()
    {
        // Arrange
        using var context = CreateContext();

        var linha = new Linha { Id = 10, Nome = "Linha City", Ativo = true };
        var modelo = new ModeloMoto
        {
            Id = 1,
            NomeModelo = "City 160",
            Marca = "Honda",
            Ativo = true,
            LinhaId = linha.Id,
            Linha = linha,
            Cilindrada = "160cc",
            Ano = 2024
        };
        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        var dataVenda = new DateTime(2026, 1, 15);

        context.Linhas.Add(linha);
        context.ModelosMotos.Add(modelo);
        context.Clientes.Add(cliente);
        context.RevisoesPadrao.AddRange(
            new RevisaoPadrao
            {
                Id = 100,
                LinhaId = linha.Id,
                Nome = "Primeira revisão",
                Ordem = 1,
                Quilometragem = 1000,
                TempoMeses = 6,
                Ativo = true
            },
            new RevisaoPadrao
            {
                Id = 101,
                LinhaId = linha.Id,
                Nome = "Segunda revisão",
                Ordem = 2,
                Quilometragem = 5000,
                TempoMeses = 12,
                Ativo = true
            },
            new RevisaoPadrao
            {
                Id = 102,
                LinhaId = linha.Id,
                Nome = "Revisão inativa",
                Ordem = 3,
                Quilometragem = 10000,
                TempoMeses = 18,
                Ativo = false
            }
        );
        await context.SaveChangesAsync();

        var service = new MotoService(context);
        var request = new MotoRequest(
            "ABC-1234",
            "CHASSI12345678901",
            modelo.Id,
            "Vermelha",
            1500,
            dataVenda
        );

        // Act
        var response = await service.CadastrarMotoAsync(request, "user123");

        // Assert
        Assert.Equal(2, response.RevisoesPlanejadas.Count);
        Assert.Collection(response.RevisoesPlanejadas,
            revisao =>
            {
                Assert.Equal("Primeira revisão", revisao.Nome);
                Assert.Equal(1, revisao.Ordem);
                Assert.Equal(1000, revisao.Quilometragem);
                Assert.Equal(dataVenda.AddMonths(6), revisao.DataPrevista);
                Assert.Equal("Planejada", revisao.Status);
            },
            revisao =>
            {
                Assert.Equal("Segunda revisão", revisao.Nome);
                Assert.Equal(2, revisao.Ordem);
                Assert.Equal(5000, revisao.Quilometragem);
                Assert.Equal(dataVenda.AddMonths(12), revisao.DataPrevista);
                Assert.Equal("Planejada", revisao.Status);
            });

        var revisoesDb = context.RevisoesMotos
            .Where(revisao => revisao.MotoId == response.Id)
            .OrderBy(revisao => revisao.Ordem)
            .ToList();
        Assert.Equal(2, revisoesDb.Count);
        Assert.All(revisoesDb, revisao => Assert.Equal("Planejada", revisao.Status));
    }

    [Fact]
    public async Task CadastrarMotoAsync_DeveLancarDuplicateDataException_QuandoPlacaOuChassiDuplicados()
    {
        // Arrange
        using var context = CreateContext();
        
        // Seed Modelo e Cliente
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true, Linha = new Linha { Nome = "Linha" }, Cilindrada = "100cc" };
        context.ModelosMotos.Add(modelo);
        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();
        SeedRevisaoPadrao(context, modelo);

        var service = new MotoService(context);
        var request1 = new MotoRequest("ABC-1234", "CHASSI12345678901", 1, "Preta", 0, DateTime.Now);
        var request2 = new MotoRequest("ABC1234", "CHASSI99999999999", 1, "Preta", 0, DateTime.Now); // Placa duplicada (sem hífen)
        var request3 = new MotoRequest("XYZ-9999", "CHASSI12345678901", 1, "Preta", 0, DateTime.Now); // Chassi duplicado

        await service.CadastrarMotoAsync(request1, "user123");

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(
            () => service.CadastrarMotoAsync(request2, "user123"));

        await Assert.ThrowsAsync<DuplicateDataException>(
            () => service.CadastrarMotoAsync(request3, "user123"));
    }

    [Fact]
    public async Task CadastrarMotoAsync_DeveLancarNotFoundException_QuandoModeloNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var service = new MotoService(context);
        var request = new MotoRequest("ABC-1234", "CHASSI12345678901", 999, "Preta", 0, DateTime.Now); // Modelo inexistente

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.CadastrarMotoAsync(request, "user123"));
    }

    [Fact]
    public async Task CadastrarMotoAsync_DeveLancarNotFoundException_QuandoClienteNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true, Linha = new Linha { Nome = "Linha" }, Cilindrada = "100cc" };
        context.ModelosMotos.Add(modelo);
        await context.SaveChangesAsync();
        SeedRevisaoPadrao(context, modelo);

        var service = new MotoService(context);
        var request = new MotoRequest("ABC-1234", "CHASSI12345678901", 1, "Preta", 0, DateTime.Now);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.CadastrarMotoAsync(request, "invalid-user"));
    }

    [Fact]
    public async Task CadastrarMotoAsync_DeveLancarBusinessRuleException_QuandoModeloNaoPossuiRevisaoPadraoAtiva()
    {
        // Arrange
        using var context = CreateContext();
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true, Linha = new Linha { Nome = "Linha" }, Cilindrada = "100cc" };
        context.ModelosMotos.Add(modelo);
        context.Clientes.Add(new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" });
        await context.SaveChangesAsync();

        var service = new MotoService(context);
        var request = new MotoRequest("ABC-1234", "CHASSI12345678901", 1, "Preta", 0, DateTime.Now);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CadastrarMotoAsync(request, "user123"));
    }

    [Fact]
    public async Task ListarMotosClienteAsync_DeveRetornarListaDeMotos()
    {
        // Arrange
        using var context = CreateContext();
        
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true, Linha = new Linha { Nome = "CB" } };
        context.ModelosMotos.Add(modelo);
        
        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        
        context.Motos.Add(new Moto { 
            Placa = "ABC1234", Chassi = "CHASSI1", ClienteId = 1, ModeloMotoId = 1, Ativo = true,
            Cor = "Preta", KilometragemAtual = 0, DataVenda = DateTime.Now
        });
        context.Motos.Add(new Moto { 
            Placa = "XYZ9999", Chassi = "CHASSI2", ClienteId = 1, ModeloMotoId = 1, Ativo = true,
            Cor = "Azul", KilometragemAtual = 5000, DataVenda = DateTime.Now.AddYears(-1)
        });
        
        await context.SaveChangesAsync();

        var service = new MotoService(context);

        // Act
        var result = await service.ListarMotosClienteAsync("user123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ListarMotosClienteAsync_DeveRetornarVazio_QuandoClienteNaoTemMotos()
    {
        // Arrange
        using var context = CreateContext();
        
        var cliente = new Cliente { Id = 1, Nome = "Novo Cliente", UsuarioId = "newuser" };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var service = new MotoService(context);

        // Act
        var result = await service.ListarMotosClienteAsync("newuser");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task AtualizarMotoAsync_DeveAtualizarPlacaECorComSucesso()
    {
        // Arrange
        using var context = CreateContext();

        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true, Linha = new Linha { Nome = "CB" }, Cilindrada = "500cc", Ano = 2023 };
        context.ModelosMotos.Add(modelo);
        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        context.Motos.Add(new Moto
        {
            Id = 1,
            Placa = "ABC1234",
            Chassi = "CHASSI12345678901",
            ClienteId = 1,
            ModeloMotoId = 1,
            Ativo = true,
            Cor = "Preta",
            KilometragemAtual = 1000,
            DataVenda = DateTime.Now.AddYears(-1)
        });
        await context.SaveChangesAsync();

        var service = new MotoService(context);
        var request = new MotoUpdateRequest("XYZ-9999", "Azul", 1000);

        // Act
        var response = await service.AtualizarMotoAsync(1, request, "user123");

        // Assert
        Assert.NotNull(response);
        Assert.Equal("XYZ9999", response.Placa);
        Assert.Equal("Azul", response.Cor);
        // Chassi deve permanecer inalterado
        Assert.Equal("CHASSI12345678901", response.Chassi);
        // ModeloMotoId deve permanecer inalterado
        Assert.Equal(1, response.ModeloMotoId);
    }

    [Fact]
    public async Task AtualizarMotoAsync_DeveLancarDuplicateDataException_QuandoPlacaJaEmUso()
    {
        // Arrange
        using var context = CreateContext();

        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true, Linha = new Linha { Nome = "CB" }, Cilindrada = "500cc" };
        context.ModelosMotos.Add(modelo);
        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        context.Motos.Add(new Moto
        {
            Id = 1,
            Placa = "ABC1234",
            Chassi = "CHASSI12345678901",
            ClienteId = 1,
            ModeloMotoId = 1,
            Ativo = true,
            Cor = "Preta",
            KilometragemAtual = 0,
            DataVenda = DateTime.Now
        });
        // Segunda moto com outra placa
        context.Motos.Add(new Moto
        {
            Id = 2,
            Placa = "XYZ9999",
            Chassi = "CHASSI99999999999",
            ClienteId = 1,
            ModeloMotoId = 1,
            Ativo = true,
            Cor = "Azul",
            KilometragemAtual = 0,
            DataVenda = DateTime.Now
        });
        await context.SaveChangesAsync();

        var service = new MotoService(context);
        // Tenta atualizar moto 1 com a placa da moto 2
        var request = new MotoUpdateRequest("XYZ-9999", "Verde");

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(
            () => service.AtualizarMotoAsync(1, request, "user123"));
    }

    [Fact]
    public async Task AtualizarMotoAsync_DeveLancarNotFoundException_QuandoMotoNaoEncontrada()
    {
        // Arrange
        using var context = CreateContext();

        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var service = new MotoService(context);
        var request = new MotoUpdateRequest("ABC-1234", "Preta");

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.AtualizarMotoAsync(999, request, "user123"));
    }

    [Fact]
    public async Task AtualizarMotoAsync_DeveManter_ChassiModeloAnoImutaveis()
    {
        // Arrange
        using var context = CreateContext();

        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true, Linha = new Linha { Nome = "CB" }, Cilindrada = "500cc", Ano = 2023 };
        context.ModelosMotos.Add(modelo);
        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        context.Motos.Add(new Moto
        {
            Id = 1,
            Placa = "ABC1234",
            Chassi = "CHASSI12345678901",
            ClienteId = 1,
            ModeloMotoId = 1,
            Ativo = true,
            Cor = "Preta",
            KilometragemAtual = 5000,
            DataVenda = new DateTime(2022, 1, 15)
        });
        await context.SaveChangesAsync();

        var service = new MotoService(context);
        // O DTO de update NÃO contém Chassi, ModeloMotoId nem Ano — garantia estrutural
        var request = new MotoUpdateRequest("DEF-5678", "Branca", 5000);

        // Act
        var response = await service.AtualizarMotoAsync(1, request, "user123");

        // Assert: apenas Placa e Cor mudam
        Assert.Equal("DEF5678", response.Placa);
        Assert.Equal("Branca", response.Cor);
        Assert.Equal("CHASSI12345678901", response.Chassi);
        Assert.Equal(1, response.ModeloMotoId);
    }

    [Fact]
    public async Task AtualizarMotoAsync_DeveAtualizarKilometragem_QuandoValorMaiorOuIgual()
    {
        // Arrange
        using var context = CreateContext();

        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true, Linha = new Linha { Nome = "CB" }, Cilindrada = "500cc", Ano = 2023 };
        context.ModelosMotos.Add(modelo);
        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        context.Motos.Add(new Moto
        {
            Id = 1,
            Placa = "ABC1234",
            Chassi = "CHASSI12345678901",
            ClienteId = 1,
            ModeloMotoId = 1,
            Ativo = true,
            Cor = "Preta",
            KilometragemAtual = 5000,
            DataVenda = new DateTime(2022, 1, 15)
        });
        await context.SaveChangesAsync();

        var service = new MotoService(context);
        var request = new MotoUpdateRequest("ABC-1234", "Preta", 6000);

        // Act
        var response = await service.AtualizarMotoAsync(1, request, "user123");

        // Assert
        Assert.Equal(6000, response.KilometragemAtual);
    }

    [Fact]
    public async Task AtualizarMotoAsync_DeveLancarBusinessRuleException_QuandoValorMenor()
    {
        // Arrange
        using var context = CreateContext();

        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true, Linha = new Linha { Nome = "CB" }, Cilindrada = "500cc", Ano = 2023 };
        context.ModelosMotos.Add(modelo);
        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        context.Motos.Add(new Moto
        {
            Id = 1,
            Placa = "ABC1234",
            Chassi = "CHASSI12345678901",
            ClienteId = 1,
            ModeloMotoId = 1,
            Ativo = true,
            Cor = "Preta",
            KilometragemAtual = 5000,
            DataVenda = new DateTime(2022, 1, 15)
        });
        await context.SaveChangesAsync();

        var service = new MotoService(context);
        var request = new MotoUpdateRequest("ABC-1234", "Preta", 4999);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.AtualizarMotoAsync(1, request, "user123"));
    }

    [Fact]
    public async Task GetByIdAsync_DeveRetornarMotoComSucesso_QuandoMotoExisteEPertenceAoCliente()
    {
        // Arrange
        using var context = CreateContext();

        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true, Linha = new Linha { Nome = "CB" }, Cilindrada = "500cc", Ano = 2023 };
        context.ModelosMotos.Add(modelo);
        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        context.Motos.Add(new Moto
        {
            Id = 10,
            Placa = "ABC1234",
            Chassi = "CHASSI12345678901",
            ClienteId = 1,
            ModeloMotoId = 1,
            Ativo = true,
            Cor = "Preta",
            KilometragemAtual = 5000,
            DataVenda = new DateTime(2022, 1, 15)
        });
        await context.SaveChangesAsync();

        var service = new MotoService(context);

        // Act
        var response = await service.GetByIdAsync(10, "user123");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(10, response.Id);
        Assert.Equal("ABC1234", response.Placa);
        Assert.Equal("CHASSI12345678901", response.Chassi);
        Assert.Equal(1, response.ModeloMotoId);
        Assert.Equal("CB 500F", response.NomeModelo);
        Assert.Equal("Honda", response.Marca);
        Assert.Equal(2023, response.Ano);
    }

    [Fact]
    public async Task GetByIdAsync_DeveLancarNotFoundException_QuandoMotoNaoExiste()
    {
        // Arrange
        using var context = CreateContext();

        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var service = new MotoService(context);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetByIdAsync(999, "user123"));
    }

    [Fact]
    public async Task GetByIdAsync_DeveLancarNotFoundException_QuandoMotoPertenceAOutroCliente()
    {
        // Arrange
        using var context = CreateContext();

        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true };
        context.ModelosMotos.Add(modelo);
        var cliente1 = new Cliente { Id = 1, Nome = "Cliente 1", UsuarioId = "user1" };
        var cliente2 = new Cliente { Id = 2, Nome = "Cliente 2", UsuarioId = "user2" };
        context.Clientes.AddRange(cliente1, cliente2);
        
        // Moto cadastrada para cliente 2
        context.Motos.Add(new Moto
        {
            Id = 10,
            Placa = "ABC1234",
            Chassi = "CHASSI12345678901",
            ClienteId = 2,
            ModeloMotoId = 1,
            Ativo = true,
            Cor = "Preta",
            KilometragemAtual = 5000,
            DataVenda = DateTime.Now
        });
        await context.SaveChangesAsync();

        var service = new MotoService(context);

        // Act & Assert: Cliente 1 tenta acessar moto do Cliente 2, deve lançar NotFoundException
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetByIdAsync(10, "user1"));
    }

    [Fact]
    public async Task InativarMotoAsync_DeveInativarComSucesso_QuandoMotoExisteESemPendencias()
    {
        // Arrange
        using var context = CreateContext();

        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        
        var moto = new Moto
        {
            Id = 10,
            Placa = "ABC1234",
            Chassi = "CHASSI12345678901",
            ClienteId = 1,
            ModeloMotoId = 1,
            Ativo = true,
            Cor = "Preta",
            KilometragemAtual = 5000,
            DataVenda = DateTime.Now
        };
        context.Motos.Add(moto);
        await context.SaveChangesAsync();

        var service = new MotoService(context);

        // Act
        await service.InativarMotoAsync(10, "user123");

        // Assert
        using var verifyContext = CreateContext();
        var updatedMoto = await verifyContext.Motos.FindAsync(10);
        Assert.NotNull(updatedMoto);
        Assert.False(updatedMoto.Ativo);
    }

    [Fact]
    public async Task InativarMotoAsync_DeveLancarBusinessRuleException_QuandoMotoPossuiAgendamentosPendentes()
    {
        // Arrange
        using var context = CreateContext();

        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        
        var moto = new Moto
        {
            Id = 10,
            Placa = "ABC1234",
            Chassi = "CHASSI12345678901",
            ClienteId = 1,
            ModeloMotoId = 1,
            Ativo = true,
            Cor = "Preta",
            KilometragemAtual = 5000,
            DataVenda = DateTime.Now
        };
        context.Motos.Add(moto);
        await context.SaveChangesAsync();

        // Criar Mock do MotoService para forçar TemAgendamentosPendentesAsync a retornar true
        var serviceMock = new Mock<MotoService>(context);
        serviceMock.Setup(s => s.TemAgendamentosPendentesAsync(10))
            .ReturnsAsync(true);

        // Configurar as outras chamadas virtuais para se comportarem normalmente chamando a base
        serviceMock.CallBase = true;

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => serviceMock.Object.InativarMotoAsync(10, "user123"));
        Assert.Equal("Não é possível inativar uma moto com agendamentos pendentes.", ex.Message);
    }

    [Fact]
    public async Task InativarMotoAsync_DeveLancarNotFoundException_QuandoMotoNaoExiste()
    {
        // Arrange
        using var context = CreateContext();

        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var service = new MotoService(context);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.InativarMotoAsync(999, "user123"));
    }

    [Fact]
    public async Task InativarMotoAsync_DeveLancarNotFoundException_QuandoMotoPertenceAOutroCliente()
    {
        // Arrange
        using var context = CreateContext();

        var cliente1 = new Cliente { Id = 1, Nome = "Cliente 1", UsuarioId = "user1" };
        var cliente2 = new Cliente { Id = 2, Nome = "Cliente 2", UsuarioId = "user2" };
        context.Clientes.AddRange(cliente1, cliente2);

        var moto = new Moto
        {
            Id = 10,
            Placa = "ABC1234",
            Chassi = "CHASSI12345678901",
            ClienteId = 2, // Pertence ao cliente 2
            ModeloMotoId = 1,
            Ativo = true,
            Cor = "Preta",
            KilometragemAtual = 5000,
            DataVenda = DateTime.Now
        };
        context.Motos.Add(moto);
        await context.SaveChangesAsync();

        var service = new MotoService(context);

        // Act & Assert: Cliente 1 tenta inativar moto do Cliente 2, deve lançar NotFoundException
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.InativarMotoAsync(10, "user1"));
    }
}
