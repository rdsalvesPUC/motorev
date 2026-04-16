namespace MotoRevApi.Dto.Response;

public record ClienteResponse(
    int Id,
    string Nome,
    string Email,
    string Cpf
);
