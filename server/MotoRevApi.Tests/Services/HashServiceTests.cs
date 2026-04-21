using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Services;

public class HashServiceTests
{
    private readonly HashService _hashService;

    public HashServiceTests()
    {
        _hashService = new HashService();
    }

    [Fact]
    public void HashToken_DeveRetornarHashConsistente()
    {
        // Arrange
        var token = "meu-token-secreto";

        // Act
        var hash1 = _hashService.HashToken(token);
        var hash2 = _hashService.HashToken(token);

        // Assert
        Assert.NotNull(hash1);
        Assert.NotEmpty(hash1);
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void VerifyToken_DeveRetornarTrue_QuandoTokenCorreto()
    {
        // Arrange
        var token = "token-valido";
        var hash = _hashService.HashToken(token);

        // Act
        var result = _hashService.VerifyToken(token, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyToken_DeveRetornarFalse_QuandoTokenIncorreto()
    {
        // Arrange
        var token = "token-valido";
        var outroToken = "token-errado";
        var hash = _hashService.HashToken(token);

        // Act
        var result = _hashService.VerifyToken(outroToken, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyToken_DeveRetornarFalse_QuandoTokenOuHashNuloOuVazio()
    {
        // Act & Assert
        Assert.False(_hashService.VerifyToken(null!, "algum-hash"));
        Assert.False(_hashService.VerifyToken("algum-token", null!));
        Assert.False(_hashService.VerifyToken("", "algum-hash"));
        Assert.False(_hashService.VerifyToken("algum-token", ""));
    }
    
    [Fact]
    public void HashToken_DeveGerarHashesDiferentes_ParaTokensDiferentes()
    {
        // Arrange
        var token1 = "token1";
        var token2 = "token2";

        // Act
        var hash1 = _hashService.HashToken(token1);
        var hash2 = _hashService.HashToken(token2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }
}
