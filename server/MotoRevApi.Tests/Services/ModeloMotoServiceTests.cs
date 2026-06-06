using Mapster;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Model;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Services;

public class ModeloMotoServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public ModeloMotoServiceTests()
    {
        // Usa um banco de dados em memória, criando um novo para cada teste rodado
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private AppDbContext CreateContext() => new AppDbContext(_dbContextOptions);

    [Fact]
    public void CadastrarModeloMoto_DeveSalvarNoBancoERetornarResponse()
    {
        // Arrange
        using var context = CreateContext();
        var linha = new Linha { Nome = "Ninja", Ativo = true };
        context.Linhas.Add(linha);
        context.SaveChanges();

        var service = new ModeloMotoService(context);
        var request = new ModeloMotoRequest("Ninja", "Kawasaki", "Esportiva", linha.Id, "400cc", 2023);

        // Act
        var result = service.CadastrarModeloMoto(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Ninja", result.NomeModelo);
        Assert.Equal(linha.Id, result.LinhaId);
        Assert.True(result.Ativo);

        // Verifica se realmente salvou no banco
        var savedModel = context.ModelosMotos.FirstOrDefault(m => m.Id == result.Id);
        Assert.NotNull(savedModel);
        Assert.Equal("Kawasaki", savedModel.Marca);
        Assert.Equal(linha.Id, savedModel.LinhaId);
    }

    [Fact]
    public void ObterModeloMoto_DeveRetornarModelo_QuandoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var linha = new Linha { Nome = "Linha R1", Ativo = true };
        context.Linhas.Add(linha);
        context.SaveChanges();

        var modelo = new ModeloMoto { NomeModelo = "R1", Marca = "Yamaha", Categoria = "Esportiva", LinhaId = linha.Id, Ativo = true };
        context.ModelosMotos.Add(modelo);
        context.SaveChanges();

        var service = new ModeloMotoService(context);

        // Act
        var result = service.ObterModeloMoto(modelo.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("R1", result.NomeModelo);
    }

    [Fact]
    public void ObterModeloMoto_DeveLancarNotFoundException_QuandoNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ModeloMotoService(context);

        // Act & Assert
        Assert.Throws<MotoRevApi.Exceptions.NotFoundException>(() => service.ObterModeloMoto(999));
    }

    [Fact]
    public void ListarModelosMotos_DeveRetornarTodos()
    {
        // Arrange
        using var context = CreateContext();
        context.ModelosMotos.Add(new ModeloMoto { NomeModelo = "R1", Marca = "Yamaha", Categoria = "Esportiva", Ativo = true });
        context.ModelosMotos.Add(new ModeloMoto { NomeModelo = "Ninja", Marca = "Kawasaki", Categoria = "Esportiva", Ativo = false });
        context.SaveChanges();

        var service = new ModeloMotoService(context);

        // Act
        var result = service.ListarModelosMotos();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void AtualizarModeloMoto_DeveAtualizarERetornarResponse_QuandoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var linha = new Linha { Nome = "YZF", Ativo = true };
        context.Linhas.Add(linha);
        var modelo = new ModeloMoto { NomeModelo = "R1", Marca = "Yamaha", Categoria = "Esportiva", Ativo = true };
        context.ModelosMotos.Add(modelo);
        context.SaveChanges();

        var service = new ModeloMotoService(context);
        var request = new ModeloMotoRequest("R1 M", "Yamaha", "Super Esportiva", linha.Id, "1000cc", 2024);

        // Act
        var result = service.AtualizarModeloMoto(modelo.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("R1 M", result.NomeModelo);
        Assert.Equal("Super Esportiva", result.Categoria);
        Assert.Equal(linha.Id, result.LinhaId);

        // Verifica o banco de dados
        var updatedModel = context.ModelosMotos.Find(modelo.Id);
        Assert.NotNull(updatedModel);
        Assert.Equal("R1 M", updatedModel.NomeModelo);
        Assert.Equal(linha.Id, updatedModel.LinhaId);
    }

    [Fact]
    public void AtualizarModeloMoto_DeveLancarNotFoundException_QuandoNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ModeloMotoService(context);
        var request = new ModeloMotoRequest("R1 M", "Yamaha", "Super Esportiva", 1, null, null);

        // Act & Assert
        Assert.Throws<MotoRevApi.Exceptions.NotFoundException>(() => service.AtualizarModeloMoto(999, request));
    }

    [Fact]
    public void AlternarStatus_DeveInverterOStatus_QuandoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var modelo = new ModeloMoto { NomeModelo = "R1", Marca = "Yamaha", Categoria = "Esportiva", Ativo = true };
        context.ModelosMotos.Add(modelo);
        context.SaveChanges();

        var service = new ModeloMotoService(context);

        // Act 1 - Desativar
        var result1 = service.AlternarStatus(modelo.Id);

        // Assert 1
        Assert.NotNull(result1);
        Assert.False(result1.Ativo);

        // Act 2 - Ativar novamente
        var result2 = service.AlternarStatus(modelo.Id);

        // Assert 2
        Assert.NotNull(result2);
        Assert.True(result2.Ativo);
    }

    [Fact]
    public void AlternarStatus_DeveLancarNotFoundException_QuandoNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ModeloMotoService(context);

        // Act & Assert
        Assert.Throws<MotoRevApi.Exceptions.NotFoundException>(() => service.AlternarStatus(999));
    }

    [Fact]
    public void CadastrarModeloMoto_DeveLancarNotFoundException_QuandoLinhaIdNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ModeloMotoService(context);
        var request = new ModeloMotoRequest("Ninja", "Kawasaki", "Esportiva", 999, "400cc", 2023);

        // Act & Assert
        Assert.Throws<MotoRevApi.Exceptions.NotFoundException>(() => service.CadastrarModeloMoto(request));
    }

    [Fact]
    public void AtualizarModeloMoto_DeveLancarNotFoundException_QuandoLinhaIdNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var modelo = new ModeloMoto { NomeModelo = "R1", Marca = "Yamaha", Categoria = "Esportiva", Ativo = true };
        context.ModelosMotos.Add(modelo);
        context.SaveChanges();

        var service = new ModeloMotoService(context);
        var request = new ModeloMotoRequest("R1", "Yamaha", "Esportiva", 999, "1000cc", 2023);

        // Act & Assert
        Assert.Throws<MotoRevApi.Exceptions.NotFoundException>(() => service.AtualizarModeloMoto(modelo.Id, request));
    }
}
