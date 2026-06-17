using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Enums;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Services;

public class DashboardConcessionariaServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;
    private readonly DateTime _hoje = new(2026, 6, 15);

    public DashboardConcessionariaServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    private AppDbContext CreateContext() => new(_dbContextOptions);

    [Fact]
    public async Task ObterDashboardRevisoesAsync_DeveRetornarSomenteAgendamentosDoDiaDaConcessionaria()
    {
        // Arrange
        using var context = CreateContext();
        var (moto, loja) = SeedBaseData(context, "concessionaria-1", "cliente-1");
        var (motoOutraConcessionaria, lojaOutraConcessionaria) = SeedBaseData(context, "concessionaria-2", "cliente-2", idBase: 20);

        AddAgendamento(context, moto, loja, ordem: 1, dataAgendada: _hoje.AddHours(8), StatusAgendamento.Agendada);
        AddAgendamento(context, moto, loja, ordem: 2, dataAgendada: _hoje.AddHours(9), StatusAgendamento.EmExecucao);
        AddAgendamento(context, moto, loja, ordem: 3, dataAgendada: _hoje.AddDays(1), StatusAgendamento.Agendada);
        AddAgendamento(context, moto, loja, ordem: 4, dataAgendada: _hoje.AddHours(10), StatusAgendamento.AguardandoConfirmacao);
        AddAgendamento(context, motoOutraConcessionaria, lojaOutraConcessionaria, ordem: 1, dataAgendada: _hoje.AddHours(8), StatusAgendamento.Agendada);
        await context.SaveChangesAsync();

        var service = new DashboardConcessionariaService(context, () => _hoje);

        // Act
        var result = await service.ObterDashboardRevisoesAsync("concessionaria-1");

        // Assert
        Assert.Equal(_hoje.Date, result.Data);
        Assert.Equal(4, result.TotalMecanicos);
        Assert.Equal(2, result.TotalRevisoes);
        Assert.Equal(1, result.RevisoesAgendadas);
        Assert.Equal(1, result.RevisoesEmExecucao);
        Assert.Equal(0, result.RevisoesConcluidas);
        Assert.Equal(2, result.Filas.Sum(f => f.Itens.Count));
        Assert.All(result.Filas.SelectMany(f => f.Itens), item => Assert.Equal(loja.Id, item.LojaId));
    }

    [Fact]
    public async Task ObterDashboardRevisoesAsync_DeveDistribuirFilaDeFormaBalanceadaPorOrdemDeChegada()
    {
        // Arrange
        using var context = CreateContext();
        var (moto, loja) = SeedBaseData(context, "concessionaria-1", "cliente-1");

        for (var i = 1; i <= 6; i++)
        {
            AddAgendamento(context, moto, loja, ordem: i, dataAgendada: _hoje.AddHours(8).AddMinutes(i), StatusAgendamento.Agendada);
        }

        await context.SaveChangesAsync();
        var service = new DashboardConcessionariaService(context, () => _hoje);

        // Act
        var result = await service.ObterDashboardRevisoesAsync("concessionaria-1");

        // Assert
        Assert.Equal([2, 2, 1, 1], result.Filas.Select(f => f.Itens.Count).ToArray());
        Assert.Equal(1, result.Filas[0].Itens[0].NumeroRevisao);
        Assert.Equal(5, result.Filas[0].Itens[1].NumeroRevisao);
        Assert.Equal(2, result.Filas[1].Itens[0].NumeroRevisao);
        Assert.All(result.Filas.SelectMany(f => f.Itens), item => Assert.True(item.PosicaoFila >= 1));
    }

    [Fact]
    public async Task ObterDashboardRevisoesAsync_DeveCalcularProgressoConformeStatus()
    {
        // Arrange
        using var context = CreateContext();
        var (moto, loja) = SeedBaseData(context, "concessionaria-1", "cliente-1");

        AddAgendamento(context, moto, loja, ordem: 1, dataAgendada: _hoje.AddHours(8), StatusAgendamento.Agendada);
        AddAgendamento(context, moto, loja, ordem: 2, dataAgendada: _hoje.AddHours(9), StatusAgendamento.EmExecucao);
        AddAgendamento(context, moto, loja, ordem: 3, dataAgendada: _hoje.AddHours(10), StatusAgendamento.Concluida);
        await context.SaveChangesAsync();

        var service = new DashboardConcessionariaService(context, () => _hoje);

        // Act
        var result = await service.ObterDashboardRevisoesAsync("concessionaria-1");
        var itens = result.Filas.SelectMany(f => f.Itens).OrderBy(i => i.NumeroRevisao).ToList();

        // Assert
        Assert.Equal(0, itens[0].ProgressoPercentual);
        Assert.True(itens[1].ProgressoPercentual is > 0 and < 100);
        Assert.Equal(100, itens[2].ProgressoPercentual);
        Assert.Equal(itens[2].TotalItens, itens[2].ItensConcluidos);
    }

    [Fact]
    public async Task ObterDashboardRevisoesAsync_DeveFalharQuandoConcessionariaNaoExiste()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DashboardConcessionariaService(context, () => _hoje);

        // Act / Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ObterDashboardRevisoesAsync("concessionaria-inexistente"));
    }

    private static (Moto Moto, Loja Loja) SeedBaseData(
        AppDbContext context,
        string concessionariaUserId,
        string clienteUserId,
        int idBase = 1)
    {
        var linha = new Linha { Id = idBase, Nome = $"Linha {idBase}", Ativo = true };
        var modelo = new ModeloMoto
        {
            Id = idBase,
            NomeModelo = $"Modelo {idBase}",
            Marca = "Honda",
            LinhaId = linha.Id,
            Linha = linha,
            Ativo = true
        };
        var cliente = new Cliente { Id = idBase, Nome = $"Cliente {idBase}", UsuarioId = clienteUserId };
        var moto = new Moto
        {
            Id = idBase,
            Placa = $"ABC{idBase:D4}",
            Chassi = $"9SB000000000{idBase:D6}",
            ClienteId = cliente.Id,
            Cliente = cliente,
            ModeloMotoId = modelo.Id,
            ModeloMoto = modelo,
            Cor = "Preta",
            KilometragemAtual = 1000,
            DataVenda = new DateTime(2025, 12, 15),
            Ativo = true
        };
        var concessionaria = new Concessionaria
        {
            Id = idBase,
            Nome = $"Concessionaria {idBase}",
            Cnpj = $"000000000{idBase:D5}",
            Telefone = $"4199999{idBase:D4}",
            UsuarioId = concessionariaUserId
        };
        var loja = new Loja
        {
            Id = idBase,
            Nome = $"Loja {idBase}",
            Tipo = "Matriz",
            Cnpj = $"100000000{idBase:D5}",
            Telefone = $"4198888{idBase:D4}",
            Cep = "80000000",
            Logradouro = "Rua Centro",
            Numero = "100",
            Bairro = "Centro",
            Cidade = "Curitiba",
            Uf = "PR",
            Ativo = true,
            ConcessionariaId = concessionaria.Id,
            Concessionaria = concessionaria
        };

        context.Linhas.Add(linha);
        context.ModelosMotos.Add(modelo);
        context.Clientes.Add(cliente);
        context.Motos.Add(moto);
        context.Concessionarias.Add(concessionaria);
        context.Lojas.Add(loja);
        context.SaveChanges();
        return (moto, loja);
    }

    private static void AddAgendamento(
        AppDbContext context,
        Moto moto,
        Loja loja,
        int ordem,
        DateTime dataAgendada,
        StatusAgendamento status)
    {
        var servico = new Servico
        {
            Id = (loja.Id * 1000) + ordem,
            Codigo = $"SERV-{loja.Id}-{ordem}",
            Nome = $"Servico {ordem}",
            Descricao = "Servico de revisao",
            Categoria = CategoriaServico.Verificacao,
            TempoEstimado = 30,
            Custo = 100m,
            Ativo = true
        };
        var peca = new Peca
        {
            Id = (loja.Id * 10000) + ordem,
            Codigo = $"PECA-{loja.Id}-{ordem}",
            Nome = $"Peca {ordem}",
            Categoria = CategoriaPeca.Motor,
            Preco = 50m,
            Estoque = 10,
            Status = StatusCadastro.Ativo
        };
        var revisaoPadrao = new RevisaoPadrao
        {
            Id = (loja.Id * 100) + ordem,
            Nome = $"{ordem} revisao",
            Ordem = ordem,
            Quilometragem = ordem * 1000,
            TempoMeses = ordem * 6,
            LinhaId = moto.ModeloMoto.LinhaId,
            Ativo = true,
            Servicos = new List<RevisaoPadraoServico>
            {
                new() { ServicoId = servico.Id, Servico = servico }
            },
            Pecas = new List<RevisaoPadraoPeca>
            {
                new() { PecaId = peca.Id, Peca = peca, Quantidade = 1 }
            }
        };
        var revisaoMoto = new RevisaoMoto
        {
            MotoId = moto.Id,
            RevisaoPadraoId = revisaoPadrao.Id,
            RevisaoPadrao = revisaoPadrao,
            Nome = revisaoPadrao.Nome,
            Ordem = ordem,
            Quilometragem = revisaoPadrao.Quilometragem,
            TempoMeses = revisaoPadrao.TempoMeses,
            DataPrevista = dataAgendada.Date,
            Status = status == StatusAgendamento.Concluida ? "Concluida" : "Planejada"
        };
        var agendamento = new Agendamento
        {
            RevisaoMoto = revisaoMoto,
            LojaId = loja.Id,
            DataAgendada = dataAgendada,
            Status = status,
            CriadoEm = dataAgendada.AddMinutes(-ordem),
            AtualizadoEm = dataAgendada.AddMinutes(-ordem)
        };

        context.Servicos.Add(servico);
        context.Pecas.Add(peca);
        context.RevisoesPadrao.Add(revisaoPadrao);
        context.RevisoesMotos.Add(revisaoMoto);
        context.Agendamentos.Add(agendamento);
    }
}
