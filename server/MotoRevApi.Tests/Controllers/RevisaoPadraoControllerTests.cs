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
        _concessionariaServiceMock = new Mock<ConcessionariaService>();
        _controller = new RevisaoPadraoController(_revisaoServiceMock.Object, _concessionariaServiceMock.Object);

        _concessionariaServiceMock
            .Setup(s => s.GetByUserIdAsync(UserId))
            .ReturnsAsync(new ConcessionariaResponse(
                ConcessionariaId,
                "Concessionária",
                "12345678000190",
                "11999999999",
                "Matriz",
                "01001000",
                "Rua A",
                "100",
                "Centro",
                "São Paulo",
                "SP",
                new List<LojaResponse>()));
        
        // Mock do usuário padrão para a maioria dos testes
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, UserId) }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoSucesso()
    {
        // Arrange
        var response = new RevisaoPadraoResponse(1, "Revisão 1000km", 1, 1000, 6, 1, "Ninja", new List<ServicoResponse>(), new List<RevisaoPadraoPecaResponse>());
        _revisaoServiceMock.Setup(s => s.GetByIdAsync(1, ConcessionariaId)).ReturnsAsync(response);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task Post_DeveRetornarCreated_QuandoSucesso()
    {
        // Arrange
        var request = new RevisaoPadraoRequest("Revisão 1000km", 1, 1000, 6, 1, new List<int> { 1, 2 });
        var response = new RevisaoPadraoResponse(1, "Revisão 1000km", 1, 1000, 6, 1, "Ninja", new List<ServicoResponse>(), new List<RevisaoPadraoPecaResponse>());
        
        _revisaoServiceMock.Setup(s => s.CadastrarRevisaoAsync(request, ConcessionariaId)).ReturnsAsync(response);

        // Act
        var result = await _controller.Post(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(response, createdResult.Value);
    }

    [Fact]
    public async Task Post_DeveLancarNotFoundException_QuandoEntidadeNaoExiste()
    {
        // Arrange
        var request = new RevisaoPadraoRequest("Revisão com erro", 99, 1000, 6, 1, new List<int> { 99 });
        _revisaoServiceMock.Setup(s => s.CadastrarRevisaoAsync(request, ConcessionariaId))
            .ThrowsAsync(new NotFoundException("Modelo de moto não encontrado."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _controller.Post(request));
        Assert.Equal("Modelo de moto não encontrado.", exception.Message);
    }

    [Fact]
    public async Task Post_DeveLancarDuplicateDataException_QuandoOrdemDuplicada()
    {
        // Arrange
        var request = new RevisaoPadraoRequest("Revisão duplicada", 1, 1000, 6, 1, new List<int> { 1 });
        _revisaoServiceMock.Setup(s => s.CadastrarRevisaoAsync(request, ConcessionariaId))
            .ThrowsAsync(new DuplicateDataException("Ordem de revisão já existe."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DuplicateDataException>(() => _controller.Post(request));
        Assert.Equal("Ordem de revisão já existe.", exception.Message);
    }

    [Fact]
    public async Task Post_DeveRetornarUnauthorized_QuandoSemUsuario()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // Usuário sem claims
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        var request = new RevisaoPadraoRequest("Revisão 1000km", 1, 1000, 6, 1, new List<int> { 1 });

        // Act
        var result = await _controller.Post(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
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
            new(1, "Revisão 1000km", 1, "Ninja", 2, "Street", 1, 1000, 6, true)
        };

        _revisaoServiceMock.Setup(s => s.ListarRevisoesAsync(ConcessionariaId, null, 2)).ReturnsAsync(response);

        // Act
        var result = await _controller.Get(null, 2);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
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
            new(1, "Revisão 1000km", 1, 1000, 6, 1, "Ninja", new List<ServicoResponse>(), new List<RevisaoPadraoPecaResponse>())
        };

        _revisaoServiceMock.Setup(s => s.CadastrarRevisoesPorLinhaAsync(request, ConcessionariaId)).ReturnsAsync(response);

        // Act
        var result = await _controller.PostPorLinha(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(response, createdResult.Value);
    }

    [Fact]
    public async Task AlternarStatusPorLinha_DeveRetornarOk_QuandoSucesso()
    {
        // Arrange
        var response = new List<RevisaoPadraoListResponse>
        {
            new(1, "Revisão 1000km", 1, "Ninja", 2, "Street", 1, 1000, 6, false)
        };

        _revisaoServiceMock.Setup(s => s.AlternarStatusPorLinhaAsync(2, ConcessionariaId)).ReturnsAsync(response);

        // Act
        var result = await _controller.AlternarStatusPorLinha(2);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }
}
