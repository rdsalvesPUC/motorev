using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Authorization;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
using MotoRevApi.Model;
using MotoRevApi.Services;
using Xunit;
using PecaApiController = MotoRevApi.Controller.PecaController;

namespace MotoRevApi.Tests.Controllers;

public class PecaControllerTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public PecaControllerTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    private AppDbContext CreateContext() => new AppDbContext(_dbContextOptions);

    [Fact]
    public void AdicionarPeca_DeveRetornarCreatedAtAction_QuandoSucesso()
    {
        using var context = CreateContext();
        var controller = new PecaApiController(new PecaService(context));
        var request = new PecaRequest("P001", "Filtro de oleo", CategoriaPeca.Filtros, 10.99m, 25);

        var result = controller.AdicionarPeca(request);

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
        Assert.Equal(nameof(PecaApiController.ObterPeca), createdAtActionResult.ActionName);

        var response = Assert.IsType<PecaResponse>(createdAtActionResult.Value);
        Assert.Equal("P001", response.Codigo);
        Assert.Equal("Filtro de oleo", response.Nome);
        Assert.Equal(nameof(CategoriaPeca.Filtros), response.Categoria);
        Assert.Equal(10.99m, response.Preco);
        Assert.Equal(25, response.Estoque);
    }

    [Fact]
    public void AdicionarPeca_DeveTerAuthorizeComRoleConcessionaria()
    {
        var method = typeof(PecaApiController).GetMethod(nameof(PecaApiController.AdicionarPeca));

        var attribute = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        Assert.NotNull(attribute);
        Assert.Equal(Roles.Concessionaria, attribute.Roles);
    }

    [Theory]
    [InlineData(StatusCodes.Status201Created)]
    [InlineData(StatusCodes.Status400BadRequest)]
    [InlineData(StatusCodes.Status401Unauthorized)]
    [InlineData(StatusCodes.Status403Forbidden)]
    [InlineData(StatusCodes.Status409Conflict)]
    public void AdicionarPeca_DeveDeclararStatusDeRespostaEsperados(int statusCode)
    {
        var method = typeof(PecaApiController).GetMethod(nameof(PecaApiController.AdicionarPeca));

        var declaredStatusCodes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), true)
            .Cast<ProducesResponseTypeAttribute>()
            .Select(attribute => attribute.StatusCode);

        Assert.Contains(statusCode, declaredStatusCodes);
    }

    [Fact]
    public void ObterPeca_DeveRetornarOk_QuandoEncontrado()
    {
        using var context = CreateContext();
        var peca = new Peca { Codigo = "P001", Nome = "Peca 1", Categoria = CategoriaPeca.Motor, Preco = 100, Estoque = 10, Status = StatusCadastro.Ativo };
        context.Pecas.Add(peca);
        context.SaveChanges();
        
        var controller = new PecaApiController(new PecaService(context));

        var result = controller.ObterPeca(peca.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PecaResponse>(okResult.Value);
        Assert.Equal(peca.Id, response.Id);
    }

    [Fact]
    public void AtualizarPeca_DeveRetornarOk_QuandoSucesso()
    {
        using var context = CreateContext();
        var peca = new Peca { Codigo = "P001", Nome = "Peca 1", Categoria = CategoriaPeca.Motor, Preco = 100, Estoque = 10, Status = StatusCadastro.Ativo };
        context.Pecas.Add(peca);
        context.SaveChanges();
        
        var controller = new PecaApiController(new PecaService(context));
        var request = new PecaUpdateRequest("P001", "Peca Alt", CategoriaPeca.Motor, 150, 5, StatusCadastro.Ativo);

        var result = controller.AtualizarPeca(peca.Id, request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PecaResponse>(okResult.Value);
        Assert.Equal("Peca Alt", response.Nome);
        Assert.Equal(150, response.Preco);
    }

    [Fact]
    public void AtualizarStatusPeca_DeveRetornarOk_QuandoSucesso()
    {
        using var context = CreateContext();
        var peca = new Peca { Codigo = "P001", Nome = "Peca 1", Categoria = CategoriaPeca.Motor, Preco = 100, Estoque = 10, Status = StatusCadastro.Ativo };
        context.Pecas.Add(peca);
        context.SaveChanges();
        
        var controller = new PecaApiController(new PecaService(context));
        var request = new PecaStatusRequest(StatusCadastro.Inativo);

        var result = controller.AtualizarStatusPeca(peca.Id, request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PecaResponse>(okResult.Value);
        Assert.Equal(nameof(StatusCadastro.Inativo), response.Status);
    }

    [Fact]
    public void ObterPecas_DeveRetornarLista()
    {
        using var context = CreateContext();
        context.Pecas.Add(new Peca { Codigo = "P001", Nome = "Peca 1", Categoria = CategoriaPeca.Motor, Preco = 100, Estoque = 10, Status = StatusCadastro.Ativo });
        context.Pecas.Add(new Peca { Codigo = "P002", Nome = "Peca 2", Categoria = CategoriaPeca.Motor, Preco = 200, Estoque = 20, Status = StatusCadastro.Ativo });
        context.SaveChanges();
        
        var controller = new PecaApiController(new PecaService(context));

        var result = controller.ObterPecas(null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<List<PecaResponse>>(okResult.Value);
        Assert.Equal(2, response.Count);
    }
}
