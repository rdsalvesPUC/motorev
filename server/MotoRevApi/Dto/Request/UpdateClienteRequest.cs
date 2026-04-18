namespace MotoRevApi.Dto.Request;

public record UpdateClienteRequest(
    string? Nome,
    string? Cel,
    string? Tel
);
