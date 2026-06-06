using Microsoft.AspNetCore.Mvc;
using Moq;
using MotoRevApi.Controller;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Exceptions;
using MotoRevApi.Services;
using System.Collections.Generic;
using Xunit;

namespace MotoRevApi.Tests.Controllers;

public class LinhaControllerTests
{
    private readonly Mock<LinhaService> _linhaServiceMock;
    private readonly LinhaController _controller;

    public LinhaControllerTests()
    {
        _linhaServiceMock = new Mock<LinhaService>();
        _controller = new LinhaController(_linhaServiceMock.Object);
    }

    [Fact]
    public void CriarLinha_DeveRetornarCreatedAtAction_QuandoValido()
    {
        // Arrange
        var request = new LinhaRequest("Linha Esportiva", "Desc");
        var response = new LinhaResponse(1, "Linha Esportiva", "Desc", true);

        _linhaServiceMock.Setup(s => s.CadastrarLinha(request))
            .Returns(response);

        // Act
        var result = _controller.CriarLinha(request);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.ObterLinha), createdAtActionResult.ActionName);
        var returnedLinha = Assert.IsType<LinhaResponse>(createdAtActionResult.Value);
        Assert.Equal(1, returnedLinha.Id);
    }

    [Fact]
    public void ListarLinhas_DeveRetornarOk_ComListaDeLinhas()
    {
        // Arrange
        var linhas = new List<LinhaResponse>
        {
            new List<LinhaResponse> { new LinhaResponse(1, "Linha Esportiva", "Desc", true) }[0]
        };

        _linhaServiceMock.Setup(s => s.ListarLinhas(true))
            .Returns(linhas);

        // Act
        var result = _controller.ListarLinhas(true);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedLinhas = Assert.IsAssignableFrom<IEnumerable<LinhaResponse>>(okResult.Value);
        Assert.Single(returnedLinhas);
    }

    [Fact]
    public void ObterLinha_DeveRetornarOk_QuandoLinhaExiste()
    {
        // Arrange
        var id = 1;
        var response = new LinhaResponse(id, "Linha Esportiva", "Desc", true);

        _linhaServiceMock.Setup(s => s.ObterLinha(id))
            .Returns(response);

        // Act
        var result = _controller.ObterLinha(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedLinha = Assert.IsType<LinhaResponse>(okResult.Value);
        Assert.Equal(id, returnedLinha.Id);
    }

    [Fact]
    public void ObterLinha_DeveLancarNotFoundException_QuandoLinhaNaoExiste()
    {
        // Arrange
        var id = 999;
        _linhaServiceMock.Setup(s => s.ObterLinha(id))
            .Throws(new NotFoundException("Linha não encontrada."));

        // Act & Assert
        var exception = Assert.Throws<NotFoundException>(() => _controller.ObterLinha(id));
        Assert.Equal("Linha não encontrada.", exception.Message);
    }

    [Fact]
    public void AtualizarLinha_DeveRetornarOk_QuandoValido()
    {
        // Arrange
        var id = 1;
        var request = new LinhaRequest("Linha Custom", "Nova Desc");
        var response = new LinhaResponse(id, "Linha Custom", "Nova Desc", true);

        _linhaServiceMock.Setup(s => s.AlternarStatus(id))
            .Returns(response); // We setup setup just in case but let's setup update
        _linhaServiceMock.Setup(s => s.AtualizarLinha(id, request))
            .Returns(response);

        // Act
        var result = _controller.AtualizarLinha(id, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedLinha = Assert.IsType<LinhaResponse>(okResult.Value);
        Assert.Equal("Linha Custom", returnedLinha.Nome);
    }

    [Fact]
    public void InativarLinha_DeveRetornarOk_QuandoLinhaExiste()
    {
        // Arrange
        var id = 1;
        var response = new LinhaResponse(id, "Linha Custom", "Desc", false);

        _linhaServiceMock.Setup(s => s.InativarLinha(id))
            .Returns(response);

        // Act
        var result = _controller.InativarLinha(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedLinha = Assert.IsType<LinhaResponse>(okResult.Value);
        Assert.False(returnedLinha.Ativo);
    }

    [Fact]
    public void AlternarStatus_DeveRetornarOk_QuandoLinhaExiste()
    {
        // Arrange
        var id = 1;
        var response = new LinhaResponse(id, "Linha Custom", "Desc", false);

        _linhaServiceMock.Setup(s => s.AlternarStatus(id))
            .Returns(response);

        // Act
        var result = _controller.AlternarStatus(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedLinha = Assert.IsType<LinhaResponse>(okResult.Value);
        Assert.False(returnedLinha.Ativo);
    }
}
