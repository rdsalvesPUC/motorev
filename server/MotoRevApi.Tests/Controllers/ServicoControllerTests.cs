using Microsoft.AspNetCore.Mvc;
using Moq;
using MotoRevApi.Controller;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
using MotoRevApi.Exceptions;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Controllers;

public class ServicoControllerTests
{
    private readonly Mock<ServicoService> _servicoServiceMock;
    private readonly ServicoController _controller;

    public ServicoControllerTests()
    {
        _servicoServiceMock = new Mock<ServicoService>();
        _controller = new ServicoController(_servicoServiceMock.Object);
    }

    [Fact]
    public async Task Create_DeveRetornarCreatedAtAction_QuandoDadosValidos()
    {
        // Arrange
        var request = new ServicoRequest("COD001", "Troca de Óleo", "Desc", CategoriaServico.Troca, 30, 100);
        var response = new ServicoResponse(1, "COD001", "Troca de Óleo", "Desc", CategoriaServico.Troca, 30, 100, true);

        _servicoServiceMock.Setup(s => s.CreateAsync(request))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetById), createdAtActionResult.ActionName);
        var returnedServico = Assert.IsType<ServicoResponse>(createdAtActionResult.Value);
        Assert.Equal(1, returnedServico.Id);
    }

    [Fact]
    public async Task GetAll_DeveRetornarOk_ComListaDeServicos()
    {
        // Arrange
        var categoria = CategoriaServico.Troca;
        var servicos = new List<ServicoResponse>
        {
            new ServicoResponse(1, "COD001", "Troca de Óleo", "Desc", CategoriaServico.Troca, 30, 100, true)
        };

        _servicoServiceMock.Setup(s => s.GetAllAsync(categoria))
            .ReturnsAsync(servicos);

        // Act
        var result = await _controller.GetAll(categoria);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedServicos = Assert.IsAssignableFrom<IEnumerable<ServicoResponse>>(okResult.Value);
        Assert.Single(returnedServicos);
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoServicoExiste()
    {
        // Arrange
        var servicoId = 1;
        var servicoResponse = new ServicoResponse(
            servicoId,
            "COD001",
            "Troca de Óleo",
            "Desc",
            CategoriaServico.Troca,
            30,
            100,
            true
        );

        _servicoServiceMock.Setup(s => s.GetByIdAsync(servicoId))
            .ReturnsAsync(servicoResponse);

        // Act
        var result = await _controller.GetById(servicoId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedServico = Assert.IsType<ServicoResponse>(okResult.Value);
        Assert.Equal(servicoId, returnedServico.Id);
    }

    [Fact]
    public async Task GetById_DeveLancarNotFoundException_QuandoServicoNaoExiste()
    {
        // Arrange
        var servicoId = 999;
        _servicoServiceMock.Setup(s => s.GetByIdAsync(servicoId))
            .ThrowsAsync(new NotFoundException("Serviço não encontrado."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _controller.GetById(servicoId));
            
        Assert.Equal("Serviço não encontrado.", exception.Message);
    }

    [Fact]
    public async Task Update_DeveRetornarOk_QuandoDadosValidos()
    {
        // Arrange
        var servicoId = 1;
        var request = new ServicoUpdateRequest("COD002", "Troca de Pneu", "Desc Pneu", CategoriaServico.Troca, 60, 200);
        var response = new ServicoResponse(servicoId, "COD002", "Troca de Pneu", "Desc Pneu", CategoriaServico.Troca, 60, 200, true);

        _servicoServiceMock.Setup(s => s.UpdateAsync(servicoId, request))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Update(servicoId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedServico = Assert.IsType<ServicoResponse>(okResult.Value);
        Assert.Equal("Troca de Pneu", returnedServico.Nome);
    }

    [Fact]
    public async Task Inactivate_DeveRetornarNoContent_QuandoServicoExiste()
    {
        // Arrange
        var servicoId = 1;
        _servicoServiceMock.Setup(s => s.InactivateAsync(servicoId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Inactivate(servicoId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}
