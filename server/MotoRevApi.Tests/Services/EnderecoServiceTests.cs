using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Services;

public class EnderecoServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public EnderecoServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private AppDbContext CreateContext() => new AppDbContext(_dbContextOptions);

    [Fact]
    public async Task AdicionarEnderecoAsync_DeveSalvarERetornarEndereco_QuandoConcessionariaExiste()
    {
        // Arrange
        using var context = CreateContext();
        var concessionaria = new Concessionaria { Id = 1, Nome = "Loja Teste", Cnpj = "12345678000190", UsuarioId = "u1" };
        context.Concessionarias.Add(concessionaria);
        await context.SaveChangesAsync();

        var service = new EnderecoService(context);
        var request = new EnderecoRequest("01001000", "Praça da Sé", "10", "Lado Ímpar", "Sé", "São Paulo", "SP");

        // Act
        var result = await service.AdicionarEnderecoAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Praça da Sé", result.Logradouro);
        Assert.Equal(1, await context.Enderecos.CountAsync());
    }

    [Fact]
    public async Task AdicionarEnderecoAsync_DeveLancarNotFoundException_QuandoConcessionariaNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EnderecoService(context);
        var request = new EnderecoRequest("01001000", "Praça da Sé", "10", null, "Sé", "São Paulo", "SP");

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.AdicionarEnderecoAsync(99, request));
    }

    [Fact]
    public async Task RemoverEnderecoAsync_DeveRemover_QuandoEnderecoExisteEPertenceAConcessionaria()
    {
        // Arrange
        using var context = CreateContext();
        var endereco = new Endereco 
        { 
            Id = 1, ConcessionariaId = 1, Cep = "01001000", Logradouro = "Rua A", Numero = "1", Bairro = "B", Cidade = "C", Estado = "SP" 
        };
        context.Enderecos.Add(endereco);
        await context.SaveChangesAsync();

        var service = new EnderecoService(context);

        // Act
        await service.RemoverEnderecoAsync(1, 1);

        // Assert
        Assert.Equal(0, await context.Enderecos.CountAsync());
    }

    [Fact]
    public async Task RemoverEnderecoAsync_DeveLancarNotFoundException_QuandoEnderecoNaoPertenceAConcessionaria()
    {
        // Arrange
        using var context = CreateContext();
        var endereco = new Endereco 
        { 
            Id = 1, ConcessionariaId = 2, Cep = "01001000", Logradouro = "Rua A", Numero = "1", Bairro = "B", Cidade = "C", Estado = "SP" 
        };
        context.Enderecos.Add(endereco);
        await context.SaveChangesAsync();

        var service = new EnderecoService(context);

        // Act & Assert
        // Tenta apagar um endereço que pertence a concessionária 2 usando a concessionária 1
        await Assert.ThrowsAsync<NotFoundException>(() => service.RemoverEnderecoAsync(1, 1));
    }
}
