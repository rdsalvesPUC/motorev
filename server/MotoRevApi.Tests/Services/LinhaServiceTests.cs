using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Services;

public class LinhaServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public LinhaServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private AppDbContext CreateContext() => new AppDbContext(_dbContextOptions);

    [Fact]
    public void CadastrarLinha_DeveSalvarNoBancoERetornarResponse_QuandoValido()
    {
        // Arrange
        using var context = CreateContext();
        var service = new LinhaService(context);
        var request = new LinhaRequest("Linha Esportiva", "Descrição da linha esportiva");

        // Act
        var result = service.CadastrarLinha(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Linha Esportiva", result.Nome);
        Assert.Equal("Descrição da linha esportiva", result.Descricao);
        Assert.True(result.Ativo);

        var saved = context.Linhas.Find(result.Id);
        Assert.NotNull(saved);
        Assert.Equal("Linha Esportiva", saved.Nome);
    }

    [Fact]
    public void CadastrarLinha_DeveLancarDuplicateDataException_QuandoNomeDuplicado()
    {
        // Arrange
        using var context = CreateContext();
        var existing = new Linha { Nome = "Esportiva", Descricao = "Antiga" };
        context.Linhas.Add(existing);
        context.SaveChanges();

        var service = new LinhaService(context);
        var request = new LinhaRequest("esportiva", "Nova");

        // Act & Assert
        var exception = Assert.Throws<DuplicateDataException>(() => service.CadastrarLinha(request));
        Assert.Equal("Já existe uma linha cadastrada com este nome.", exception.Message);
    }

    [Fact]
    public void ObterLinha_DeveRetornarLinha_QuandoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var existing = new Linha { Nome = "Esportiva", Descricao = "Detalhes" };
        context.Linhas.Add(existing);
        context.SaveChanges();

        var service = new LinhaService(context);

        // Act
        var result = service.ObterLinha(existing.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Esportiva", result.Nome);
    }

    [Fact]
    public void ObterLinha_DeveLancarNotFoundException_QuandoNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new LinhaService(context);

        // Act & Assert
        Assert.Throws<NotFoundException>(() => service.ObterLinha(999));
    }

    [Fact]
    public void ListarLinhas_DeveRetornarApenasAtivos_PorPadrao()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Nome = "Ativa 1", Ativo = true });
        context.Linhas.Add(new Linha { Nome = "Inativa 2", Ativo = false });
        context.Linhas.Add(new Linha { Nome = "Ativa 3", Ativo = true });
        context.SaveChanges();

        var service = new LinhaService(context);

        // Act
        var result = service.ListarLinhas();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, l => Assert.True(l.Ativo));
    }

    [Fact]
    public void ListarLinhas_DeveRetornarTodos_QuandoEspecificado()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Nome = "Ativa 1", Ativo = true });
        context.Linhas.Add(new Linha { Nome = "Inativa 2", Ativo = false });
        context.SaveChanges();

        var service = new LinhaService(context);

        // Act
        var result = service.ListarLinhas(apenasAtivos: false);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void AtualizarLinha_DeveAtualizarNoBancoERetornarResponse_QuandoValido()
    {
        // Arrange
        using var context = CreateContext();
        var existing = new Linha { Nome = "Custom", Descricao = "Antiga" };
        context.Linhas.Add(existing);
        context.SaveChanges();

        var service = new LinhaService(context);
        var request = new LinhaRequest("Custom Refatorada", "Nova Descrição");

        // Act
        var result = service.AtualizarLinha(existing.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Custom Refatorada", result.Nome);
        Assert.Equal("Nova Descrição", result.Descricao);

        var updated = context.Linhas.Find(existing.Id);
        Assert.Equal("Custom Refatorada", updated.Nome);
    }

    [Fact]
    public void AtualizarLinha_DeveLancarDuplicateDataException_QuandoNomeDuplicadoEmOutraLinha()
    {
        // Arrange
        using var context = CreateContext();
        context.Linhas.Add(new Linha { Nome = "Custom" });
        var target = new Linha { Nome = "Adventure" };
        context.Linhas.Add(target);
        context.SaveChanges();

        var service = new LinhaService(context);
        var request = new LinhaRequest("custom", "Tentando duplicar");

        // Act & Assert
        var exception = Assert.Throws<DuplicateDataException>(() => service.AtualizarLinha(target.Id, request));
        Assert.Equal("Já existe outra linha cadastrada com este nome.", exception.Message);
    }

    [Fact]
    public void InativarLinha_DeveSetarAtivoComoFalse_QuandoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var existing = new Linha { Nome = "OffRoad", Ativo = true };
        context.Linhas.Add(existing);
        context.SaveChanges();

        var service = new LinhaService(context);

        // Act
        var result = service.InativarLinha(existing.Id);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Ativo);

        var updated = context.Linhas.Find(existing.Id);
        Assert.False(updated.Ativo);
    }

    [Fact]
    public void InativarLinha_DeveLancarNotFoundException_QuandoNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new LinhaService(context);

        // Act & Assert
        Assert.Throws<NotFoundException>(() => service.InativarLinha(999));
    }

    [Fact]
    public void AlternarStatus_DeveInativar_QuandoAtivo()
    {
        // Arrange
        using var context = CreateContext();
        var existing = new Linha { Nome = "Custom", Ativo = true };
        context.Linhas.Add(existing);
        context.SaveChanges();

        var service = new LinhaService(context);

        // Act
        var result = service.AlternarStatus(existing.Id);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Ativo);

        var updated = context.Linhas.Find(existing.Id);
        Assert.False(updated.Ativo);
    }

    [Fact]
    public void AlternarStatus_DeveAtivar_QuandoInativo()
    {
        // Arrange
        using var context = CreateContext();
        var existing = new Linha { Nome = "Custom", Ativo = false };
        context.Linhas.Add(existing);
        context.SaveChanges();

        var service = new LinhaService(context);

        // Act
        var result = service.AlternarStatus(existing.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Ativo);

        var updated = context.Linhas.Find(existing.Id);
        Assert.True(updated.Ativo);
    }

    [Fact]
    public void AlternarStatus_DeveLancarNotFoundException_QuandoNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new LinhaService(context);

        // Act & Assert
        Assert.Throws<NotFoundException>(() => service.AlternarStatus(999));
    }

    [Fact]
    public void InativarLinha_DeveLancarBusinessRuleException_QuandoTemModelosVinculados()
    {
        // Arrange
        using var context = CreateContext();
        var linha = new Linha { Nome = "Trail", Ativo = true };
        context.Linhas.Add(linha);
        context.SaveChanges();

        var modelo = new ModeloMoto { NomeModelo = "XTZ 150 Crosser", Marca = "Yamaha", LinhaId = linha.Id, Ativo = true };
        context.ModelosMotos.Add(modelo);
        context.SaveChanges();

        var service = new LinhaService(context);

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() => service.InativarLinha(linha.Id));
        Assert.Equal("Não é possível desativar uma linha com modelos de motos vinculados.", exception.Message);
    }

    [Fact]
    public void AlternarStatus_DeveLancarBusinessRuleException_QuandoInativandoLinhaComModelosVinculados()
    {
        // Arrange
        using var context = CreateContext();
        var linha = new Linha { Nome = "Trail", Ativo = true };
        context.Linhas.Add(linha);
        context.SaveChanges();

        var modelo = new ModeloMoto { NomeModelo = "XTZ 150 Crosser", Marca = "Yamaha", LinhaId = linha.Id, Ativo = true };
        context.ModelosMotos.Add(modelo);
        context.SaveChanges();

        var service = new LinhaService(context);

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() => service.AlternarStatus(linha.Id));
        Assert.Equal("Não é possível desativar uma linha com modelos de motos vinculados.", exception.Message);
    }
}
