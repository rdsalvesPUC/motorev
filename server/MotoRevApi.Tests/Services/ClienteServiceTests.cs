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
        var request = new RegisterClienteRequest("teste@email.com", "Password123", "Teste", "12345678901", "11999999999", null);

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
        Assert.Equal("12345678901", clienteNoDb.Cpf);
    }

    [Fact]
    public async Task RegisterAsync_DeveLancarExcecao_QuandoCpfJaExiste()
    {
        // Arrange
        using var context = CreateContext();
        context.Clientes.Add(new Cliente { Cpf = "12345678901", Nome = "Teste", Email = "teste@email.com", Cel = "11999999999", UsuarioId = "user-1" });
        await context.SaveChangesAsync();
        
        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new RegisterClienteRequest("", "", "", "12345678901", "", null);

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

        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new RegisterClienteRequest(email, "Password123", "Teste", "99999999999", "11999999999", null);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_DeveLancarExcecao_QuandoFalhaNoIdentity()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new RegisterClienteRequest("teste@email.com", "Password123", "Teste", "12345678901", "11999999999", null);

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
        context.Clientes.Add(new Cliente { UsuarioId = userId, Nome = "Cliente Teste", IsActive = true, Cpf = "12345678901", Email = "teste@email.com", Cel = "11999999999" });
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
    
    [Fact]
    public async Task UpdateAsync_DeveAtualizarClienteComSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-id-456";
        var cliente = new Cliente { UsuarioId = userId, Nome = "Nome Antigo", IsActive = true, Cpf = "12345678901", Email = "teste@email.com", Cel = "11999999999" };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new UpdateClienteRequest("Nome Novo", null, null);

        // Act
        await service.UpdateAsync(userId, request);

        // Assert
        var clienteAtualizado = await context.Clientes.FindAsync(cliente.Id);
        Assert.Equal("Nome Novo", clienteAtualizado.Nome);
    }
    
    [Fact]
    public async Task UpdateAsync_DeveLancarExcecao_QuandoClienteNaoEncontrado()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new UpdateClienteRequest("Novo", null, null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync("invalido", request));
    }

    [Fact]
    public async Task DeleteAsync_DeveMarcarClienteComoInativo()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-id-789";
        var cliente = new Cliente { UsuarioId = userId, Nome = "A ser deletado", IsActive = true, Cpf = "12345678901", Email = "teste@email.com", Cel = "11999999999" };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var service = new ClienteService(context, _mockUserManager.Object);

        // Act
        await service.DeleteAsync(userId);

        // Assert
        var clienteDeletado = await context.Clientes.FindAsync(cliente.Id);
        Assert.False(clienteDeletado.IsActive);
        Assert.NotNull(clienteDeletado.DeletedAt);
    }
    [Fact]
    public async Task DeleteAsync_DeveLancarExcecao_QuandoClienteNaoEncontrado()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ClienteService(context, _mockUserManager.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync("invalido"));
    }
}
