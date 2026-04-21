using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using MotoRevApi.Services;
using System.Security.Claims;
using Xunit;

namespace MotoRevApi.Tests.Services;

public class AuthServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;
    private readonly Mock<UserManager<Usuario>> _mockUserManager;
    private readonly Mock<SignInManager<Usuario>> _mockSignInManager;
    private readonly Mock<TokenService> _mockTokenService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<HashService> _mockHashService;

    public AuthServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var userStoreMock = new Mock<IUserStore<Usuario>>();
        _mockUserManager = new Mock<UserManager<Usuario>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

        var contextAccessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<Usuario>>();
        _mockSignInManager = new Mock<SignInManager<Usuario>>(_mockUserManager.Object, contextAccessorMock.Object, claimsFactoryMock.Object, null, null, null, null);
        
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(x => x["JwtSettings:Secret"]).Returns("uma-chave-secreta-super-longa-para-testes");
        _mockConfiguration.Setup(x => x["JwtSettings:Issuer"]).Returns("test.com");
        _mockConfiguration.Setup(x => x["JwtSettings:Audience"]).Returns("test.com");
        _mockConfiguration.Setup(x => x["JwtSettings:ExpirationInMinutes"]).Returns("60");
        
        var configSection = new Mock<IConfigurationSection>();
        configSection.Setup(x => x.Value).Returns("7");
        _mockConfiguration.Setup(x => x.GetSection("JwtSettings:RefreshTokenExpirationInDays")).Returns(configSection.Object);

        _mockTokenService = new Mock<TokenService>(_mockConfiguration.Object);
        _mockTokenService.CallBase = true; // Permite chamar a implementação real dos métodos não mockados

        _mockHashService = new Mock<HashService>();
        _mockHashService.Setup(x => x.HashToken(It.IsAny<string>())).Returns((string s) => s);
        _mockHashService.Setup(x => x.VerifyToken(It.IsAny<string>(), It.IsAny<string>())).Returns((string p, string s) => p == s);
    }

    private AppDbContext CreateContext() => new AppDbContext(_dbContextOptions);

    [Fact]
    public async Task LoginAsync_DeveRetornarLoginResponse_ParaCliente()
    {
        // Arrange
        using var context = CreateContext();
        var user = new Usuario { Id = "user-cliente", Email = "cliente@email.com", UserName = "cliente@email.com" };
        var request = new LoginRequest("cliente@email.com", "password");
        
        context.Clientes.Add(new Cliente { UsuarioId = user.Id, Nome = "Cliente Teste" });
        await context.SaveChangesAsync();

        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false)).ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Cliente" });
        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<Usuario>(), It.IsAny<IList<string>>())).Returns("access_token");
        _mockTokenService.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token");
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access_token", result.Token);
        Assert.Equal("Cliente", result.Perfil);
        Assert.NotNull(result.Usuario);
        Assert.Equal("Cliente Teste", result.Usuario.Nome);
    }

    [Fact]
    public async Task LoginAsync_DeveRetornarLoginResponse_ParaConcessionaria()
    {
        // Arrange
        using var context = CreateContext();
        var user = new Usuario { Id = "user-conc", Email = "conc@email.com", UserName = "conc@email.com" };
        var request = new LoginRequest("conc@email.com", "password");
        
        context.Concessionarias.Add(new Concessionaria { UsuarioId = user.Id, Nome = "Conc Teste" });
        await context.SaveChangesAsync();

        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false)).ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Concessionaria" });
        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<Usuario>(), It.IsAny<IList<string>>())).Returns("access_token");
        _mockTokenService.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token");
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access_token", result.Token);
        Assert.Equal("Concessionaria", result.Perfil);
        Assert.NotNull(result.Usuario);
        Assert.Equal("Conc Teste", result.Usuario.Nome);
    }

    [Fact]
    public async Task LoginAsync_DeveRetornarUserDataNulo_QuandoClienteNaoExisteNoBanco()
    {
        // Arrange
        using var context = CreateContext();
        var user = new Usuario { Id = "user-cliente-sem-perfil", Email = "cliente@email.com", UserName = "cliente@email.com" };
        var request = new LoginRequest("cliente@email.com", "password");
        
        // NÃO adicionamos cliente ao banco

        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false)).ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Cliente" });
        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<Usuario>(), It.IsAny<IList<string>>())).Returns("access_token");
        _mockTokenService.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token");
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_DeveRetornarUserDataNulo_QuandoConcessionariaNaoExisteNoBanco()
    {
        // Arrange
        using var context = CreateContext();
        var user = new Usuario { Id = "user-conc-sem-perfil", Email = "conc@email.com", UserName = "conc@email.com" };
        var request = new LoginRequest("conc@email.com", "password");
        
        // NÃO adicionamos concessionaria ao banco

        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false)).ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Concessionaria" });
        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<Usuario>(), It.IsAny<IList<string>>())).Returns("access_token");
        _mockTokenService.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token");
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_DeveLancarNotFoundException_QuandoUsuarioNaoEncontrado()
    {
        // Arrange
        using var context = CreateContext();
        var request = new LoginRequest("naoexiste@email.com", "password");
        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((Usuario)null);
        
        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_DeveLancarNotFoundException_QuandoSenhaInvalida()
    {
        // Arrange
        using var context = CreateContext();
        var user = new Usuario { Id = "1", Email = "teste@email.com" };
        var request = new LoginRequest("teste@email.com", "wrongpassword");

        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false)).ReturnsAsync(SignInResult.Failed);

        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(request));
    }

    [Fact]
    public async Task RefreshTokenAsync_DeveRetornarNovosTokens_QuandoValidos()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-1";
        var refreshToken = "old-refresh-token";
        var user = new Usuario 
        { 
            Id = userId, 
            RefreshToken = refreshToken, 
            RefreshTokenExpiryTime = DateTime.Now.AddDays(1) 
        };
        var request = new RefreshTokenRequest("expired-access-token", refreshToken);

        context.Clientes.Add(new Cliente { UsuarioId = userId, Nome = "Cliente Teste" });
        await context.SaveChangesAsync();

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _mockTokenService.Setup(x => x.GetPrincipalFromExpiredToken(request.AccessToken)).Returns(principal);
        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Cliente" });
        _mockTokenService.Setup(x => x.GenerateToken(user, It.IsAny<IList<string>>())).Returns("new-access-token");
        _mockTokenService.Setup(x => x.GenerateRefreshToken()).Returns("new-refresh-token");
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act
        var result = await service.RefreshTokenAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new-access-token", result.Token);
        Assert.Equal("new-refresh-token", result.RefreshToken);
        Assert.NotNull(result.Usuario);
        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_DeveRetornarNovosTokensEUserData_ParaCliente()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-cliente-refresh";
        var refreshToken = "old-refresh-token";
        var user = new Usuario 
        { 
            Id = userId, 
            RefreshToken = refreshToken, 
            RefreshTokenExpiryTime = DateTime.Now.AddDays(1) 
        };
        var request = new RefreshTokenRequest("expired-token", refreshToken);

        context.Clientes.Add(new Cliente { UsuarioId = userId, Nome = "Cliente Refresh" });
        await context.SaveChangesAsync();

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _mockTokenService.Setup(x => x.GetPrincipalFromExpiredToken(request.AccessToken)).Returns(principal);
        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Cliente" });
        _mockTokenService.Setup(x => x.GenerateToken(user, It.IsAny<IList<string>>())).Returns("new-access-token");
        _mockTokenService.Setup(x => x.GenerateRefreshToken()).Returns("new-refresh-token");
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act
        var result = await service.RefreshTokenAsync(request);

        // Assert
        Assert.NotNull(result.Usuario);
        Assert.Equal("Cliente Refresh", result.Usuario.Nome);
    }

    [Fact]
    public async Task RefreshTokenAsync_DeveRetornarNovosTokensEUserData_ParaConcessionaria()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-conc-refresh";
        var refreshToken = "old-refresh-token";
        var user = new Usuario 
        { 
            Id = userId, 
            RefreshToken = refreshToken, 
            RefreshTokenExpiryTime = DateTime.Now.AddDays(1) 
        };
        var request = new RefreshTokenRequest("expired-token", refreshToken);

        context.Concessionarias.Add(new Concessionaria { UsuarioId = userId, Nome = "Conc Refresh" });
        await context.SaveChangesAsync();

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _mockTokenService.Setup(x => x.GetPrincipalFromExpiredToken(request.AccessToken)).Returns(principal);
        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Concessionaria" });
        _mockTokenService.Setup(x => x.GenerateToken(user, It.IsAny<IList<string>>())).Returns("new-access-token");
        _mockTokenService.Setup(x => x.GenerateRefreshToken()).Returns("new-refresh-token");
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act
        var result = await service.RefreshTokenAsync(request);

        // Assert
        Assert.NotNull(result.Usuario);
        Assert.Equal("Conc Refresh", result.Usuario.Nome);
    }

    [Fact]
    public async Task RefreshTokenAsync_DeveLancarSecurityTokenException_QuandoPrincipalNulo()
    {
        // Arrange
        using var context = CreateContext();
        var request = new RefreshTokenRequest("invalid-token", "refresh");
        _mockTokenService.Setup(x => x.GetPrincipalFromExpiredToken(It.IsAny<string>())).Returns((ClaimsPrincipal)null);

        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<SecurityTokenException>(() => service.RefreshTokenAsync(request));
    }

    [Fact]
    public async Task RefreshTokenAsync_DeveLancarSecurityTokenException_QuandoUserNaoEncontradoOuTokenInvalido()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-1";
        var request = new RefreshTokenRequest("access", "wrong-refresh");
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _mockTokenService.Setup(x => x.GetPrincipalFromExpiredToken(It.IsAny<string>())).Returns(principal);
        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((Usuario)null);

        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<SecurityTokenException>(() => service.RefreshTokenAsync(request));
    }

    [Fact]
    public async Task LoginAsync_DeveLancarException_QuandoUsuarioNaoPossuiRole()
    {
        // Arrange
        using var context = CreateContext();
        var user = new Usuario { Id = "user-sem-role", Email = "teste@email.com" };
        var request = new LoginRequest("teste@email.com", "password");

        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false)).ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>()); // Vazio

        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => service.LoginAsync(request));
        Assert.Equal("Usuário não possui um perfil associado.", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_DeveLancarException_QuandoUpdateAsyncFalha()
    {
        // Arrange
        using var context = CreateContext();
        var user = new Usuario { Id = "user-update-fail", Email = "teste@email.com" };
        var request = new LoginRequest("teste@email.com", "password");

        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false)).ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Cliente" });
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Erro de teste" }));

        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => service.LoginAsync(request));
        Assert.Equal("Não foi possível salvar o refresh token.", exception.Message);
    }

    [Fact]
    public async Task RefreshTokenAsync_DeveLancarException_QuandoUsuarioNaoPossuiRole()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-1";
        var user = new Usuario { Id = userId, RefreshToken = "valid", RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(1) };
        var request = new RefreshTokenRequest("expired", "valid");
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _mockTokenService.Setup(x => x.GetPrincipalFromExpiredToken(request.AccessToken)).Returns(principal);
        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>()); // Vazio

        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => service.RefreshTokenAsync(request));
        Assert.Equal("Usuário não possui um perfil associado.", exception.Message);
    }

    [Fact]
    public async Task RefreshTokenAsync_DeveLancarException_QuandoUpdateAsyncFalha()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-1";
        var user = new Usuario { Id = userId, RefreshToken = "valid", RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(1) };
        var request = new RefreshTokenRequest("expired", "valid");
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _mockTokenService.Setup(x => x.GetPrincipalFromExpiredToken(request.AccessToken)).Returns(principal);
        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Cliente" });
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Failed());

        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => service.RefreshTokenAsync(request));
        Assert.Equal("Não foi possível atualizar o refresh token.", exception.Message);
    }

    [Fact]
    public async Task LogoutAsync_DeveLancarException_QuandoUpdateAsyncFalha()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-1";
        var user = new Usuario { Id = userId };
        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Failed());

        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => service.LogoutAsync(userId));
        Assert.Equal("Não foi possível realizar o logout.", exception.Message);
    }

    [Fact]
    public async Task LogoutAsync_DeveLimparRefreshToken()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-1";
        var user = new Usuario { Id = userId, RefreshToken = "some-token" };
        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var service = new AuthService(_mockUserManager.Object, _mockSignInManager.Object, _mockTokenService.Object, context, _mockConfiguration.Object, _mockHashService.Object);

        // Act
        await service.LogoutAsync(userId);

        // Assert
        Assert.Null(user.RefreshToken);
        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
    }
}
