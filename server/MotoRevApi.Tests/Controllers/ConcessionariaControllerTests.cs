using System.Security.Claims;
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
    public async Task ListarOuBuscarConcessionarias_DeveRetornarOkComLista()
    {
        // Arrange
        var list = new List<ConcessionariaListResponse>
        {
            new ConcessionariaListResponse { Id = 1, Nome = "Conc Teste", Enderecos = new List<EnderecoResponse>() }
        };
        _concessionariaServiceMock.Setup(s => s.BuscarConcessionariasAsync("Teste", "São Paulo")).ReturnsAsync(list);

        // Act
        var result = await _controller.ListarOuBuscarConcessionarias("Teste", "São Paulo");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(list, okResult.Value);
    }

    [Fact]
    public async Task UpdateMe_DeveRetornarOk_QuandoSucesso()
    {
        // Arrange
        var userId = "user-123";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        var concessionariaAtual = new ConcessionariaResponse { Id = 1, Nome = "Nome Antigo", Email = "antigo@email.com", Cnpj = "12345678000190" };
        _concessionariaServiceMock.Setup(s => s.GetByUserIdAsync(userId)).ReturnsAsync(concessionariaAtual);

        var request = new UpdateConcessionariaRequest("Novo Nome Fantasia", "novo@email.com");
        var responseEsperada = new ConcessionariaResponse { Id = 1, Nome = "Novo Nome Fantasia", Email = "novo@email.com", Cnpj = "12345678000190" };
        
        _concessionariaServiceMock.Setup(s => s.UpdateAsync(1, request)).ReturnsAsync(responseEsperada);

        // Act
        var result = await _controller.UpdateMe(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(responseEsperada, okResult.Value);
    }

    [Fact]
    public async Task DeleteMe_DeveRetornarOk_QuandoSucesso()
    {
        // Arrange
        var userId = "user-123";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        _concessionariaServiceMock.Setup(s => s.InativarAsync(userId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteMe();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task DeleteMe_DeveRetornarBadRequest_QuandoExistemAgendamentosPendentes()
    {
        // Arrange
        var userId = "user-123";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        _concessionariaServiceMock.Setup(s => s.InativarAsync(userId))
            .ThrowsAsync(new InvalidOperationException("Não é possível excluir a conta pois existem agendamentos pendentes ou em andamento."));

        // Act
        var result = await _controller.DeleteMe();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
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
    public async Task RemoverEndereco_DeveRetornarOk_QuandoSucesso()
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
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public void Atributos_MetodosRestritos_DevemTerRoleConcessionaria()
    {
        var metodosParaChecar = new[] { 
            nameof(ConcessionariaController.GetMe), 
            nameof(ConcessionariaController.UpdateMe), 
            nameof(ConcessionariaController.DeleteMe), 
            nameof(ConcessionariaController.AdicionarEndereco), 
            nameof(ConcessionariaController.RemoverEndereco) 
        };

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
