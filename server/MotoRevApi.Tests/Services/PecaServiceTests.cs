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
        var request = new PecaRequest("Filtro de oleo", "Descricao valida", 10.99m);

        var result = service.CadastrarPeca(request);

        Assert.NotNull(result);
        Assert.Equal("Filtro de oleo", result.Nome);
        Assert.Equal("Descricao valida", result.Descricao);
        Assert.Equal(10.99m, result.Valor);

        var pecaNoDb = context.Pecas.SingleOrDefault();
        Assert.NotNull(pecaNoDb);
        Assert.Equal("Filtro de oleo", pecaNoDb.Nome);
        Assert.Equal(StatusCadastro.Ativo, pecaNoDb.Status);
    }

    [Fact]
    public void CadastrarPeca_DeveCriarPecaSemDescricao()
    {
        using var context = CreateContext();
        var service = new PecaService(context);
        var request = new PecaRequest("Filtro de oleo", null, 10.99m);

        var result = service.CadastrarPeca(request);

        Assert.Null(result.Descricao);

        var pecaNoDb = context.Pecas.SingleOrDefault();
        Assert.NotNull(pecaNoDb);
        Assert.Null(pecaNoDb.Descricao);
    }

    [Theory]
    [InlineData("Filtro de oleo", "Filtro de oleo")]
    [InlineData("Filtro de oleo", "  Filtro de oleo  ")]
    [InlineData("Filtro de Oleo", "filtro de oleo")]
    public void CadastrarPeca_DeveLancarDuplicateDataException_QuandoNomeJaExiste(
        string nomeExistente,
        string novoNome)
    {
        using var context = CreateContext();
        context.Pecas.Add(new Peca
        {
            Nome = nomeExistente,
            Descricao = "Descricao existente",
            Valor = 10.99m,
            Status = StatusCadastro.Ativo
        });
        context.SaveChanges();

        var service = new PecaService(context);
        var request = new PecaRequest(novoNome, "Nova descricao", 20.99m);

        Assert.Throws<DuplicateDataException>(() => service.CadastrarPeca(request));
        Assert.Single(context.Pecas);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("AB")]
    public void CadastrarPeca_DeveLancarValidationException_QuandoNomeInvalido(string nome)
    {
        using var context = CreateContext();
        var service = new PecaService(context);
        var request = new PecaRequest(nome, "Descricao valida", 10.99m);

        Assert.Throws<ValidationException>(() => service.CadastrarPeca(request));
        Assert.Empty(context.Pecas);
    }

    [Fact]
    public void CadastrarPeca_DeveLancarValidationException_QuandoDescricaoTemMaisDe1024Caracteres()
    {
        using var context = CreateContext();
        var service = new PecaService(context);
        var request = new PecaRequest("Filtro de oleo", new string('A', 1025), 10.99m);

        Assert.Throws<ValidationException>(() => service.CadastrarPeca(request));
        Assert.Empty(context.Pecas);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public void CadastrarPeca_DeveLancarValidationException_QuandoValorAusenteZeroOuNegativo(int? valor)
    {
        using var context = CreateContext();
        var service = new PecaService(context);
        var request = new PecaRequest("Filtro de oleo", "Descricao valida", valor);

        Assert.Throws<ValidationException>(() => service.CadastrarPeca(request));
        Assert.Empty(context.Pecas);
    }

    [Fact]
    public void CadastrarPeca_DeveLancarValidationException_QuandoValorTemMaisDe2CasasDecimais()
    {
        using var context = CreateContext();
        var service = new PecaService(context);
        var request = new PecaRequest("Filtro de oleo", "Descricao valida", 10.999m);

        Assert.Throws<ValidationException>(() => service.CadastrarPeca(request));
        Assert.Empty(context.Pecas);
    }
}
