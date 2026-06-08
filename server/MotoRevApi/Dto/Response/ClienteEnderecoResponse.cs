namespace MotoRevApi.Dto.Response;

public record ClienteEnderecoResponse(
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Uf
);
