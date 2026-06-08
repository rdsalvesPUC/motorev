namespace MotoRevApi.Dto.Response;

public record EnderecoResponse(
    int Id,
    string Cep,
    string Logradouro,
    string Numero,
    string? Complemento,
    string Bairro,
    string Cidade,
    string Estado
);
