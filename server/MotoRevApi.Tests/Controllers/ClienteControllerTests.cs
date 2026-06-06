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

public class ClienteControllerTests
{
    private readonly Mock<ClienteService> _clienteServiceMock;
    private readonly ClienteController _controller;

    public ClienteControllerTests()
    {
        _clienteServiceMock = new Mock<ClienteService>();
        _controller = new ClienteController(_clienteServiceMock.Object);
    }

    [Fact]
    public async Task Register_DeveRetornarCreated_QuandoSucesso()
    {
        // Arrange
        var request = new RegisterClienteRequest("john@test.com", "Password123!", "John Doe", "529.982.247-25", "(11) 99999-0000");
        var response = new ClienteResponse(1, "John Doe");
        _clienteServiceMock.Setup(s => s.RegisterAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdAtActionResult.StatusCode);
        Assert.Equal(response, createdAtActionResult.Value);
    }

    [Fact]
    public async Task Get_DeveRetornarOk_QuandoUsuarioAutenticado()
    {
        // Arrange
        var userId = "user-id-123";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var response = new ClienteResponse(1, "John Doe");
        _clienteServiceMock.Setup(s => s.GetByUserIdAsync(userId)).ReturnsAsync(response);

        // Act
        var result = await _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task GetMe_DeveRetornarOk_QuandoUsuarioAutenticado()
    {
        // Arrange
        var userId = "user-id-123";
        SetAuthenticatedUser(userId);

        var response = new ClientePerfilResponse(
            1,
            "John Doe",
            "john@test.com",
            "52998224725",
            "11999990000",
            new ClienteEnderecoResponse(null, null, null, null, null, null, null));
        _clienteServiceMock.Setup(s => s.GetPerfilByUserIdAsync(userId)).ReturnsAsync(response);

        // Act
        var result = await _controller.GetMe();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task UpdateDadosPessoais_DeveRetornarOk_QuandoUsuarioAutenticado()
    {
        // Arrange
        var userId = "user-id-123";
        SetAuthenticatedUser(userId);

        var request = new ClienteDadosPessoaisRequest("John Doe", "john@test.com", "529.982.247-25", "(11) 99999-0000");
        var response = new ClientePerfilResponse(
            1,
            "John Doe",
            "john@test.com",
            "52998224725",
            "11999990000",
            new ClienteEnderecoResponse(null, null, null, null, null, null, null));
        _clienteServiceMock.Setup(s => s.UpdateDadosPessoaisAsync(userId, request)).ReturnsAsync(response);

        // Act
        var result = await _controller.UpdateDadosPessoais(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task AlterarSenha_DeveRetornarNoContent_QuandoUsuarioAutenticado()
    {
        // Arrange
        var userId = "user-id-123";
        SetAuthenticatedUser(userId);

        var request = new ClienteAlterarSenhaRequest("SenhaAtual123!", "NovaSenha123!", "NovaSenha123!");
        _clienteServiceMock.Setup(s => s.AlterarSenhaAsync(userId, request)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AlterarSenha(request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Get_DeveRetornarUnauthorized_QuandoSemUserId()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.Get();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    private void SetAuthenticatedUser(string userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }
}
