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
    private readonly Mock<ConcessionariaService> _serviceMock;
    private readonly ConcessionariaController _controller;

    public ConcessionariaControllerTests()
    {
        _serviceMock = new Mock<ConcessionariaService>();
        _controller = new ConcessionariaController(_serviceMock.Object);
    }

    private static RegisterConcessionariaRequest CreateRegisterRequest()
    {
        return new RegisterConcessionariaRequest(
            "conc@test.com",
            "Pass123!",
            "Conc Test",
            "12.345.678/0001-90",
            "(11) 99999-9999"
        );
    }

    private static ConcessionariaResponse CreateConcessionariaResponse(int id = 1, string nome = "Conc Test")
    {
        return new ConcessionariaResponse(
            id,
            nome,
            "conc@test.com",
            "12.345.678/0001-90",
            "(11) 99999-9999",
            "Matriz",
            "01001-000",
            "Rua Teste",
            "100",
            "Centro",
            "Sao Paulo",
            "SP",
            []
        );
    }

    private void SetAuthenticatedConcessionaria(string userId = "user-123")
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
    }

    [Fact]
    public async Task Register_DeveRetornarCreated_QuandoSucesso()
    {
        // Arrange
        var request = CreateRegisterRequest();
        var response = CreateConcessionariaResponse();
        _serviceMock.Setup(s => s.RegisterAsync(request)).ReturnsAsync(response);

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
        var response = CreateConcessionariaResponse(nome: "Conc 1");
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(response);

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
        SetAuthenticatedConcessionaria(userId);
        
        var response = CreateConcessionariaResponse(nome: "Conc 1");
        _serviceMock.Setup(s => s.GetByUserIdAsync(userId)).ReturnsAsync(response);

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
    public void Atributo_GetMe_DeveTerRoleConcessionaria()
    {
        var method = typeof(ConcessionariaController).GetMethod(nameof(ConcessionariaController.GetMe));
        var attribute = method.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(attribute);
        Assert.Equal(Authorization.Roles.Concessionaria, attribute.Roles);
    }

    [Fact]
    public async Task UpdateMe_DeveRetornarOk_QuandoAutenticado()
    {
        // Arrange
        var userId = "user-123";
        SetAuthenticatedConcessionaria(userId);
        var request = new ConcessionariaPerfilRequest(
            "Conc Atualizada",
            "conc@test.com",
            "12.345.678/0001-90",
            "(11) 99999-9999",
            "01001-000",
            "Rua Teste",
            "100",
            "Centro",
            "Sao Paulo",
            "SP"
        );
        var response = CreateConcessionariaResponse(nome: "Conc Atualizada");
        _serviceMock.Setup(s => s.UpdatePerfilAsync(userId, request)).ReturnsAsync(response);

        // Act
        var result = await _controller.UpdateMe(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task UpdateMe_DeveRetornarUnauthorized_QuandoSemUserId()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        var request = new ConcessionariaPerfilRequest("Conc", "conc@test.com", "12.345.678/0001-90", "(11) 99999-9999", "01001-000", "Rua", "1", "Bairro", "Cidade", "SP");

        // Act
        var result = await _controller.UpdateMe(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task AlterarSenha_DeveRetornarNoContent_QuandoAutenticado()
    {
        // Arrange
        var userId = "user-123";
        SetAuthenticatedConcessionaria(userId);
        var request = new ConcessionariaAlterarSenhaRequest("Atual123!", "Nova123!", "Nova123!");
        _serviceMock.Setup(s => s.AlterarSenhaAsync(userId, request)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AlterarSenha(request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Atributo_UpdateMe_DeveTerRoleConcessionaria()
    {
        var method = typeof(ConcessionariaController).GetMethod(nameof(ConcessionariaController.UpdateMe));
        var attribute = method.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(attribute);
        Assert.Equal(Authorization.Roles.Concessionaria, attribute.Roles);
    }

    [Fact]
    public async Task AddLoja_DeveRetornarCreated_QuandoSucesso()
    {
        // Arrange
        var userId = "user-123";
        SetAuthenticatedConcessionaria(userId);
        var request = new LojaRequest("Loja", "11.222.333/0001-44", "(11) 3000-0000", "04000-000", "Rua", "10", "Bairro", "Cidade", "SP");
        var response = new LojaResponse(1, "Loja", "Filial", "11.222.333/0001-44", "(11) 3000-0000", "04000-000", "Rua", "10", "Bairro", "Cidade", "SP", 1, true);
        _serviceMock.Setup(s => s.ConcessionariaPertenceAoUsuarioAsync(userId, 1)).ReturnsAsync(true);
        _serviceMock.Setup(s => s.AddLojaAsync(1, request)).ReturnsAsync(response);

        // Act
        var result = await _controller.AddLoja(1, request);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdAtActionResult.StatusCode);
        Assert.Equal(response, createdAtActionResult.Value);
    }

    [Fact]
    public async Task UpdateLoja_DeveRetornarOk_QuandoSucesso()
    {
        // Arrange
        var userId = "user-123";
        SetAuthenticatedConcessionaria(userId);
        var request = new LojaRequest("Loja", "11.222.333/0001-44", "(11) 3000-0000", "04000-000", "Rua", "10", "Bairro", "Cidade", "SP");
        var response = new LojaResponse(1, "Loja", "Filial", "11.222.333/0001-44", "(11) 3000-0000", "04000-000", "Rua", "10", "Bairro", "Cidade", "SP", 1, true);
        _serviceMock.Setup(s => s.ConcessionariaPertenceAoUsuarioAsync(userId, 1)).ReturnsAsync(true);
        _serviceMock.Setup(s => s.UpdateLojaAsync(1, 1, request)).ReturnsAsync(response);

        // Act
        var result = await _controller.UpdateLoja(1, 1, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task AlternarStatusLoja_DeveRetornarOk_QuandoSucesso()
    {
        // Arrange
        var userId = "user-123";
        SetAuthenticatedConcessionaria(userId);
        var response = new LojaResponse(1, "Loja", "Filial", "11.222.333/0001-44", "(11) 3000-0000", "04000-000", "Rua", "10", "Bairro", "Cidade", "SP", 1, false);
        _serviceMock.Setup(s => s.ConcessionariaPertenceAoUsuarioAsync(userId, 1)).ReturnsAsync(true);
        _serviceMock.Setup(s => s.AlternarStatusLojaAsync(1, 1)).ReturnsAsync(response);

        // Act
        var result = await _controller.AlternarStatusLoja(1, 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task AddMinhaLoja_DeveUsarUsuarioAutenticado()
    {
        // Arrange
        var userId = "user-123";
        SetAuthenticatedConcessionaria(userId);
        var request = new LojaRequest("Loja", "11.222.333/0001-44", "(11) 3000-0000", "04000-000", "Rua", "10", "Bairro", "Cidade", "SP");
        var response = new LojaResponse(1, "Loja", "Filial", "11.222.333/0001-44", "(11) 3000-0000", "04000-000", "Rua", "10", "Bairro", "Cidade", "SP", 1, true);
        _serviceMock.Setup(s => s.AddLojaAsync(userId, request)).ReturnsAsync(response);

        // Act
        var result = await _controller.AddMinhaLoja(request);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(response, createdAtActionResult.Value);
    }

    [Fact]
    public async Task GetMinhasLojas_DeveUsarUsuarioAutenticado()
    {
        // Arrange
        var userId = "user-123";
        SetAuthenticatedConcessionaria(userId);
        var response = new[]
        {
            new LojaResponse(1, "Loja", "Filial", "11.222.333/0001-44", "(11) 3000-0000", "04000-000", "Rua", "10", "Bairro", "Cidade", "SP", 1, true)
        };
        _serviceMock.Setup(s => s.GetLojasAsync(userId)).ReturnsAsync(response);

        // Act
        var result = await _controller.GetMinhasLojas();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task AddLoja_DeveRetornarForbid_QuandoConcessionariaNaoPertenceAoUsuario()
    {
        // Arrange
        var userId = "user-123";
        SetAuthenticatedConcessionaria(userId);
        var request = new LojaRequest("Loja", "11.222.333/0001-44", "(11) 3000-0000", "04000-000", "Rua", "10", "Bairro", "Cidade", "SP");
        _serviceMock.Setup(s => s.ConcessionariaPertenceAoUsuarioAsync(userId, 99)).ReturnsAsync(false);

        // Act
        var result = await _controller.AddLoja(99, request);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetLojasAtivas_DeveRetornarOk()
    {
        // Arrange
        var response = new[]
        {
            new LojaResponse(1, "Loja", "Filial", "11.222.333/0001-44", "(11) 3000-0000", "04000-000", "Rua", "10", "Bairro", "Cidade", "SP", 1, true)
        };
        _serviceMock.Setup(s => s.GetLojasAtivasAsync()).ReturnsAsync(response);

        // Act
        var result = await _controller.GetLojasAtivas();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }
}
