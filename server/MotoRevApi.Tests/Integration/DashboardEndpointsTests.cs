using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using MotoRevApi.Authorization;
using MotoRevApi.Data;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
using MotoRevApi.Model;
using Xunit;

namespace MotoRevApi.Tests.Integration;

public class DashboardEndpointsTests : IDisposable
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
    public async Task ObterDashboardRevisoesConcessionaria_DeveRetornarFilasDoDia()
    {
        // Arrange
        const string userId = "concessionaria-dashboard";
        var data = new DateTime(2026, 6, 15, 0, 0, 0);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var loja = SeedBaseData(context, userId);
            AddAgendamento(context, loja, ordem: 1, data.AddHours(8), StatusAgendamento.Agendada);
            AddAgendamento(context, loja, ordem: 2, data.AddHours(9), StatusAgendamento.EmExecucao);
            AddAgendamento(context, loja, ordem: 3, data.AddDays(1).AddHours(8), StatusAgendamento.Agendada);
            context.SaveChanges();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateToken(Roles.Concessionaria, userId));

        // Act
        var response = await client.GetAsync("/api/Dashboard/concessionaria/revisoes?data=2026-06-15");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dashboard = await response.Content.ReadFromJsonAsync<DashboardConcessionariaResponse>(JsonOptions);
        Assert.NotNull(dashboard);
        Assert.Equal(2, dashboard.TotalRevisoes);
        Assert.Equal(4, dashboard.TotalMecanicos);
        Assert.Contains(dashboard.Filas.SelectMany(f => f.Itens), item =>
            item.Status == "em_execucao" && item.ProgressoPercentual > 0);
    }

    [Fact]
    public async Task ObterDashboardRevisoesConcessionaria_DeveExigirRoleConcessionaria()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateToken(Roles.Cliente, "cliente-dashboard"));

        // Act
        var response = await client.GetAsync("/api/Dashboard/concessionaria/revisoes");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static Loja SeedBaseData(AppDbContext context, string concessionariaUserId)
    {
        var linha = new Linha { Id = 500, Nome = "Linha Dashboard", Ativo = true };
        var modelo = new ModeloMoto
        {
            Id = 500,
            NomeModelo = "CG 160",
            Marca = "Honda",
            LinhaId = linha.Id,
            Linha = linha,
            Ativo = true
        };
        var cliente = new Cliente { Id = 500, Nome = "Cliente Dashboard", UsuarioId = "cliente-dashboard" };
        var moto = new Moto
        {
            Id = 500,
            Placa = "DAS5000",
            Chassi = "9SB0000000000500",
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
            Id = 500,
            Nome = "Concessionaria Dashboard",
            Cnpj = "00000000000500",
            Telefone = "41999990500",
            UsuarioId = concessionariaUserId
        };
        var loja = new Loja
        {
            Id = 500,
            Nome = "Loja Dashboard",
            Tipo = "Matriz",
            Cnpj = "10000000000500",
            Telefone = "41988880500",
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
        return loja;
    }

    private static void AddAgendamento(
        AppDbContext context,
        Loja loja,
        int ordem,
        DateTime dataAgendada,
        StatusAgendamento status)
    {
        var revisaoPadrao = new RevisaoPadrao
        {
            Id = 5000 + ordem,
            Nome = $"{ordem} revisao",
            Ordem = ordem,
            Quilometragem = ordem * 1000,
            TempoMeses = ordem * 6,
            LinhaId = 500,
            Ativo = true,
            Servicos = new List<RevisaoPadraoServico>
            {
                new()
                {
                    ServicoId = 5000 + ordem,
                    Servico = new Servico
                    {
                        Id = 5000 + ordem,
                        Codigo = $"SERV-DASH-{ordem}",
                        Nome = $"Servico {ordem}",
                        Descricao = "Servico de revisao",
                        Categoria = CategoriaServico.Verificacao,
                        TempoEstimado = 30,
                        Custo = 100m,
                        Ativo = true
                    }
                }
            }
        };
        var revisaoMoto = new RevisaoMoto
        {
            MotoId = 500,
            RevisaoPadraoId = revisaoPadrao.Id,
            RevisaoPadrao = revisaoPadrao,
            Nome = revisaoPadrao.Nome,
            Ordem = ordem,
            Quilometragem = revisaoPadrao.Quilometragem,
            TempoMeses = revisaoPadrao.TempoMeses,
            DataPrevista = dataAgendada.Date,
            Status = "Planejada"
        };

        context.RevisoesPadrao.Add(revisaoPadrao);
        context.RevisoesMotos.Add(revisaoMoto);
        context.Agendamentos.Add(new Agendamento
        {
            RevisaoMoto = revisaoMoto,
            LojaId = loja.Id,
            DataAgendada = dataAgendada,
            Status = status,
            CriadoEm = dataAgendada.AddMinutes(-ordem),
            AtualizadoEm = dataAgendada.AddMinutes(-ordem)
        });
    }
}
