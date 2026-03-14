namespace MotoRevApi.Dto.Response;

public record MotoResponse(
    int Id,
    string Modelo,
    string? Cor,
    string Ano
);