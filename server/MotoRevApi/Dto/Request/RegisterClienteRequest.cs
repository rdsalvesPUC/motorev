using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record RegisterClienteRequest(
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "O e-mail deve conter '@' e '.'")]
    string Email,
    [Required] string Password,
    [Required] string Nome,
    [Required] string Cpf,
    [Required] string Telefone
);
