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
    }

    private AppDbContext CreateContext() => new AppDbContext(_dbContextOptions);

    [Fact]
    public async Task CadastrarMotoAsync_DeveCriarMotoComSucesso()
    {
        // Arrange
        using var context = CreateContext();
        
        // Seed Modelo
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true };
        context.ModelosMotos.Add(modelo);
        
        // Seed Cliente
        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        
        await context.SaveChangesAsync();

        var service = new MotoService(context);
        var request = new MotoRequest("ABC-1234", "CHASSI12345678901", 1, null);

        // Act
        var response = await service.CadastrarMotoAsync(request, "user123");

        // Assert
        Assert.NotNull(response);
        Assert.Equal("ABC1234", response.Placa); // Sem hífen e em caixa alta
        Assert.Equal("CHASSI12345678901", response.Chassi);
        Assert.Equal(1, response.ModeloMotoId);
        Assert.Equal("CB 500F", response.NomeModelo);
        Assert.Equal("Honda", response.Marca);
        Assert.Equal(1, response.ClienteId);
    }

    [Fact]
    public async Task CadastrarMotoAsync_DeveLancarDuplicateDataException_QuandoPlacaOuChassiDuplicados()
    {
        // Arrange
        using var context = CreateContext();
        
        // Seed Modelo e Cliente
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true };
        context.ModelosMotos.Add(modelo);
        var cliente = new Cliente { Id = 1, Nome = "Cliente Teste", UsuarioId = "user123" };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var service = new MotoService(context);
        var request1 = new MotoRequest("ABC-1234", "CHASSI12345678901", 1, null);
        var request2 = new MotoRequest("ABC1234", "CHASSI99999999999", 1, null); // Placa duplicada (sem hífen)
        var request3 = new MotoRequest("XYZ-9999", "CHASSI12345678901", 1, null); // Chassi duplicado

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
        var request = new MotoRequest("ABC-1234", "CHASSI12345678901", 999, null); // Modelo inexistente

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.CadastrarMotoAsync(request, "user123"));
    }

    [Fact]
    public async Task CadastrarMotoAsync_DeveLancarNotFoundException_QuandoClienteNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "CB 500F", Marca = "Honda", Ativo = true };
        context.ModelosMotos.Add(modelo);
        await context.SaveChangesAsync();

        var service = new MotoService(context);
        var request = new MotoRequest("ABC-1234", "CHASSI12345678901", 1, null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => service.CadastrarMotoAsync(request, "invalid-user"));
    }
}
