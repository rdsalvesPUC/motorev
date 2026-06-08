using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using MotoRevApi.Services;
using System.ComponentModel.DataAnnotations;
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
        var request = new RegisterClienteRequest("teste@email.com", "Password123", "Teste", "529.982.247-25", "(11) 99999-0000");

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
        Assert.Equal("52998224725", clienteNoDb.Cpf);
        _mockUserManager.Verify(x => x.CreateAsync(
            It.Is<Usuario>(u => u.PhoneNumber == "11999990000"),
            request.Password), Times.Once);
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
        var request = new RegisterClienteRequest(email, "Password123", "Teste", "529.982.247-25", "(11) 99999-0000");

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_DeveLancarExcecao_QuandoFalhaNoIdentity()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new RegisterClienteRequest("teste@email.com", "Password123", "Teste", "529.982.247-25", "(11) 99999-0000");

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

    [Fact]
    public async Task RegisterAsync_DeveLancarExcecao_QuandoCpfJaExiste()
    {
        // Arrange
        using var context = CreateContext();
        context.Clientes.Add(new Cliente { UsuarioId = "user-existente", Nome = "Cliente Existente", Cpf = "52998224725" });
        await context.SaveChangesAsync();

        _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario)null);

        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new RegisterClienteRequest("novo@email.com", "Password123", "Teste", "529.982.247-25", "(11) 99999-0000");

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task GetPerfilByUserIdAsync_DeveRetornarPerfilCompleto()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-id-123";
        context.Users.Add(new Usuario { Id = userId, UserName = "cliente@email.com", Email = "cliente@email.com", PhoneNumber = "11999990000" });
        context.Clientes.Add(new Cliente
        {
            UsuarioId = userId,
            Nome = "Cliente Teste",
            Cpf = "52998224725",
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
        await context.SaveChangesAsync();

        var service = new ClienteService(context, _mockUserManager.Object);

        // Act
        var result = await service.GetPerfilByUserIdAsync(userId);

        // Assert
        Assert.Equal("Cliente Teste", result.Nome);
        Assert.Equal("cliente@email.com", result.Email);
        Assert.Equal("52998224725", result.Cpf);
        Assert.Equal("11999990000", result.Telefone);
        Assert.NotNull(result.Endereco);
        Assert.Equal("04538132", result.Endereco.Cep);
        Assert.Equal("Rua Funchal", result.Endereco.Logradouro);
    }

    [Fact]
    public async Task GetPerfilByUserIdAsync_DeveRetornarEnderecoNulo_QuandoClienteNaoPossuirEndereco()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-id-123";
        context.Users.Add(new Usuario { Id = userId, UserName = "cliente@email.com", Email = "cliente@email.com", PhoneNumber = "11999990000" });
        context.Clientes.Add(new Cliente { UsuarioId = userId, Nome = "Cliente Teste", Cpf = "52998224725" });
        await context.SaveChangesAsync();

        var service = new ClienteService(context, _mockUserManager.Object);

        // Act
        var result = await service.GetPerfilByUserIdAsync(userId);

        // Assert
        Assert.Null(result.Endereco);
    }

    [Fact]
    public async Task UpdateDadosPessoaisAsync_DeveAtualizarClienteEUsuarioSemAlterarCpf()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-id-123";
        context.Users.Add(new Usuario { Id = userId, UserName = "antigo@email.com", Email = "antigo@email.com", PhoneNumber = "11999990000" });
        context.Clientes.Add(new Cliente { UsuarioId = userId, Nome = "Cliente Antigo", Cpf = "52998224725" });
        await context.SaveChangesAsync();

        _mockUserManager.Setup(x => x.FindByEmailAsync("novo@email.com")).ReturnsAsync((Usuario)null);
        _mockUserManager.Setup(x => x.SetEmailAsync(It.IsAny<Usuario>(), "novo@email.com")).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.SetUserNameAsync(It.IsAny<Usuario>(), "novo@email.com")).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.SetPhoneNumberAsync(It.IsAny<Usuario>(), "11988887777")).ReturnsAsync(IdentityResult.Success);

        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new ClienteDadosPessoaisRequest("Cliente Novo", "novo@email.com", "(11) 98888-7777");

        // Act
        var result = await service.UpdateDadosPessoaisAsync(userId, request);

        // Assert
        Assert.Equal("Cliente Novo", result.Nome);
        Assert.Equal("novo@email.com", result.Email);
        Assert.Equal("52998224725", result.Cpf);
        Assert.Equal("11988887777", result.Telefone);

        var clienteNoDb = await context.Clientes.SingleAsync(c => c.UsuarioId == userId);
        Assert.Equal("52998224725", clienteNoDb.Cpf);
    }

    [Fact]
    public async Task UpdateDadosPessoaisAsync_DeveLancarExcecao_QuandoEmailJaExisteParaOutroUsuario()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-id-123";
        context.Users.Add(new Usuario { Id = userId, UserName = "cliente@email.com", Email = "cliente@email.com", PhoneNumber = "11999990000" });
        context.Clientes.Add(new Cliente { UsuarioId = userId, Nome = "Cliente Teste", Cpf = "52998224725" });
        await context.SaveChangesAsync();

        _mockUserManager.Setup(x => x.FindByEmailAsync("existente@email.com"))
            .ReturnsAsync(new Usuario { Id = "outro-user-id", Email = "existente@email.com", UserName = "existente@email.com" });

        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new ClienteDadosPessoaisRequest("Cliente Teste", "existente@email.com", "(11) 99999-0000");

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.UpdateDadosPessoaisAsync(userId, request));
    }

    [Fact]
    public async Task UpdateDadosPessoaisAsync_DeveLancarExcecao_QuandoTelefoneForInvalido()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-id-123";
        context.Users.Add(new Usuario { Id = userId, UserName = "cliente@email.com", Email = "cliente@email.com", PhoneNumber = "11999990000" });
        context.Clientes.Add(new Cliente { UsuarioId = userId, Nome = "Cliente Teste", Cpf = "52998224725" });
        await context.SaveChangesAsync();

        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new ClienteDadosPessoaisRequest("Cliente Teste", "cliente@email.com", "123");

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => service.UpdateDadosPessoaisAsync(userId, request));
    }

    [Fact]
    public async Task UpdateEnderecoAsync_DeveCriarEndereco_QuandoClienteNaoPossuirEndereco()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-id-123";
        context.Users.Add(new Usuario { Id = userId, UserName = "cliente@email.com", Email = "cliente@email.com" });
        context.Clientes.Add(new Cliente { UsuarioId = userId, Nome = "Cliente Teste", Cpf = "52998224725" });
        await context.SaveChangesAsync();

        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new ClienteEnderecoRequest("04538-132", "Rua Funchal", "418", null, "Vila Olímpia", "São Paulo", "sp");

        // Act
        var result = await service.UpdateEnderecoAsync(userId, request);

        // Assert
        Assert.Equal("04538132", result.Endereco.Cep);
        Assert.Equal("Rua Funchal", result.Endereco.Logradouro);
        Assert.Equal("418", result.Endereco.Numero);
        Assert.Equal("Vila Olímpia", result.Endereco.Bairro);
        Assert.Equal("São Paulo", result.Endereco.Cidade);
        Assert.Equal("SP", result.Endereco.Uf);

        var clienteNoDb = await context.Clientes.Include(c => c.Endereco).SingleAsync(c => c.UsuarioId == userId);
        Assert.NotNull(clienteNoDb.EnderecoId);
        Assert.NotNull(clienteNoDb.Endereco);
        Assert.Equal("04538132", clienteNoDb.Endereco.Cep);
        Assert.Single(context.Enderecos);
    }

    [Fact]
    public async Task UpdateEnderecoAsync_DeveAtualizarEnderecoExistente_QuandoClienteJaPossuirEndereco()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-id-123";
        context.Users.Add(new Usuario { Id = userId, UserName = "cliente@email.com", Email = "cliente@email.com" });
        context.Clientes.Add(new Cliente
        {
            UsuarioId = userId,
            Nome = "Cliente Teste",
            Cpf = "52998224725",
            Endereco = new Endereco
            {
                Cep = "01001000",
                Logradouro = "Praça da Sé",
                Numero = "1",
                Bairro = "Sé",
                Cidade = "São Paulo",
                Uf = "SP"
            }
        });
        await context.SaveChangesAsync();
        var enderecoId = await context.Enderecos.Select(e => e.Id).SingleAsync();

        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new ClienteEnderecoRequest("04538-132", "Rua Funchal", "418", "Apto 52", "Vila Olímpia", "São Paulo", "sp");

        // Act
        var result = await service.UpdateEnderecoAsync(userId, request);

        // Assert
        Assert.NotNull(result.Endereco);
        Assert.Equal("04538132", result.Endereco.Cep);
        Assert.Equal("Rua Funchal", result.Endereco.Logradouro);
        Assert.Equal("Apto 52", result.Endereco.Complemento);

        var clienteNoDb = await context.Clientes.Include(c => c.Endereco).SingleAsync(c => c.UsuarioId == userId);
        Assert.Equal(enderecoId, clienteNoDb.EnderecoId);
        Assert.Equal(enderecoId, clienteNoDb.Endereco!.Id);
        Assert.Single(context.Enderecos);
    }

    [Fact]
    public async Task UpdateEnderecoAsync_DeveLancarExcecao_QuandoCepForInvalido()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-id-123";
        context.Users.Add(new Usuario { Id = userId, UserName = "cliente@email.com", Email = "cliente@email.com" });
        context.Clientes.Add(new Cliente { UsuarioId = userId, Nome = "Cliente Teste", Cpf = "52998224725" });
        await context.SaveChangesAsync();

        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new ClienteEnderecoRequest("123", "Rua Funchal", "418", null, "Vila Olímpia", "São Paulo", "SP");

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => service.UpdateEnderecoAsync(userId, request));
    }

    [Fact]
    public async Task UpdateEnderecoAsync_DeveLancarExcecao_QuandoUfForInvalida()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-id-123";
        context.Users.Add(new Usuario { Id = userId, UserName = "cliente@email.com", Email = "cliente@email.com" });
        context.Clientes.Add(new Cliente { UsuarioId = userId, Nome = "Cliente Teste", Cpf = "52998224725" });
        await context.SaveChangesAsync();

        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new ClienteEnderecoRequest("04538-132", "Rua Funchal", "418", null, "Vila Olímpia", "São Paulo", "SPO");

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => service.UpdateEnderecoAsync(userId, request));
    }

    [Fact]
    public async Task UpdateEnderecoAsync_DeveLancarExcecao_QuandoCampoObrigatorioEstiverVazio()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-id-123";
        context.Users.Add(new Usuario { Id = userId, UserName = "cliente@email.com", Email = "cliente@email.com" });
        context.Clientes.Add(new Cliente { UsuarioId = userId, Nome = "Cliente Teste", Cpf = "52998224725" });
        await context.SaveChangesAsync();

        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new ClienteEnderecoRequest("04538-132", "", "418", null, "Vila Olímpia", "São Paulo", "SP");

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => service.UpdateEnderecoAsync(userId, request));
    }

    [Fact]
    public async Task AlterarSenhaAsync_DeveUsarSenhaAtualENovaSenha()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-id-123";
        var user = new Usuario { Id = userId, UserName = "cliente@email.com", Email = "cliente@email.com" };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.ChangePasswordAsync(user, "SenhaAtual123!", "NovaSenha123!")).ReturnsAsync(IdentityResult.Success);

        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new ClienteAlterarSenhaRequest("SenhaAtual123!", "NovaSenha123!", "NovaSenha123!");

        // Act
        await service.AlterarSenhaAsync(userId, request);

        // Assert
        _mockUserManager.Verify(x => x.ChangePasswordAsync(user, "SenhaAtual123!", "NovaSenha123!"), Times.Once);
    }

    [Fact]
    public async Task AlterarSenhaAsync_DeveLancarExcecao_QuandoConfirmacaoDiverge()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new ClienteAlterarSenhaRequest("SenhaAtual123!", "NovaSenha123!", "OutraSenha123!");

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => service.AlterarSenhaAsync("user-id-123", request));
    }

    [Fact]
    public async Task AlterarSenhaAsync_DeveLancarExcecao_QuandoIdentityFalhar()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-id-123";
        var user = new Usuario { Id = userId, UserName = "cliente@email.com", Email = "cliente@email.com" };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.ChangePasswordAsync(user, "SenhaAtualErrada123!", "NovaSenha123!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Senha atual inválida" }));

        var service = new ClienteService(context, _mockUserManager.Object);
        var request = new ClienteAlterarSenhaRequest("SenhaAtualErrada123!", "NovaSenha123!", "NovaSenha123!");

        // Act & Assert
        await Assert.ThrowsAsync<RegistrationException>(() => service.AlterarSenhaAsync(userId, request));
    }
}
