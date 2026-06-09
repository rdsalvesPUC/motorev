using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record ConcessionariaPerfilRequest(
    [Required] string Nome,
    [Required, EmailAddress] string Email,
    [Required] string Cnpj,
    [Required] string Telefone,
    string? Cep = null,
    string? Logradouro = null,
    string? Numero = null,
    string? Bairro = null,
    string? Cidade = null,
    [StringLength(2, MinimumLength = 2)] string? Uf = null
);
