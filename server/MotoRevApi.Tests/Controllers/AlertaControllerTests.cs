using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MotoRevApi.Controller;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
using MotoRevApi.Model;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Controllers;

public class AlertaControllerTests
{
    private readonly Mock<AlertaService> _alertaServiceMock;
    private readonly AlertaController _controller;
    private const string UsuarioId = "user-123";

    public AlertaControllerTests()
    {
        _alertaServiceMock = new Mock<AlertaService>();
        _controller = new AlertaController(_alertaServiceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, UsuarioId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task Listar_DeveRetornarOk_ComListaDeAlertas()
    {
        // Arrange
        var alertas = new List<AlertaResponse>
        {
            new AlertaResponse(1, TipoAlerta.RevisaoProxima, UsuarioId, null, null, null, false, DateTime.UtcNow),
            new AlertaResponse(2, TipoAlerta.AgendamentoCriado, UsuarioId, null, null, null, false, DateTime.UtcNow)
        };

        _alertaServiceMock.Setup(s => s.ListarAlertasAsync(UsuarioId, null, null))
            .ReturnsAsync(alertas);

        // Act
        var result = await _controller.Listar(null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnAlertas = Assert.IsType<List<AlertaResponse>>(okResult.Value);
        Assert.Equal(2, returnAlertas.Count);
    }

    [Fact]
    public async Task ContarNaoLidos_DeveRetornarOk_ComTotal()
    {
        // Arrange
        _alertaServiceMock.Setup(s => s.ContarNaoLidosAsync(UsuarioId))
            .ReturnsAsync(5);

        // Act
        var result = await _controller.ContarNaoLidos();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value;
        var totalProp = value?.GetType().GetProperty("total");
        var totalValue = totalProp?.GetValue(value, null);
        Assert.Equal(5, totalValue);
    }

    [Fact]
    public async Task MarcarComoLido_DeveRetornarOk_QuandoEncontrado()
    {
        // Arrange
        var alerta = new AlertaResponse(1, TipoAlerta.RevisaoProxima, UsuarioId, null, null, null, true, DateTime.UtcNow);
        _alertaServiceMock.Setup(s => s.MarcarComoLidoAsync(1, UsuarioId))
            .ReturnsAsync(alerta);

        // Act
        var result = await _controller.MarcarComoLido(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(alerta, okResult.Value);
    }

    [Fact]
    public async Task MarcarComoLido_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        // Arrange
        _alertaServiceMock.Setup(s => s.MarcarComoLidoAsync(1, UsuarioId))
            .ReturnsAsync((AlertaResponse?)null);

        // Act
        var result = await _controller.MarcarComoLido(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task MarcarTodosComoLidos_DeveRetornarNoContent()
    {
        // Arrange
        _alertaServiceMock.Setup(s => s.MarcarTodosComoLidosAsync(UsuarioId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.MarcarTodosComoLidos();

        // Assert
        Assert.IsType<NoContentResult>(result);
        _alertaServiceMock.Verify(s => s.MarcarTodosComoLidosAsync(UsuarioId), Times.Once);
    }

    [Fact]
    public async Task Listar_DeveRetornarUnauthorized_QuandoUsuarioIdForNulo()
    {
        // Arrange
        var controllerWithoutUser = new AlertaController(_alertaServiceMock.Object);
        controllerWithoutUser.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await controllerWithoutUser.Listar(null, null);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task ContarNaoLidos_DeveRetornarUnauthorized_QuandoUsuarioIdForNulo()
    {
        // Arrange
        var controllerWithoutUser = new AlertaController(_alertaServiceMock.Object);
        controllerWithoutUser.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await controllerWithoutUser.ContarNaoLidos();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task MarcarComoLido_DeveRetornarUnauthorized_QuandoUsuarioIdForNulo()
    {
        // Arrange
        var controllerWithoutUser = new AlertaController(_alertaServiceMock.Object);
        controllerWithoutUser.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await controllerWithoutUser.MarcarComoLido(1);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task MarcarTodosComoLidos_DeveRetornarUnauthorized_QuandoUsuarioIdForNulo()
    {
        // Arrange
        var controllerWithoutUser = new AlertaController(_alertaServiceMock.Object);
        controllerWithoutUser.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await controllerWithoutUser.MarcarTodosComoLidos();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}
