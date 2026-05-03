using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MotoRevApi.Controller;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Controllers;

public class ConcessionariaControllerTests
{
    private readonly Mock<ConcessionariaService> _concessionariaServiceMock;
    private readonly Mock<EnderecoService> _enderecoServiceMock;
    private readonly ConcessionariaController _controller;

    public ConcessionariaControllerTests()
    {
        _concessionariaServiceMock = new Mock<ConcessionariaService>();
        _enderecoServiceMock = new Mock<EnderecoService>();
        _controller = new ConcessionariaController(_concessionariaServiceMock.Object, _enderecoServiceMock.Object);
    }

    [Fact]
    public async Task Register_DeveRetornarCreated_QuandoSucesso()
    {
        // Arrange
        var request = new RegisterConcessionariaRequest("conc@test.com", "Pass123!", "Conc Test", "12345678000190");
        var response = new ConcessionariaResponse { Id = 1, Nome = "Conc Test", Cnpj = "12345678000190" };
        _concessionariaServiceMock.Setup(s => s.RegisterAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdAtActionResult.StatusCode);
        Assert.Equal(response, createdAtActionResult.Value);
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoEncontrado()
    {
        // Arrange
        var response = new ConcessionariaResponse { Id = 1, Nome = "Conc 1", Cnpj = "12345678000190" };
        _concessionariaServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(response);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetMe_DeveRetornarOk_QuandoAutenticado()
    {
        // Arrange
        var userId = "user-123";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        
        var response = new ConcessionariaResponse { Id = 1, Nome = "Conc 1", Cnpj = "12345678000190" };
        _concessionariaServiceMock.Setup(s => s.GetByUserIdAsync(userId)).ReturnsAsync(response);

        // Act
        var result = await _controller.GetMe();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetMe_DeveRetornarUnauthorized_QuandoSemUserId()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        // Act
        var result = await _controller.GetMe();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task AdicionarEndereco_DeveRetornarCreated_QuandoSucesso()
    {
        // Arrange
        var userId = "user-123";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        var concessionaria = new ConcessionariaResponse { Id = 1, Nome = "Conc 1", Cnpj = "12345678000190" };
        _concessionariaServiceMock.Setup(s => s.GetByUserIdAsync(userId)).ReturnsAsync(concessionaria);

        var request = new EnderecoRequest("01001000", "Praça da Sé", "10", null, "Sé", "São Paulo", "SP");
        var response = new EnderecoResponse(1, "01001000", "Praça da Sé", "10", null, "Sé", "São Paulo", "SP");

        _enderecoServiceMock.Setup(s => s.AdicionarEnderecoAsync(1, request)).ReturnsAsync(response);

        // Act
        var result = await _controller.AdicionarEndereco(request);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(response, createdResult.Value);
    }

    [Fact]
    public async Task RemoverEndereco_DeveRetornarNoContent_QuandoSucesso()
    {
        // Arrange
        var userId = "user-123";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        var concessionaria = new ConcessionariaResponse { Id = 1, Nome = "Conc 1", Cnpj = "12345678000190" };
        _concessionariaServiceMock.Setup(s => s.GetByUserIdAsync(userId)).ReturnsAsync(concessionaria);

        _enderecoServiceMock.Setup(s => s.RemoverEnderecoAsync(99, 1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RemoverEndereco(99);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public void Atributos_MetodosRestritos_DevemTerRoleConcessionaria()
    {
        var metodosParaChecar = new[] { nameof(ConcessionariaController.GetMe), nameof(ConcessionariaController.AdicionarEndereco), nameof(ConcessionariaController.RemoverEndereco) };

        foreach (var nomeMetodo in metodosParaChecar)
        {
            var method = typeof(ConcessionariaController).GetMethod(nomeMetodo);
            var attribute = method?.GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>()
                .FirstOrDefault();

            Assert.NotNull(attribute);
            Assert.Equal(Authorization.Roles.Concessionaria, attribute.Roles);
        }
    }
}
