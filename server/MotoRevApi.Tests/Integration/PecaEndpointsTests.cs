using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MotoRevApi.Authorization;
using MotoRevApi.Data;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
using MotoRevApi.Model;
using MotoRevApi.Tests.Factories;
using Xunit;

namespace MotoRevApi.Tests.Integration;

public class PecaEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();

    [Fact]
    public async Task Criar_DeveCadastrarPeca_QuandoUsuarioForConcessionaria()
    {
        var client = CreateClient(Roles.Concessionaria);
        var request = CreateValidRequest(codigo: " p001 ");

        var response = await client.PostAsJsonAsync("/api/Peca/criar", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var peca = await response.Content.ReadFromJsonAsync<PecaResponse>();
        Assert.NotNull(peca);
        Assert.True(peca.Id > 0);
        Assert.Equal("P001", peca.Codigo);
        Assert.Equal("Filtro de oleo", peca.Nome);
        Assert.Equal(nameof(CategoriaPeca.Filtros), peca.Categoria);
        Assert.Equal(10.99m, peca.Preco);
        Assert.Equal(25, peca.Estoque);
        Assert.Equal(nameof(StatusCadastro.Ativo), peca.Status);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pecaNoDb = context.Pecas.Single();
        Assert.Equal("P001", pecaNoDb.Codigo);
    }

    [Fact]
    public async Task Criar_DeveRetornarUnauthorized_QuandoNaoEnviarToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/Peca/criar", CreateValidRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Criar_DeveRetornarForbidden_QuandoUsuarioForCliente()
    {
        var client = CreateClient(Roles.Cliente);

        var response = await client.PostAsJsonAsync("/api/Peca/criar", CreateValidRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(InvalidCreateRequests))]
    public async Task Criar_DeveRetornarBadRequest_QuandoPayloadForInvalido(object request)
    {
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.PostAsJsonAsync("/api/Peca/criar", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Criar_DeveRetornarConflict_QuandoCodigoJaExistir()
    {
        SeedPecas(new Peca
        {
            Codigo = "P001",
            Nome = "Filtro de oleo",
            Categoria = CategoriaPeca.Filtros,
            Preco = 10.99m,
            Estoque = 25,
            Status = StatusCadastro.Ativo
        });

        var client = CreateClient(Roles.Concessionaria);

        var response = await client.PostAsJsonAsync("/api/Peca/criar", CreateValidRequest(codigo: "p001"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarPeca_QuandoUsuarioForConcessionaria()
    {
        var id = SeedPecas(new Peca
        {
            Codigo = "P001",
            Nome = "Filtro de oleo",
            Categoria = CategoriaPeca.Transmissao,
            Preco = 10.99m,
            Estoque = 25,
            Status = StatusCadastro.Ativo
        }).Single();
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.GetAsync($"/api/Peca/id/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var peca = await response.Content.ReadFromJsonAsync<PecaResponse>();
        Assert.NotNull(peca);
        Assert.Equal(id, peca.Id);
        Assert.Equal("P001", peca.Codigo);
        Assert.Equal("Transmissão", peca.Categoria);
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarNotFound_QuandoPecaNaoExistir()
    {
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.GetAsync("/api/Peca/id/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarUnauthorized_QuandoNaoEnviarToken()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/Peca/id/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarForbidden_QuandoUsuarioForCliente()
    {
        var client = CreateClient(Roles.Cliente);

        var response = await client.GetAsync("/api/Peca/id/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Atualizar_DeveAtualizarPeca_QuandoUsuarioForConcessionaria()
    {
        var id = SeedPecas(CreatePeca("P001", "Filtro", StatusCadastro.Ativo)).Single();
        var client = CreateClient(Roles.Concessionaria);
        var request = CreateValidUpdateRequest(
            codigo: " p002 ",
            nome: "Kit relacao",
            categoria: "Transmissão",
            preco: 199.90m,
            estoque: 8,
            status: "Inativo");

        var response = await client.PutAsJsonAsync($"/api/Peca/id/{id}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var peca = await response.Content.ReadFromJsonAsync<PecaResponse>();
        Assert.NotNull(peca);
        Assert.Equal(id, peca.Id);
        Assert.Equal("P002", peca.Codigo);
        Assert.Equal("Kit relacao", peca.Nome);
        Assert.Equal("Transmissão", peca.Categoria);
        Assert.Equal(199.90m, peca.Preco);
        Assert.Equal(8, peca.Estoque);
        Assert.Equal(nameof(StatusCadastro.Inativo), peca.Status);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pecaNoDb = context.Pecas.Single();
        Assert.Equal("P002", pecaNoDb.Codigo);
        Assert.Equal("Kit relacao", pecaNoDb.Nome);
        Assert.Equal(CategoriaPeca.Transmissao, pecaNoDb.Categoria);
        Assert.Equal(199.90m, pecaNoDb.Preco);
        Assert.Equal(8, pecaNoDb.Estoque);
        Assert.Equal(StatusCadastro.Inativo, pecaNoDb.Status);
    }

    [Theory]
    [MemberData(nameof(InvalidUpdateRequests))]
    public async Task Atualizar_DeveRetornarBadRequest_QuandoPayloadForInvalido(object request)
    {
        var id = SeedPecas(CreatePeca("P001", "Filtro", StatusCadastro.Ativo)).Single();
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.PutAsJsonAsync($"/api/Peca/id/{id}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Atualizar_DeveRetornarNotFound_QuandoPecaNaoExistir()
    {
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.PutAsJsonAsync("/api/Peca/id/999", CreateValidUpdateRequest());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Atualizar_DeveRetornarConflict_QuandoCodigoJaExistirEmOutraPeca()
    {
        var ids = SeedPecas(
            CreatePeca("P001", "Filtro", StatusCadastro.Ativo),
            CreatePeca("P002", "Vela", StatusCadastro.Ativo));
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.PutAsJsonAsync(
            $"/api/Peca/id/{ids[0]}",
            CreateValidUpdateRequest(codigo: "p002"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Atualizar_DevePermitirMesmoCodigo_QuandoCodigoPertencerAoMesmoRegistro()
    {
        var id = SeedPecas(CreatePeca("P001", "Filtro", StatusCadastro.Ativo)).Single();
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.PutAsJsonAsync(
            $"/api/Peca/id/{id}",
            CreateValidUpdateRequest(codigo: " p001 ", nome: "Filtro atualizado"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var peca = await response.Content.ReadFromJsonAsync<PecaResponse>();
        Assert.NotNull(peca);
        Assert.Equal("P001", peca.Codigo);
        Assert.Equal("Filtro atualizado", peca.Nome);
    }

    [Fact]
    public async Task Atualizar_DeveRetornarUnauthorized_QuandoNaoEnviarToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/Peca/id/1", CreateValidUpdateRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Atualizar_DeveRetornarForbidden_QuandoUsuarioForCliente()
    {
        var client = CreateClient(Roles.Cliente);

        var response = await client.PutAsJsonAsync("/api/Peca/id/1", CreateValidUpdateRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AtualizarStatus_DeveRetornarOkEInativarPeca_QuandoUsuarioForConcessionaria()
    {
        var id = SeedPecas(CreatePeca("P001", "Filtro", StatusCadastro.Ativo)).Single();
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.PatchAsJsonAsync(
            $"/api/Peca/id/{id}/status",
            CreateValidStatusRequest("Inativo"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var peca = await response.Content.ReadFromJsonAsync<PecaResponse>();
        Assert.NotNull(peca);
        Assert.Equal(id, peca.Id);
        Assert.Equal(nameof(StatusCadastro.Inativo), peca.Status);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pecaNoDb = context.Pecas.Single();
        Assert.Equal(StatusCadastro.Inativo, pecaNoDb.Status);
    }

    [Fact]
    public async Task AtualizarStatus_DeveRetornarOkEAtivarPeca_QuandoUsuarioForConcessionaria()
    {
        var id = SeedPecas(CreatePeca("P001", "Filtro", StatusCadastro.Inativo)).Single();
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.PatchAsJsonAsync(
            $"/api/Peca/id/{id}/status",
            CreateValidStatusRequest("Ativo"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var peca = await response.Content.ReadFromJsonAsync<PecaResponse>();
        Assert.NotNull(peca);
        Assert.Equal(id, peca.Id);
        Assert.Equal(nameof(StatusCadastro.Ativo), peca.Status);
    }

    [Theory]
    [MemberData(nameof(InvalidStatusRequests))]
    public async Task AtualizarStatus_DeveRetornarBadRequest_QuandoPayloadForInvalido(object request)
    {
        var id = SeedPecas(CreatePeca("P001", "Filtro", StatusCadastro.Ativo)).Single();
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.PatchAsJsonAsync($"/api/Peca/id/{id}/status", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AtualizarStatus_DeveRetornarNotFound_QuandoPecaNaoExistir()
    {
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.PatchAsJsonAsync(
            "/api/Peca/id/999/status",
            CreateValidStatusRequest("Inativo"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AtualizarStatus_DeveRetornarUnauthorized_QuandoNaoEnviarToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PatchAsJsonAsync(
            "/api/Peca/id/1/status",
            CreateValidStatusRequest("Inativo"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AtualizarStatus_DeveRetornarForbidden_QuandoUsuarioForCliente()
    {
        var client = CreateClient(Roles.Cliente);

        var response = await client.PatchAsJsonAsync(
            "/api/Peca/id/1/status",
            CreateValidStatusRequest("Inativo"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Listar_DeveRetornarPecasOrdenadasPorNome_QuandoUsuarioForConcessionaria()
    {
        SeedPecas(
            new Peca
            {
                Codigo = "P002",
                Nome = "Vela",
                Categoria = CategoriaPeca.Eletrica,
                Preco = 20m,
                Estoque = 5,
                Status = StatusCadastro.Ativo
            },
            new Peca
            {
                Codigo = "P001",
                Nome = "Filtro",
                Categoria = CategoriaPeca.Filtros,
                Preco = 10m,
                Estoque = 15,
                Status = StatusCadastro.Ativo
            });
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.GetAsync("/api/Peca/listar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var pecas = await response.Content.ReadFromJsonAsync<List<PecaResponse>>();
        Assert.NotNull(pecas);
        Assert.Equal(2, pecas.Count);
        Assert.Collection(
            pecas,
            peca => Assert.Equal("Filtro", peca.Nome),
            peca => Assert.Equal("Vela", peca.Nome));
    }

    [Fact]
    public async Task Listar_DeveRetornarListaVazia_QuandoNaoExistiremPecas()
    {
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.GetAsync("/api/Peca/listar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var pecas = await response.Content.ReadFromJsonAsync<List<PecaResponse>>();
        Assert.NotNull(pecas);
        Assert.Empty(pecas);
    }

    [Fact]
    public async Task Listar_DeveRetornarTodasAsPecas_QuandoStatusNaoForInformado()
    {
        SeedPecas(
            CreatePeca("P001", "Filtro", StatusCadastro.Ativo),
            CreatePeca("P002", "Pastilha", StatusCadastro.Inativo));
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.GetAsync("/api/Peca/listar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var pecas = await response.Content.ReadFromJsonAsync<List<PecaResponse>>();
        Assert.NotNull(pecas);
        Assert.Equal(2, pecas.Count);
    }

    [Fact]
    public async Task Listar_DeveRetornarApenasPecasAtivas_QuandoStatusForAtivo()
    {
        SeedPecas(
            CreatePeca("P001", "Filtro", StatusCadastro.Ativo),
            CreatePeca("P002", "Pastilha", StatusCadastro.Inativo),
            CreatePeca("P003", "Vela", StatusCadastro.Ativo));
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.GetAsync("/api/Peca/listar?status=Ativo");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var pecas = await response.Content.ReadFromJsonAsync<List<PecaResponse>>();
        Assert.NotNull(pecas);
        Assert.Equal(2, pecas.Count);
        Assert.All(pecas, peca => Assert.Equal(nameof(StatusCadastro.Ativo), peca.Status));
        Assert.Collection(
            pecas,
            peca => Assert.Equal("Filtro", peca.Nome),
            peca => Assert.Equal("Vela", peca.Nome));
    }

    [Fact]
    public async Task Listar_DeveRetornarApenasPecasInativas_QuandoStatusForInativo()
    {
        SeedPecas(
            CreatePeca("P001", "Filtro", StatusCadastro.Ativo),
            CreatePeca("P002", "Pastilha", StatusCadastro.Inativo));
        var client = CreateClient(Roles.Concessionaria);

        var response = await client.GetAsync("/api/Peca/listar?status=Inativo");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var pecas = await response.Content.ReadFromJsonAsync<List<PecaResponse>>();
        Assert.NotNull(pecas);
        Assert.Single(pecas);
        Assert.Equal("Pastilha", pecas[0].Nome);
        Assert.Equal(nameof(StatusCadastro.Inativo), pecas[0].Status);
    }

    [Fact]
    public async Task Listar_DeveRetornarUnauthorized_QuandoNaoEnviarToken()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/Peca/listar");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Listar_DeveRetornarForbidden_QuandoUsuarioForCliente()
    {
        var client = CreateClient(Roles.Cliente);

        var response = await client.GetAsync("/api/Peca/listar");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    public static IEnumerable<object[]> InvalidCreateRequests()
    {
        yield return [CreateValidRequest(codigo: "")];
        yield return [CreateValidRequest(codigo: "A")];
        yield return [CreateValidRequest(nome: "")];
        yield return [CreateValidRequest(nome: "AB")];
        yield return [CreateValidRequest(categoria: null)];
        yield return [CreateValidRequest(categoria: "CategoriaInvalida")];
        yield return [CreateValidRequest(preco: null)];
        yield return [CreateValidRequest(preco: 0m)];
        yield return [CreateValidRequest(preco: -1m)];
        yield return [CreateValidRequest(preco: 10.999m)];
        yield return [CreateValidRequest(estoque: null)];
        yield return [CreateValidRequest(estoque: -1)];
    }

    public static IEnumerable<object[]> InvalidUpdateRequests()
    {
        yield return [CreateValidUpdateRequest(codigo: "")];
        yield return [CreateValidUpdateRequest(codigo: "A")];
        yield return [CreateValidUpdateRequest(nome: "")];
        yield return [CreateValidUpdateRequest(nome: "AB")];
        yield return [CreateValidUpdateRequest(categoria: null)];
        yield return [CreateValidUpdateRequest(categoria: "CategoriaInvalida")];
        yield return [CreateValidUpdateRequest(preco: null)];
        yield return [CreateValidUpdateRequest(preco: 0m)];
        yield return [CreateValidUpdateRequest(preco: -1m)];
        yield return [CreateValidUpdateRequest(preco: 10.999m)];
        yield return [CreateValidUpdateRequest(estoque: null)];
        yield return [CreateValidUpdateRequest(estoque: -1)];
        yield return [CreateValidUpdateRequest(status: null)];
        yield return [CreateValidUpdateRequest(status: "StatusInvalido")];
    }

    public static IEnumerable<object[]> InvalidStatusRequests()
    {
        yield return [CreateValidStatusRequest(null)];
        yield return [CreateValidStatusRequest("StatusInvalido")];
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    private HttpClient CreateClient(string role)
    {
        var client = _factory.CreateClient();
        var token = TestJwtTokenFactory.CreateToken(role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static object CreateValidRequest(
        string? codigo = "P001",
        string? nome = "Filtro de oleo",
        string? categoria = "Filtros",
        decimal? preco = 10.99m,
        int? estoque = 25)
    {
        return new
        {
            codigo,
            nome,
            categoria,
            preco,
            estoque
        };
    }

    private static object CreateValidUpdateRequest(
        string? codigo = "P001",
        string? nome = "Filtro de oleo atualizado",
        string? categoria = "Filtros",
        decimal? preco = 20.99m,
        int? estoque = 30,
        string? status = "Ativo")
    {
        return new
        {
            codigo,
            nome,
            categoria,
            preco,
            estoque,
            status
        };
    }

    private static object CreateValidStatusRequest(string? status = "Ativo")
    {
        return new
        {
            status
        };
    }

    private List<int> SeedPecas(params Peca[] pecas)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Pecas.AddRange(pecas);
        context.SaveChanges();
        return pecas.Select(peca => peca.Id).ToList();
    }

    private static Peca CreatePeca(string codigo, string nome, StatusCadastro status)
    {
        return new Peca
        {
            Codigo = codigo,
            Nome = nome,
            Categoria = CategoriaPeca.Filtros,
            Preco = 10m,
            Estoque = 10,
            Status = status
        };
    }
}
