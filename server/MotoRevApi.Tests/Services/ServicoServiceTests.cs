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
        var request = new ServicoRequest("COD001", "Troca de Óleo", "Desc", CategoriaServico.Troca, 30, 100);

        // Act
        var response = await service.CreateAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Troca de Óleo", response.Nome);
        Assert.Equal(CategoriaServico.Troca, response.Categoria);
        Assert.Equal(30, response.TempoEstimado);
    }

    [Fact]
    public async Task CreateAsync_DeveRetornarErroParaDuplicidadeGlobalmente()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        var request1 = new ServicoRequest("COD002", "Limpeza Geral", "Desc", CategoriaServico.Limpeza, 60, 150);
        var request2 = new ServicoRequest("COD002", "Outra Limpeza", "Desc", CategoriaServico.Troca, 30, 100);

        // Criar primeiro serviço
        await service.CreateAsync(request1);

        // Act & Assert - tentar criar um serviço com mesmo código em categoria diferente
        var exception = await Assert.ThrowsAsync<DuplicateDataException>(
            () => service.CreateAsync(request2));
            
        Assert.Contains("Já existe um serviço ativo com o código 'COD002'", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_DevePermitirReutilizarCodigoDeServicoInativo()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        
        // Criar e inativar serviço
        var res1 = await service.CreateAsync(new ServicoRequest("COD_REUSE", "Serviço 1", "Desc", CategoriaServico.Troca, 30, 100));
        await service.InactivateAsync(res1.Id);

        // Tentar criar novo serviço com mesmo código
        var request2 = new ServicoRequest("COD_REUSE", "Serviço 2", "Desc", CategoriaServico.Limpeza, 60, 150);

        // Act
        var response2 = await service.CreateAsync(request2);

        // Assert
        Assert.NotNull(response2);
        Assert.Equal("COD_REUSE", response2.Codigo);
        Assert.NotEqual(res1.Id, response2.Id);
    }

    [Fact]
    public async Task CreateAsync_DevePermitirReutilizarNomeECategoriaDeServicoInativo()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        
        // Criar e inativar serviço
        var res1 = await service.CreateAsync(new ServicoRequest("COD1", "Nome Repetido", "Desc", CategoriaServico.Troca, 30, 100));
        await service.InactivateAsync(res1.Id);

        // Tentar criar novo serviço com mesmo nome e categoria, mas código diferente
        var request2 = new ServicoRequest("COD2", "Nome Repetido", "Desc", CategoriaServico.Troca, 60, 150);

        // Act
        var response2 = await service.CreateAsync(request2);

        // Assert
        Assert.NotNull(response2);
        Assert.Equal("Nome Repetido", response2.Nome);
        Assert.Equal(CategoriaServico.Troca, response2.Categoria);
    }

    [Fact]
    public async Task CreateAsync_DevePermitirMesmoNomeEmCategoriasDiferentes()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        var request1 = new ServicoRequest("COD003", "Troca de Óleo", "Desc", CategoriaServico.Troca, 30, 100);
        var request2 = new ServicoRequest("COD004", "Troca de Óleo", "Desc", CategoriaServico.Verificacao, 15, 50);

        // Criar primeiro serviço
        await service.CreateAsync(request1);

        // Act
        var response2 = await service.CreateAsync(request2);

        // Assert
        Assert.NotNull(response2);
        Assert.Equal("Troca de Óleo", response2.Nome);
        Assert.Equal(CategoriaServico.Verificacao, response2.Categoria);
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
        var request = new ServicoRequest("COD_CAT", "Serviço Categoria", "Desc", categoria, 45, 80);

        // Act
        var response = await service.CreateAsync(request);

        // Assert
        Assert.Equal(categoria, response.Categoria);
    }

    [Fact]
    public async Task GetAllAsync_DeveRetornarListaVaziaQuandoNaoHouverServicos()
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
        await service.CreateAsync(new ServicoRequest("COD005", "Ajuste Corrente", "Desc", CategoriaServico.Ajuste, 10, 20));
        await service.CreateAsync(new ServicoRequest("COD006", "Troca Pneu", "Desc", CategoriaServico.Troca, 45, 200));

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
        await service.CreateAsync(new ServicoRequest("COD007", "Ajuste Corrente", "Desc", CategoriaServico.Ajuste, 10, 20));
        await service.CreateAsync(new ServicoRequest("COD008", "Ajuste Embreagem", "Desc", CategoriaServico.Ajuste, 15, 30));
        await service.CreateAsync(new ServicoRequest("COD009", "Troca Pneu", "Desc", CategoriaServico.Troca, 45, 200));

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
        
        var servicoAtivo1 = await service.CreateAsync(new ServicoRequest("COD010", "Ajuste Corrente", "Desc", CategoriaServico.Ajuste, 10, 20));
        var servicoInativo = await service.CreateAsync(new ServicoRequest("COD011", "Ajuste Embreagem", "Desc", CategoriaServico.Ajuste, 15, 30));
        var servicoAtivo2 = await service.CreateAsync(new ServicoRequest("COD012", "Troca Pneu", "Desc", CategoriaServico.Troca, 45, 200));

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
        var servicoCriado = await service.CreateAsync(new ServicoRequest("COD013", "Ajuste Corrente", "Desc", CategoriaServico.Ajuste, 10, 20));

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
        var servicoCriado = await service.CreateAsync(new ServicoRequest("COD014", "Ajuste Corrente", "Desc", CategoriaServico.Ajuste, 10, 20));
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
        var createRequest = new ServicoRequest("COD015", "Troca de Óleo", "Desc", CategoriaServico.Troca, 30, 100);
        var servicoCriado = await service.CreateAsync(createRequest);

        var updateRequest = new ServicoUpdateRequest("COD015-U", "Troca de Óleo Sintético", "Nova Desc", CategoriaServico.Troca, 45, 120);

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
        var updateRequest = new ServicoUpdateRequest("COD_OFF", "Serviço Inexistente", "Desc", CategoriaServico.Troca, 30, 100);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateAsync(999, updateRequest));
            
        Assert.Contains("não encontrado", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_DevePermitirAlterarParaMesmoNomeEmCategoriaDiferente()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        
        await service.CreateAsync(new ServicoRequest("COD_A", "Serviço A", "Desc A", CategoriaServico.Troca, 30, 100));
        var servico2 = await service.CreateAsync(new ServicoRequest("COD_B", "Serviço B", "Desc B", CategoriaServico.Verificacao, 45, 150));

        // Tenta atualizar o serviço 2 com o nome do serviço 1, mas mantendo categoria diferente
        var updateRequest = new ServicoUpdateRequest("COD_B", "Serviço A", "Desc B", CategoriaServico.Verificacao, 60, 100);

        // Act
        var response = await service.UpdateAsync(servico2.Id, updateRequest);
            
        // Assert
        Assert.Equal("Serviço A", response.Nome);
        Assert.Equal(CategoriaServico.Verificacao, response.Categoria);
    }

    [Fact]
    public async Task UpdateAsync_DeveRetornarErroParaMesmoNomeNaMesmaCategoria()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        
        await service.CreateAsync(new ServicoRequest("COD_A", "Serviço A", "Desc A", CategoriaServico.Troca, 30, 100));
        var servico2 = await service.CreateAsync(new ServicoRequest("COD_B", "Serviço B", "Desc B", CategoriaServico.Troca, 45, 150));

        // Tenta atualizar o serviço 2 com o nome e categoria do serviço 1
        var updateRequest = new ServicoUpdateRequest("COD_B", "Serviço A", "Desc A", CategoriaServico.Troca, 60, 100);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DuplicateDataException>(
            () => service.UpdateAsync(servico2.Id, updateRequest));
            
        Assert.Contains("Já existe outro serviço ativo com o código 'COD_B' ou com o nome 'Serviço A' nesta categoria.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_DeveRetornarErroParaCodigoDuplicadoAtivo()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        
        await service.CreateAsync(new ServicoRequest("COD_EXISTENTE", "Serviço 1", "Desc", CategoriaServico.Troca, 30, 100));
        var servico2 = await service.CreateAsync(new ServicoRequest("COD_B", "Serviço 2", "Desc", CategoriaServico.Limpeza, 45, 150));

        // Tenta atualizar o serviço 2 com o código do serviço 1
        var updateRequest = new ServicoUpdateRequest("COD_EXISTENTE", "Serviço 2", "Desc", CategoriaServico.Limpeza, 45, 150);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DuplicateDataException>(
            () => service.UpdateAsync(servico2.Id, updateRequest));
            
        Assert.Contains("Já existe outro serviço ativo com o código 'COD_EXISTENTE'", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_DevePermitirReutilizarCodigoDeServicoInativo()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        
        var res1 = await service.CreateAsync(new ServicoRequest("COD_INATIVO", "Serviço 1", "Desc", CategoriaServico.Troca, 30, 100));
        await service.InactivateAsync(res1.Id);

        var servico2 = await service.CreateAsync(new ServicoRequest("COD_B", "Serviço 2", "Desc", CategoriaServico.Limpeza, 45, 150));

        var updateRequest = new ServicoUpdateRequest("COD_INATIVO", "Serviço 2", "Desc", CategoriaServico.Limpeza, 45, 150);

        // Act
        var response = await service.UpdateAsync(servico2.Id, updateRequest);

        // Assert
        Assert.Equal("COD_INATIVO", response.Codigo);
    }

    [Fact]
    public async Task UpdateAsync_DevePermitirReutilizarNomeECategoriaDeServicoInativo()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        
        var res1 = await service.CreateAsync(new ServicoRequest("COD1", "Nome Inativo", "Desc", CategoriaServico.Troca, 30, 100));
        await service.InactivateAsync(res1.Id);

        var servico2 = await service.CreateAsync(new ServicoRequest("COD2", "Nome Ativo", "Desc", CategoriaServico.Limpeza, 45, 150));

        var updateRequest = new ServicoUpdateRequest("COD2", "Nome Inativo", "Desc", CategoriaServico.Troca, 45, 150);

        // Act
        var response = await service.UpdateAsync(servico2.Id, updateRequest);

        // Assert
        Assert.Equal("Nome Inativo", response.Nome);
        Assert.Equal(CategoriaServico.Troca, response.Categoria);
    }

    [Fact]
    public async Task InactivateAsync_DeveInativarServicoComSucessoENaoRemoverFisicamente()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ServicoService(context);
        var createRequest = new ServicoRequest("COD016", "Troca de Óleo", "Desc", CategoriaServico.Troca, 30, 100);
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
        var createRequest = new ServicoRequest("COD017", "Troca de Óleo", "Desc", CategoriaServico.Troca, 30, 100);
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