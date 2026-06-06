using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record ClienteDadosPessoaisRequest(
    [Required] string Nome,
    [Required, EmailAddress] string Email,
    [Required] string Cpf,
    [Required] string Telefone
);
