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

    private static RegisterConcessionariaRequest CreateRegisterRequest(
        string email = "contato@top.com",
        string password = "Password123",
        string nome = "Concessionaria Top",
        string cnpj = "12.345.678/0001-90")
    {
        return new RegisterConcessionariaRequest(
            email,
            password,
            nome,
            cnpj,
            "(11) 99999-9999",
            "01001-000",
            "Rua Teste",
            "100",
            "Centro",
            "Sao Paulo",
            "SP"
        );
    }

    private static Concessionaria CreateConcessionaria(
        int id = 1,
        string nome = "Teste",
        string usuarioId = "u1",
        string cnpj = "98.765.432/0001-10")
    {
        return new Concessionaria
        {
            Id = id,
            Nome = nome,
            Cnpj = cnpj,
            Telefone = "(11) 99999-9999",
            Tipo = "Matriz",
            Cep = "01001-000",
            Logradouro = "Rua Teste",
            Numero = "100",
            Bairro = "Centro",
            Cidade = "Sao Paulo",
            Uf = "SP",
            UsuarioId = usuarioId
        };
    }

    [Fact]
    public async Task RegisterAsync_DeveCriarConcessionariaComSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = CreateRegisterRequest();

        _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<Usuario>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await service.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Concessionaria Top", result.Nome);
        Assert.Equal("Matriz", result.Tipo);
        var concessionariaNoDb = await context.Concessionarias.SingleOrDefaultAsync();
        Assert.NotNull(concessionariaNoDb);
        Assert.Equal("Concessionaria Top", concessionariaNoDb.Nome);
        Assert.Equal("Matriz", concessionariaNoDb.Tipo);
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
        var request = CreateRegisterRequest(email: email, nome: "Conc");

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_DeveLancarExcecao_QuandoFalhaNoIdentity()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = CreateRegisterRequest(email: "teste@email.com", nome: "Conc");

        _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Erro" }));

        // Act & Assert
        await Assert.ThrowsAsync<RegistrationException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task GetByIdAsync_DeveRetornarConcessionaria_QuandoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var concessionaria = CreateConcessionaria();
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
        context.Concessionarias.Add(CreateConcessionaria(nome: "Conc", usuarioId: userId));
        await context.SaveChangesAsync();

        var service = new ConcessionariaService(context, _mockUserManager.Object);

        // Act
        var result = await service.GetByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Conc", result.Nome);
    }

    [Fact]
    public async Task UpdatePerfilAsync_DeveAtualizarDadosDaMatriz()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-1";
        context.Concessionarias.Add(CreateConcessionaria(usuarioId: userId));
        await context.SaveChangesAsync();

        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new ConcessionariaPerfilRequest(
            "Concessionaria Atualizada",
            "98.765.432/0001-10",
            "(21) 98888-7777",
            "20000-000",
            "Avenida Nova",
            "500",
            "Centro",
            "Rio de Janeiro",
            "rj"
        );

        // Act
        var result = await service.UpdatePerfilAsync(userId, request);

        // Assert
        Assert.Equal("Concessionaria Atualizada", result.Nome);
        Assert.Equal("(21) 98888-7777", result.Telefone);
        Assert.Equal("RJ", result.Uf);
        Assert.Equal("Matriz", result.Tipo);
    }

    [Fact]
    public async Task UpdatePerfilAsync_DeveLancarExcecao_QuandoCnpjDeLojaJaExiste()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-1";
        context.Concessionarias.Add(CreateConcessionaria(usuarioId: userId));
        context.Lojas.Add(new Loja
        {
            Id = 10,
            Nome = "Loja",
            Tipo = "Filial",
            Cnpj = "11.222.333/0001-44",
            Cep = "04000-000",
            Logradouro = "Rua",
            Numero = "10",
            Bairro = "Bairro",
            Cidade = "Cidade",
            Uf = "SP",
            ConcessionariaId = 1
        });
        await context.SaveChangesAsync();

        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new ConcessionariaPerfilRequest(
            "Concessionaria",
            "11.222.333/0001-44",
            "(11) 99999-9999",
            "01001-000",
            "Rua Teste",
            "100",
            "Centro",
            "Sao Paulo",
            "SP"
        );

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.UpdatePerfilAsync(userId, request));
    }

    [Fact]
    public async Task AlterarSenhaAsync_DeveAlterarSenha()
    {
        // Arrange
        using var context = CreateContext();
        var userId = "user-1";
        var user = new Usuario { Id = userId, UserName = "conc@test.com", Email = "conc@test.com" };
        _mockUserManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _mockUserManager
            .Setup(x => x.ChangePasswordAsync(user, "Atual123!", "Nova123!"))
            .ReturnsAsync(IdentityResult.Success);

        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new ConcessionariaAlterarSenhaRequest("Atual123!", "Nova123!", "Nova123!");

        // Act
        await service.AlterarSenhaAsync(userId, request);

        // Assert
        _mockUserManager.Verify(x => x.ChangePasswordAsync(user, "Atual123!", "Nova123!"), Times.Once);
    }

    [Fact]
    public async Task AlterarSenhaAsync_DeveLancarExcecao_QuandoConfirmacaoNaoConfere()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new ConcessionariaAlterarSenhaRequest("Atual123!", "Nova123!", "Outra123!");

        // Act & Assert
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            service.AlterarSenhaAsync("user-1", request));
    }

    [Fact]
    public async Task AddLojaAsync_DeveCriarLojaComoFilial()
    {
        // Arrange
        using var context = CreateContext();
        context.Concessionarias.Add(CreateConcessionaria());
        await context.SaveChangesAsync();

        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new LojaRequest(
            "Loja Zona Sul",
            "11.222.333/0001-44",
            "04000-000",
            "Avenida Teste",
            "200",
            "Vila Teste",
            "Sao Paulo",
            "SP"
        );

        // Act
        var result = await service.AddLojaAsync(1, request);

        // Assert
        Assert.Equal("Loja Zona Sul", result.Nome);
        Assert.Equal("Filial", result.Tipo);
        Assert.Equal(1, result.ConcessionariaId);
        Assert.True(result.Ativo);
        Assert.Single(context.Lojas);
    }

    [Fact]
    public async Task AddLojaAsync_DeveLancarExcecao_QuandoConcessionariaNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new LojaRequest("Loja", "11.222.333/0001-44", "04000-000", "Rua", "1", "Bairro", "Cidade", "SP");

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.AddLojaAsync(99, request));
    }

    [Fact]
    public async Task AddLojaAsync_DeveLancarExcecao_QuandoCnpjJaExiste()
    {
        // Arrange
        using var context = CreateContext();
        context.Concessionarias.Add(CreateConcessionaria(cnpj: "11.222.333/0001-44"));
        await context.SaveChangesAsync();

        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new LojaRequest("Loja", "11.222.333/0001-44", "04000-000", "Rua", "1", "Bairro", "Cidade", "SP");

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.AddLojaAsync(1, request));
    }

    [Fact]
    public async Task UpdateLojaAsync_DeveAtualizarDadosDaLoja()
    {
        // Arrange
        using var context = CreateContext();
        context.Concessionarias.Add(CreateConcessionaria());
        context.Lojas.Add(new Loja
        {
            Id = 10,
            Nome = "Loja Antiga",
            Tipo = "Filial",
            Cnpj = "11.222.333/0001-44",
            Cep = "04000-000",
            Logradouro = "Rua Antiga",
            Numero = "10",
            Bairro = "Bairro",
            Cidade = "Cidade",
            Uf = "SP",
            ConcessionariaId = 1
        });
        await context.SaveChangesAsync();

        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new LojaRequest("Loja Nova", "11.222.333/0001-44", "05000-000", "Rua Nova", "20", "Novo Bairro", "Nova Cidade", "RJ");

        // Act
        var result = await service.UpdateLojaAsync(1, 10, request);

        // Assert
        Assert.Equal("Loja Nova", result.Nome);
        Assert.Equal("Filial", result.Tipo);
        Assert.Equal("RJ", result.Uf);
    }

    [Fact]
    public async Task AlternarStatusLojaAsync_DeveAlternarAtivo()
    {
        // Arrange
        using var context = CreateContext();
        context.Concessionarias.Add(CreateConcessionaria());
        context.Lojas.Add(new Loja
        {
            Id = 10,
            Nome = "Loja",
            Tipo = "Filial",
            Cnpj = "11.222.333/0001-44",
            Cep = "04000-000",
            Logradouro = "Rua",
            Numero = "10",
            Bairro = "Bairro",
            Cidade = "Cidade",
            Uf = "SP",
            Ativo = true,
            ConcessionariaId = 1
        });
        await context.SaveChangesAsync();

        var service = new ConcessionariaService(context, _mockUserManager.Object);

        // Act
        var result = await service.AlternarStatusLojaAsync(1, 10);

        // Assert
        Assert.False(result.Ativo);
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
}
