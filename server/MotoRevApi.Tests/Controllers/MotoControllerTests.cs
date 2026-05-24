using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MotoRevApi.Controller;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Controllers;

public class MotoControllerTests
{
    private readonly Mock<MotoService> _motoServiceMock;
    private readonly MotoController _controller;

    public MotoControllerTests()
    {
        _motoServiceMock = new Mock<MotoService>();
        _controller = new MotoController(_motoServiceMock.Object);
    }

    [Fact]
    public async Task AdicionarMoto_DeveRetornarCreated_QuandoDadosValidos()
    {
        // Arrange
        var request = new MotoRequest("ABC-1234", "CHASSI12345678901", 1, null);
        var response = new MotoResponse(1, "ABC1234", "CHASSI12345678901", 1, "CB 500F", "Honda", 1, null, null, null);
        
        var userId = "user-id-123";
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        _motoServiceMock.Setup(s => s.CadastrarMotoAsync(request, userId))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.AdicionarMoto(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
        Assert.Equal(response, objectResult.Value);
    }

    [Fact]
    public async Task AdicionarMoto_DeveRetornarUnauthorized_QuandoSemUserId()
    {
        // Arrange
        var request = new MotoRequest("ABC-1234", "CHASSI12345678901", 1, null);
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity()); // Sem NameIdentifier Claim

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        // Act
        var result = await _controller.AdicionarMoto(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}
