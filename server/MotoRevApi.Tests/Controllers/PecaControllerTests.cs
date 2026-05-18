using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Authorization;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
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
        var request = new PecaRequest("Filtro de oleo", "Descricao valida", 10.99m);

        var result = controller.AdicionarPeca(request);

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
        Assert.Equal(nameof(PecaApiController.ObterPeca), createdAtActionResult.ActionName);

        var response = Assert.IsType<PecaResponse>(createdAtActionResult.Value);
        Assert.Equal("Filtro de oleo", response.Nome);
        Assert.Equal(10.99m, response.Valor);
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
}
