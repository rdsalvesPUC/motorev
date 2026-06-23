using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record ConcessionariaPerfilRequest(
    [property: Required] string Nome,
    [property: Required, EmailAddress] string Email,
    [property: Required] string Cnpj,
    [property: Required] string Telefone,
    string? Cep = null,
    string? Logradouro = null,
    string? Numero = null,
    string? Bairro = null,
    string? Cidade = null,
    [property: StringLength(2, MinimumLength = 2)] string? Uf = null
);
