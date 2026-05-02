using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Enums;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Services;

public class ServicoServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public ServicoServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    private AppDbContext CreateContext() => new AppDbContext(_dbContextOptions);

    [Fact]
    public async Task CreateAsync_DeveCriarServicoComSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        var request = new ServicoRequest("Troca de Óleo", CategoriaServico.Troca, 30);

        // Act
        var response = await service.CreateAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Troca de Óleo", response.Nome);
        Assert.Equal(CategoriaServico.Troca, response.Categoria);
        Assert.Equal(30, response.TempoEstimado);
    }

    [Fact]
    public async Task CreateAsync_DeveRetornarErroParaDuplicidadeGlobally()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        var request = new ServicoRequest("Limpeza Geral", CategoriaServico.Limpeza, 60);

        // Criar primeiro serviço
        await service.CreateAsync(request);

        // Act & Assert - tentar criar um serviço duplicado
        await Assert.ThrowsAsync<DuplicateDataException>(
            () => service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_DevePermitirServicosDiferentesPorCategoria()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        var request1 = new ServicoRequest("Troca de Óleo", CategoriaServico.Troca, 30);
        var request2 = new ServicoRequest("Troca de Óleo", CategoriaServico.Verificacao, 15);

        // Act
        var response1 = await service.CreateAsync(request1);
        var response2 = await service.CreateAsync(request2);

        // Assert
        Assert.NotNull(response1);
        Assert.NotNull(response2);
        Assert.Equal(request1.Nome, response1.Nome);
        Assert.Equal(request2.Nome, response2.Nome);
        Assert.NotEqual(response1.Categoria, response2.Categoria);
    }

    [Theory]
    [InlineData(CategoriaServico.Verificacao)]
    [InlineData(CategoriaServico.Ajuste)]
    [InlineData(CategoriaServico.Limpeza)]
    [InlineData(CategoriaServico.Troca)]
    public async Task CreateAsync_DeveAceitarTodasAsCategorias(CategoriaServico categoria)
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        var request = new ServicoRequest("Serviço Categoria", categoria, 45);

        // Act
        var response = await service.CreateAsync(request);

        // Assert
        Assert.Equal(categoria, response.Categoria);
    }

    [Fact]
    public async Task GetAllAsync_DeveRetornarListaVaziaQuandoNaoHouverservicos()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);

        // Act
        var response = await service.GetAllAsync();

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response);
    }

    [Fact]
    public async Task GetAllAsync_DeveRetornarListaDeServicos()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        await service.CreateAsync(new ServicoRequest("Ajuste Corrente", CategoriaServico.Ajuste, 10));
        await service.CreateAsync(new ServicoRequest("Troca Pneu", CategoriaServico.Troca, 45));

        // Act
        var response = await service.GetAllAsync();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Count());
    }

    [Fact]
    public async Task GetAllAsync_DeveFiltrarPorCategoria()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        await service.CreateAsync(new ServicoRequest("Ajuste Corrente", CategoriaServico.Ajuste, 10));
        await service.CreateAsync(new ServicoRequest("Ajuste Embreagem", CategoriaServico.Ajuste, 15));
        await service.CreateAsync(new ServicoRequest("Troca Pneu", CategoriaServico.Troca, 45));

        // Act
        var response = await service.GetAllAsync(CategoriaServico.Ajuste);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Count());
        Assert.All(response, s => Assert.Equal(CategoriaServico.Ajuste, s.Categoria));
    }
}
