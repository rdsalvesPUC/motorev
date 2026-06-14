using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Enums;
using MotoRevApi.Model;
using MotoRevApi.Services;
using Xunit;

namespace MotoRevApi.Tests.Services;

public class AgendamentoServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;
    private readonly DateTime _hoje = new(2026, 6, 14);

    public AgendamentoServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    private AppDbContext CreateContext() => new(_dbContextOptions);

    [Fact]
    public async Task ListarAgendamentosClienteAsync_DeveExibirSomenteRevisoesDentroDaToleranciaOuComAgendamentoAtivo()
    {
        // Arrange
        using var context = CreateContext();
        var (moto, loja) = SeedBaseData(context, "cliente-user");

        AddRevisao(context, moto, ordem: 1, dataIdeal: _hoje.AddDays(16)); // Planejada
        AddRevisao(context, moto, ordem: 2, dataIdeal: _hoje.AddDays(15)); // D-15
        AddRevisao(context, moto, ordem: 3, dataIdeal: _hoje.AddDays(-16)); // Perdida
        var revisaoAgendada = AddRevisao(context, moto, ordem: 4, dataIdeal: _hoje.AddDays(5));
        var revisaoEmExecucao = AddRevisao(context, moto, ordem: 5, dataIdeal: _hoje.AddDays(-30)); // Fora da tolerancia, mas em execução
        var revisaoConcluida = AddRevisao(context, moto, ordem: 6, dataIdeal: _hoje);
        revisaoConcluida.Status = "Concluida";

        context.Agendamentos.AddRange(
            new Agendamento
            {
                RevisaoMotoId = revisaoAgendada.Id,
                LojaId = loja.Id,
                DataAgendada = _hoje.AddDays(2),
                Status = StatusAgendamento.Agendada,
                CriadoEm = _hoje.AddDays(-1),
                AtualizadoEm = _hoje.AddDays(-1)
            },
            new Agendamento
            {
                RevisaoMotoId = revisaoEmExecucao.Id,
                LojaId = loja.Id,
                DataAgendada = _hoje,
                Status = StatusAgendamento.EmExecucao,
                CriadoEm = _hoje.AddDays(-1),
                AtualizadoEm = _hoje.AddDays(-1)
            });
        await context.SaveChangesAsync();

        var service = new AgendamentoService(context, () => _hoje);

        // Act
        var result = await service.ListarAgendamentosClienteAsync("cliente-user");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, item => item.NumeroRevisao is 1 or 3 or 6);
        Assert.Contains(result, item => item.NumeroRevisao == 2 && item.Status == "aguardando_agendamento");
        Assert.Contains(result, item => item.NumeroRevisao == 4 && item.Status == "agendada" && item.NomeLoja == "Loja Centro");
        Assert.Contains(result, item => item.NumeroRevisao == 5 && item.Status == "em_execucao");
    }

    [Fact]
    public async Task ListarAgendamentosClienteAsync_DeveMarcarComoAtrasada_QuandoAgendamentoFoiPerdidoDentroDaTolerancia()
    {
        // Arrange
        using var context = CreateContext();
        var (moto, loja) = SeedBaseData(context, "cliente-user");
        var revisao = AddRevisao(context, moto, ordem: 1, dataIdeal: _hoje);
        context.Agendamentos.Add(new Agendamento
        {
            RevisaoMotoId = revisao.Id,
            LojaId = loja.Id,
            DataAgendada = _hoje.AddDays(-1),
            Status = StatusAgendamento.Agendada,
            CriadoEm = _hoje.AddDays(-5),
            AtualizadoEm = _hoje.AddDays(-5)
        });
        await context.SaveChangesAsync();

        var service = new AgendamentoService(context, () => _hoje);

        // Act
        var result = await service.ListarAgendamentosClienteAsync("cliente-user");

        // Assert
        var item = Assert.Single(result);
        Assert.Equal("atrasada", item.Status);
        Assert.Equal(_hoje.AddDays(15), item.DataLimite);
    }

    [Fact]
    public async Task ListarAgendamentosClienteAsync_NaoDeveRetornarAgendamentosDeOutroCliente()
    {
        // Arrange
        using var context = CreateContext();
        var (motoCliente, _) = SeedBaseData(context, "cliente-user", clienteId: 1, motoId: 1);
        var (motoOutroCliente, _) = SeedBaseData(context, "outro-user", clienteId: 2, motoId: 2);
        AddRevisao(context, motoCliente, ordem: 1, dataIdeal: _hoje);
        AddRevisao(context, motoOutroCliente, ordem: 1, dataIdeal: _hoje);
        await context.SaveChangesAsync();

        var service = new AgendamentoService(context, () => _hoje);

        // Act
        var result = await service.ListarAgendamentosClienteAsync("cliente-user");

        // Assert
        var item = Assert.Single(result);
        Assert.Equal(motoCliente.Id, item.MotoId);
    }

    private static (Moto Moto, Loja Loja) SeedBaseData(
        AppDbContext context,
        string userId,
        int clienteId = 1,
        int motoId = 1)
    {
        var linha = new Linha { Id = clienteId, Nome = $"Linha {clienteId}", Ativo = true };
        var modelo = new ModeloMoto
        {
            Id = clienteId,
            NomeModelo = $"Modelo {clienteId}",
            Marca = "Honda",
            LinhaId = linha.Id,
            Linha = linha,
            Ativo = true
        };
        var cliente = new Cliente { Id = clienteId, Nome = $"Cliente {clienteId}", UsuarioId = userId };
        var moto = new Moto
        {
            Id = motoId,
            Placa = $"ABC{clienteId}234",
            Chassi = $"9SB0000000000000{clienteId}",
            ClienteId = cliente.Id,
            Cliente = cliente,
            ModeloMotoId = modelo.Id,
            ModeloMoto = modelo,
            Cor = "Preta",
            KilometragemAtual = 1000,
            DataVenda = new DateTime(2025, 12, 14),
            Ativo = true
        };
        var concessionaria = new Concessionaria
        {
            Id = clienteId,
            Nome = $"Concessionaria {clienteId}",
            Cnpj = $"0000000000000{clienteId}",
            Telefone = $"4199999000{clienteId}",
            UsuarioId = $"concessionaria-{clienteId}"
        };
        var loja = new Loja
        {
            Id = clienteId,
            Nome = "Loja Centro",
            Tipo = "Matriz",
            Cnpj = $"1000000000000{clienteId}",
            Telefone = $"4198888000{clienteId}",
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

    private static RevisaoMoto AddRevisao(AppDbContext context, Moto moto, int ordem, DateTime dataIdeal)
    {
        var servico = new Servico
        {
            Id = (moto.Id * 1000) + ordem,
            Codigo = $"SERV-{moto.Id}-{ordem}",
            Nome = $"Servico {ordem}",
            Descricao = "Servico de revisao",
            Categoria = CategoriaServico.Verificacao,
            TempoEstimado = 30,
            Custo = 100m,
            Ativo = true
        };
        var revisaoPadrao = new RevisaoPadrao
        {
            Id = (moto.Id * 100) + ordem,
            Nome = $"{ordem} revisao",
            Ordem = ordem,
            Quilometragem = ordem * 1000,
            TempoMeses = ordem * 6,
            LinhaId = moto.ModeloMoto.LinhaId,
            Ativo = true,
            Servicos = new List<RevisaoPadraoServico>
            {
                new() { ServicoId = servico.Id, Servico = servico }
            }
        };
        var revisaoMoto = new RevisaoMoto
        {
            MotoId = moto.Id,
            Moto = moto,
            RevisaoPadraoId = revisaoPadrao.Id,
            RevisaoPadrao = revisaoPadrao,
            Nome = revisaoPadrao.Nome,
            Ordem = ordem,
            Quilometragem = revisaoPadrao.Quilometragem,
            TempoMeses = revisaoPadrao.TempoMeses,
            DataPrevista = dataIdeal,
            Status = "Planejada"
        };

        context.Servicos.Add(servico);
        context.RevisoesPadrao.Add(revisaoPadrao);
        context.RevisoesMotos.Add(revisaoMoto);
        context.SaveChanges();
        return revisaoMoto;
    }
}
