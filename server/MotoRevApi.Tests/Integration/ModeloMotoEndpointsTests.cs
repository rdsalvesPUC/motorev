using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MotoRevApi.Authorization;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using Xunit;

namespace MotoRevApi.Tests.Integration;

public class ModeloMotoEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly string _token;

    public ModeloMotoEndpointsTests()
    {
        _token = TestJwtTokenFactory.CreateToken(Roles.Concessionaria);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public async Task ObterModelosMotos_DeveRetornarOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        // Act
        var response = await client.GetAsync("/api/ModeloMoto/listar");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ObterModeloMoto_DeveRetornarNotFound_QuandoIdNaoExiste()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        // Act
        var response = await client.GetAsync("/api/ModeloMoto/id/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ObterCatalogoModelosMotos_DeveRetornarOk_ParaConcessionaria()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        // Act
        var response = await client.GetAsync("/api/ModeloMoto/catalogo");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
