using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Enums;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Services;

public class PecaServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public PecaServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    private AppDbContext CreateContext() => new AppDbContext(_dbContextOptions);

    [Fact]
    public void CadastrarPeca_DeveCriarPecaValidaComStatusAtivo()
    {
        using var context = CreateContext();
        var service = new PecaService(context);
        var request = new PecaRequest("p001", "Filtro de oleo", CategoriaPeca.Filtros, 10.99m, 25);

        var result = service.CadastrarPeca(request);

        Assert.NotNull(result);
        Assert.Equal("P001", result.Codigo);
        Assert.Equal("Filtro de oleo", result.Nome);
        Assert.Equal(nameof(CategoriaPeca.Filtros), result.Categoria);
        Assert.Equal(10.99m, result.Preco);
        Assert.Equal(25, result.Estoque);
        Assert.Equal(nameof(StatusCadastro.Ativo), result.Status);

        var pecaNoDb = context.Pecas.SingleOrDefault();
        Assert.NotNull(pecaNoDb);
        Assert.Equal("P001", pecaNoDb.Codigo);
        Assert.Equal("Filtro de oleo", pecaNoDb.Nome);
        Assert.Equal(CategoriaPeca.Filtros, pecaNoDb.Categoria);
        Assert.Equal(10.99m, pecaNoDb.Preco);
        Assert.Equal(25, pecaNoDb.Estoque);
        Assert.Equal(StatusCadastro.Ativo, pecaNoDb.Status);
    }

    [Theory]
    [InlineData("P001", "P001")]
    [InlineData("P001", "  P001  ")]
    [InlineData("P001", "p001")]
    public void CadastrarPeca_DeveLancarDuplicateDataException_QuandoCodigoJaExiste(
        string codigoExistente,
        string novoCodigo)
    {
        using var context = CreateContext();
        context.Pecas.Add(new Peca
        {
            Codigo = codigoExistente,
            Nome = "Filtro de oleo",
            Categoria = CategoriaPeca.Filtros,
            Preco = 10.99m,
            Estoque = 25,
            Status = StatusCadastro.Ativo
        });
        context.SaveChanges();

        var service = new PecaService(context);
        var request = new PecaRequest(novoCodigo, "Vela de ignicao", CategoriaPeca.Eletrica, 20.99m, 10);

        Assert.Throws<DuplicateDataException>(() => service.CadastrarPeca(request));
        Assert.Single(context.Pecas);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void CadastrarPeca_DeveLancarValidationException_QuandoCodigoInvalido(string codigo)
    {
        using var context = CreateContext();
        var service = new PecaService(context);
        var request = new PecaRequest(codigo, "Filtro de oleo", CategoriaPeca.Filtros, 10.99m, 25);

        Assert.Throws<ValidationException>(() => service.CadastrarPeca(request));
        Assert.Empty(context.Pecas);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("AB")]
    public void CadastrarPeca_DeveLancarValidationException_QuandoNomeInvalido(string nome)
    {
        using var context = CreateContext();
        var service = new PecaService(context);
        var request = new PecaRequest("P001", nome, CategoriaPeca.Filtros, 10.99m, 25);

        Assert.Throws<ValidationException>(() => service.CadastrarPeca(request));
        Assert.Empty(context.Pecas);
    }

    [Fact]
    public void CadastrarPeca_DeveLancarValidationException_QuandoCategoriaAusente()
    {
        using var context = CreateContext();
        var service = new PecaService(context);
        var request = new PecaRequest("P001", "Filtro de oleo", null, 10.99m, 25);

        Assert.Throws<ValidationException>(() => service.CadastrarPeca(request));
        Assert.Empty(context.Pecas);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public void CadastrarPeca_DeveLancarValidationException_QuandoPrecoAusenteZeroOuNegativo(int? preco)
    {
        using var context = CreateContext();
        var service = new PecaService(context);
        var request = new PecaRequest("P001", "Filtro de oleo", CategoriaPeca.Filtros, preco, 25);

        Assert.Throws<ValidationException>(() => service.CadastrarPeca(request));
        Assert.Empty(context.Pecas);
    }

    [Fact]
    public void CadastrarPeca_DeveLancarValidationException_QuandoPrecoTemMaisDe2CasasDecimais()
    {
        using var context = CreateContext();
        var service = new PecaService(context);
        var request = new PecaRequest("P001", "Filtro de oleo", CategoriaPeca.Filtros, 10.999m, 25);

        Assert.Throws<ValidationException>(() => service.CadastrarPeca(request));
        Assert.Empty(context.Pecas);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(-1)]
    public void CadastrarPeca_DeveLancarValidationException_QuandoEstoqueAusenteOuNegativo(int? estoque)
    {
        using var context = CreateContext();
        var service = new PecaService(context);
        var request = new PecaRequest("P001", "Filtro de oleo", CategoriaPeca.Filtros, 10.99m, estoque);

        Assert.Throws<ValidationException>(() => service.CadastrarPeca(request));
        Assert.Empty(context.Pecas);
    }

    [Fact]
    public void ListarPecas_DeveRetornarTodasAsPecas_QuandoStatusNaoForInformado()
    {
        using var context = CreateContext();
        SeedPecas(context);
        var service = new PecaService(context);

        var result = service.ListarPecas();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ListarPecas_DeveRetornarApenasPecasAtivas_QuandoStatusForAtivo()
    {
        using var context = CreateContext();
        SeedPecas(context);
        var service = new PecaService(context);

        var result = service.ListarPecas(StatusCadastro.Ativo);

        Assert.Equal(2, result.Count);
        Assert.All(result, peca => Assert.Equal(nameof(StatusCadastro.Ativo), peca.Status));
        Assert.Collection(
            result,
            peca => Assert.Equal("Filtro", peca.Nome),
            peca => Assert.Equal("Vela", peca.Nome));
    }

    [Fact]
    public void ListarPecas_DeveRetornarApenasPecasInativas_QuandoStatusForInativo()
    {
        using var context = CreateContext();
        SeedPecas(context);
        var service = new PecaService(context);

        var result = service.ListarPecas(StatusCadastro.Inativo);

        Assert.Single(result);
        Assert.Equal("Pastilha", result[0].Nome);
        Assert.Equal(nameof(StatusCadastro.Inativo), result[0].Status);
    }

    private static void SeedPecas(AppDbContext context)
    {
        context.Pecas.AddRange(
            new Peca
            {
                Codigo = "P002",
                Nome = "Vela",
                Categoria = CategoriaPeca.Eletrica,
                Preco = 20m,
                Estoque = 5,
                Status = StatusCadastro.Ativo
            },
            new Peca
            {
                Codigo = "P001",
                Nome = "Filtro",
                Categoria = CategoriaPeca.Filtros,
                Preco = 10m,
                Estoque = 15,
                Status = StatusCadastro.Ativo
            },
            new Peca
            {
                Codigo = "P003",
                Nome = "Pastilha",
                Categoria = CategoriaPeca.Freios,
                Preco = 30m,
                Estoque = 0,
                Status = StatusCadastro.Inativo
            });
        context.SaveChanges();
    }
}
