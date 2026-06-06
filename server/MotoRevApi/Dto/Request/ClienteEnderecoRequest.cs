namespace MotoRevApi.Dto.Request;

public record ClienteEnderecoRequest(
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Uf
);
