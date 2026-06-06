using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record LojaRequest(
    [Required] string Nome,
    [Required] string Cnpj,
    [Required] string Cep,
    [Required] string Logradouro,
    [Required] string Numero,
    [Required] string Bairro,
    [Required] string Cidade,
    [Required, StringLength(2, MinimumLength = 2)] string Uf
);
