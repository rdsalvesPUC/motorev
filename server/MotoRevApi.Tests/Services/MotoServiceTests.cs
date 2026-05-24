using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using MotoRevApi.Services;
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
            Linha = "CB",
            Cilindrada = "500cc",
            Ano = 2023
        };
        context.ModelosMotos.Add(modelo);
        
        // Seed Cliente
        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        
        await context.SaveChangesAsync();

        var service = new MotoService(context);
        var request = new MotoRequest(
            "ABC-1234", 
            "CHASSI12345678901", 
            1, 
            2023, 
            "Vermelha", 
            1500, 
            DateTime.Now.AddMonths(-6), 
            null, 
            null,
            null
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
    public async Task CadastrarMotoAsync_DeveLancarDuplicateDataException_QuandoPlacaOuChassiDuplicados()
    {
        // Arrange
        using var context = CreateContext();
        
        // Seed Modelo e Cliente
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true, Linha = "Linha", Cilindrada = "100cc" };
        context.ModelosMotos.Add(modelo);
        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var service = new MotoService(context);
        var request1 = new MotoRequest("ABC-1234", "CHASSI12345678901", 1, 2023, "Preta", 0, DateTime.Now, null, null, null);
        var request2 = new MotoRequest("ABC1234", "CHASSI99999999999", 1, 2023, "Preta", 0, DateTime.Now, null, null, null); // Placa duplicada (sem hífen)
        var request3 = new MotoRequest("XYZ-9999", "CHASSI12345678901", 1, 2023, "Preta", 0, DateTime.Now, null, null, null); // Chassi duplicado

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
        var request = new MotoRequest("ABC-1234", "CHASSI12345678901", 999, 2023, "Preta", 0, DateTime.Now, null, null, null); // Modelo inexistente

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.CadastrarMotoAsync(request, "user123"));
    }

    [Fact]
    public async Task CadastrarMotoAsync_DeveLancarNotFoundException_QuandoClienteNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true, Linha = "Linha", Cilindrada = "100cc" };
        context.ModelosMotos.Add(modelo);
        await context.SaveChangesAsync();

        var service = new MotoService(context);
        var request = new MotoRequest("ABC-1234", "CHASSI12345678901", 1, 2023, "Preta", 0, DateTime.Now, null, null, null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.CadastrarMotoAsync(request, "invalid-user"));
    }

    [Fact]
    public async Task ListarMotosClienteAsync_DeveRetornarListaDeMotos()
    {
        // Arrange
        using var context = CreateContext();
        
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true };
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
}
