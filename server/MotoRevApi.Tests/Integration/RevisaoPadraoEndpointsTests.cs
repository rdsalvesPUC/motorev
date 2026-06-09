using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MotoRevApi.Authorization;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using Xunit;

namespace MotoRevApi.Tests.Integration;

public class RevisaoPadraoEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly string _token;

    public RevisaoPadraoEndpointsTests()
    {
        _token = TestJwtTokenFactory.CreateToken(Roles.Concessionaria);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public async Task Get_DeveRetornarOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        // Act
        var response = await client.GetAsync("/api/revisao-padrao");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoIdNaoExiste()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        // Act
        var response = await client.GetAsync("/api/revisao-padrao/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
