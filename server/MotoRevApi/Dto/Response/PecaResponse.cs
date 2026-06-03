namespace MotoRevApi.Dto.Response;

public record PecaResponse(
    int Id,
    string Codigo,
    string Nome,
    string Categoria,
    decimal Preco,
    int Estoque,
    string Status
);
