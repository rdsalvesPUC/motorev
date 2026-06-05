using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Enums;
using MotoRevApi.Model;

namespace MotoRevApi.Tests.Factories;

public static class PecaTestFactory
{
    public static Peca CreatePeca(
        string codigo = "P001",
        string nome = "Filtro de oleo",
        CategoriaPeca categoria = CategoriaPeca.Filtros,
        decimal preco = 10.99m,
        int estoque = 25,
        StatusCadastro status = StatusCadastro.Ativo)
    {
        return new Peca
        {
            Codigo = codigo,
            Nome = nome,
            Categoria = categoria,
            Preco = preco,
            Estoque = estoque,
            Status = status
        };
    }

    public static PecaRequest CreateValidRequest(
        string codigo = "P001",
        string nome = "Filtro de oleo",
        CategoriaPeca? categoria = CategoriaPeca.Filtros,
        decimal? preco = 10.99m,
        int? estoque = 25)
    {
        return new PecaRequest(codigo, nome, categoria, preco, estoque);
    }

    public static PecaUpdateRequest CreateValidUpdateRequest(
        string codigo = "P001",
        string nome = "Filtro de oleo atualizado",
        CategoriaPeca? categoria = CategoriaPeca.Motor,
        decimal? preco = 20.99m,
        int? estoque = 30,
        StatusCadastro? status = StatusCadastro.Ativo)
    {
        return new PecaUpdateRequest(codigo, nome, categoria, preco, estoque, status);
    }

    public static IEnumerable<object[]> InvalidUpdateRequests()
    {
        yield return [CreateValidUpdateRequest(codigo: null!)];
        yield return [CreateValidUpdateRequest(codigo: "")];
        yield return [CreateValidUpdateRequest(codigo: "   ")];
        yield return [CreateValidUpdateRequest(codigo: "A")];
        yield return [CreateValidUpdateRequest(nome: null!)];
        yield return [CreateValidUpdateRequest(nome: "")];
        yield return [CreateValidUpdateRequest(nome: "   ")];
        yield return [CreateValidUpdateRequest(nome: "AB")];
        yield return [CreateValidUpdateRequest(categoria: null)];
        yield return [CreateValidUpdateRequest(preco: null)];
        yield return [CreateValidUpdateRequest(preco: 0m)];
        yield return [CreateValidUpdateRequest(preco: -1m)];
        yield return [CreateValidUpdateRequest(preco: 10.999m)];
        yield return [CreateValidUpdateRequest(estoque: null)];
        yield return [CreateValidUpdateRequest(estoque: -1)];
        yield return [CreateValidUpdateRequest(status: null)];
    }

    public static List<int> SeedPecas(AppDbContext context, params Peca[] pecas)
    {
        context.Pecas.AddRange(pecas);
        context.SaveChanges();
        return pecas.Select(peca => peca.Id).ToList();
    }
}
