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

public class MotoEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();

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
            Categoria = "Street",
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

    private Concessionaria SeedConcessionaria(string concessionariaId, string nome)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var usuario = new Usuario
        {
            Id = concessionariaId,
            UserName = $"{concessionariaId}@email.com",
            NormalizedUserName = $"{concessionariaId.ToUpper()}@EMAIL.COM",
            Email = $"{concessionariaId}@email.com",
            NormalizedEmail = $"{concessionariaId.ToUpper()}@EMAIL.COM"
        };
        context.Users.Add(usuario);

        var concessionaria = new Concessionaria
        {
            UsuarioId = concessionariaId,
            Nome = nome,
            Cnpj = "12345678000199",
            Telefone = "1234567890"
        };
        context.Concessionarias.Add(concessionaria);
        context.SaveChanges();

        var lojaMatriz = new Loja
        {
            ConcessionariaId = concessionaria.Id,
            Nome = nome,
            Tipo = "Matriz",
            Cnpj = "12345678000199",
            Telefone = "1234567890",
            Cep = "12345678",
            Logradouro = "Rua Teste",
            Numero = "123",
            Bairro = "Bairro Teste",
            Cidade = "Cidade Teste",
            Uf = "SP"
        };
        context.Lojas.Add(lojaMatriz);
        context.SaveChanges();

        return concessionaria;
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

        var client = CreateClient(Roles.Cliente, userId);
        var request = new MotoRequest(
            placa: "XYZ-9876",
            chassi: "9SB98765432109876",
            modeloMotoId: modelo.Id,
            cor: "Vermelho",
            kilometragemAtual: 1000,
            dataVenda: DateTime.UtcNow.AddMonths(-6)
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/Moto", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var motoResponse = await response.Content.ReadFromJsonAsync<MotoResponse>();
        Assert.NotNull(motoResponse);
        Assert.True(motoResponse.Id > 0);
        Assert.Equal("XYZ9876", motoResponse.Placa);
        Assert.Equal("Vermelho", motoResponse.Cor);

        // Verificar DB
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var motoDb = context.Motos.Single(m => m.Id == motoResponse.Id);
        Assert.Equal("XYZ9876", motoDb.Placa);
        Assert.Equal("9SB98765432109876", motoDb.Chassi);
        Assert.Equal(cliente.Id, motoDb.ClienteId);
    }

    [Fact]
    public async Task AdicionarMoto_DeveRetornarNotFound_QuandoConcessionariaNaoExistir()
    {
        // Arrange
        var userId = "user-cliente-moto-add-badconcessionaria";
        var (cliente, modelo) = SeedBaseData(userId, "Cliente Cadastro");

        var client = CreateClient(Roles.Cliente, userId);
        var request = new MotoRequest(
            placa: "XYZ-9876",
            chassi: "9SB98765432109876",
            modeloMotoId: modelo.Id,
            cor: "Vermelho",
            kilometragemAtual: 1000,
            dataVenda: DateTime.UtcNow.AddMonths(-6),
            concessionariaId: 99999 // Concessionária inexistente
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/Moto", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AdicionarMoto_DeveCadastrarMotoComConcessionaria_QuandoConcessionariaExistir()
    {
        // Arrange
        var userId = "user-cliente-moto-add-concessionaria";
        var (cliente, modelo) = SeedBaseData(userId, "Cliente Cadastro");
        var concessionaria = SeedConcessionaria("user-concessionaria-moto-add", "Concessionaria Teste");

        var client = CreateClient(Roles.Cliente, userId);
        var request = new MotoRequest(
            placa: "XYZ-9876",
            chassi: "9SB98765432109876",
            modeloMotoId: modelo.Id,
            cor: "Vermelho",
            kilometragemAtual: 1000,
            dataVenda: DateTime.UtcNow.AddMonths(-6),
            concessionariaId: concessionaria.Id // Concessionária existente
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/Moto", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var motoResponse = await response.Content.ReadFromJsonAsync<MotoResponse>();
        Assert.NotNull(motoResponse);
        Assert.Equal(concessionaria.Id, motoResponse.ConcessionariaId);

        // Verificar DB
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var motoDb = context.Motos.Single(m => m.Id == motoResponse.Id);
        Assert.Equal(concessionaria.Id, motoDb.ConcessionariaId);
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
