namespace MotoRevApi.Dto.Response;

public record LinhaResponse(
    int Id,
    string Nome,
    string? Descricao,
    bool Ativo
);
