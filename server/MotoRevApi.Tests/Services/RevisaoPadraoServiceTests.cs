using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Enums;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using MotoRevApi.Profiles;
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
        
        MapsterConfig.RegisterMapsterConfiguration();
    }

    private AppDbContext CreateContext() => new AppDbContext(_dbContextOptions);

    [Fact]
    public async Task ListarRevisoesAsync_DeveRetornarListaCorreta()
    {
        // Arrange
        using var context = CreateContext();
        var linha1 = new Linha { Id = 1, Nome = "Street" };
        var linha2 = new Linha { Id = 2, Nome = "Adventure" };
        context.Linhas.AddRange(linha1, linha2);

        context.RevisoesPadrao.AddRange(
            new RevisaoPadrao { Id = 1, Nome = "Rev 1 Street", LinhaId = 1, Ordem = 1, Quilometragem = 1000, TempoMeses = 6 },
            new RevisaoPadrao { Id = 2, Nome = "Rev 2 Street", LinhaId = 1, Ordem = 2, Quilometragem = 6000, TempoMeses = 12 },
            new RevisaoPadrao { Id = 3, Nome = "Rev 1 Adventure", LinhaId = 2, Ordem = 1, Quilometragem = 1000, TempoMeses = 6 }
        );
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);

        // Act
        var result = await service.ListarRevisoesAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, r => r.Nome == "Rev 1 Street" && r.NomeLinha == "Street");
        Assert.Contains(result, r => r.NomeLinha == "Adventure" && r.Quilometragem == 1000 && r.TempoMeses == 6);
    }

    [Fact]
    public async Task ListarRevisoesAsync_DeveRetornarListaVazia_QuandoNaoHaRevisoes()
    {
        // Arrange
        using var context = CreateContext();
        var service = new RevisaoPadraoService(context);

        // Act
        var result = await service.ListarRevisoesAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ListarRevisoesAsync_DeveFiltrarPorModeloMotoId_QuandoInformado()
    {
        // Arrange
        using var context = CreateContext();
        var linha = new Linha { Id = 1, Nome = "Street" };
        var modelo1 = new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki", LinhaId = 1 };
        context.Linhas.Add(linha);
        context.ModelosMotos.Add(modelo1);

        context.RevisoesPadrao.AddRange(
            new RevisaoPadrao { Id = 1, Nome = "Rev 1 Ninja", LinhaId = 1, Ordem = 1, Quilometragem = 1000, TempoMeses = 6 },
            new RevisaoPadrao { Id = 2, Nome = "Rev 2 Ninja", LinhaId = 1, Ordem = 2, Quilometragem = 6000, TempoMeses = 12 }
        );
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);

        // Act
        var result = await service.ListarRevisoesAsync(1); // Filtra pelo modelo Ninja

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Rev 1 Ninja", result.First().Nome);
    }

    [Fact]
    public async Task ListarRevisoesAsync_DeveFiltrarPorLinhaId_QuandoInformado()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.AddRange(
            new Linha { Id = 1, Nome = "Street" },
            new Linha { Id = 2, Nome = "Adventure" }
        );
        context.ModelosMotos.AddRange(
            new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki", LinhaId = 1 },
            new ModeloMoto { Id = 2, NomeModelo = "GS", Marca = "BMW", LinhaId = 2 }
        );
        context.RevisoesPadrao.AddRange(
            new RevisaoPadrao { Id = 1, Nome = "Rev Street", LinhaId = 1, Ordem = 1, Quilometragem = 1000, TempoMeses = 6 },
            new RevisaoPadrao { Id = 2, Nome = "Rev Adventure", LinhaId = 2, Ordem = 1, Quilometragem = 1000, TempoMeses = 6 }
        );
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);

        // Act
        var result = await service.ListarRevisoesAsync(linhaId: 2);

        // Assert
        Assert.Single(result);
        Assert.Equal("Rev Adventure", result.First().Nome);
        Assert.Equal("Adventure", result.First().NomeLinha);
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveCriarParaALinha()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        context.Servicos.Add(new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao, Ativo = true });
        context.Pecas.Add(new Peca { Id = 1, Codigo = "P1", Nome = "Filtro", Categoria = CategoriaPeca.Motor, Preco = 50, Estoque = 10, Status = StatusCadastro.Ativo });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoLinhaRequest(
            "Plano Street",
            1,
            new List<RevisaoPadraoLinhaItemRequest>
            {
                new("Primeira revisão", 1, 1000, 6, new List<int> { 1 }, new List<RevisaoPadraoPecaRequest> { new(1, 1) }),
                new("Segunda revisão", 2, 6000, 12, new List<int> { 1 })
            });

        // Act
        var result = await service.CadastrarRevisoesPorLinhaAsync(request);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(2, await context.RevisoesPadrao.CountAsync());
        Assert.Equal(1, await context.RevisaoPadraoPecas.CountAsync());
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveLancarExcecao_QuandoLinhaInativaOuNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street", Ativo = false });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoLinhaRequest(
            "Plano Street",
            1,
            new List<RevisaoPadraoLinhaItemRequest>
            {
                new("Primeira revisão", 1, 1000, 6, new List<int> { 1 })
            });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.CadastrarRevisoesPorLinhaAsync(request));
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveLancarExcecao_QuandoRequestTemOrdemDuplicada()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoLinhaRequest(
            "Plano Street",
            1,
            new List<RevisaoPadraoLinhaItemRequest>
            {
                new("Primeira revisão", 1, 1000, 6, new List<int> { 1 }),
                new("Outra revisão", 1, 6000, 12, new List<int> { 1 })
            });

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.CadastrarRevisoesPorLinhaAsync(request));
    }

    [Fact]
    public async Task AlternarStatusPorLinhaAsync_DeveAlterarStatusDeTodasAsRevisoesDaLinha()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        context.RevisoesPadrao.AddRange(
            new RevisaoPadrao { Id = 1, Nome = "Rev 1", LinhaId = 1, Ordem = 1, Quilometragem = 1000, TempoMeses = 6, Ativo = true },
            new RevisaoPadrao { Id = 2, Nome = "Rev 2", LinhaId = 1, Ordem = 2, Quilometragem = 6000, TempoMeses = 12, Ativo = true }
        );
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);

        // Act
        var result = await service.AlternarStatusPorLinhaAsync(1);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.False(r.Ativo));
    }

    [Fact]
    public async Task GetByIdAsync_DeveRetornarRevisaoCompleta_QuandoSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var linha = new Linha { Id = 1, Nome = "Street" };
        var servico = new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao, Ativo = true };
        var peca = new Peca { Id = 1, Codigo = "P1", Nome = "Filtro", Categoria = CategoriaPeca.Motor, Preco = 50, Estoque = 10, Status = StatusCadastro.Ativo };
        context.Linhas.Add(linha);
        context.Servicos.Add(servico);
        context.Pecas.Add(peca);
        
        var revisao = new RevisaoPadrao { Id = 1, Nome = "Rev", LinhaId = 1, Ordem = 1, Quilometragem = 1000, TempoMeses = 6 };
        context.RevisoesPadrao.Add(revisao);
        context.RevisaoPadraoServicos.Add(new RevisaoPadraoServico { RevisaoPadraoId = 1, ServicoId = 1 });
        context.RevisaoPadraoPecas.Add(new RevisaoPadraoPeca { RevisaoPadraoId = 1, PecaId = 1, Quantidade = 2 });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Rev", result.Nome);
        Assert.Equal("Street", result.NomeLinha);
        Assert.Single(result.Servicos);
        Assert.Equal("Oleo", result.Servicos.First().Nome);
        Assert.Single(result.Pecas);
        Assert.Equal("Filtro", result.Pecas.First().Nome);
        Assert.Equal(2, result.Pecas.First().Quantidade);
    }

    [Fact]
    public async Task AtualizarRevisoesPorLinhaAsync_DeveAtualizarComSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var linha = new Linha { Id = 1, Nome = "Street" };
        var servico = new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao, Ativo = true };
        var peca = new Peca { Id = 1, Codigo = "P1", Nome = "Filtro", Preco = 50.0m, Estoque = 10, Status = StatusCadastro.Ativo };
        var revisaoAntiga = new RevisaoPadrao { Id = 1, Nome = "Rev Antiga", Ordem = 1, LinhaId = 1 };

        context.Linhas.Add(linha);
        context.Servicos.Add(servico);
        context.Pecas.Add(peca);
        context.RevisoesPadrao.Add(revisaoAntiga);
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var itemRequest = new RevisaoPadraoLinhaItemRequest("Revisão Nova", 1, 1500, 8, new List<int> { 1 }, new List<RevisaoPadraoPecaRequest> { new RevisaoPadraoPecaRequest(1, 2) });
        var request = new RevisaoPadraoLinhaRequest("Plano Novo", 1, new List<RevisaoPadraoLinhaItemRequest> { itemRequest });

        // Act
        var result = await service.AtualizarRevisoesPorLinhaAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var updatedRev = result.First();
        Assert.Equal("Revisão Nova", updatedRev.Nome);
        Assert.Equal(1500, updatedRev.Quilometragem);
        Assert.Single(updatedRev.Servicos);
        Assert.Equal("Oleo", updatedRev.Servicos.First().Nome);
        Assert.Single(updatedRev.Pecas);
        Assert.Equal(2, updatedRev.Pecas.First().Quantidade);
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveLancarExcecao_QuandoOrdemJaExisteNoBanco()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        context.RevisoesPadrao.Add(new RevisaoPadrao { Id = 1, Nome = "Existente", Ordem = 1, LinhaId = 1 });
        context.Servicos.Add(new Servico { Id = 1, Codigo = "S1", Nome = "S", Descricao = "D", Categoria = CategoriaServico.Verificacao, Ativo = true });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoLinhaRequest("Plano", 1, new List<RevisaoPadraoLinhaItemRequest> 
        { 
            new("Nova", 1, 1000, 6, new List<int> { 1 }) 
        });

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.CadastrarRevisoesPorLinhaAsync(request));
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveLancarExcecao_QuandoServicoNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoLinhaRequest("Plano", 1, new List<RevisaoPadraoLinhaItemRequest> 
        { 
            new("Nova", 1, 1000, 6, new List<int> { 999 }) 
        });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.CadastrarRevisoesPorLinhaAsync(request));
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveLancarExcecao_QuandoServicoInativo()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        context.Servicos.Add(new Servico { Id = 1, Codigo = "S1", Nome = "S", Descricao = "D", Categoria = CategoriaServico.Verificacao, Ativo = false });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoLinhaRequest("Plano", 1, new List<RevisaoPadraoLinhaItemRequest> 
        { 
            new("Nova", 1, 1000, 6, new List<int> { 1 }) 
        });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.CadastrarRevisoesPorLinhaAsync(request));
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveLancarExcecao_QuandoNaoInformaServico()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoLinhaRequest("Plano", 1, new List<RevisaoPadraoLinhaItemRequest> 
        { 
            new("Nova", 1, 1000, 6, new List<int>()) 
        });

        // Act & Assert
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() => service.CadastrarRevisoesPorLinhaAsync(request));
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveLancarExcecao_QuandoPecaNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        context.Servicos.Add(new Servico { Id = 1, Codigo = "S1", Nome = "S", Descricao = "D", Categoria = CategoriaServico.Verificacao, Ativo = true });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoLinhaRequest("Plano", 1, new List<RevisaoPadraoLinhaItemRequest> 
        { 
            new("Nova", 1, 1000, 6, new List<int> { 1 }, new List<RevisaoPadraoPecaRequest> { new(999, 1) }) 
        });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.CadastrarRevisoesPorLinhaAsync(request));
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveLancarExcecao_QuandoPecaInativa()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        context.Servicos.Add(new Servico { Id = 1, Codigo = "S1", Nome = "S", Descricao = "D", Categoria = CategoriaServico.Verificacao, Ativo = true });
        context.Pecas.Add(new Peca { Id = 1, Codigo = "P1", Nome = "P", Status = StatusCadastro.Inativo });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoLinhaRequest("Plano", 1, new List<RevisaoPadraoLinhaItemRequest> 
        { 
            new("Nova", 1, 1000, 6, new List<int> { 1 }, new List<RevisaoPadraoPecaRequest> { new(1, 1) }) 
        });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.CadastrarRevisoesPorLinhaAsync(request));
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveLancarExcecao_QuandoPecaComQuantidadeInvalida()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        context.Servicos.Add(new Servico { Id = 1, Codigo = "S1", Nome = "S", Descricao = "D", Categoria = CategoriaServico.Verificacao, Ativo = true });
        context.Pecas.Add(new Peca { Id = 1, Codigo = "P1", Nome = "P", Status = StatusCadastro.Ativo });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoLinhaRequest("Plano", 1, new List<RevisaoPadraoLinhaItemRequest> 
        { 
            new("Nova", 1, 1000, 6, new List<int> { 1 }, new List<RevisaoPadraoPecaRequest> { new(1, 0) }) 
        });

        // Act & Assert
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() => service.CadastrarRevisoesPorLinhaAsync(request));
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveLancarExcecao_QuandoHaServicosDuplicados()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        context.Servicos.Add(new Servico { Id = 1, Codigo = "S1", Nome = "S", Descricao = "D", Categoria = CategoriaServico.Verificacao, Ativo = true });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoLinhaRequest("Plano", 1, new List<RevisaoPadraoLinhaItemRequest> 
        { 
            new("Nova", 1, 1000, 6, new List<int> { 1, 1 }) 
        });

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.CadastrarRevisoesPorLinhaAsync(request));
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveLancarExcecao_QuandoHaPecasDuplicadas()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        context.Servicos.Add(new Servico { Id = 1, Codigo = "S1", Nome = "S", Descricao = "D", Categoria = CategoriaServico.Verificacao, Ativo = true });
        context.Pecas.Add(new Peca { Id = 1, Codigo = "P1", Nome = "P", Status = StatusCadastro.Ativo });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoLinhaRequest("Plano", 1, new List<RevisaoPadraoLinhaItemRequest> 
        { 
            new("Nova", 1, 1000, 6, new List<int> { 1 }, new List<RevisaoPadraoPecaRequest> { new(1, 1), new(1, 1) }) 
        });

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.CadastrarRevisoesPorLinhaAsync(request));
    }
}
