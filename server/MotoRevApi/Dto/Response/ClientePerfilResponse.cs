namespace MotoRevApi.Dto.Response;

public record ClientePerfilResponse(
    int Id,
    string Nome,
    string Email,
    string Cpf,
    string? Telefone,
    ClienteEnderecoResponse Endereco
);
