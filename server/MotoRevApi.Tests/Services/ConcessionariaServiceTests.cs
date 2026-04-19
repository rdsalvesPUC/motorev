using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Services;

public class ConcessionariaServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;
    private readonly Mock<UserManager<Usuario>> _mockUserManager;

    public ConcessionariaServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var userStoreMock = new Mock<IUserStore<Usuario>>();
        _mockUserManager = new Mock<UserManager<Usuario>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
    }

    private AppDbContext CreateContext() => new AppDbContext(_dbContextOptions);

    [Fact]
    public async Task RegisterAsync_DeveCriarConcessionariaComSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new RegisterConcessionariaRequest("contato@top.com", "Password123", "Concessionaria Top", "12345678000195", "1133334444");

        _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<Usuario>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await service.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Concessionaria Top", result.Nome);
        var concessionariaNoDb = await context.Concessionarias.SingleOrDefaultAsync();
        Assert.NotNull(concessionariaNoDb);
        Assert.Equal("12345678000195", concessionariaNoDb.Cnpj);
    }

    [Fact]
    public async Task RegisterAsync_DeveLancarExcecao_QuandoCnpjJaExiste()
    {
        // Arrange
        using var context = CreateContext();
        var cnpj = "12345678000195";
        context.Concessionarias.Add(new Concessionaria { Cnpj = cnpj, Nome = "Conc", Tel = "1", UsuarioId = "user-1" });
        await context.SaveChangesAsync();
        
        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new RegisterConcessionariaRequest("teste@email.com", "Password123", "Outra Conc", cnpj, "222");

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_DeveLancarExcecao_QuandoEmailJaExiste()
    {
        // Arrange
        using var context = CreateContext();
        var email = "existente@email.com";
        var user = new Usuario { UserName = email, Email = email };
        _mockUserManager.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);

        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new RegisterConcessionariaRequest(email, "Password123", "Conc", "99999999999999", "111");

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_DeveLancarExcecao_QuandoFalhaNoIdentity()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new RegisterConcessionariaRequest("teste@email.com", "Password123", "Conc", "12345678000195", "111");

        _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Erro" }));

        // Act & Assert
        await Assert.ThrowsAsync<RegistrationException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task GetAllAsync_DeveRetornarTodasAsConcessionariasAtivas()
    {
        // Arrange
        using var context = CreateContext();
        context.Concessionarias.AddRange(
            new Concessionaria { Nome = "Ativa 1", IsActive = true, Cnpj = "1", Tel = "1", UsuarioId = "u1" },
            new Concessionaria { Nome = "Inativa", IsActive = false, Cnpj = "2", Tel = "2", UsuarioId = "u2" },
            new Concessionaria { Nome = "Ativa 2", IsActive = true, Cnpj = "3", Tel = "3", UsuarioId = "u3" }
        );
        await context.SaveChangesAsync();
        
        var service = new ConcessionariaService(context, _mockUserManager.Object);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.True(c.Nome.StartsWith("Ativa")));
    }
    
    [Fact]
    public async Task GetByIdAsync_DeveRetornarConcessionaria_QuandoExisteEAtiva()
    {
        // Arrange
        using var context = CreateContext();
        var concessionaria = new Concessionaria { Id = 1, Nome = "Teste", IsActive = true, Cnpj = "1", Tel = "1", UsuarioId = "u1" };
        context.Concessionarias.Add(concessionaria);
        await context.SaveChangesAsync();

        var service = new ConcessionariaService(context, _mockUserManager.Object);

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Teste", result.Nome);
    }

    [Fact]
    public async Task GetByIdAsync_DeveLancarExcecao_QuandoNaoEncontrada()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ConcessionariaService(context, _mockUserManager.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(99));
    }

    [Fact]
    public async Task GetByUserIdAsync_DeveRetornarConcessionaria_QuandoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-1";
        context.Concessionarias.Add(new Concessionaria { UsuarioId = userId, Nome = "Conc", IsActive = true, Cnpj = "1", Tel = "1" });
        await context.SaveChangesAsync();

        var service = new ConcessionariaService(context, _mockUserManager.Object);

        // Act
        var result = await service.GetByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Conc", result.Nome);
    }

    [Fact]
    public async Task GetByUserIdAsync_DeveLancarExcecao_QuandoNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ConcessionariaService(context, _mockUserManager.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByUserIdAsync("invalido"));
    }

    [Fact]
    public async Task UpdateAsync_DeveAtualizarComSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-1";
        var conc = new Concessionaria { UsuarioId = userId, Nome = "Antigo", IsActive = true, Cnpj = "1", Tel = "1" };
        context.Concessionarias.Add(conc);
        await context.SaveChangesAsync();

        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new UpdateConcessionariaRequest("Novo", "999");

        // Act
        await service.UpdateAsync(userId, request);

        // Assert
        var atualizada = await context.Concessionarias.FindAsync(conc.Id);
        Assert.Equal("Novo", atualizada.Nome);
        Assert.Equal("999", atualizada.Tel);
    }

    [Fact]
    public async Task UpdateAsync_DeveLancarExcecao_QuandoNaoEncontrada()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new UpdateConcessionariaRequest("Novo", "999");

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync("invalido", request));
    }

    [Fact]
    public async Task DeleteAsync_DeveMarcarComoInativo()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-1";
        var conc = new Concessionaria { UsuarioId = userId, Nome = "Conc", IsActive = true, Cnpj = "1", Tel = "1" };
        context.Concessionarias.Add(conc);
        await context.SaveChangesAsync();

        var service = new ConcessionariaService(context, _mockUserManager.Object);

        // Act
        await service.DeleteAsync(userId);

        // Assert
        var deletada = await context.Concessionarias.FindAsync(conc.Id);
        Assert.False(deletada.IsActive);
        Assert.NotNull(deletada.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_DeveLancarExcecao_QuandoNaoEncontrada()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ConcessionariaService(context, _mockUserManager.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync("invalido"));
    }
}
