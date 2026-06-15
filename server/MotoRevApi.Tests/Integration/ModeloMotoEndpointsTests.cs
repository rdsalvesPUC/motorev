using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MotoRevApi.Authorization;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Model;
using Xunit;

namespace MotoRevApi.Tests.Integration;

public class ModeloMotoEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public void Dispose()
    {
        _factory.Dispose();
    }

    private HttpClient CreateClient(string role, string userId = "test-user")
    {
        var client = _factory.CreateClient();
        var token = TestJwtTokenFactory.CreateToken(role, userId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private Linha SeedLinha(bool ativo = true)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var linha = new Linha
        {
            Nome = ativo ? "Linha Teste" : "Linha Inativa",
            Descricao = "Linha para testes de modelo de moto",
            Ativo = ativo
        };
        context.Linhas.Add(linha);
        context.SaveChanges();
        return linha;
    }

    private List<int> SeedModelos(params ModeloMoto[] modelos)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.ModelosMotos.AddRange(modelos);
        context.SaveChanges();
        return modelos.Select(modelo => modelo.Id).ToList();
    }

    private void SeedRevisaoPadrao(int linhaId, bool ativo = true)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.RevisoesPadrao.Add(new RevisaoPadrao
        {
            Nome = "Primeira revisão",
            Ordem = 1,
            Quilometragem = 1000,
            TempoMeses = 6,
            LinhaId = linhaId,
            Ativo = ativo
        });
        context.SaveChanges();
    }

    [Fact]
    public async Task GetListar_DeveRetornarApenasModelosAtivos_QuandoUsuarioForCliente()
    {
        var linha = SeedLinha();
        SeedModelos(
            new ModeloMoto { NomeModelo = "CG 160", Marca = "Honda", LinhaId = linha.Id, Cilindrada = "160cc", Ano = 2024, Ativo = true },
            new ModeloMoto { NomeModelo = "Modelo Inativo", Marca = "Honda", LinhaId = linha.Id, Cilindrada = "300cc", Ano = 2023, Ativo = false }
        );
        var client = CreateClient(Roles.Cliente);

        var response = await client.GetAsync("/api/ModeloMoto/listar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var modelos = await response.Content.ReadFromJsonAsync<List<ModeloMotoResponse>>(JsonOptions);
        Assert.NotNull(modelos);
        var singleModelo = Assert.Single(modelos);
        Assert.Equal("CG 160", singleModelo.NomeModelo);
        Assert.True(singleModelo.Ativo);
    }

    [Fact]
    public async Task GetListar_DeveIgnorarQueryDeInativos_QuandoUsuarioForCliente()
    {
        var linha = SeedLinha();
        SeedModelos(
            new ModeloMoto { NomeModelo = "CG 160", Marca = "Honda", LinhaId = linha.Id, Ativo = true },
            new ModeloMoto { NomeModelo = "Modelo Inativo", Marca = "Honda", LinhaId = linha.Id, Ativo = false }
        );
        var client = CreateClient(Roles.Cliente);

        var response = await client.GetAsync("/api/ModeloMoto/listar?apenasAtivos=false");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var modelos = await response.Content.ReadFromJsonAsync<List<ModeloMotoResponse>>(JsonOptions);
        Assert.NotNull(modelos);
        var singleModelo = Assert.Single(modelos);
        Assert.Equal("CG 160", singleModelo.NomeModelo);
    }

    [Fact]
    public async Task GetListar_DeveRetornarUnauthorized_QuandoUsuarioNaoEstiverAutenticado()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/ModeloMoto/listar");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDisponiveisCadastro_DeveRetornarSomenteModelosAtivosComRevisaoPadraoAtiva_QuandoUsuarioForCliente()
    {
        var linhaComRevisao = SeedLinha();
        var linhaSemRevisao = new Linha { Nome = "Linha Sem Revisao", Ativo = true };
        var linhaRevisaoInativa = new Linha { Nome = "Linha Revisao Inativa", Ativo = true };
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Linhas.AddRange(linhaSemRevisao, linhaRevisaoInativa);
            context.SaveChanges();
        }

        SeedModelos(
            new ModeloMoto { NomeModelo = "Apto", Marca = "Honda", LinhaId = linhaComRevisao.Id, Cilindrada = "160cc", Ano = 2024, Ativo = true },
            new ModeloMoto { NomeModelo = "Sem Plano", Marca = "Yamaha", LinhaId = linhaSemRevisao.Id, Cilindrada = "150cc", Ano = 2024, Ativo = true },
            new ModeloMoto { NomeModelo = "Inativo", Marca = "Honda", LinhaId = linhaComRevisao.Id, Cilindrada = "300cc", Ano = 2023, Ativo = false },
            new ModeloMoto { NomeModelo = "Plano Inativo", Marca = "BMW", LinhaId = linhaRevisaoInativa.Id, Cilindrada = "400cc", Ano = 2024, Ativo = true }
        );
        SeedRevisaoPadrao(linhaComRevisao.Id);
        SeedRevisaoPadrao(linhaRevisaoInativa.Id, ativo: false);
        var client = CreateClient(Roles.Cliente);

        var response = await client.GetAsync("/api/ModeloMoto/disponiveis-cadastro");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var modelos = await response.Content.ReadFromJsonAsync<List<ModeloMotoResponse>>(JsonOptions);
        Assert.NotNull(modelos);
        var modelo = Assert.Single(modelos);
        Assert.Equal("Apto", modelo.NomeModelo);
    }

    [Fact]
    public async Task GetDisponiveisCadastro_DeveRetornarForbidden_QuandoUsuarioForConcessionaria()
    {
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.GetAsync("/api/ModeloMoto/disponiveis-cadastro");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetCatalogo_DeveRetornarAtivosEInativos_QuandoUsuarioForConcessionaria()
    {
        var linha = SeedLinha();
        SeedModelos(
            new ModeloMoto { NomeModelo = "CG 160", Marca = "Honda", LinhaId = linha.Id, Ativo = true },
            new ModeloMoto { NomeModelo = "Modelo Inativo", Marca = "Honda", LinhaId = linha.Id, Ativo = false }
        );
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.GetAsync("/api/ModeloMoto/catalogo");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var modelos = await response.Content.ReadFromJsonAsync<List<ModeloMotoResponse>>(JsonOptions);
        Assert.NotNull(modelos);
        Assert.Equal(2, modelos.Count);
        Assert.Contains(modelos, modelo => modelo.NomeModelo == "CG 160" && modelo.Ativo);
        Assert.Contains(modelos, modelo => modelo.NomeModelo == "Modelo Inativo" && !modelo.Ativo);
    }

    [Fact]
    public async Task GetCatalogo_DeveFiltrarPorStatus_QuandoInformado()
    {
        var linha = SeedLinha();
        SeedModelos(
            new ModeloMoto { NomeModelo = "Ativo", Marca = "Honda", LinhaId = linha.Id, Ativo = true },
            new ModeloMoto { NomeModelo = "Inativo", Marca = "Honda", LinhaId = linha.Id, Ativo = false }
        );
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.GetAsync("/api/ModeloMoto/catalogo?ativo=false");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var modelos = await response.Content.ReadFromJsonAsync<List<ModeloMotoResponse>>(JsonOptions);
        Assert.NotNull(modelos);
        var singleModelo = Assert.Single(modelos);
        Assert.Equal("Inativo", singleModelo.NomeModelo);
        Assert.False(singleModelo.Ativo);
    }

    [Fact]
    public async Task GetCatalogo_DeveRetornarForbidden_QuandoUsuarioForCliente()
    {
        var client = CreateClient(Roles.Cliente);

        var response = await client.GetAsync("/api/ModeloMoto/catalogo");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_DeveCadastrarModelo_QuandoUsuarioForConcessionaria()
    {
        var linha = SeedLinha();
        var client = CreateClient(Roles.Concessionaria);
        var request = new ModeloMotoRequest("Ninja 400", "Kawasaki", linha.Id, "400cc", 2024);

        var response = await client.PostAsJsonAsync("/api/ModeloMoto/criar", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var modelo = await response.Content.ReadFromJsonAsync<ModeloMotoResponse>(JsonOptions);
        Assert.NotNull(modelo);
        Assert.Equal("Ninja 400", modelo.NomeModelo);
        Assert.Equal("Kawasaki", modelo.Marca);
        Assert.Equal(linha.Id, modelo.LinhaId);
        Assert.True(modelo.Ativo);
    }

    [Fact]
    public async Task Create_DeveRetornarForbidden_QuandoUsuarioForCliente()
    {
        var linha = SeedLinha();
        var client = CreateClient(Roles.Cliente);
        var request = new ModeloMotoRequest("Ninja 400", "Kawasaki", linha.Id, "400cc", 2024);

        var response = await client.PostAsJsonAsync("/api/ModeloMoto/criar", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AlternarStatus_DeveInativarModelo_QuandoUsuarioForConcessionaria()
    {
        var linha = SeedLinha();
        var ids = SeedModelos(new ModeloMoto { NomeModelo = "CG 160", Marca = "Honda", LinhaId = linha.Id, Ativo = true });
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.PatchAsync($"/api/ModeloMoto/alternar-status/{ids[0]}", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var modelo = context.ModelosMotos.Single(modelo => modelo.Id == ids[0]);
        Assert.False(modelo.Ativo);
    }
}
