using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using MotoRevApi.Authorization;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
using MotoRevApi.Model;
using Xunit;

namespace MotoRevApi.Tests.Integration;

public class AgendamentoEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public async Task ListarAgendamentosCliente_DeveRetornarSomenteCardsVisiveisDoCliente()
    {
        // Arrange
        var userId = "cliente-agendamentos";
        var outroUserId = "outro-cliente-agendamentos";
        var hoje = DateTime.Today;
        Moto moto;
        RevisaoMoto revisaoDisponivel;
        RevisaoMoto revisaoAgendada;
        RevisaoMoto revisaoPerdida;

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Loja loja;
            (moto, loja) = SeedMotoComLoja(context, userId, clienteId: 1, motoId: 1);
            revisaoDisponivel = SeedRevisao(context, moto, ordem: 1, dataIdeal: hoje);
            revisaoAgendada = SeedRevisao(context, moto, ordem: 2, dataIdeal: hoje.AddDays(3));
            revisaoPerdida = SeedRevisao(context, moto, ordem: 3, dataIdeal: hoje.AddDays(-16));

            context.Agendamentos.Add(new Agendamento
            {
                RevisaoMotoId = revisaoAgendada.Id,
                LojaId = loja.Id,
                DataAgendada = hoje.AddDays(2),
                Status = StatusAgendamento.AguardandoConfirmacao,
                CriadoEm = hoje.AddDays(-1),
                AtualizadoEm = hoje.AddDays(-1)
            });

            var (motoOutroCliente, _) = SeedMotoComLoja(context, outroUserId, clienteId: 2, motoId: 2);
            SeedRevisao(context, motoOutroCliente, ordem: 1, dataIdeal: hoje);
            context.SaveChanges();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateToken(Roles.Cliente, userId));

        // Act
        var response = await client.GetAsync("/api/Agendamento/cliente");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var agendamentos = await response.Content.ReadFromJsonAsync<List<AgendamentoClienteResponse>>(JsonOptions);
        Assert.NotNull(agendamentos);
        Assert.Equal(2, agendamentos.Count);
        Assert.Contains(agendamentos, item =>
            item.RevisaoMotoId == revisaoDisponivel.Id &&
            item.Status == "aguardando_agendamento" &&
            item.QuantidadeServicos == 1);
        Assert.Contains(agendamentos, item =>
            item.RevisaoMotoId == revisaoAgendada.Id &&
            item.Status == "aguardando_confirmacao" &&
            item.NomeLoja == "Loja Centro");
        Assert.DoesNotContain(agendamentos, item => item.RevisaoMotoId == revisaoPerdida.Id);
        Assert.All(agendamentos, item => Assert.Equal(moto.Id, item.MotoId));
    }

    [Fact]
    public async Task ListarAgendamentosCliente_DeveExigirRoleCliente()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateToken(Roles.Concessionaria, "concessionaria-user"));

        // Act
        var response = await client.GetAsync("/api/Agendamento/cliente");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CancelarAgendamentoCliente_DeveVoltarRevisaoParaAguardandoAgendamento()
    {
        // Arrange
        var userId = "cliente-cancelar-agendamento";
        var hoje = DateTime.Today;
        int agendamentoId;

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var (moto, loja) = SeedMotoComLoja(context, userId, clienteId: 10, motoId: 10);
            var revisao = SeedRevisao(context, moto, ordem: 1, dataIdeal: hoje);
            var agendamento = new Agendamento
            {
                RevisaoMotoId = revisao.Id,
                LojaId = loja.Id,
                DataAgendada = hoje.AddDays(1),
                Status = StatusAgendamento.Agendada,
                CriadoEm = hoje.AddDays(-1),
                AtualizadoEm = hoje.AddDays(-1)
            };
            context.Agendamentos.Add(agendamento);
            context.SaveChanges();
            agendamentoId = agendamento.Id;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateToken(Roles.Cliente, userId));

        // Act
        var response = await client.PatchAsync($"/api/Agendamento/cliente/{agendamentoId}/cancelar", null);
        var listResponse = await client.GetAsync("/api/Agendamento/cliente");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var agendamentos = await listResponse.Content.ReadFromJsonAsync<List<AgendamentoClienteResponse>>(JsonOptions);
        var item = Assert.Single(agendamentos!);
        Assert.Equal("aguardando_agendamento", item.Status);
        Assert.Null(item.AgendamentoId);
    }

    [Fact]
    public async Task RemarcarAgendamentoCliente_DeveCriarSolicitacaoAguardandoConfirmacao()
    {
        // Arrange
        var userId = "cliente-remarcar-agendamento";
        var hoje = DateTime.Today;
        int agendamentoId;
        var novaData = hoje.AddDays(3);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var (moto, loja) = SeedMotoComLoja(context, userId, clienteId: 11, motoId: 11);
            var revisao = SeedRevisao(context, moto, ordem: 1, dataIdeal: hoje);
            var agendamento = new Agendamento
            {
                RevisaoMotoId = revisao.Id,
                LojaId = loja.Id,
                DataAgendada = hoje.AddDays(1),
                Status = StatusAgendamento.Agendada,
                CriadoEm = hoje.AddDays(-1),
                AtualizadoEm = hoje.AddDays(-1)
            };
            context.Agendamentos.Add(agendamento);
            context.SaveChanges();
            agendamentoId = agendamento.Id;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateToken(Roles.Cliente, userId));

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/Agendamento/cliente/{agendamentoId}/remarcar",
            new RemarcarAgendamentoRequest(novaData));
        var listResponse = await client.GetAsync("/api/Agendamento/cliente");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var agendamentos = await listResponse.Content.ReadFromJsonAsync<List<AgendamentoClienteResponse>>(JsonOptions);
        var item = Assert.Single(agendamentos!);
        Assert.Equal("aguardando_confirmacao", item.Status);
        Assert.Equal(novaData.Date, item.DataAgendada?.Date);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var registros = context.Agendamentos
                .Where(a => a.RevisaoMoto.Moto.Cliente.UsuarioId == userId)
                .OrderBy(a => a.Id)
                .ToList();

            Assert.Equal(2, registros.Count);
            Assert.Equal(StatusAgendamento.Cancelada, registros.First().Status);
            Assert.Equal(StatusAgendamento.AguardandoConfirmacao, registros.Last().Status);
        }
    }

    [Fact]
    public async Task AgendarRevisaoCliente_DeveCriarSolicitacaoAguardandoConfirmacao()
    {
        // Arrange
        var userId = "cliente-agendar-revisao";
        var hoje = DateTime.Today;
        int revisaoMotoId;
        int lojaId;
        var dataAgendada = hoje.AddDays(2);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var (moto, loja) = SeedMotoComLoja(context, userId, clienteId: 12, motoId: 12);
            var revisao = SeedRevisao(context, moto, ordem: 1, dataIdeal: hoje);
            revisaoMotoId = revisao.Id;
            lojaId = loja.Id;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateToken(Roles.Cliente, userId));

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/Agendamento/cliente/revisoes/{revisaoMotoId}/agendar",
            new AgendarRevisaoRequest(lojaId, dataAgendada));
        var listResponse = await client.GetAsync("/api/Agendamento/cliente");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var agendamentos = await listResponse.Content.ReadFromJsonAsync<List<AgendamentoClienteResponse>>(JsonOptions);
        var item = Assert.Single(agendamentos!);
        Assert.Equal("aguardando_confirmacao", item.Status);
        Assert.Equal(lojaId, item.LojaId);
        Assert.Equal(dataAgendada.Date, item.DataAgendada?.Date);
    }

    [Fact]
    public async Task AgendarRevisaoCliente_DeveCancelarAgendamentoAtrasadoAntesDeReagendar()
    {
        // Arrange
        var userId = "cliente-reagendar-revisao-atrasada";
        var hoje = DateTime.Today;
        int revisaoMotoId;
        int lojaId;
        var dataAgendada = hoje.AddDays(2);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var (moto, loja) = SeedMotoComLoja(context, userId, clienteId: 17, motoId: 17);
            var revisao = SeedRevisao(context, moto, ordem: 1, dataIdeal: hoje);
            revisaoMotoId = revisao.Id;
            lojaId = loja.Id;

            context.Agendamentos.Add(new Agendamento
            {
                RevisaoMotoId = revisao.Id,
                LojaId = loja.Id,
                DataAgendada = hoje.AddDays(-1),
                Status = StatusAgendamento.Agendada,
                CriadoEm = hoje.AddDays(-5),
                AtualizadoEm = hoje.AddDays(-5)
            });
            context.SaveChanges();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateToken(Roles.Cliente, userId));

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/Agendamento/cliente/revisoes/{revisaoMotoId}/agendar",
            new AgendarRevisaoRequest(lojaId, dataAgendada));
        var listResponse = await client.GetAsync("/api/Agendamento/cliente");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var agendamentos = await listResponse.Content.ReadFromJsonAsync<List<AgendamentoClienteResponse>>(JsonOptions);
        var item = Assert.Single(agendamentos!);
        Assert.Equal("aguardando_confirmacao", item.Status);
        Assert.Equal(lojaId, item.LojaId);
        Assert.Equal(dataAgendada.Date, item.DataAgendada?.Date);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var registros = context.Agendamentos
                .Where(a => a.RevisaoMotoId == revisaoMotoId)
                .OrderBy(a => a.Id)
                .ToList();

            Assert.Equal(2, registros.Count);
            Assert.Equal(StatusAgendamento.Cancelada, registros.First().Status);
            Assert.Equal(StatusAgendamento.AguardandoConfirmacao, registros.Last().Status);
        }
    }

    [Fact]
    public async Task ListarAgendamentosConcessionaria_DeveRetornarSolicitacoesDaConcessionaria()
    {
        // Arrange
        var userId = "cliente-listar-agendamentos-concessionaria";
        var hoje = DateTime.Today;
        int agendamentoId;

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var (moto, loja) = SeedMotoComLoja(context, userId, clienteId: 13, motoId: 13);
            var revisao = SeedRevisao(context, moto, ordem: 1, dataIdeal: hoje);
            var agendamento = new Agendamento
            {
                RevisaoMotoId = revisao.Id,
                LojaId = loja.Id,
                DataAgendada = hoje.AddDays(2),
                Status = StatusAgendamento.AguardandoConfirmacao,
                CriadoEm = hoje.AddDays(-1),
                AtualizadoEm = hoje.AddDays(-1)
            };
            context.Agendamentos.Add(agendamento);
            context.SaveChanges();
            agendamentoId = agendamento.Id;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateToken(Roles.Concessionaria, "concessionaria-13"));

        // Act
        var response = await client.GetAsync("/api/Agendamento/concessionaria");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var agendamentos = await response.Content.ReadFromJsonAsync<List<AgendamentoConcessionariaResponse>>(JsonOptions);
        var item = Assert.Single(agendamentos!);
        Assert.Equal(agendamentoId, item.AgendamentoId);
        Assert.Equal("aguardando_confirmacao", item.Status);
        Assert.Equal("Cliente 13", item.ClienteNome);
    }

    [Fact]
    public async Task AceitarSolicitacaoConcessionaria_DeveAtualizarStatusParaAgendadaNoCliente()
    {
        // Arrange
        var userId = "cliente-aceitar-solicitacao";
        var hoje = DateTime.Today;
        int agendamentoId;

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var (moto, loja) = SeedMotoComLoja(context, userId, clienteId: 14, motoId: 14);
            var revisao = SeedRevisao(context, moto, ordem: 1, dataIdeal: hoje);
            var agendamento = new Agendamento
            {
                RevisaoMotoId = revisao.Id,
                LojaId = loja.Id,
                DataAgendada = hoje.AddDays(2),
                Status = StatusAgendamento.AguardandoConfirmacao,
                CriadoEm = hoje.AddDays(-1),
                AtualizadoEm = hoje.AddDays(-1)
            };
            context.Agendamentos.Add(agendamento);
            context.SaveChanges();
            agendamentoId = agendamento.Id;
        }

        var concessionariaClient = _factory.CreateClient();
        concessionariaClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateToken(Roles.Concessionaria, "concessionaria-14"));
        var clienteClient = _factory.CreateClient();
        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateToken(Roles.Cliente, userId));

        // Act
        var response = await concessionariaClient.PatchAsync(
            $"/api/Agendamento/concessionaria/{agendamentoId}/aceitar",
            null);
        var listResponse = await clienteClient.GetAsync("/api/Agendamento/cliente");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var agendamentos = await listResponse.Content.ReadFromJsonAsync<List<AgendamentoClienteResponse>>(JsonOptions);
        var item = Assert.Single(agendamentos!);
        Assert.Equal("agendada", item.Status);
    }

    [Fact]
    public async Task RecusarSolicitacaoConcessionaria_DeveVoltarClienteParaAguardandoAgendamentoComMensagem()
    {
        // Arrange
        var userId = "cliente-recusar-solicitacao";
        var hoje = DateTime.Today;
        int agendamentoId;

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var (moto, loja) = SeedMotoComLoja(context, userId, clienteId: 15, motoId: 15);
            var revisao = SeedRevisao(context, moto, ordem: 1, dataIdeal: hoje);
            var agendamento = new Agendamento
            {
                RevisaoMotoId = revisao.Id,
                LojaId = loja.Id,
                DataAgendada = hoje.AddDays(2),
                Status = StatusAgendamento.AguardandoConfirmacao,
                CriadoEm = hoje.AddDays(-1),
                AtualizadoEm = hoje.AddDays(-1)
            };
            context.Agendamentos.Add(agendamento);
            context.SaveChanges();
            agendamentoId = agendamento.Id;
        }

        var concessionariaClient = _factory.CreateClient();
        concessionariaClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateToken(Roles.Concessionaria, "concessionaria-15"));
        var clienteClient = _factory.CreateClient();
        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateToken(Roles.Cliente, userId));

        // Act
        var response = await concessionariaClient.PostAsJsonAsync(
            $"/api/Agendamento/concessionaria/{agendamentoId}/recusar",
            new RecusarAgendamentoRequest("Sem agenda para esta data."));
        var listResponse = await clienteClient.GetAsync("/api/Agendamento/cliente");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var agendamentos = await listResponse.Content.ReadFromJsonAsync<List<AgendamentoClienteResponse>>(JsonOptions);
        var item = Assert.Single(agendamentos!);
        Assert.Equal("aguardando_agendamento", item.Status);
        Assert.Equal("Sem agenda para esta data.", item.MensagemRecusa);
    }

    [Fact]
    public async Task VisualizarRecusaCliente_DeveOcultarMensagemRecusa()
    {
        // Arrange
        var userId = "cliente-visualizar-recusa";
        var hoje = DateTime.Today;
        int agendamentoId;

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var (moto, loja) = SeedMotoComLoja(context, userId, clienteId: 16, motoId: 16);
            var revisao = SeedRevisao(context, moto, ordem: 1, dataIdeal: hoje);
            var agendamento = new Agendamento
            {
                RevisaoMotoId = revisao.Id,
                LojaId = loja.Id,
                DataAgendada = hoje.AddDays(2),
                Status = StatusAgendamento.Recusada,
                MensagemRecusa = "Sem agenda para esta data.",
                DataRecusa = hoje,
                RecusaVisualizadaCliente = false,
                CriadoEm = hoje.AddDays(-1),
                AtualizadoEm = hoje.AddDays(-1)
            };
            context.Agendamentos.Add(agendamento);
            context.SaveChanges();
            agendamentoId = agendamento.Id;
        }

        var clienteClient = _factory.CreateClient();
        clienteClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateToken(Roles.Cliente, userId));

        // Act
        var response = await clienteClient.PatchAsync(
            $"/api/Agendamento/cliente/{agendamentoId}/recusa/visualizar",
            null);
        var listResponse = await clienteClient.GetAsync("/api/Agendamento/cliente");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var agendamentos = await listResponse.Content.ReadFromJsonAsync<List<AgendamentoClienteResponse>>(JsonOptions);
        var item = Assert.Single(agendamentos!);
        Assert.Equal("aguardando_agendamento", item.Status);
        Assert.Null(item.MensagemRecusa);
        Assert.Null(item.DataRecusa);
    }

    private static (Moto Moto, Loja Loja) SeedMotoComLoja(
        AppDbContext context,
        string userId,
        int clienteId,
        int motoId)
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
            DataVenda = DateTime.Today.AddMonths(-6),
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

    private static RevisaoMoto SeedRevisao(AppDbContext context, Moto moto, int ordem, DateTime dataIdeal)
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
