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
        var concessionaria = new Concessionaria { Id = 1, Nome = "Conc", Cnpj = "123", UsuarioId = "u1", Usuario = new Usuario { Id = "u1", UserName = "user1" }, Telefone = "123" };
        var linha = new Linha { Id = 1, Nome = "Street" };
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki", LinhaId = 1 };
        var servico = new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao };
        
        context.Concessionarias.Add(concessionaria);
        context.Linhas.Add(linha);
        context.ModelosMotos.Add(modelo);
        context.Servicos.Add(servico);
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoRequest("Revisão 1000km", 1, 1000, 6, 1, new List<int> { 1 });

        // Act
        var result = await service.CadastrarRevisaoAsync(request, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Revisão 1000km", result.Nome);
        Assert.Equal("Street", result.NomeLinha);
        Assert.Equal(1000, result.Quilometragem);
        Assert.Equal(6, result.TempoMeses);
        Assert.Single(result.Servicos);
        Assert.Equal("Oleo", result.Servicos.First().Nome);
    }

    [Fact]
    public async Task CadastrarRevisaoAsync_DeveLancarExcecao_QuandoModeloNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoRequest("Rev", 1, 1000, 6, 99, new List<int> { 1 });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.CadastrarRevisaoAsync(request, 1));
    }

    [Fact]
    public async Task CadastrarRevisaoAsync_DeveLancarExcecao_QuandoOrdemDuplicada()
    {
        // Arrange
        using var context = CreateContext();
        var linha = new Linha { Id = 1, Nome = "Street" };
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki", LinhaId = 1 };
        var revisaoExistente = new RevisaoPadrao { Id = 1, Nome = "Rev Antiga", Ordem = 1, LinhaId = 1, ConcessionariaId = 1 };
        
        context.Linhas.Add(linha);
        context.ModelosMotos.Add(modelo);
        context.RevisoesPadrao.Add(revisaoExistente);
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoRequest("Rev Nova", 1, 1000, 6, 1, new List<int> { 1 }); // Ordem 1 repetida para Concessionaria 1 e Modelo 1

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
        var request = new RevisaoPadraoRequest("Rev", 1, 1000, 6, 1, new List<int> { 1, 99 }); // Serviço 99 não existe

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.CadastrarRevisaoAsync(request, 1));
        Assert.Contains("99", ex.Message);
    }

    [Fact]
    public async Task CadastrarRevisaoAsync_DeveLancarExcecao_QuandoServicoInativo()
    {
        // Arrange
        using var context = CreateContext();
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki" };
        var servico = new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao, Ativo = false };

        context.ModelosMotos.Add(modelo);
        context.Servicos.Add(servico);
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoRequest("Rev", 1, 1000, 6, 1, new List<int> { 1 });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.CadastrarRevisaoAsync(request, 1));
        Assert.Contains("inativos", ex.Message);
    }

    [Fact]
    public async Task CadastrarRevisaoAsync_DeveSalvarPecasComQuantidade()
    {
        // Arrange
        using var context = CreateContext();
        var concessionaria = new Concessionaria { Id = 1, Nome = "Conc", Cnpj = "123", UsuarioId = "u1", Usuario = new Usuario { Id = "u1", UserName = "user1" }, Telefone = "123" };
        var linha = new Linha { Id = 1, Nome = "Street" };
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki", LinhaId = 1 };
        var servico = new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao };
        var peca = new Peca { Id = 1, Codigo = "P1", Nome = "Filtro", Categoria = CategoriaPeca.Motor, Preco = 50, Estoque = 10, Status = StatusCadastro.Ativo };

        context.Concessionarias.Add(concessionaria);
        context.Linhas.Add(linha);
        context.ModelosMotos.Add(modelo);
        context.Servicos.Add(servico);
        context.Pecas.Add(peca);
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoRequest(
            "Revisão 1000km",
            1,
            1000,
            6,
            1,
            new List<int> { 1 },
            new List<RevisaoPadraoPecaRequest> { new(1, 2) });

        // Act
        var result = await service.CadastrarRevisaoAsync(request, 1);

        // Assert
        Assert.Single(result.Pecas);
        Assert.Equal("Filtro", result.Pecas.First().Nome);
        Assert.Equal(2, result.Pecas.First().Quantidade);
    }

    [Fact]
    public async Task CadastrarRevisaoAsync_DeveLancarExcecao_QuandoPecaInativa()
    {
        // Arrange
        using var context = CreateContext();
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki" };
        var servico = new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao };
        var peca = new Peca { Id = 1, Codigo = "P1", Nome = "Filtro", Categoria = CategoriaPeca.Motor, Preco = 50, Estoque = 10, Status = StatusCadastro.Inativo };

        context.ModelosMotos.Add(modelo);
        context.Servicos.Add(servico);
        context.Pecas.Add(peca);
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoRequest(
            "Rev",
            1,
            1000,
            6,
            1,
            new List<int> { 1 },
            new List<RevisaoPadraoPecaRequest> { new(1, 1) });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.CadastrarRevisaoAsync(request, 1));
        Assert.Contains("peças", ex.Message);
    }

    [Fact]
    public async Task CadastrarRevisaoAsync_DeveLancarExcecao_QuandoQuantidadePecaForInvalida()
    {
        // Arrange
        using var context = CreateContext();
        var modelo = new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki" };
        var servico = new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao };

        context.ModelosMotos.Add(modelo);
        context.Servicos.Add(servico);
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoRequest(
            "Rev",
            1,
            1000,
            6,
            1,
            new List<int> { 1 },
            new List<RevisaoPadraoPecaRequest> { new(1, 0) });

        // Act & Assert
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() => service.CadastrarRevisaoAsync(request, 1));
    }

    [Fact]
    public async Task ListarRevisoesAsync_DeveRetornarListaCorreta()
    {
        // Arrange
        using var context = CreateContext();
        var linha1 = new Linha { Id = 1, Nome = "Street" };
        var linha2 = new Linha { Id = 2, Nome = "Adventure" };
        context.Linhas.AddRange(linha1, linha2);

        context.RevisoesPadrao.AddRange(
            new RevisaoPadrao { Id = 1, Nome = "Rev 1 Street", LinhaId = 1, ConcessionariaId = 1, Ordem = 1, Quilometragem = 1000, TempoMeses = 6 },
            new RevisaoPadrao { Id = 2, Nome = "Rev 2 Street", LinhaId = 1, ConcessionariaId = 1, Ordem = 2, Quilometragem = 6000, TempoMeses = 12 },
            new RevisaoPadrao { Id = 3, Nome = "Rev 1 Adventure", LinhaId = 2, ConcessionariaId = 1, Ordem = 1, Quilometragem = 1000, TempoMeses = 6 }
        );
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);

        // Act
        var result = await service.ListarRevisoesAsync(1);

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
        var result = await service.ListarRevisoesAsync(1);

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
            new RevisaoPadrao { Id = 1, Nome = "Rev 1 Ninja", LinhaId = 1, ConcessionariaId = 1, Ordem = 1, Quilometragem = 1000, TempoMeses = 6 },
            new RevisaoPadrao { Id = 2, Nome = "Rev 2 Ninja", LinhaId = 1, ConcessionariaId = 1, Ordem = 2, Quilometragem = 6000, TempoMeses = 12 }
        );
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);

        // Act
        var result = await service.ListarRevisoesAsync(1, 1); // Filtra pelo modelo Ninja

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
            new RevisaoPadrao { Id = 1, Nome = "Rev Street", LinhaId = 1, ConcessionariaId = 1, Ordem = 1, Quilometragem = 1000, TempoMeses = 6 },
            new RevisaoPadrao { Id = 2, Nome = "Rev Adventure", LinhaId = 2, ConcessionariaId = 1, Ordem = 1, Quilometragem = 1000, TempoMeses = 6 }
        );
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);

        // Act
        var result = await service.ListarRevisoesAsync(1, linhaId: 2);

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
        context.Servicos.Add(new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao });
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
        var result = await service.CadastrarRevisoesPorLinhaAsync(request, 1);

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
        await Assert.ThrowsAsync<NotFoundException>(() => service.CadastrarRevisoesPorLinhaAsync(request, 1));
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveSalvarComSucesso_QuandoNaoHaModelos()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        context.Servicos.Add(new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoLinhaRequest(
            "Plano Street",
            1,
            new List<RevisaoPadraoLinhaItemRequest>
            {
                new("Primeira revisão", 1, 1000, 6, new List<int> { 1 })
            });

        // Act
        var result = await service.CadastrarRevisoesPorLinhaAsync(request, 1);

        // Assert
        Assert.Single(result);
        Assert.Equal("Primeira revisão", result.First().Nome);
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveLancarExcecao_QuandoRequestTemOrdemDuplicada()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        context.ModelosMotos.Add(new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki", LinhaId = 1 });
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
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.CadastrarRevisoesPorLinhaAsync(request, 1));
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveLancarExcecao_QuandoServicoInativo()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        context.ModelosMotos.Add(new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki", LinhaId = 1 });
        context.Servicos.Add(new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao, Ativo = false });
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
        await Assert.ThrowsAsync<NotFoundException>(() => service.CadastrarRevisoesPorLinhaAsync(request, 1));
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveLancarExcecao_QuandoPecaInativa()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        context.ModelosMotos.Add(new ModeloMoto { Id = 1, NomeModelo = "Ninja", Marca = "Kawasaki", LinhaId = 1 });
        context.Servicos.Add(new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao });
        context.Pecas.Add(new Peca { Id = 1, Codigo = "P1", Nome = "Filtro", Categoria = CategoriaPeca.Motor, Preco = 50, Estoque = 10, Status = StatusCadastro.Inativo });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoLinhaRequest(
            "Plano Street",
            1,
            new List<RevisaoPadraoLinhaItemRequest>
            {
                new("Primeira revisão", 1, 1000, 6, new List<int> { 1 }, new List<RevisaoPadraoPecaRequest> { new(1, 1) })
            });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.CadastrarRevisoesPorLinhaAsync(request, 1));
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveLancarExcecao_QuandoQuantidadePecaForInvalida()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        context.Servicos.Add(new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoLinhaRequest(
            "Plano Street",
            1,
            new List<RevisaoPadraoLinhaItemRequest>
            {
                new("Primeira revisão", 1, 1000, 6, new List<int> { 1 }, new List<RevisaoPadraoPecaRequest> { new(1, 0) })
            });

        // Act & Assert
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() => service.CadastrarRevisoesPorLinhaAsync(request, 1));
    }

    [Fact]
    public async Task CadastrarRevisoesPorLinhaAsync_DeveLancarExcecao_QuandoJaExisteRevisaoParaLinhaComMesmaOrdem()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        context.Servicos.Add(new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao });
        context.RevisoesPadrao.Add(new RevisaoPadrao { Id = 1, Nome = "Existente", LinhaId = 1, ConcessionariaId = 1, Ordem = 1, Quilometragem = 1000, TempoMeses = 6 });
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
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.CadastrarRevisoesPorLinhaAsync(request, 1));
    }

    [Fact]
    public async Task AlternarStatusPorLinhaAsync_DeveAlterarApenasRevisoesDaConcessionariaAutenticada()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Id = 1, Nome = "Street" });
        context.RevisoesPadrao.AddRange(
            new RevisaoPadrao { Id = 1, Nome = "Rev 1", LinhaId = 1, ConcessionariaId = 1, Ordem = 1, Quilometragem = 1000, TempoMeses = 6, Ativo = true },
            new RevisaoPadrao { Id = 2, Nome = "Rev 2", LinhaId = 1, ConcessionariaId = 1, Ordem = 2, Quilometragem = 6000, TempoMeses = 12, Ativo = true },
            new RevisaoPadrao { Id = 3, Nome = "Outra Conc", LinhaId = 1, ConcessionariaId = 2, Ordem = 1, Quilometragem = 1000, TempoMeses = 6, Ativo = true }
        );
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);

        // Act
        var result = await service.AlternarStatusPorLinhaAsync(1, 1);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.False(r.Ativo));
        Assert.True((await context.RevisoesPadrao.FindAsync(3))!.Ativo);
    }
    
    [Fact]
    public async Task GetByIdAsync_DeveRetornarRevisaoCompleta_QuandoSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var linha = new Linha { Id = 1, Nome = "Street" };
        var servico = new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao };
        var peca = new Peca { Id = 1, Codigo = "P1", Nome = "Filtro", Categoria = CategoriaPeca.Motor, Preco = 50, Estoque = 10, Status = StatusCadastro.Ativo };
        context.Linhas.Add(linha);
        context.Servicos.Add(servico);
        context.Pecas.Add(peca);
        
        var revisao = new RevisaoPadrao { Id = 1, Nome = "Rev", LinhaId = 1, ConcessionariaId = 1, Ordem = 1, Quilometragem = 1000, TempoMeses = 6 };
        context.RevisoesPadrao.Add(revisao);
        context.RevisaoPadraoServicos.Add(new RevisaoPadraoServico { RevisaoPadraoId = 1, ServicoId = 1 });
        context.RevisaoPadraoPecas.Add(new RevisaoPadraoPeca { RevisaoPadraoId = 1, PecaId = 1, Quantidade = 2 });
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);

        // Act
        var result = await service.GetByIdAsync(1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Rev", result.Nome);
        Assert.Equal("Street", result.NomeLinha);
        Assert.Equal(1000, result.Quilometragem);
        Assert.Equal(6, result.TempoMeses);
        Assert.Single(result.Servicos);
        Assert.Equal("Oleo", result.Servicos.First().Nome);
        Assert.Single(result.Pecas);
        Assert.Equal("Filtro", result.Pecas.First().Nome);
        Assert.Equal(2, result.Pecas.First().Quantidade);
    }

    [Fact]
    public async Task GetByIdAsync_DeveLancarExcecao_QuandoNaoEncontradaOuDeOutraConcessionaria()
    {
        // Arrange
        using var context = CreateContext();
        var linha = new Linha { Id = 1, Nome = "Street" };
        context.Linhas.Add(linha);
        var revisao = new RevisaoPadrao { Id = 1, Nome = "Rev", LinhaId = 1, ConcessionariaId = 2, Ordem = 1 }; // Pertence à concessionaria 2
        context.RevisoesPadrao.Add(revisao);
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);

        // Act & Assert
        // Tenta buscar com ID 1, mas simulando estar logado como concessionária 1
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(1, 1));
        Assert.Contains("não encontrada", exception.Message);
    }

    [Fact]
    public async Task AtualizarRevisaoAsync_DeveAtualizarComSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var concessionaria = new Concessionaria { Id = 1, Nome = "Conc", Cnpj = "123", UsuarioId = "u1", Usuario = new Usuario { Id = "u1", UserName = "user1" }, Telefone = "123" };
        var linha = new Linha { Id = 1, Nome = "Street" };
        var servico = new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao };
        var peca = new Peca { Id = 1, Codigo = "P1", Nome = "Filtro", Preco = 50.0m, Estoque = 10, Status = StatusCadastro.Ativo };
        var revisao = new RevisaoPadrao { Id = 1, Nome = "Rev Antiga", Ordem = 1, LinhaId = 1, ConcessionariaId = 1 };

        context.Concessionarias.Add(concessionaria);
        context.Linhas.Add(linha);
        context.Servicos.Add(servico);
        context.Pecas.Add(peca);
        context.RevisoesPadrao.Add(revisao);
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var request = new RevisaoPadraoUpdateRequest("Revisão Atualizada", 2, 2000, 12, new List<int> { 1 }, new List<RevisaoPadraoPecaRequest> { new RevisaoPadraoPecaRequest(1, 3) });

        // Act
        var result = await service.AtualizarRevisaoAsync(1, request, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Revisão Atualizada", result.Nome);
        Assert.Equal(2, result.Ordem);
        Assert.Equal(2000, result.Quilometragem);
        Assert.Equal(12, result.TempoMeses);
        Assert.Single(result.Servicos);
        Assert.Equal("Oleo", result.Servicos.First().Nome);
        Assert.Single(result.Pecas);
        Assert.Equal("Filtro", result.Pecas.First().Nome);
        Assert.Equal(3, result.Pecas.First().Quantidade);
    }

    [Fact]
    public async Task AtualizarRevisoesPorLinhaAsync_DeveAtualizarComSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var concessionaria = new Concessionaria { Id = 1, Nome = "Conc", Cnpj = "123", UsuarioId = "u1", Usuario = new Usuario { Id = "u1", UserName = "user1" }, Telefone = "123" };
        var linha = new Linha { Id = 1, Nome = "Street" };
        var servico = new Servico { Id = 1, Codigo = "S1", Nome = "Oleo", Descricao = "D", Categoria = CategoriaServico.Verificacao };
        var peca = new Peca { Id = 1, Codigo = "P1", Nome = "Filtro", Preco = 50.0m, Estoque = 10, Status = StatusCadastro.Ativo };
        var revisaoAntiga = new RevisaoPadrao { Id = 1, Nome = "Rev Antiga", Ordem = 1, LinhaId = 1, ConcessionariaId = 1 };

        context.Concessionarias.Add(concessionaria);
        context.Linhas.Add(linha);
        context.Servicos.Add(servico);
        context.Pecas.Add(peca);
        context.RevisoesPadrao.Add(revisaoAntiga);
        await context.SaveChangesAsync();

        var service = new RevisaoPadraoService(context);
        var itemRequest = new RevisaoPadraoLinhaItemRequest("Revisão Nova", 1, 1500, 8, new List<int> { 1 }, new List<RevisaoPadraoPecaRequest> { new RevisaoPadraoPecaRequest(1, 2) });
        var request = new RevisaoPadraoLinhaRequest("Plano Novo", 1, new List<RevisaoPadraoLinhaItemRequest> { itemRequest });

        // Act
        var result = await service.AtualizarRevisoesPorLinhaAsync(1, request, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var updatedRev = result.First();
        Assert.Equal("Revisão Nova", updatedRev.Nome);
        Assert.Equal(1, updatedRev.Ordem);
        Assert.Equal(1500, updatedRev.Quilometragem);
        Assert.Equal(8, updatedRev.TempoMeses);
        Assert.Single(updatedRev.Servicos);
        Assert.Equal("Oleo", updatedRev.Servicos.First().Nome);
        Assert.Single(updatedRev.Pecas);
        Assert.Equal("Filtro", updatedRev.Pecas.First().Nome);
        Assert.Equal(2, updatedRev.Pecas.First().Quantidade);
    }
}

