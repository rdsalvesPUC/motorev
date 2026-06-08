using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record LoginRequest(
    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "O email informado não é válido.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "O e-mail deve conter '@' e '.'")]
    string Email,
    [Required(ErrorMessage = "A senha é obrigatória.")]
    string Password
);
