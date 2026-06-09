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

public class MotoEndpointsTests : IDisposable
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

    private HttpClient CreateClient(string role, string userId)
    {
        var client = _factory.CreateClient();
        var token = TestJwtTokenFactory.CreateToken(role, userId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private (Cliente Cliente, ModeloMoto Modelo) SeedBaseData(string userId, string nomeCliente)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var usuario = new Usuario
        {
            Id = userId,
            UserName = $"{userId}@email.com",
            NormalizedUserName = $"{userId.ToUpper()}@EMAIL.COM",
            Email = $"{userId}@email.com",
            NormalizedEmail = $"{userId.ToUpper()}@EMAIL.COM"
        };
        context.Users.Add(usuario);

        var cliente = new Cliente
        {
            UsuarioId = userId,
            Nome = nomeCliente,
            Cpf = "12345678901"
        };
        context.Clientes.Add(cliente);

        var linha = new Linha
        {
            Nome = "Linha CG",
            Descricao = "Motos CG de teste"
        };
        context.Linhas.Add(linha);
        context.SaveChanges(); // Persiste a linha para obter o LinhaId

        var modelo = new ModeloMoto
        {
            NomeModelo = "Titan 160",
            Marca = "Honda",
            LinhaId = linha.Id,
            Ativo = true
        };
        context.ModelosMotos.Add(modelo);
        context.SaveChanges();

        return (cliente, modelo);
    }

    private Moto SeedMoto(int clienteId, int modeloId, string placa, string chassi, string cor, int km)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var moto = new Moto
        {
            Placa = placa.ToUpper().Replace("-", ""),
            Chassi = chassi.ToUpper(),
            ClienteId = clienteId,
            ModeloMotoId = modeloId,
            Cor = cor,
            KilometragemAtual = km,
            DataVenda = DateTime.UtcNow.AddYears(-1),
            Ativo = true
        };

        context.Motos.Add(moto);
        context.SaveChanges();
        return moto;
    }

    [Fact]
    public async Task ListarMinhasMotos_DeveRetornarMotos_QuandoClientePossuirMotos()
    {
        // Arrange
        var userId = "user-cliente-moto-list";
        var (cliente, modelo) = SeedBaseData(userId, "Cliente Lista");
        var moto = SeedMoto(cliente.Id, modelo.Id, "ABC-1234", "9SB12345678901234", "Preto", 5000);

        var client = CreateClient(Roles.Cliente, userId);

        // Act
        var response = await client.GetAsync("/api/Moto");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var motos = await response.Content.ReadFromJsonAsync<List<MotoResponse>>();
        Assert.NotNull(motos);
        var singleMoto = Assert.Single(motos);
        Assert.Equal("ABC1234", singleMoto.Placa);
        Assert.Equal("Preto", singleMoto.Cor);
        Assert.Equal(5000, singleMoto.KilometragemAtual);
    }

    [Fact]
    public async Task AdicionarMoto_DeveCadastrarMoto_QuandoDadosForemValidos()
    {
        // Arrange
        var userId = "user-cliente-moto-add";
        var (cliente, modelo) = SeedBaseData(userId, "Cliente Cadastro");
        using (var seedScope = _factory.Services.CreateScope())
        {
            var seedContext = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();

            var servico = new Servico
            {
                Codigo = "SERV-REV-001",
                Nome = "Troca de óleo",
                Descricao = "Troca completa do óleo do motor",
                Categoria = CategoriaServico.Troca,
                TempoEstimado = 30,
                Custo = 80m,
                Ativo = true
            };
            var peca = new Peca
            {
                Codigo = "PECA-REV-001",
                Nome = "Filtro de óleo",
                Categoria = CategoriaPeca.Filtros,
                Preco = 45m,
                Estoque = 20,
                Status = StatusCadastro.Ativo
            };
            var primeiraRevisao = new RevisaoPadrao
            {
                LinhaId = modelo.LinhaId,
                Nome = "Primeira revisão",
                Ordem = 1,
                Quilometragem = 1000,
                TempoMeses = 6,
                Ativo = true
            };
            var segundaRevisao = new RevisaoPadrao
            {
                LinhaId = modelo.LinhaId,
                Nome = "Segunda revisão",
                Ordem = 2,
                Quilometragem = 5000,
                TempoMeses = 12,
                Ativo = true
            };

            seedContext.Servicos.Add(servico);
            seedContext.Pecas.Add(peca);
            seedContext.RevisoesPadrao.AddRange(primeiraRevisao, segundaRevisao);
            seedContext.SaveChanges();

            seedContext.RevisaoPadraoServicos.Add(new RevisaoPadraoServico
            {
                RevisaoPadraoId = primeiraRevisao.Id,
                ServicoId = servico.Id
            });
            seedContext.RevisaoPadraoPecas.Add(new RevisaoPadraoPeca
            {
                RevisaoPadraoId = primeiraRevisao.Id,
                PecaId = peca.Id,
                Quantidade = 2
            });
            seedContext.SaveChanges();
        }

        var dataVenda = DateTime.UtcNow.AddMonths(-6);
        var client = CreateClient(Roles.Cliente, userId);
        var request = new MotoRequest(
            placa: "XYZ-9876",
            chassi: "9SB98765432109876",
            modeloMotoId: modelo.Id,
            cor: "Vermelho",
            kilometragemAtual: 1000,
            dataVenda: dataVenda
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/Moto", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var motoResponse = await response.Content.ReadFromJsonAsync<MotoResponse>(JsonOptions);
        Assert.NotNull(motoResponse);
        Assert.True(motoResponse.Id > 0);
        Assert.Equal("XYZ9876", motoResponse.Placa);
        Assert.Equal("Vermelho", motoResponse.Cor);
        Assert.Equal(2, motoResponse.RevisoesPlanejadas.Count);
        Assert.All(motoResponse.RevisoesPlanejadas, revisao => Assert.Equal("Planejada", revisao.Status));
        Assert.Collection(motoResponse.RevisoesPlanejadas,
            revisao =>
            {
                Assert.Equal("Primeira revisão", revisao.Nome);
                Assert.Equal(1, revisao.Ordem);
                Assert.Equal(1000, revisao.Quilometragem);
                Assert.Equal(6, revisao.TempoMeses);
                Assert.Equal(dataVenda.Date.AddMonths(6), revisao.DataPrevista.Date);

                var servico = Assert.Single(revisao.Servicos);
                Assert.Equal("SERV-REV-001", servico.Codigo);
                Assert.Equal("Troca de óleo", servico.Nome);
                Assert.Equal(30, servico.TempoEstimado);
                Assert.Equal(80m, servico.Custo);

                var peca = Assert.Single(revisao.Pecas);
                Assert.Equal("PECA-REV-001", peca.Codigo);
                Assert.Equal("Filtro de óleo", peca.Nome);
                Assert.Equal(45m, peca.Preco);
                Assert.Equal(2, peca.Quantidade);
            },
            revisao =>
            {
                Assert.Equal("Segunda revisão", revisao.Nome);
                Assert.Equal(2, revisao.Ordem);
                Assert.Equal(5000, revisao.Quilometragem);
                Assert.Equal(12, revisao.TempoMeses);
                Assert.Equal(dataVenda.Date.AddMonths(12), revisao.DataPrevista.Date);
                Assert.Empty(revisao.Servicos);
                Assert.Empty(revisao.Pecas);
            });

        // Verificar DB
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var motoDb = context.Motos.Single(m => m.Id == motoResponse.Id);
        Assert.Equal("XYZ9876", motoDb.Placa);
        Assert.Equal("9SB98765432109876", motoDb.Chassi);
        Assert.Equal(cliente.Id, motoDb.ClienteId);
        Assert.Equal(2, context.RevisoesMotos.Count(revisao => revisao.MotoId == motoResponse.Id));
    }

    [Fact]
    public async Task AdicionarMoto_DeveRetornarConflict_QuandoPlacaJaEstiverCadastrada()
    {
        // Arrange
        var userId = "user-cliente-moto-conflict";
        var (cliente, modelo) = SeedBaseData(userId, "Cliente Conflito");
        SeedMoto(cliente.Id, modelo.Id, "KKK-1111", "9SB11111111111111", "Azul", 2000);

        var client = CreateClient(Roles.Cliente, userId);
        var request = new MotoRequest(
            placa: "KKK-1111", // mesma placa
            chassi: "9SB22222222222222",
            modeloMotoId: modelo.Id,
            cor: "Preto",
            kilometragemAtual: 500,
            dataVenda: DateTime.UtcNow.AddMonths(-1)
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/Moto", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ObterMoto_DeveRetornarMoto_QuandoExistirPertencerAoCliente()
    {
        // Arrange
        var userId = "user-cliente-moto-get";
        var (cliente, modelo) = SeedBaseData(userId, "Cliente Get");
        var moto = SeedMoto(cliente.Id, modelo.Id, "AAA-9999", "9SB99999999999999", "Prata", 15000);

        var client = CreateClient(Roles.Cliente, userId);

        // Act
        var response = await client.GetAsync($"/api/Moto/{moto.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var motoResponse = await response.Content.ReadFromJsonAsync<MotoResponse>();
        Assert.NotNull(motoResponse);
        Assert.Equal(moto.Id, motoResponse.Id);
        Assert.Equal("AAA9999", motoResponse.Placa);
    }

    [Fact]
    public async Task ObterMoto_DeveRetornarNotFound_QuandoMotoNaoPertencerAoCliente()
    {
        // Arrange
        var userIdA = "user-cliente-get-a";
        var userIdB = "user-cliente-get-b";
        var (clienteA, modelo) = SeedBaseData(userIdA, "Cliente A");
        
        // Cadastra cliente B usando o mesmo modelo
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var usuarioB = new Usuario { Id = userIdB, UserName = $"{userIdB}@email.com", Email = $"{userIdB}@email.com" };
        context.Users.Add(usuarioB);
        var clienteB = new Cliente { UsuarioId = userIdB, Nome = "Cliente B", Cpf = "98765432109" };
        context.Clientes.Add(clienteB);
        context.SaveChanges();

        var motoB = SeedMoto(clienteB.Id, modelo.Id, "BBB-2222", "9SB22222222222223", "Branco", 100);

        // Cliente A tenta obter moto de B
        var clientA = CreateClient(Roles.Cliente, userIdA);

        // Act
        var response = await clientA.GetAsync($"/api/Moto/{motoB.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AtualizarMoto_DeveAtualizarCamposEditaveis_QuandoDadosForemValidos()
    {
        // Arrange
        var userId = "user-cliente-moto-update";
        var (cliente, modelo) = SeedBaseData(userId, "Cliente Update");
        var moto = SeedMoto(cliente.Id, modelo.Id, "OLD-1234", "9SB12341234123412", "Azul", 1000);

        var client = CreateClient(Roles.Cliente, userId);
        var request = new MotoUpdateRequest(
            placa: "NEW-1234",
            cor: "Verde",
            kilometragemAtual: 1500
        );

        // Act
        var response = await client.PutAsJsonAsync($"/api/Moto/{moto.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var motoResponse = await response.Content.ReadFromJsonAsync<MotoResponse>();
        Assert.NotNull(motoResponse);
        Assert.Equal("NEW1234", motoResponse.Placa);
        Assert.Equal("Verde", motoResponse.Cor);
        Assert.Equal(1500, motoResponse.KilometragemAtual);

        // Verificar persistência no DB
        using var scope = _factory.Services.CreateScope();
        var assertContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var motoDb = assertContext.Motos.Find(moto.Id);
        Assert.NotNull(motoDb);
        Assert.Equal("NEW1234", motoDb.Placa);
        Assert.Equal("Verde", motoDb.Cor);
        Assert.Equal(1500, motoDb.KilometragemAtual);
    }

    [Fact]
    public async Task AtualizarMoto_DeveRetornarUnprocessableEntity_QuandoQuilometragemForReduzida()
    {
        // Arrange
        var userId = "user-cliente-moto-reduce-km";
        var (cliente, modelo) = SeedBaseData(userId, "Cliente Reduce KM");
        var moto = SeedMoto(cliente.Id, modelo.Id, "OLD-5678", "9SB56785678567856", "Azul", 10000);

        var client = CreateClient(Roles.Cliente, userId);
        var request = new MotoUpdateRequest(
            placa: "OLD-5678",
            cor: "Azul",
            kilometragemAtual: 9000 // Menor que 10000
        );

        // Act
        var response = await client.PutAsJsonAsync($"/api/Moto/{moto.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task InativarMoto_DeveDesativarMoto_QuandoMotoForDoCliente()
    {
        // Arrange
        var userId = "user-cliente-moto-delete";
        var (cliente, modelo) = SeedBaseData(userId, "Cliente Delete");
        var moto = SeedMoto(cliente.Id, modelo.Id, "DEL-1234", "9SB12345678909876", "Preto", 2000);

        var client = CreateClient(Roles.Cliente, userId);

        // Act
        var response = await client.DeleteAsync($"/api/Moto/{moto.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verificar DB (deve estar inativo)
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var motoDb = context.Motos.Find(moto.Id);
        Assert.NotNull(motoDb);
        Assert.False(motoDb.Ativo);
    }
}
