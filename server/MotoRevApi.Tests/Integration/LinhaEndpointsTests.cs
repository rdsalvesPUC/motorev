using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MotoRevApi.Authorization;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Model;
using Xunit;

namespace MotoRevApi.Tests.Integration;

public class LinhaEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly string _token;

    public LinhaEndpointsTests()
    {
        _token = TestJwtTokenFactory.CreateToken(Roles.Concessionaria);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public async Task ListarLinhas_DeveRetornarOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        // Act
        var response = await client.GetAsync("/api/Linha");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CriarLinha_DeveRetornarCreated_QuandoDadosValidos()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var request = new LinhaRequest("Linha Teste Integration", "Descricao Teste");

        // Act
        var response = await client.PostAsJsonAsync("/api/Linha", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var linha = await response.Content.ReadFromJsonAsync<LinhaResponse>();
        Assert.NotNull(linha);
        Assert.Equal(request.Nome, linha.Nome);
    }

    [Fact]
    public async Task ObterLinha_DeveRetornarNotFound_QuandoIdNaoExiste()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        // Act
        var response = await client.GetAsync("/api/Linha/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
