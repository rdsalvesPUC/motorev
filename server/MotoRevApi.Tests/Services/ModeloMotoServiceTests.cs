using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Services;

public class ModeloMotoServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public ModeloMotoServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private AppDbContext CreateContext() => new(_dbContextOptions);

    private static Linha SeedLinha(AppDbContext context, bool ativo = true)
    {
        var linha = new Linha { Nome = ativo ? "Linha Ativa" : "Linha Inativa", Ativo = ativo };
        context.Linhas.Add(linha);
        context.SaveChanges();
        return linha;
    }

    [Fact]
    public void CadastrarModeloMoto_DeveSalvarNoBancoERetornarResponse()
    {
        using var context = CreateContext();
        var linha = SeedLinha(context);
        var service = new ModeloMotoService(context);
        var request = new ModeloMotoRequest(" Ninja 400 ", " Kawasaki ", linha.Id, " 400cc ", 2023);

        var result = service.CadastrarModeloMoto(request);

        Assert.NotNull(result);
        Assert.Equal("Ninja 400", result.NomeModelo);
        Assert.Equal("Kawasaki", result.Marca);
        Assert.Equal(linha.Id, result.LinhaId);
        Assert.Equal("400cc", result.Cilindrada);
        Assert.True(result.Ativo);

        var savedModel = context.ModelosMotos.FirstOrDefault(m => m.Id == result.Id);
        Assert.NotNull(savedModel);
        Assert.Equal("Ninja 400", savedModel.NomeModelo);
        Assert.Equal(linha.Id, savedModel.LinhaId);
    }

    [Fact]
    public void CadastrarModeloMoto_DeveLancarDuplicateDataException_QuandoNomeAtivoDuplicado()
    {
        using var context = CreateContext();
        var linha = SeedLinha(context);
        context.ModelosMotos.Add(new ModeloMoto { NomeModelo = "Ninja 400", Marca = "Kawasaki", LinhaId = linha.Id, Ativo = true });
        context.SaveChanges();

        var service = new ModeloMotoService(context);
        var request = new ModeloMotoRequest(" ninja 400 ", "Kawasaki", linha.Id, "400cc", 2024);

        Assert.Throws<DuplicateDataException>(() => service.CadastrarModeloMoto(request));
    }

    [Fact]
    public void CadastrarModeloMoto_DevePermitirMesmoNomeDeModeloInativo()
    {
        using var context = CreateContext();
        var linha = SeedLinha(context);
        context.ModelosMotos.Add(new ModeloMoto { NomeModelo = "Ninja 400", Marca = "Kawasaki", LinhaId = linha.Id, Ativo = false });
        context.SaveChanges();

        var service = new ModeloMotoService(context);
        var request = new ModeloMotoRequest("Ninja 400", "Kawasaki", linha.Id, "400cc", 2024);

        var result = service.CadastrarModeloMoto(request);

        Assert.True(result.Ativo);
        Assert.Equal(2, context.ModelosMotos.Count());
    }

    [Fact]
    public void CadastrarModeloMoto_DeveLancarNotFoundException_QuandoLinhaNaoExisteOuEstaInativa()
    {
        using var context = CreateContext();
        var linhaInativa = SeedLinha(context, ativo: false);
        var service = new ModeloMotoService(context);
        var request = new ModeloMotoRequest("Ninja", "Kawasaki", linhaInativa.Id, "400cc", 2023);

        Assert.Throws<NotFoundException>(() => service.CadastrarModeloMoto(request));
    }

    [Fact]
    public void ObterModeloMoto_DeveRetornarModelo_QuandoExiste()
    {
        using var context = CreateContext();
        var linha = SeedLinha(context);
        var modelo = new ModeloMoto { NomeModelo = "R1", Marca = "Yamaha", LinhaId = linha.Id, Ativo = true };
        context.ModelosMotos.Add(modelo);
        context.SaveChanges();

        var service = new ModeloMotoService(context);

        var result = service.ObterModeloMoto(modelo.Id);

        Assert.NotNull(result);
        Assert.Equal("R1", result.NomeModelo);
    }

    [Fact]
    public void ObterModeloMoto_DeveLancarNotFoundException_QuandoNaoExiste()
    {
        using var context = CreateContext();
        var service = new ModeloMotoService(context);

        Assert.Throws<NotFoundException>(() => service.ObterModeloMoto(999));
    }

    [Fact]
    public void ListarModelosMotos_DeveRetornarApenasAtivosPorPadrao()
    {
        using var context = CreateContext();
        var linha = SeedLinha(context);
        context.ModelosMotos.Add(new ModeloMoto { NomeModelo = "R1", Marca = "Yamaha", LinhaId = linha.Id, Ativo = true });
        context.ModelosMotos.Add(new ModeloMoto { NomeModelo = "Ninja", Marca = "Kawasaki", LinhaId = linha.Id, Ativo = false });
        context.SaveChanges();

        var service = new ModeloMotoService(context);

        var result = service.ListarModelosMotos();

        var singleModel = Assert.Single(result);
        Assert.True(singleModel.Ativo);
        Assert.Equal("R1", singleModel.NomeModelo);
    }

    [Fact]
    public void ListarModelosMotos_DeveRetornarTodos_QuandoApenasAtivosFalse()
    {
        using var context = CreateContext();
        var linha = SeedLinha(context);
        context.ModelosMotos.Add(new ModeloMoto { NomeModelo = "R1", Marca = "Yamaha", LinhaId = linha.Id, Ativo = true });
        context.ModelosMotos.Add(new ModeloMoto { NomeModelo = "Ninja", Marca = "Kawasaki", LinhaId = linha.Id, Ativo = false });
        context.SaveChanges();

        var service = new ModeloMotoService(context);

        var result = service.ListarModelosMotos(apenasAtivos: false);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ListarModelosDisponiveisParaCadastro_DeveRetornarApenasAtivosComRevisaoPadraoAtiva()
    {
        using var context = CreateContext();
        var linhaComRevisao = SeedLinha(context);
        var linhaSemRevisao = new Linha { Nome = "Linha Sem Revisao", Ativo = true };
        var linhaComRevisaoInativa = new Linha { Nome = "Linha Revisao Inativa", Ativo = true };
        context.Linhas.AddRange(linhaSemRevisao, linhaComRevisaoInativa);
        context.SaveChanges();

        context.ModelosMotos.AddRange(
            new ModeloMoto { NomeModelo = "Apto", Marca = "Honda", LinhaId = linhaComRevisao.Id, Ativo = true },
            new ModeloMoto { NomeModelo = "Sem Plano", Marca = "Yamaha", LinhaId = linhaSemRevisao.Id, Ativo = true },
            new ModeloMoto { NomeModelo = "Modelo Inativo", Marca = "Honda", LinhaId = linhaComRevisao.Id, Ativo = false },
            new ModeloMoto { NomeModelo = "Plano Inativo", Marca = "BMW", LinhaId = linhaComRevisaoInativa.Id, Ativo = true }
        );
        context.RevisoesPadrao.AddRange(
            new RevisaoPadrao { Nome = "Primeira revisão", Ordem = 1, LinhaId = linhaComRevisao.Id, Ativo = true },
            new RevisaoPadrao { Nome = "Revisão inativa", Ordem = 1, LinhaId = linhaComRevisaoInativa.Id, Ativo = false }
        );
        context.SaveChanges();

        var service = new ModeloMotoService(context);

        var result = service.ListarModelosDisponiveisParaCadastro();

        var modelo = Assert.Single(result);
        Assert.Equal("Apto", modelo.NomeModelo);
    }

    [Fact]
    public void ListarCatalogoModelosMotos_DeveRetornarAtivosEInativos()
    {
        using var context = CreateContext();
        var linha = SeedLinha(context);
        context.ModelosMotos.Add(new ModeloMoto { NomeModelo = "R1", Marca = "Yamaha", LinhaId = linha.Id, Ativo = true });
        context.ModelosMotos.Add(new ModeloMoto { NomeModelo = "Ninja", Marca = "Kawasaki", LinhaId = linha.Id, Ativo = false });
        context.SaveChanges();

        var service = new ModeloMotoService(context);

        var result = service.ListarCatalogoModelosMotos();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, modelo => modelo.NomeModelo == "R1" && modelo.Ativo);
        Assert.Contains(result, modelo => modelo.NomeModelo == "Ninja" && !modelo.Ativo);
    }

    [Fact]
    public void ListarCatalogoModelosMotos_DeveFiltrarPorStatus()
    {
        using var context = CreateContext();
        var linha = SeedLinha(context);
        context.ModelosMotos.Add(new ModeloMoto { NomeModelo = "R1", Marca = "Yamaha", LinhaId = linha.Id, Ativo = true });
        context.ModelosMotos.Add(new ModeloMoto { NomeModelo = "Ninja", Marca = "Kawasaki", LinhaId = linha.Id, Ativo = false });
        context.SaveChanges();

        var service = new ModeloMotoService(context);

        var result = service.ListarCatalogoModelosMotos(ativo: false);

        var singleModel = Assert.Single(result);
        Assert.False(singleModel.Ativo);
        Assert.Equal("Ninja", singleModel.NomeModelo);
    }

    [Fact]
    public void AtualizarModeloMoto_DeveAtualizarERetornarResponse_QuandoExiste()
    {
        using var context = CreateContext();
        var linha = SeedLinha(context);
        var modelo = new ModeloMoto { NomeModelo = "R1", Marca = "Yamaha", LinhaId = linha.Id, Ativo = true };
        context.ModelosMotos.Add(modelo);
        context.SaveChanges();

        var service = new ModeloMotoService(context);
        var request = new ModeloMotoRequest("R1 M", "Yamaha", linha.Id, "1000cc", 2024);

        var result = service.AtualizarModeloMoto(modelo.Id, request);

        Assert.NotNull(result);
        Assert.Equal("R1 M", result.NomeModelo);
        Assert.Equal(linha.Id, result.LinhaId);

        var updatedModel = context.ModelosMotos.Find(modelo.Id);
        Assert.NotNull(updatedModel);
        Assert.Equal("R1 M", updatedModel.NomeModelo);
        Assert.Equal(linha.Id, updatedModel.LinhaId);
    }

    [Fact]
    public void AtualizarModeloMoto_DeveLancarNotFoundException_QuandoNaoExiste()
    {
        using var context = CreateContext();
        var linha = SeedLinha(context);
        var service = new ModeloMotoService(context);
        var request = new ModeloMotoRequest("R1 M", "Yamaha", linha.Id, null, null);

        Assert.Throws<NotFoundException>(() => service.AtualizarModeloMoto(999, request));
    }

    [Fact]
    public void AtualizarModeloMoto_DeveLancarNotFoundException_QuandoLinhaNaoExisteOuEstaInativa()
    {
        using var context = CreateContext();
        var linha = SeedLinha(context);
        var linhaInativa = SeedLinha(context, ativo: false);
        var modelo = new ModeloMoto { NomeModelo = "R1", Marca = "Yamaha", LinhaId = linha.Id, Ativo = true };
        context.ModelosMotos.Add(modelo);
        context.SaveChanges();

        var service = new ModeloMotoService(context);
        var request = new ModeloMotoRequest("R1", "Yamaha", linhaInativa.Id, "1000cc", 2023);

        Assert.Throws<NotFoundException>(() => service.AtualizarModeloMoto(modelo.Id, request));
    }

    [Fact]
    public void AlternarStatus_DeveInverterOStatus_QuandoExiste()
    {
        using var context = CreateContext();
        var linha = SeedLinha(context);
        var modelo = new ModeloMoto { NomeModelo = "R1", Marca = "Yamaha", LinhaId = linha.Id, Ativo = true };
        context.ModelosMotos.Add(modelo);
        context.SaveChanges();

        var service = new ModeloMotoService(context);

        var result1 = service.AlternarStatus(modelo.Id);
        var result2 = service.AlternarStatus(modelo.Id);

        Assert.False(result1.Ativo);
        Assert.True(result2.Ativo);
    }

    [Fact]
    public void AlternarStatus_DeveLancarDuplicateDataException_QuandoReativarComNomeAtivoDuplicado()
    {
        using var context = CreateContext();
        var linha = SeedLinha(context);
        var modeloAtivo = new ModeloMoto { NomeModelo = "R1", Marca = "Yamaha", LinhaId = linha.Id, Ativo = true };
        var modeloInativo = new ModeloMoto { NomeModelo = "R1", Marca = "Yamaha", LinhaId = linha.Id, Ativo = false };
        context.ModelosMotos.AddRange(modeloAtivo, modeloInativo);
        context.SaveChanges();

        var service = new ModeloMotoService(context);

        Assert.Throws<DuplicateDataException>(() => service.AlternarStatus(modeloInativo.Id));
    }

    [Fact]
    public void AlternarStatus_DeveLancarNotFoundException_QuandoNaoExiste()
    {
        using var context = CreateContext();
        var service = new ModeloMotoService(context);

        Assert.Throws<NotFoundException>(() => service.AlternarStatus(999));
    }
}
