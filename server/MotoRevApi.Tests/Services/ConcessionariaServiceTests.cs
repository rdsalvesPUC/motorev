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
        _mockUserManager = new Mock<UserManager<Usuario>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private AppDbContext CreateContext() => new AppDbContext(_dbContextOptions);

    [Fact]
    public async Task RegisterAsync_DeveCriarConcessionariaComSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new RegisterConcessionariaRequest("contato@top.com", "Password123", "Concessionaria Top", "12345678000190");

        _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<Usuario>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await service.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Concessionaria Top", result.Nome);
        Assert.Equal("12345678000190", result.Cnpj);
        var concessionariaNoDb = await context.Concessionarias.SingleOrDefaultAsync();
        Assert.NotNull(concessionariaNoDb);
        Assert.Equal("Concessionaria Top", concessionariaNoDb.Nome);
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
        var request = new RegisterConcessionariaRequest(email, "Password123", "Conc", "12345678000190");

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.RegisterAsync(request));
    }
    
    [Fact]
    public async Task RegisterAsync_DeveLancarExcecao_QuandoCnpjJaExiste()
    {
        // Arrange
        using var context = CreateContext();
        var cnpj = "11222333000144";
        context.Concessionarias.Add(new Concessionaria { Nome = "Existente", Cnpj = cnpj, UsuarioId = "u-old" });
        await context.SaveChangesAsync();
        
        _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);

        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new RegisterConcessionariaRequest("novo@email.com", "Password123", "Nova Conc", cnpj);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DuplicateDataException>(() => service.RegisterAsync(request));
        Assert.Contains("já está cadastrado", exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_DeveLancarExcecao_QuandoFalhaNoIdentity()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new RegisterConcessionariaRequest("teste@email.com", "Password123", "Conc", "12345678000190");

        _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
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
        var concessionaria1 = new Concessionaria { Id = 1, Nome = "Teste 1", Cnpj = "123", UsuarioId = "u1" };
        var concessionaria2 = new Concessionaria { Id = 2, Nome = "Teste 2", Cnpj = "456", UsuarioId = "u2" };
        context.Concessionarias.Add(concessionaria1);
        context.Concessionarias.Add(concessionaria2);
        await context.SaveChangesAsync();

        var service = new ConcessionariaService(context, _mockUserManager.Object);

        // Act
        var result = await service.GetByIdAsync(2); // Buscando a ID 2 especificamente

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Teste 2", result.Nome);
        Assert.Equal(2, result.Id);
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
        context.Concessionarias.Add(new Concessionaria { UsuarioId = userId, Nome = "Conc", Cnpj = "123" });
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
    public async Task BuscarConcessionariasAsync_DeveRetornarTodas_QuandoFiltrosNulos()
    {
        // Arrange
        using var context = CreateContext();
        context.Concessionarias.Add(new Concessionaria { Id = 1, Nome = "A", Cnpj = "1", UsuarioId = "u1" });
        context.Concessionarias.Add(new Concessionaria { Id = 2, Nome = "B", Cnpj = "2", UsuarioId = "u2" });
        await context.SaveChangesAsync();

        var service = new ConcessionariaService(context, _mockUserManager.Object);

        // Act
        var result = await service.BuscarConcessionariasAsync(null, null);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task BuscarConcessionariasAsync_DeveFiltrarPorNomeOuId_QuandoTermoInformado()
    {
        // Arrange
        using var context = CreateContext();
        context.Concessionarias.Add(new Concessionaria { Id = 1, Nome = "Loja X", Cnpj = "1", UsuarioId = "u1" });
        context.Concessionarias.Add(new Concessionaria { Id = 2, Nome = "Loja Y", Cnpj = "2", UsuarioId = "u2" });
        context.Concessionarias.Add(new Concessionaria { Id = 3, Nome = "Outra", Cnpj = "3", UsuarioId = "u3" });
        
        // Adiciona endereço na Loja X
        context.Enderecos.Add(new Endereco { ConcessionariaId = 1, Cep = "123", Logradouro = "Rua", Numero = "1", Bairro = "B", Cidade = "C", Estado = "SP" });
        
        await context.SaveChangesAsync();

        var service = new ConcessionariaService(context, _mockUserManager.Object);

        // Act 1: Busca por nome parcial
        var resultNome = await service.BuscarConcessionariasAsync("Loja", null);
        
        // Act 2: Busca por ID
        var resultId = await service.BuscarConcessionariasAsync("3", null);

        // Assert 1
        Assert.Equal(2, resultNome.Count);
        var lojaX = resultNome.First(r => r.Id == 1);
        Assert.Single(lojaX.Enderecos);
        var lojaY = resultNome.First(r => r.Id == 2);
        Assert.Empty(lojaY.Enderecos);

        // Assert 2
        Assert.Single(resultId);
        Assert.Equal("Outra", resultId.First().Nome);
    }

    [Fact]
    public async Task BuscarConcessionariasAsync_DeveFiltrarPorCidade()
    {
        // Arrange
        using var context = CreateContext();
        context.Concessionarias.Add(new Concessionaria { Id = 1, Nome = "Loja 1", Cnpj = "1", UsuarioId = "u1" });
        context.Concessionarias.Add(new Concessionaria { Id = 2, Nome = "Loja 2", Cnpj = "2", UsuarioId = "u2" });
        
        context.Enderecos.Add(new Endereco { ConcessionariaId = 1, Cep = "123", Logradouro = "R", Numero = "1", Bairro = "B", Cidade = "São Paulo", Estado = "SP" });
        context.Enderecos.Add(new Endereco { ConcessionariaId = 2, Cep = "456", Logradouro = "R", Numero = "2", Bairro = "B", Cidade = "Campinas", Estado = "SP" });
        
        await context.SaveChangesAsync();

        var service = new ConcessionariaService(context, _mockUserManager.Object);

        // Act
        var result = await service.BuscarConcessionariasAsync(null, "Paulo");

        // Assert
        Assert.Single(result);
        Assert.Equal("Loja 1", result.First().Nome);
        Assert.Single(result.First().Enderecos);
        Assert.Equal("São Paulo", result.First().Enderecos.First().Cidade);
    }

    [Fact]
    public async Task UpdateAsync_DeveAtualizarNomeEEmail_QuandoConcessionariaExisteEDadosValidos()
    {
        // Arrange
        using var context = CreateContext();
        var user = new Usuario { Id = "u1", Email = "antigo@email.com", UserName = "antigo@email.com" };
        var concessionaria = new Concessionaria { Id = 1, Nome = "Nome Antigo", Cnpj = "12345678000190", UsuarioId = "u1", Usuario = user };
        context.Concessionarias.Add(concessionaria);
        await context.SaveChangesAsync();

        _mockUserManager.Setup(x => x.FindByEmailAsync("novo@email.com")).ReturnsAsync((Usuario?)null);
        _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<Usuario>())).ReturnsAsync(IdentityResult.Success);

        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new UpdateConcessionariaRequest("Novo Nome Fantasia", "novo@email.com");

        // Act
        var result = await service.UpdateAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Novo Nome Fantasia", result.Nome);
        Assert.Equal("novo@email.com", result.Email);

        var concessionariaNoDb = await context.Concessionarias.Include(c => c.Usuario).FirstAsync(c => c.Id == 1);
        Assert.Equal("Novo Nome Fantasia", concessionariaNoDb.Nome);
        Assert.Equal("novo@email.com", concessionariaNoDb.Usuario.Email);
    }

    [Fact]
    public async Task UpdateAsync_DeveLancarExcecao_QuandoEmailNovoJaExiste()
    {
        // Arrange
        using var context = CreateContext();
        var user = new Usuario { Id = "u1", Email = "antigo@email.com", UserName = "antigo@email.com" };
        var concessionaria = new Concessionaria { Id = 1, Nome = "Nome Antigo", Cnpj = "12345678000190", UsuarioId = "u1", Usuario = user };
        context.Concessionarias.Add(concessionaria);
        await context.SaveChangesAsync();

        var outroUsuario = new Usuario { Id = "u2", Email = "novo@email.com" };
        _mockUserManager.Setup(x => x.FindByEmailAsync("novo@email.com")).ReturnsAsync(outroUsuario);

        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new UpdateConcessionariaRequest("Novo Nome Fantasia", "novo@email.com");

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDataException>(() => service.UpdateAsync(1, request));
    }

    [Fact]
    public async Task UpdateAsync_DeveLancarExcecao_QuandoConcessionariaNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ConcessionariaService(context, _mockUserManager.Object);
        var request = new UpdateConcessionariaRequest("Qualquer Nome", "qualquer@email.com");

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(99, request));
    }

    [Fact]
    public async Task InativarAsync_DeveInativarConcessionariaUsuarioEEnderecos_QuandoSucesso()
    {
        // Arrange
        using var context = CreateContext();
        var user = new Usuario { Id = "u1", Email = "teste@email.com", Ativo = true };
        var concessionaria = new Concessionaria { Id = 1, Nome = "Loja X", Cnpj = "123", UsuarioId = "u1", Usuario = user, Ativo = true };
        context.Concessionarias.Add(concessionaria);
        
        var endereco = new Endereco { Id = 1, ConcessionariaId = 1, Cep = "123", Logradouro = "Rua", Numero = "1", Bairro = "B", Cidade = "C", Estado = "SP", Ativo = true };
        context.Enderecos.Add(endereco);
        
        await context.SaveChangesAsync();

        var service = new ConcessionariaService(context, _mockUserManager.Object);

        // Act
        await service.InativarAsync("u1");

        // Assert - A busca direta precisa ignorar o filtro global para enxergar o registro modificado
        var concessionariaInativada = await context.Concessionarias.IgnoreQueryFilters().FirstAsync(c => c.Id == 1);
        Assert.False(concessionariaInativada.Ativo);

        var usuarioInativado = await context.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == "u1");
        Assert.False(usuarioInativado.Ativo);

        var enderecoInativado = await context.Enderecos.IgnoreQueryFilters().FirstAsync(e => e.Id == 1);
        Assert.False(enderecoInativado.Ativo);
    }

    [Fact]
    public async Task InativarAsync_DeveLancarExcecao_QuandoConcessionariaNaoEncontrada()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ConcessionariaService(context, _mockUserManager.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.InativarAsync("u-inexistente"));
    }
}
