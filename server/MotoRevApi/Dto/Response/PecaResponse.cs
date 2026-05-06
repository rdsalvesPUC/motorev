namespace MotoRevApi.Dto.Response;

public record PecaResponse(
    int Id,
    string Nome,
    string? Descricao,
    decimal Valor
);
    
