using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MotoRevApi.Authorization;
using MotoRevApi.Data;
using MotoRevApi.Dto.Response;
using MotoRevApi.Model;
using Xunit;

namespace MotoRevApi.Tests.Integration;

public class ClienteEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();

    [Fact]
    public async Task GetMe_DeveRetornarPerfil_QuandoClienteEstiverAutenticado()
    {
        // Arrange
        var userId = "user-cliente-perfil";
        SeedCliente(userId, "João Silva", "joao@email.com", "52998224725", "11999990000");
        var client = CreateClient(Roles.Cliente, userId);

        // Act
        var response = await client.GetAsync("/api/Cliente/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var perfil = await response.Content.ReadFromJsonAsync<ClientePerfilResponse>();
        Assert.NotNull(perfil);
        Assert.Equal("João Silva", perfil.Nome);
        Assert.Equal("joao@email.com", perfil.Email);
        Assert.Equal("52998224725", perfil.Cpf);
        Assert.Equal("11999990000", perfil.Telefone);
    }

    [Fact]
    public async Task GetMe_DeveRetornarUnauthorized_QuandoNaoEnviarToken()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Cliente/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_DeveRetornarForbidden_QuandoUsuarioNaoForCliente()
    {
        // Arrange
        var client = CreateClient(Roles.Concessionaria, "user-concessionaria");

        // Act
        var response = await client.GetAsync("/api/Cliente/me");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Register_DevePersistirCpfETelefone_QuandoPayloadForValido()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new
        {
            nome = "João Silva",
            email = "joao.cadastro@email.com",
            password = "Password123!",
            cpf = "529.982.247-25",
            telefone = "(11) 99999-0000"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Cliente", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var cliente = context.Clientes.Single();
        var usuario = context.Users.Single(u => u.Id == cliente.UsuarioId);
        Assert.Equal("João Silva", cliente.Nome);
        Assert.Equal("52998224725", cliente.Cpf);
        Assert.Equal("11999990000", usuario.PhoneNumber);
        Assert.Equal("joao.cadastro@email.com", usuario.Email);
    }

    [Fact]
    public async Task Register_DeveRetornarConflict_QuandoCpfJaExistir()
    {
        // Arrange
        SeedCliente("user-cliente-existente", "Cliente Existente", "existente@email.com", "52998224725", "11999990000");
        var client = _factory.CreateClient();
        var request = new
        {
            nome = "Outro Cliente",
            email = "outro@email.com",
            password = "Password123!",
            cpf = "529.982.247-25",
            telefone = "(11) 98888-7777"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Cliente", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    private HttpClient CreateClient(string role, string userId)
    {
        var client = _factory.CreateClient();
        var token = TestJwtTokenFactory.CreateToken(role, userId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private void SeedCliente(string userId, string nome, string email, string cpf, string telefone)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var usuario = new Usuario
        {
            Id = userId,
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PhoneNumber = telefone
        };

        context.Users.Add(usuario);
        context.Clientes.Add(new Cliente
        {
            UsuarioId = userId,
            Nome = nome,
            Cpf = cpf
        });
        context.SaveChanges();
    }
}
