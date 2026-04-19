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
        var request = new RegisterClienteRequest("john@test.com", "Password123!", "John Doe", "12345678901", "11999999999", null);
        var response = new ClienteResponse(1, "John Doe", "john@test.com", "12345678901");
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

        var response = new ClienteResponse(1, "John Doe", "john@test.com", "12345678901");
        _clienteServiceMock.Setup(s => s.GetByUserIdAsync(userId)).ReturnsAsync(response);

        // Act
        var result = await _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
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

    [Fact]
    public async Task Update_DeveRetornarNoContent_QuandoSucesso()
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

        var request = new UpdateClienteRequest("John Updated", "11888888888", null);
        _clienteServiceMock.Setup(s => s.UpdateAsync(userId, request)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(request);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _clienteServiceMock.Verify(s => s.UpdateAsync(userId, request), Times.Once);
    }

    [Fact]
    public async Task Delete_DeveRetornarNoContent_QuandoSucesso()
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

        _clienteServiceMock.Setup(s => s.DeleteAsync(userId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete();

        // Assert
        Assert.IsType<NoContentResult>(result);
        _clienteServiceMock.Verify(s => s.DeleteAsync(userId), Times.Once);
    }
    [Fact]
    public async Task Update_DeveRetornarUnauthorized_QuandoSemUserId()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
        var request = new UpdateClienteRequest("John Updated", "11888888888", null);

        // Act
        var result = await _controller.Update(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Delete_DeveRetornarUnauthorized_QuandoSemUserId()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.Delete();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Update_DeveTerAtributoAuthorizeComRoleCliente()
    {
        var method = typeof(ClienteController).GetMethod(nameof(ClienteController.Update));
        var attribute = method.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(attribute);
        Assert.Equal(MotoRevApi.Authorization.Roles.Cliente, attribute.Roles);
    }
    
    [Fact]
    public void Delete_DeveTerAtributoAuthorizeComRoleCliente()
    {
        var method = typeof(ClienteController).GetMethod(nameof(ClienteController.Delete));
        var attribute = method.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(attribute);
        Assert.Equal(MotoRevApi.Authorization.Roles.Cliente, attribute.Roles);
    }
}