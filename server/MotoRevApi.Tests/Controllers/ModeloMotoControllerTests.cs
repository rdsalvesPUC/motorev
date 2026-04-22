using Microsoft.AspNetCore.Mvc;
using Moq;
using MotoRevApi.Controller;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;
using Xunit;
using MotoRevApi.Data;
using Microsoft.EntityFrameworkCore;

namespace MotoRevApi.Tests.Controllers;

public class ModeloMotoControllerTests
{
    private readonly Mock<ModeloMotoService> _modeloMotoServiceMock;
    private readonly ModeloMotoController _controller;

    public ModeloMotoControllerTests()
    {
        // Precisamos mockar o DbContext para poder mockar o service que não possui interface
        var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(dbContextOptions);
        
        _modeloMotoServiceMock = new Mock<ModeloMotoService>(context);
        _controller = new ModeloMotoController(_modeloMotoServiceMock.Object);
    }

    [Fact]
    public void AdicionarModeloMoto_DeveRetornarCreatedAtAction_QuandoSucesso()
    {
        // Arrange
        var request = new ModeloMotoRequest("Ninja", "Kawasaki", "Esportiva");
        var response = new ModeloMotoResponse(1, "Ninja", "Kawasaki", "Esportiva", true);

        _modeloMotoServiceMock.Setup(s => s.CadastrarModeloMoto(request)).Returns(response);

        // Act
        var result = _controller.AdicionarModeloMoto(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(response, createdResult.Value);
    }

    [Fact]
    public void AdicionarModeloMoto_DeveRetornarBadRequest_QuandoModelStateInvalido()
    {
        // Arrange
        _controller.ModelState.AddModelError("NomeModelo", "Obrigatório");
        var request = new ModeloMotoRequest("", "Kawasaki", "Esportiva");

        // Act
        var result = _controller.AdicionarModeloMoto(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void ObterModeloMoto_DeveRetornarOk_QuandoModeloExiste()
    {
        // Arrange
        var response = new ModeloMotoResponse(1, "Ninja", "Kawasaki", "Esportiva", true);
        _modeloMotoServiceMock.Setup(s => s.ObterModeloMoto(1)).Returns(response);

        // Act
        var result = _controller.ObterModeloMoto(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public void ObterModeloMoto_DeveRetornarNotFound_QuandoModeloNaoExiste()
    {
        // Arrange
        _modeloMotoServiceMock.Setup(s => s.ObterModeloMoto(1)).Returns((ModeloMotoResponse)null);

        // Act
        var result = _controller.ObterModeloMoto(1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public void ObterModelosMotos_DeveRetornarLista()
    {
        // Arrange
        var list = new List<ModeloMotoResponse> 
        { 
            new ModeloMotoResponse(1, "Ninja", "Kawasaki", "Esportiva", true) 
        };
        _modeloMotoServiceMock.Setup(s => s.ListarModelosMotos()).Returns(list);

        // Act
        var result = _controller.ObterModelosMotos();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(list, okResult.Value);
    }

    [Fact]
    public void AtualizarModeloMoto_DeveRetornarOk_QuandoSucesso()
    {
        // Arrange
        var request = new ModeloMotoRequest("Ninja ZX-6R", "Kawasaki", "Esportiva");
        var response = new ModeloMotoResponse(1, "Ninja ZX-6R", "Kawasaki", "Esportiva", true);

        _modeloMotoServiceMock.Setup(s => s.AtualizarModeloMoto(1, request)).Returns(response);

        // Act
        var result = _controller.AtualizarModeloMoto(1, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public void AtualizarModeloMoto_DeveRetornarNotFound_QuandoModeloNaoExiste()
    {
        // Arrange
        var request = new ModeloMotoRequest("Ninja", "Kawasaki", "Esportiva");
        _modeloMotoServiceMock.Setup(s => s.AtualizarModeloMoto(1, request)).Returns((ModeloMotoResponse)null);

        // Act
        var result = _controller.AtualizarModeloMoto(1, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public void AlternarStatusModeloMoto_DeveRetornarOk_QuandoSucesso()
    {
        // Arrange
        var response = new ModeloMotoResponse(1, "Ninja", "Kawasaki", "Esportiva", false);
        _modeloMotoServiceMock.Setup(s => s.AlternarStatus(1)).Returns(response);

        // Act
        var result = _controller.AlternarStatusModeloMoto(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public void AlternarStatusModeloMoto_DeveRetornarNotFound_QuandoModeloNaoExiste()
    {
        // Arrange
        _modeloMotoServiceMock.Setup(s => s.AlternarStatus(1)).Returns((ModeloMotoResponse)null);

        // Act
        var result = _controller.AlternarStatusModeloMoto(1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }
}
