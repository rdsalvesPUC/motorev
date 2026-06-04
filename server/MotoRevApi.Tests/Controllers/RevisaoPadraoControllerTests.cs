using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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

public class RevisaoPadraoControllerTests
{
    private readonly Mock<RevisaoPadraoService> _revisaoServiceMock;
    private readonly Mock<ConcessionariaService> _concessionariaServiceMock;
    private readonly RevisaoPadraoController _controller;

    public RevisaoPadraoControllerTests()
    {
        _revisaoServiceMock = new Mock<RevisaoPadraoService>();
        _concessionariaServiceMock = new Mock<ConcessionariaService>();
        _controller = new RevisaoPadraoController(_revisaoServiceMock.Object, _concessionariaServiceMock.Object);
        
        // Mock do usuário padrão para a maioria dos testes
        var userId = "user-123";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        var concessionaria = new ConcessionariaResponse { Id = 1, Nome = "Conc 1", Cnpj = "123" };
        _concessionariaServiceMock.Setup(s => s.GetByUserIdAsync(userId)).ReturnsAsync(concessionaria);
    }

    [Fact]
    public async Task CadastrarRevisao_DeveRetornarCreated_QuandoSucesso()
    {
        // Arrange
        var request = new RevisaoPadraoRequest("Revisão 1000km", 1, 1, new List<int> { 1, 2 });
        var response = new RevisaoPadraoResponse(1, "Revisão 1000km", 1, 1, "Ninja", new List<ServicoResponse>());
        
        _revisaoServiceMock.Setup(s => s.CadastrarRevisaoAsync(request, 1)).ReturnsAsync(response);

        // Act
        var result = await _controller.CadastrarRevisao(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(response, createdResult.Value);
    }

    [Fact]
    public async Task CadastrarRevisao_DeveLancarNotFoundException_QuandoEntidadeNaoExiste()
    {
        // Arrange
        var request = new RevisaoPadraoRequest("Revisão com erro", 99, 1, new List<int> { 99 });
        _revisaoServiceMock.Setup(s => s.CadastrarRevisaoAsync(request, 1))
            .ThrowsAsync(new NotFoundException("Modelo de moto não encontrado."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _controller.CadastrarRevisao(request));
        Assert.Equal("Modelo de moto não encontrado.", exception.Message);
    }

    [Fact]
    public async Task CadastrarRevisao_DeveLancarDuplicateDataException_QuandoOrdemDuplicada()
    {
        // Arrange
        var request = new RevisaoPadraoRequest("Revisão duplicada", 1, 1, new List<int> { 1 });
        _revisaoServiceMock.Setup(s => s.CadastrarRevisaoAsync(request, 1))
            .ThrowsAsync(new DuplicateDataException("Ordem de revisão já existe."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DuplicateDataException>(() => _controller.CadastrarRevisao(request));
        Assert.Equal("Ordem de revisão já existe.", exception.Message);
    }

    [Fact]
    public async Task CadastrarRevisao_DeveRetornarUnauthorized_QuandoSemUsuario()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // Usuário sem claims
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        var request = new RevisaoPadraoRequest("Revisão 1000km", 1, 1, new List<int> { 1 });

        // Act
        var result = await _controller.CadastrarRevisao(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Atributos_Classe_DeveTerRoleConcessionaria()
    {
        var attribute = typeof(RevisaoPadraoController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(attribute);
        Assert.Equal(Authorization.Roles.Concessionaria, attribute.Roles);
    }
}
