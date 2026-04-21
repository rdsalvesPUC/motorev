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

    [Fact]
    public async Task Register_DeveRetornarCreated_QuandoSucesso()
    {
        // Arrange
        var request = new RegisterConcessionariaRequest("conc@test.com", "Pass123!", "Conc Test");
        var response = new ConcessionariaResponse(1, "Conc Test");
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
        var response = new ConcessionariaResponse(1, "Conc 1");
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
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        
        var response = new ConcessionariaResponse(1, "Conc 1");
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
}