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
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Integration;

public class AuthEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();

    public void Dispose()
    {
        _factory.Dispose();
    }

    private async Task<Usuario> SeedUserAsync(string email, string password, string role, string nome)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }

        var user = new Usuario
        {
            UserName = email,
            Email = email,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(user, role);

        if (role == Roles.Cliente)
        {
            context.Clientes.Add(new Cliente
            {
                UsuarioId = user.Id,
                Nome = nome,
                Cpf = "12345678901"
            });
        }
        else if (role == Roles.Concessionaria)
        {
            var concessionaria = new Concessionaria
            {
                UsuarioId = user.Id,
                Nome = nome,
                Cnpj = "12345678000199",
                Telefone = "1234567890"
            };
            context.Concessionarias.Add(concessionaria);
            concessionaria.Lojas.Add(new Loja
            {
                Nome = nome,
                Tipo = "Matriz",
                Cnpj = "12345678000199",
                Telefone = "1234567890",
                Cep = "12345678",
                Logradouro = "Rua Teste",
                Numero = "123",
                Bairro = "Bairro Teste",
                Cidade = "Cidade Teste",
                Uf = "SP",
                Concessionaria = concessionaria
            });
        }

        await context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task Login_DeveRetornarTokens_QuandoCredenciaisForemValidas()
    {
        // Arrange
        var email = "test.login@email.com";
        var password = "Password123!";
        var nome = "Cliente Teste";
        await SeedUserAsync(email, password, Roles.Cliente, nome);

        var client = _factory.CreateClient();
        var request = new LoginRequest(email, password);

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        Assert.NotEmpty(loginResponse.Token);
        Assert.NotEmpty(loginResponse.RefreshToken);
        Assert.Equal(Roles.Cliente, loginResponse.Perfil);
        Assert.Equal(nome, loginResponse.Usuario.Nome);
    }

    [Fact]
    public async Task Login_DeveRetornarNotFound_QuandoEmailNaoExistir()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new LoginRequest("naoexiste@email.com", "Password123!");

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Login_DeveRetornarNotFound_QuandoSenhaForIncorreta()
    {
        // Arrange
        var email = "test.wrongpass@email.com";
        var password = "Password123!";
        await SeedUserAsync(email, password, Roles.Cliente, "Cliente Teste");

        var client = _factory.CreateClient();
        var request = new LoginRequest(email, "SenhaErrada1!");

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_DeveRetornarNovosTokens_QuandoTokensForemValidos()
    {
        // Arrange
        var email = "test.refresh@email.com";
        var password = "Password123!";
        var user = await SeedUserAsync(email, password, Roles.Cliente, "Cliente Teste");

        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TokenService>();
        var hashService = scope.ServiceProvider.GetRequiredService<HashService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

        // Gerar tokens iniciais
        var accessToken = tokenService.GenerateToken(user, new[] { Roles.Cliente });
        var rawRefreshToken = tokenService.GenerateRefreshToken();

        var dbUser = await userManager.FindByIdAsync(user.Id);
        Assert.NotNull(dbUser);
        dbUser.RefreshToken = hashService.HashToken(rawRefreshToken);
        dbUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(dbUser);

        var client = _factory.CreateClient();
        var request = new RefreshTokenRequest(accessToken, rawRefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/refresh", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        Assert.NotEmpty(loginResponse.Token);
        Assert.NotEmpty(loginResponse.RefreshToken);
        Assert.NotEqual(rawRefreshToken, loginResponse.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_DeveRetornarUnauthorized_QuandoRefreshTokenForInvalidoOuExpirado()
    {
        // Arrange
        var email = "test.badrefresh@email.com";
        var password = "Password123!";
        var user = await SeedUserAsync(email, password, Roles.Cliente, "Cliente Teste");

        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TokenService>();
        var hashService = scope.ServiceProvider.GetRequiredService<HashService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

        var accessToken = tokenService.GenerateToken(user, new[] { Roles.Cliente });
        var rawRefreshToken = tokenService.GenerateRefreshToken();

        var dbUser = await userManager.FindByIdAsync(user.Id);
        Assert.NotNull(dbUser);
        dbUser.RefreshToken = hashService.HashToken(rawRefreshToken);
        dbUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1); // Expirado
        await userManager.UpdateAsync(dbUser);

        var client = _factory.CreateClient();
        var request = new RefreshTokenRequest(accessToken, rawRefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/refresh", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_DeveLimparRefreshToken_QuandoUsuarioEstiverAutenticado()
    {
        // Arrange
        var email = "test.logout@email.com";
        var password = "Password123!";
        var user = await SeedUserAsync(email, password, Roles.Cliente, "Cliente Teste");

        using var scope = _factory.Services.CreateScope();
        var hashService = scope.ServiceProvider.GetRequiredService<HashService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

        var dbUser = await userManager.FindByIdAsync(user.Id);
        Assert.NotNull(dbUser);
        dbUser.RefreshToken = hashService.HashToken("some-token");
        dbUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(dbUser);

        var client = _factory.CreateClient();
        var token = TestJwtTokenFactory.CreateToken(Roles.Cliente, user.Id);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PostAsync("/api/Auth/logout", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verificar no banco de dados se o refresh token foi limpo
        using var assertScope = _factory.Services.CreateScope();
        var assertUserManager = assertScope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var userFromDb = await assertUserManager.FindByIdAsync(user.Id);
        Assert.NotNull(userFromDb);
        Assert.Null(userFromDb.RefreshToken);
    }

    [Fact]
    public async Task Logout_DeveRetornarUnauthorized_QuandoUsuarioNaoEstiverAutenticado()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/Auth/logout", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
