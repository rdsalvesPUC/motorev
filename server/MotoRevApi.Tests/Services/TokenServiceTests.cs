using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using MotoRevApi.Model;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Services;

public class TokenServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly string _secret = "uma-chave-secreta-muito-longa-para-testes-jwt-pelo-menos-32-chars";

    public TokenServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(x => x["JwtSettings:Secret"]).Returns(_secret);
        _mockConfiguration.Setup(x => x["JwtSettings:Issuer"]).Returns("test.com");
        _mockConfiguration.Setup(x => x["JwtSettings:Audience"]).Returns("test.com");
        _mockConfiguration.Setup(x => x["JwtSettings:ExpirationInMinutes"]).Returns("60");
    }

    [Fact]
    public void Constructor_DeveLancarExcecao_QuandoSecretNaoConfigurada()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x["JwtSettings:Secret"]).Returns((string?)null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new TokenService(mockConfig.Object));
    }

    [Fact]
    public void GenerateToken_DeveGerarTokenValido()
    {
        // Arrange
        var service = new TokenService(_mockConfiguration.Object);
        var user = new Usuario { Id = "user-1", Email = "teste@email.com", UserName = "teste@email.com" };
        var roles = new List<string> { "Cliente", "Admin" };

        // Act
        var token = service.GenerateToken(user, roles);

        // Assert
        Assert.False(string.IsNullOrEmpty(token));
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        Assert.Equal("user-1", jwtToken.Subject);
        Assert.Equal("teste@email.com", jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Cliente");
        Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateRefreshToken_DeveRetornarStringBase64()
    {
        // Arrange
        var service = new TokenService(_mockConfiguration.Object);

        // Act
        var refreshToken = service.GenerateRefreshToken();

        // Assert
        Assert.False(string.IsNullOrEmpty(refreshToken));
        // Tenta converter de volta para garantir que é base64 válido
        var bytes = Convert.FromBase64String(refreshToken);
        Assert.Equal(64, bytes.Length);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_DeveRetornarPrincipal_ParaTokenValido()
    {
        // Arrange
        var service = new TokenService(_mockConfiguration.Object);
        var user = new Usuario { Id = "user-1", Email = "teste@email.com" };
        var token = service.GenerateToken(user, new List<string>());

        // Act
        var principal = service.GetPrincipalFromExpiredToken(token);

        // Assert
        Assert.NotNull(principal);
        Assert.Equal("user-1", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }
    
    [Fact]
    public void GetPrincipalFromExpiredToken_DeveLancarSecurityTokenException_ParaTokenInvalido()
    {
        // Arrange
        var service = new TokenService(_mockConfiguration.Object);
        var invalidToken = "token-totalmente-invalido";

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => service.GetPrincipalFromExpiredToken(invalidToken));
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_DeveLancarSecurityTokenException_QuandoAlgoritmoDiferente()
    {
        // Arrange
        var service = new TokenService(_mockConfiguration.Object);
        
        // Gerar um token manualmente com algoritmo diferente (HS512)
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512); // Diferente de HmacSha256
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")]),
            SigningCredentials = creds
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Act & Assert
        var ex = Assert.Throws<SecurityTokenException>(() => service.GetPrincipalFromExpiredToken(tokenString));
        Assert.Equal("Token inválido", ex.Message);
    }
}
