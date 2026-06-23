namespace MotoRevApi.Dto.Response;

public record RevisaoPadraoPecaResponse(
    int Id,
    string Codigo,
    string Nome,
    string Categoria,
    decimal Preco,
    int Estoque,
    string Status,
    int Quantidade,
    string? StatusExecucao = null
);
