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

public class ClienteServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;
    private readonly Mock<UserManager<Usuario>> _mockUserManager;

    public ClienteServiceTests()
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
    public async Task RegisterAsync_DeveCriarClienteComSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new RegisterClienteRequest("teste@email.com", "Password123", "Teste");

        _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<Usuario>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await service.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Teste", result.Nome);
        var clienteNoDb = await context.Clientes.SingleOrDefaultAsync();
        Assert.NotNull(clienteNoDb);
        Assert.Equal("Teste", clienteNoDb.Nome);
    }

    [Fact]
    public async Task RegisterAsync_DeveLancarExcecao_QuandoEmailJaExiste()
    {
        // Arrange
        using var context = CreateContext();
        var email = "existente@email.com";
        var user = new Usuario { UserName = email, Email = email };
        _mockUserManager.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);

        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new RegisterClienteRequest(email, "Password123", "Teste");

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_DeveLancarExcecao_QuandoFalhaNoIdentity()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new RegisterClienteRequest("teste@email.com", "Password123", "Teste");

        _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Erro de teste" }));

        // Act & Assert
        await Assert.ThrowsAsync<RegistrationException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task GetByUserIdAsync_DeveRetornarCliente_QuandoClienteExiste()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-id-123";
        context.Clientes.Add(new Cliente { UsuarioId = userId, Nome = "Cliente Teste" });
        await context.SaveChangesAsync();

        var service = new ClienteService(context, _mockUserManager.Object);

        // Act
        var result = await service.GetByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Cliente Teste", result.Nome);
    }

    [Fact]
    public async Task GetByUserIdAsync_DeveLancarExcecao_QuandoClienteNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ClienteService(context, _mockUserManager.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByUserIdAsync("id-inexistente"));
    }
}
