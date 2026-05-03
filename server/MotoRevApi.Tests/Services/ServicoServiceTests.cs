using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Enums;
using MotoRevApi.Exceptions;
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

        var responseList = response.ToList();

        // Assert
        Assert.NotNull(responseList);
        Assert.Equal(2, responseList.Count);
        Assert.All(responseList, s => Assert.Equal(CategoriaServico.Ajuste, s.Categoria));
    }

    [Fact]
    public async Task GetAllAsync_NaoDeveRetornarServicosInativos()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        
        var servicoAtivo1 = await service.CreateAsync(new ServicoRequest("Ajuste Corrente", CategoriaServico.Ajuste, 10));
        var servicoInativo = await service.CreateAsync(new ServicoRequest("Ajuste Embreagem", CategoriaServico.Ajuste, 15));
        var servicoAtivo2 = await service.CreateAsync(new ServicoRequest("Troca Pneu", CategoriaServico.Troca, 45));

        await service.InactivateAsync(servicoInativo.Id);

        // Act
        var response = await service.GetAllAsync();

        var responseList = response.ToList();

        // Assert
        Assert.NotNull(responseList);
        Assert.Equal(2, responseList.Count);
        Assert.DoesNotContain(responseList, s => s.Id == servicoInativo.Id);
        Assert.Contains(responseList, s => s.Id == servicoAtivo1.Id);
        Assert.Contains(responseList, s => s.Id == servicoAtivo2.Id);
    }
    
    [Fact]
    public async Task GetByIdAsync_DeveRetornarServicoComSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        var servicoCriado = await service.CreateAsync(new ServicoRequest("Ajuste Corrente", CategoriaServico.Ajuste, 10));

        // Act
        var response = await service.GetByIdAsync(servicoCriado.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(servicoCriado.Id, response.Id);
        Assert.Equal("Ajuste Corrente", response.Nome);
        Assert.Equal(CategoriaServico.Ajuste, response.Categoria);
    }

    [Fact]
    public async Task GetByIdAsync_DeveRetornarErroSeServicoNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetByIdAsync(999));
            
        Assert.Contains("não encontrado", exception.Message);
    }

    [Fact]
    public async Task GetByIdAsync_NaoDeveRetornarServicoInativo()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        var servicoCriado = await service.CreateAsync(new ServicoRequest("Ajuste Corrente", CategoriaServico.Ajuste, 10));
        await service.InactivateAsync(servicoCriado.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetByIdAsync(servicoCriado.Id));

        Assert.Contains("não encontrado", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_DeveAtualizarServicoComSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        var createRequest = new ServicoRequest("Troca de Óleo", CategoriaServico.Troca, 30);
        var servicoCriado = await service.CreateAsync(createRequest);

        var updateRequest = new ServicoUpdateRequest("Troca de Óleo Sintético", CategoriaServico.Troca, 45);

        // Act
        var response = await service.UpdateAsync(servicoCriado.Id, updateRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(servicoCriado.Id, response.Id);
        Assert.Equal("Troca de Óleo Sintético", response.Nome);
        Assert.Equal(CategoriaServico.Troca, response.Categoria);
        Assert.Equal(45, response.TempoEstimado);

        var servicoNoBanco = await context.Servicos.FindAsync(servicoCriado.Id);
        Assert.NotNull(servicoNoBanco);
        Assert.Equal("Troca de Óleo Sintético", servicoNoBanco.Nome);
        Assert.Equal(45, servicoNoBanco.TempoEstimado);
    }

    [Fact]
    public async Task UpdateAsync_DeveRetornarErroSeServicoNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        var updateRequest = new ServicoUpdateRequest("Serviço Inexistente", CategoriaServico.Troca, 30);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateAsync(999, updateRequest));
            
        Assert.Contains("não encontrado", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_DeveRetornarErroParaDuplicidadeComOutroServico()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        
        await service.CreateAsync(new ServicoRequest("Serviço A", CategoriaServico.Troca, 30));
        var servico2 = await service.CreateAsync(new ServicoRequest("Serviço B", CategoriaServico.Troca, 45));

        // Tenta atualizar o serviço 2 com o nome e categoria do serviço 1
        var updateRequest = new ServicoUpdateRequest("Serviço A", CategoriaServico.Troca, 60);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DuplicateDataException>(
            () => service.UpdateAsync(servico2.Id, updateRequest));
            
        Assert.Contains("Já existe outro serviço com o nome", exception.Message);
    }

    [Fact]
    public async Task InactivateAsync_DeveInativarServicoComSucessoENaoRemoverFisicamente()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        var createRequest = new ServicoRequest("Troca de Óleo", CategoriaServico.Troca, 30);
        var servicoCriado = await service.CreateAsync(createRequest);

        // Act
        await service.InactivateAsync(servicoCriado.Id);

        // Assert
        var servicoNoBanco = await context.Servicos.FindAsync(servicoCriado.Id);
        Assert.NotNull(servicoNoBanco);
        Assert.False(servicoNoBanco.Ativo);
    }

    [Fact]
    public async Task InactivateAsync_ServicoJaInativo_DeveManterInativoSemErro()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        var createRequest = new ServicoRequest("Troca de Óleo", CategoriaServico.Troca, 30);
        var servicoCriado = await service.CreateAsync(createRequest);
        await service.InactivateAsync(servicoCriado.Id); // Primeira inativação

        // Act
        await service.InactivateAsync(servicoCriado.Id); // Segunda inativação

        // Assert
        var servicoNoBanco = await context.Servicos.FindAsync(servicoCriado.Id);
        Assert.NotNull(servicoNoBanco);
        Assert.False(servicoNoBanco.Ativo);
    }

    [Fact]
    public async Task InactivateAsync_DeveRetornarErroSeServicoNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.InactivateAsync(999));

        Assert.Contains("não encontrado", exception.Message);
    }
}
