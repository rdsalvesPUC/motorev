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
        SeedClienteComEndereco(userId, "João Silva", "joao@email.com", "52998224725", "11999990000");
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
        Assert.NotNull(perfil.Endereco);
        Assert.Equal("04538132", perfil.Endereco.Cep);
        Assert.Equal("Rua Funchal", perfil.Endereco.Logradouro);
    }

    [Fact]
    public async Task GetMe_DeveRetornarEnderecoNulo_QuandoClienteNaoPossuirEndereco()
    {
        // Arrange
        var userId = "user-cliente-sem-endereco";
        SeedCliente(userId, "João Silva", "joao.sem.endereco@email.com", "52998224725", "11999990000");
        var client = CreateClient(Roles.Cliente, userId);

        // Act
        var response = await client.GetAsync("/api/Cliente/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var perfil = await response.Content.ReadFromJsonAsync<ClientePerfilResponse>();
        Assert.NotNull(perfil);
        Assert.Null(perfil.Endereco);
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

    [Fact]
    public async Task UpdateEndereco_DeveCriarEnderecoFormal_QuandoPayloadForValido()
    {
        // Arrange
        var userId = "user-cliente-criar-endereco";
        SeedCliente(userId, "João Silva", "joao.criar.endereco@email.com", "52998224725", "11999990000");
        var client = CreateClient(Roles.Cliente, userId);
        var request = new
        {
            cep = "04538-132",
            logradouro = "Rua Funchal",
            numero = "418",
            complemento = "Apto 52",
            bairro = "Vila Olímpia",
            cidade = "São Paulo",
            uf = "sp"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/Cliente/me/endereco", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var perfil = await response.Content.ReadFromJsonAsync<ClientePerfilResponse>();
        Assert.NotNull(perfil?.Endereco);
        Assert.Equal("04538132", perfil.Endereco.Cep);
        Assert.Equal("SP", perfil.Endereco.Uf);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var cliente = context.Clientes.Single(c => c.UsuarioId == userId);
        Assert.NotNull(cliente.EnderecoId);
        Assert.Single(context.Enderecos);
    }

    [Fact]
    public async Task UpdateEndereco_DeveAtualizarEnderecoFormal_QuandoClienteJaPossuirEndereco()
    {
        // Arrange
        var userId = "user-cliente-atualizar-endereco";
        SeedClienteComEndereco(userId, "João Silva", "joao.atualizar.endereco@email.com", "52998224725", "11999990000");
        int enderecoIdOriginal;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            enderecoIdOriginal = context.Enderecos.Single().Id;
        }

        var client = CreateClient(Roles.Cliente, userId);
        var request = new
        {
            cep = "01310-100",
            logradouro = "Avenida Paulista",
            numero = "1000",
            complemento = (string?)null,
            bairro = "Bela Vista",
            cidade = "São Paulo",
            uf = "SP"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/Cliente/me/endereco", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var assertScope = _factory.Services.CreateScope();
        var assertContext = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var endereco = assertContext.Enderecos.Single();
        Assert.Equal(enderecoIdOriginal, endereco.Id);
        Assert.Equal("01310100", endereco.Cep);
        Assert.Equal("Avenida Paulista", endereco.Logradouro);
    }

    [Fact]
    public async Task UpdateEndereco_DeveRetornarBadRequest_QuandoPayloadForInvalido()
    {
        // Arrange
        var userId = "user-cliente-endereco-invalido";
        SeedCliente(userId, "João Silva", "joao.endereco.invalido@email.com", "52998224725", "11999990000");
        var client = CreateClient(Roles.Cliente, userId);
        var request = new
        {
            cep = "123",
            logradouro = "Rua Funchal",
            numero = "418",
            bairro = "Vila Olímpia",
            cidade = "São Paulo",
            uf = "SP"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/Cliente/me/endereco", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

    private void SeedClienteComEndereco(string userId, string nome, string email, string cpf, string telefone)
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
            Cpf = cpf,
            Endereco = new Endereco
            {
                Cep = "04538132",
                Logradouro = "Rua Funchal",
                Numero = "418",
                Bairro = "Vila Olímpia",
                Cidade = "São Paulo",
                Uf = "SP"
            }
        });
        context.SaveChanges();
    }
}
