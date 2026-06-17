using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MotoRevApi.Controller;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Exceptions;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Controllers;

public class ModeloMotoControllerTests
{
    private readonly Mock<ModeloMotoService> _modeloMotoServiceMock;
    private readonly ModeloMotoController _controller;

    public ModeloMotoControllerTests()
    {
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
        var request = new ModeloMotoRequest("Ninja", "Kawasaki", 1, "400cc", 2023);
        var response = new ModeloMotoResponse(1, "Ninja", "Kawasaki", 1, "400cc", 2023, true);

        _modeloMotoServiceMock.Setup(s => s.CadastrarModeloMoto(request)).Returns(response);
        _controller.ModelState.Clear();

        var result = _controller.AdicionarModeloMoto(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(response, createdResult.Value);
    }

    [Fact]
    public void AdicionarModeloMoto_DeveRetornarBadRequest_QuandoModelStateInvalido()
    {
        _controller.ModelState.AddModelError("NomeModelo", "Obrigatório");
        var request = new ModeloMotoRequest("", "Kawasaki", 1, null, null);

        var result = _controller.AdicionarModeloMoto(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void AdicionarModeloMoto_DeveLancarDuplicateDataException_QuandoNomeJaExiste()
    {
        var request = new ModeloMotoRequest("Ninja", "Kawasaki", 1, "400cc", 2023);
        _modeloMotoServiceMock.Setup(s => s.CadastrarModeloMoto(request))
            .Throws(new DuplicateDataException("Já existe um modelo de moto ativo com este nome."));

        Assert.Throws<DuplicateDataException>(() => _controller.AdicionarModeloMoto(request));
    }

    [Fact]
    public void AdicionarModeloMoto_DeveLancarNotFoundException_QuandoLinhaInativa()
    {
        var request = new ModeloMotoRequest("Ninja", "Kawasaki", 99, "400cc", 2023);
        _modeloMotoServiceMock.Setup(s => s.CadastrarModeloMoto(request))
            .Throws(new NotFoundException("Linha informada não encontrada."));

        Assert.Throws<NotFoundException>(() => _controller.AdicionarModeloMoto(request));
    }

    [Fact]
    public void ObterModeloMoto_DeveRetornarOk_QuandoModeloExiste()
    {
        var response = new ModeloMotoResponse(1, "Ninja", "Kawasaki", 1, "400cc", 2023, true);
        _modeloMotoServiceMock.Setup(s => s.ObterModeloMoto(1)).Returns(response);

        var result = _controller.ObterModeloMoto(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public void ObterModeloMoto_DeveLancarNotFoundException_QuandoModeloNaoExiste()
    {
        _modeloMotoServiceMock
            .Setup(s => s.ObterModeloMoto(1))
            .Throws(new NotFoundException("Modelo de moto não encontrado."));

        Assert.Throws<NotFoundException>(() => _controller.ObterModeloMoto(1));
    }

    [Fact]
    public void ObterModelosMotos_DeveRetornarListaDeAtivosPorPadrao()
    {
        var list = new List<ModeloMotoResponse>
        {
            new(1, "Ninja", "Kawasaki", 1, "400cc", 2023, true)
        };
        _modeloMotoServiceMock.Setup(s => s.ListarModelosMotos(true)).Returns(list);

        var result = _controller.ObterModelosMotos();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(list, okResult.Value);
        _modeloMotoServiceMock.Verify(s => s.ListarModelosMotos(true), Times.Once);
    }

    [Fact]
    public void ObterCatalogoModelosMotos_DeveRetornarLista()
    {
        var list = new List<ModeloMotoResponse>
        {
            new(1, "Ninja", "Kawasaki", 1, "400cc", 2023, true),
            new(2, "R1", "Yamaha", 1, "1000cc", 2024, false)
        };
        _modeloMotoServiceMock.Setup(s => s.ListarCatalogoModelosMotos(null)).Returns(list);

        var result = _controller.ObterCatalogoModelosMotos(null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(list, okResult.Value);
    }

    [Fact]
    public void ObterModelosDisponiveisParaCadastro_DeveRetornarLista()
    {
        var list = new List<ModeloMotoResponse>
        {
            new(1, "Ninja", "Kawasaki", 1, "400cc", 2023, true)
        };
        _modeloMotoServiceMock.Setup(s => s.ListarModelosDisponiveisParaCadastro()).Returns(list);

        var result = _controller.ObterModelosDisponiveisParaCadastro();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(list, okResult.Value);
        _modeloMotoServiceMock.Verify(s => s.ListarModelosDisponiveisParaCadastro(), Times.Once);
    }

    [Fact]
    public void ObterCatalogoModelosMotos_DeveRepasseFiltroStatus()
    {
        var list = new List<ModeloMotoResponse>
        {
            new(2, "R1", "Yamaha", 1, "1000cc", 2024, false)
        };
        _modeloMotoServiceMock.Setup(s => s.ListarCatalogoModelosMotos(false)).Returns(list);

        var result = _controller.ObterCatalogoModelosMotos(false);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(list, okResult.Value);
        _modeloMotoServiceMock.Verify(s => s.ListarCatalogoModelosMotos(false), Times.Once);
    }

    [Fact]
    public void AtualizarModeloMoto_DeveRetornarOk_QuandoSucesso()
    {
        var request = new ModeloMotoRequest("Ninja ZX-6R", "Kawasaki", 1, "600cc", 2024);
        var response = new ModeloMotoResponse(1, "Ninja ZX-6R", "Kawasaki", 1, "600cc", 2024, true);

        _modeloMotoServiceMock.Setup(s => s.AtualizarModeloMoto(1, request)).Returns(response);
        _controller.ModelState.Clear();

        var result = _controller.AtualizarModeloMoto(1, request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public void AtualizarModeloMoto_DeveLancarNotFoundException_QuandoModeloNaoExiste()
    {
        var request = new ModeloMotoRequest("Ninja", "Kawasaki", 1, null, null);
        _modeloMotoServiceMock
            .Setup(s => s.AtualizarModeloMoto(1, request))
            .Throws(new NotFoundException("Modelo de moto não encontrado."));

        Assert.Throws<NotFoundException>(() => _controller.AtualizarModeloMoto(1, request));
    }

    [Fact]
    public void AtualizarModeloMoto_DeveRetornarBadRequest_QuandoModelStateInvalido()
    {
        _controller.ModelState.AddModelError("NomeModelo", "Obrigatório");
        var request = new ModeloMotoRequest("", "Kawasaki", 1, null, null);

        var result = _controller.AtualizarModeloMoto(1, request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void AtualizarModeloMoto_DeveLancarDuplicateDataException_QuandoNovoNomeJaExiste()
    {
        var request = new ModeloMotoRequest("Ninja", "Kawasaki", 1, "400cc", 2023);
        _modeloMotoServiceMock.Setup(s => s.AtualizarModeloMoto(1, request))
            .Throws(new DuplicateDataException("Já existe outro modelo de moto ativo com este nome."));

        Assert.Throws<DuplicateDataException>(() => _controller.AtualizarModeloMoto(1, request));
    }

    [Fact]
    public void AlternarStatusModeloMoto_DeveRetornarOk_QuandoSucesso()
    {
        var response = new ModeloMotoResponse(1, "Ninja", "Kawasaki", 1, "400cc", 2023, false);
        _modeloMotoServiceMock.Setup(s => s.AlternarStatus(1)).Returns(response);

        var result = _controller.AlternarStatusModeloMoto(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public void AlternarStatusModeloMoto_DeveLancarDuplicateDataException_QuandoReativarComNomeJaExistente()
    {
        _modeloMotoServiceMock.Setup(s => s.AlternarStatus(1))
            .Throws(new DuplicateDataException("Já existe outro modelo de moto ativo com este nome."));

        Assert.Throws<DuplicateDataException>(() => _controller.AlternarStatusModeloMoto(1));
    }
}
