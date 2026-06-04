using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Enums;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Services;

public class RevisaoPadraoServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public RevisaoPadraoServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    private AppDbContext CreateContext() => new AppDbContext(_dbContextOptions);

    [Fact]
    public async Task CadastrarRevisaoAsync_DeveSalvarComSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var concessionaria = new Concessionaria { Id = 1, Nome = "Conc", Cnpj = "123", UsuarioId = "u1" };
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki" };
        var servico = new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao };
        
        context.Concessionarias.Add(concessionaria);
        context.ModelosMotos.Add(modelo);
        context.Servicos.Add(servico);
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoRequest("Revisão 1000km", 1, 1, new List<int> { 1 });

        // Act
        var result = await service.CadastrarRevisaoAsync(request, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Revisão 1000km", result.Nome);
        Assert.Equal("Ninja", result.NomeModeloMoto);
        Assert.Single(result.Servicos);
        Assert.Equal("Oleo", result.Servicos.First().Nome);
    }

    [Fact]
    public async Task CadastrarRevisaoAsync_DeveLancarExcecao_QuandoModeloNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoRequest("Rev", 1, 99, new List<int> { 1 });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.CadastrarRevisaoAsync(request, 1));
    }

    [Fact]
    public async Task CadastrarRevisaoAsync_DeveLancarExcecao_QuandoOrdemDuplicada()
    {
        // Arrange
        using var context = CreateContext();
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki" };
        var revisaoExistente = new RevisaoPadrao { Id = 1, Nome = "Rev Antiga", Ordem = 1, ModeloMotoId = 1, ConcessionariaId = 1 };
        
        context.ModelosMotos.Add(modelo);
        context.RevisoesPadrao.Add(revisaoExistente);
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoRequest("Rev Nova", 1, 1, new List<int> { 1 }); // Ordem 1 repetida para Concessionaria 1 e Modelo 1

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DuplicateDataException>(() => service.CadastrarRevisaoAsync(request, 1));
        Assert.Contains("Já existe uma revisão", ex.Message);
    }

    [Fact]
    public async Task CadastrarRevisaoAsync_DeveLancarExcecao_QuandoServicoNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki" };
        var servico = new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao };
        
        context.ModelosMotos.Add(modelo);
        context.Servicos.Add(servico);
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoRequest("Rev", 1, 1, new List<int> { 1, 99 }); // Serviço 99 não existe

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.CadastrarRevisaoAsync(request, 1));
        Assert.Contains("99", ex.Message);
    }

    [Fact]
    public async Task ListarRevisoesAsync_DeveRetornarListaCorreta()
    {
        // Arrange
        using var context = CreateContext();
        var modelo1 = new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki", Categoria = "Esportiva" };
        var modelo2 = new ModeloMoto { Id = 2, NomeModelo = "Z400", Marca = "Yamaha", Categoria = "Super Esportiva" };
        context.ModelosMotos.AddRange(modelo1, modelo2);

        context.RevisoesPadrao.AddRange(
            new RevisaoPadrao { Id = 1, Nome = "Rev 1 Ninja", ModeloMotoId = 1, ConcessionariaId = 1, Ordem = 1 },
            new RevisaoPadrao { Id = 2, Nome = "Rev 2 Ninja", ModeloMotoId = 1, ConcessionariaId = 1, Ordem = 2 },
            new RevisaoPadrao { Id = 3, Nome = "Rev 1 Z400", ModeloMotoId = 2, ConcessionariaId = 1, Ordem = 1 }
        );
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);

        // Act
        var result = await service.ListarRevisoesAsync(1);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, r => r.Nome == "Rev 1 Ninja" && r.NomeModeloMoto == "Ninja");
    }

    [Fact]
    public async Task ListarRevisoesAsync_DeveRetornarListaVazia_QuandoNaoHaRevisoes()
    {
        // Arrange
        using var context = CreateContext();
        var service = new RevisaoPadraoService(context);

        // Act
        var result = await service.ListarRevisoesAsync(1);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ListarRevisoesAsync_DeveFiltrarPorModeloMotoId_QuandoInformado()
    {
        // Arrange
        using var context = CreateContext();
        var modelo1 = new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki", Categoria = "Esportiva" };
        var modelo2 = new ModeloMoto { Id = 2, NomeModelo = "Z400", Marca = "Yamaha", Categoria = "Super Esportiva" };
        context.ModelosMotos.AddRange(modelo1, modelo2);

        context.RevisoesPadrao.AddRange(
            new RevisaoPadrao { Id = 1, Nome = "Rev 1 Ninja", ModeloMotoId = 1, ConcessionariaId = 1, Ordem = 1 },
            new RevisaoPadrao { Id = 2, Nome = "Rev 2 Ninja", ModeloMotoId = 1, ConcessionariaId = 1, Ordem = 2 },
            new RevisaoPadrao { Id = 3, Nome = "Rev 1 Z400", ModeloMotoId = 2, ConcessionariaId = 1, Ordem = 1 }
        );
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);

        // Act
        var result = await service.ListarRevisoesAsync(1, 2); // Filtra pelo modelo Z400

        // Assert
        Assert.Single(result);
        Assert.Equal("Rev 1 Z400", result.First().Nome);
        Assert.Equal("Z400", result.First().NomeModeloMoto);
    }
    
    [Fact]
    public async Task GetByIdAsync_DeveRetornarRevisaoCompleta_QuandoSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki" };
        var servico = new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao };
        context.ModelosMotos.Add(modelo);
        context.Servicos.Add(servico);
        
        var revisao = new RevisaoPadrao { Id = 1, Nome = "Rev", ModeloMotoId = 1, ConcessionariaId = 1, Ordem = 1 };
        context.RevisoesPadrao.Add(revisao);
        context.RevisaoPadraoServicos.Add(new RevisaoPadraoServico { RevisaoPadraoId = 1, ServicoId = 1 });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);

        // Act
        var result = await service.GetByIdAsync(1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Rev", result.Nome);
        Assert.Equal("Ninja", result.NomeModeloMoto);
        Assert.Single(result.Servicos);
        Assert.Equal("Oleo", result.Servicos.First().Nome);
    }

    [Fact]
    public async Task GetByIdAsync_DeveLancarExcecao_QuandoNaoEncontradaOuDeOutraConcessionaria()
    {
        // Arrange
        using var context = CreateContext();
        var revisao = new RevisaoPadrao { Id = 1, Nome = "Rev", ModeloMotoId = 1, ConcessionariaId = 2, Ordem = 1 }; // Pertence à concessionaria 2
        context.RevisoesPadrao.Add(revisao);
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);

        // Act & Assert
        // Tenta buscar com ID 1, mas simulando estar logado como concessionária 1
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(1, 1));
        Assert.Contains("não encontrada", exception.Message);
    }
}
