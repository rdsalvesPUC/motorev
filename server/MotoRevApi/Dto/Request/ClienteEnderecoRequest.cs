using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record ClienteEnderecoRequest(
    [Required] string Cep,
    [Required] string Logradouro,
    [Required] string Numero,
    string? Complemento,
    [Required] string Bairro,
    [Required] string Cidade,
    [Required] string Uf
);
