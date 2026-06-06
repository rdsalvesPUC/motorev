using System.Security.Claims;
using System.Threading.Tasks;
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
        var request = new MotoRequest("ABC-1234", "CHASSI12345678901", 1, 2023, "Vermelha", 0, DateTime.Now, null, null, null);
        var response = new MotoResponse(1, "ABC1234", "CHASSI12345678901", 1, "CB 500F", "Honda", 1, null, null, null, 2023, "Vermelha", 0, DateTime.Now, "Linha", "100cc");
        
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
        var request = new MotoRequest("ABC-1234", "CHASSI12345678901", 1, 2023, "Vermelha", 0, DateTime.Now, null, null, null);
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

    [Fact]
    public async Task ListarMinhasMotos_DeveRetornarOk_ComListaDeMotos()
    {
        // Arrange
        var userId = "user-id-123";
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        var motos = new List<MotoResponse>
        {
            new MotoResponse(1, "ABC1234", "CHASSI1", 1, "CB 500F", "Honda", 1, null, null, null, 2023, "Vermelha", 0, DateTime.Now, "Linha", "100cc"),
            new MotoResponse(2, "XYZ9999", "CHASSI2", 1, "CG 160", "Honda", 1, null, null, null, 2022, "Azul", 5000, DateTime.Now.AddYears(-1), "Linha", "160cc")
        };

        _motoServiceMock.Setup(s => s.ListarMotosClienteAsync(userId))
            .ReturnsAsync(motos);

        // Act
        var result = await _controller.ListarMinhasMotos();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(motos, okResult.Value);
    }

    [Fact]
    public async Task AtualizarMoto_DeveRetornarOk_QuandoDadosValidos()
    {
        // Arrange
        var request = new MotoUpdateRequest("XYZ-9999", "Azul", 0);
        var response = new MotoResponse(1, "XYZ9999", "CHASSI12345678901", 1, "CB 500F", "Honda", 1, null, null, null, 2023, "Azul", 0, DateTime.Now, "Linha", "100cc");

        var userId = "user-id-123";
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        _motoServiceMock.Setup(s => s.AtualizarMotoAsync(1, request, userId))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.AtualizarMoto(1, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task AtualizarMoto_DeveRetornarUnauthorized_QuandoSemUserId()
    {
        // Arrange
        var request = new MotoUpdateRequest("XYZ-9999", "Azul", 0);
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity()); // Sem NameIdentifier Claim

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        // Act
        var result = await _controller.AtualizarMoto(1, request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task ObterMoto_DeveRetornarOk_QuandoMotoExisteEPertenceAoCliente()
    {
        // Arrange
        var response = new MotoResponse(1, "ABC1234", "CHASSI12345678901", 1, "CB 500F", "Honda", 1, null, null, null, 2023, "Vermelha", 0, DateTime.Now, "Linha", "100cc");
        var userId = "user-id-123";
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        _motoServiceMock.Setup(s => s.GetByIdAsync(1, userId))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ObterMoto(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task ObterMoto_DeveRetornarUnauthorized_QuandoSemUserId()
    {
        // Arrange
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity()); // Sem NameIdentifier Claim

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        // Act
        var result = await _controller.ObterMoto(1);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task ObterMoto_DeveLancarNotFoundException_QuandoServiceLancaNotFound()
    {
        // Arrange
        var userId = "user-id-123";
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        _motoServiceMock.Setup(s => s.GetByIdAsync(1, userId))
            .ThrowsAsync(new NotFoundException("Moto não encontrada."));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.ObterMoto(1));
    }

    [Fact]
    public async Task InativarMoto_DeveRetornarNoContent_QuandoInativadaComSucesso()
    {
        // Arrange
        var userId = "user-id-123";
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        _motoServiceMock.Setup(s => s.InativarMotoAsync(1, userId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.InativarMoto(1);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task InativarMoto_DeveRetornarUnauthorized_QuandoSemUserId()
    {
        // Arrange
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity()); // Sem NameIdentifier Claim

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        // Act
        var result = await _controller.InativarMoto(1);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task InativarMoto_DeveLancarNotFoundException_QuandoMotoInexistente()
    {
        // Arrange
        var userId = "user-id-123";
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        _motoServiceMock.Setup(s => s.InativarMotoAsync(1, userId))
            .ThrowsAsync(new NotFoundException("Moto não encontrada."));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.InativarMoto(1));
    }

    [Fact]
    public async Task InativarMoto_DeveLancarBusinessRuleException_QuandoMotoPossuiAgendamentosPendentes()
    {
        // Arrange
        var userId = "user-id-123";
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userPrincipal }
        };

        _motoServiceMock.Setup(s => s.InativarMotoAsync(1, userId))
            .ThrowsAsync(new BusinessRuleException("Não é possível inativar uma moto com agendamentos pendentes."));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => _controller.InativarMoto(1));
    }
}
