using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MotoRevApi.Authorization;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
using MotoRevApi.Model;
using Xunit;

namespace MotoRevApi.Tests.Integration;

public class ServicoEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private static readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
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

    private List<int> SeedServicos(params Servico[] servicos)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Servicos.AddRange(servicos);
        context.SaveChanges();
        return servicos.Select(s => s.Id).ToList();
    }

    [Fact]
    public async Task Create_DeveCadastrarServico_QuandoUsuarioForConcessionaria()
    {
        // Arrange
        var client = CreateClient(Roles.Concessionaria);
        var request = new ServicoRequest("SERV-001", "Troca de Óleo", "Troca completa do óleo do motor", CategoriaServico.Troca, 30, 80.00m);

        // Act
        var response = await client.PostAsJsonAsync("/api/Servico", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var servico = await response.Content.ReadFromJsonAsync<ServicoResponse>(_jsonOptions);
        Assert.NotNull(servico);
        Assert.True(servico.Id > 0);
        Assert.Equal("SERV-001", servico.Codigo);
        Assert.Equal("Troca de Óleo", servico.Nome);
        Assert.Equal(CategoriaServico.Troca, servico.Categoria);
        Assert.Equal(30, servico.TempoEstimado);
        Assert.Equal(80.00m, servico.Custo);

        // Verificar persistência no banco
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var servicoDb = context.Servicos.Single();
        Assert.Equal("SERV-001", servicoDb.Codigo);
    }

    [Fact]
    public async Task Create_DeveRetornarForbidden_QuandoUsuarioForCliente()
    {
        // Arrange
        var client = CreateClient(Roles.Cliente);
        var request = new ServicoRequest("SERV-002", "Limpeza Geral", "Limpeza da moto", CategoriaServico.Limpeza, 60, 50.00m);

        // Act
        var response = await client.PostAsJsonAsync("/api/Servico", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_DeveRetornarUnauthorized_QuandoNaoEstiverAutenticado()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new ServicoRequest("SERV-003", "Ajuste Corrente", "Ajuste de tensão da corrente", CategoriaServico.Ajuste, 15, 20.00m);

        // Act
        var response = await client.PostAsJsonAsync("/api/Servico", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_DeveRetornarConflict_QuandoCodigoJaExistir()
    {
        // Arrange
        SeedServicos(new Servico
        {
            Codigo = "SERV-100",
            Nome = "Serviço Original",
            Descricao = "Descrição",
            Categoria = CategoriaServico.Verificacao,
            TempoEstimado = 20,
            Custo = 10.00m,
            Ativo = true
        });

        var client = CreateClient(Roles.Concessionaria);
        var request = new ServicoRequest("SERV-100", "Outro Nome", "Outra descrição", CategoriaServico.Verificacao, 20, 10.00m);

        // Act
        var response = await client.PostAsJsonAsync("/api/Servico", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_DeveRetornarTodosServicosAtivos_SemAutenticacao()
    {
        // Arrange
        SeedServicos(
            new Servico { Codigo = "S-01", Nome = "Revisão 10k", Descricao = "D", Categoria = CategoriaServico.Verificacao, TempoEstimado = 120, Custo = 150m, Ativo = true },
            new Servico { Codigo = "S-02", Nome = "Inativo", Descricao = "D", Categoria = CategoriaServico.Troca, TempoEstimado = 30, Custo = 40m, Ativo = false }
        );

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Servico");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var servicos = await response.Content.ReadFromJsonAsync<List<ServicoResponse>>(_jsonOptions);
        Assert.NotNull(servicos);
        var singleServico = Assert.Single(servicos);
        Assert.Equal("S-01", singleServico.Codigo);
    }

    [Fact]
    public async Task GetAll_DeveFiltrarPorCategoria_QuandoEspecificado()
    {
        // Arrange
        SeedServicos(
            new Servico { Codigo = "S-01", Nome = "Check-up", Descricao = "D", Categoria = CategoriaServico.Verificacao, TempoEstimado = 120, Custo = 150m, Ativo = true },
            new Servico { Codigo = "S-02", Nome = "Troca Filtro", Descricao = "D", Categoria = CategoriaServico.Troca, TempoEstimado = 30, Custo = 40m, Ativo = true }
        );

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Servico?categoria={CategoriaServico.Troca}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var servicos = await response.Content.ReadFromJsonAsync<List<ServicoResponse>>(_jsonOptions);
        Assert.NotNull(servicos);
        var singleServico = Assert.Single(servicos);
        Assert.Equal("S-02", singleServico.Codigo);
    }

    [Fact]
    public async Task GetById_DeveRetornarServico_QuandoExistir()
    {
        // Arrange
        var id = SeedServicos(new Servico
        {
            Codigo = "SERV-GET",
            Nome = "Obter Serviço",
            Descricao = "D",
            Categoria = CategoriaServico.Limpeza,
            TempoEstimado = 45,
            Custo = 60m,
            Ativo = true
        }).Single();

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Servico/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var servico = await response.Content.ReadFromJsonAsync<ServicoResponse>(_jsonOptions);
        Assert.NotNull(servico);
        Assert.Equal(id, servico.Id);
        Assert.Equal("SERV-GET", servico.Codigo);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoNaoExistir()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Servico/9999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_DeveAtualizarServico_QuandoUsuarioForConcessionaria()
    {
        // Arrange
        var id = SeedServicos(new Servico
        {
            Codigo = "SERV-UP",
            Nome = "Original",
            Descricao = "D",
            Categoria = CategoriaServico.Verificacao,
            TempoEstimado = 30,
            Custo = 100m,
            Ativo = true
        }).Single();

        var client = CreateClient(Roles.Concessionaria);
        var request = new ServicoUpdateRequest("SERV-UP-NEW", "Atualizado", "Descrição Nova", CategoriaServico.Ajuste, 40, 120m);

        // Act
        var response = await client.PutAsJsonAsync($"/api/Servico/{id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var servico = await response.Content.ReadFromJsonAsync<ServicoResponse>(_jsonOptions);
        Assert.NotNull(servico);
        Assert.Equal("SERV-UP-NEW", servico.Codigo);
        Assert.Equal("Atualizado", servico.Nome);
        Assert.Equal("Descrição Nova", servico.Descricao);
        Assert.Equal(CategoriaServico.Ajuste, servico.Categoria);

        // Verificar no banco
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var servicoDb = context.Servicos.Find(id);
        Assert.NotNull(servicoDb);
        Assert.Equal("SERV-UP-NEW", servicoDb.Codigo);
    }

    [Fact]
    public async Task Inactivate_DeveDesativarServico_QuandoUsuarioForConcessionaria()
    {
        // Arrange
        var id = SeedServicos(new Servico
        {
            Codigo = "SERV-DEL",
            Nome = "Para Excluir",
            Descricao = "D",
            Categoria = CategoriaServico.Verificacao,
            TempoEstimado = 30,
            Custo = 100m,
            Ativo = true
        }).Single();

        var client = CreateClient(Roles.Concessionaria);

        // Act
        var response = await client.PatchAsync($"/api/Servico/{id}/inativar", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verificar no banco (soft-delete)
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var servicoDb = context.Servicos.Find(id);
        Assert.NotNull(servicoDb);
        Assert.False(servicoDb.Ativo);
    }
}
