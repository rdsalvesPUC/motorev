using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MotoRevApi.Controller;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Exceptions;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Controllers;

public class RevisaoPadraoControllerTests
{
    private readonly Mock<RevisaoPadraoService> _revisaoServiceMock;
    private readonly Mock<ConcessionariaService> _concessionariaServiceMock;
    private readonly RevisaoPadraoController _controller;
    private const string UserId = "44c403a7-754a-4763-9f72-a02c68fc44e0";
    private const int ConcessionariaId = 1;

    public RevisaoPadraoControllerTests()
    {
        _revisaoServiceMock = new Mock<RevisaoPadraoService>();
        _controller = new RevisaoPadraoController(_revisaoServiceMock.Object);
        
        // Mock do usuário padrão para a maioria dos testes
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, UserId) }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoSucesso()
    {
        // Arrange
        var response = new RevisaoPadraoResponse(1, "Revisão 1000km", 1, 1000, 6, 2, "Street", new List<ServicoResponse>(), new List<RevisaoPadraoPecaResponse>());
        _revisaoServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(response);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task PostPorLinha_DeveRetornarCreated_QuandoSucesso()
    {
        // Arrange
        var request = new RevisaoPadraoLinhaRequest(
            "Plano Street",
            2,
            new List<RevisaoPadraoLinhaItemRequest>
            {
                new("Revisão 1000km", 1, 1000, 6, new List<int> { 1 })
            });
        var response = new List<RevisaoPadraoResponse>
        {
            new(1, "Revisão 1000km", 1, 1000, 6, 2, "Street", new List<ServicoResponse>(), new List<RevisaoPadraoPecaResponse>())
        };

        _revisaoServiceMock.Setup(s => s.CadastrarRevisoesPorLinhaAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _controller.PostPorLinha(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(response, createdResult.Value);
    }

    [Fact]
    public void Atributos_Classe_DeveTerRoleConcessionaria()
    {
        var attribute = typeof(RevisaoPadraoController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(attribute);
        Assert.Equal("Concessionaria", attribute.Roles);
    }

    [Fact]
    public async Task Get_DeveRepassarFiltroDeLinha_QuandoInformado()
    {
        // Arrange
        var response = new List<RevisaoPadraoListResponse>
        {
            new(1, "Revisão 1000km", 2, "Street", 1, 1000, 6, true)
        };

        _revisaoServiceMock.Setup(s => s.ListarRevisoesAsync(null, 2)).ReturnsAsync(response);

        // Act
        var result = await _controller.Get(null, 2);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task AlternarStatusPorLinha_DeveRetornarOk_QuandoSucesso()
    {
        // Arrange
        var response = new List<RevisaoPadraoListResponse>
        {
            new(1, "Revisão 1000km", 2, "Street", 1, 1000, 6, false)
        };

        _revisaoServiceMock.Setup(s => s.AlternarStatusPorLinhaAsync(2)).ReturnsAsync(response);

        // Act
        var result = await _controller.AlternarStatusPorLinha(2);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }
}
