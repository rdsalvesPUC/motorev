using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MotoRevApi.Controller;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<AuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<AuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Login_DeveRetornarOk_QuandoSucesso()
    {
        // Arrange
        var request = new LoginRequest("test@test.com", "Password123!");
        var response = new LoginResponse("access_token", "refresh_token", "Cliente", new UserData(1, "Test User"));
        _authServiceMock.Setup(s => s.LoginAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task RefreshToken_DeveRetornarOk_QuandoSucesso()
    {
        // Arrange
        var request = new RefreshTokenRequest("expired_access_token", "valid_refresh_token");
        var response = new LoginResponse("new_access_token", "new_refresh_token", "Cliente", new UserData(1, "Test User"));
        _authServiceMock.Setup(s => s.RefreshTokenAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task RefreshToken_DeveLancarExcecao_QuandoErro()
    {
        // Arrange
        var request = new RefreshTokenRequest("invalid_access_token", "invalid_refresh_token");
        _authServiceMock.Setup(s => s.RefreshTokenAsync(request)).ThrowsAsync(new System.Exception("Token inválido"));

        // Act & Assert
        await Assert.ThrowsAsync<System.Exception>(() => _controller.RefreshToken(request));
    }

    [Fact]
    public async Task Logout_DeveRetornarOk_QuandoUsuarioAutenticado()
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

        _authServiceMock.Setup(s => s.LogoutAsync(userId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Logout();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        _authServiceMock.Verify(s => s.LogoutAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Logout_DeveRetornarUnauthorized_QuandoSemUserId()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // Sem claims

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.Logout();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}