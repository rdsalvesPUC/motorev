using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record LoginRequest(
    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "O email informado não é válido.")]
    string Email,
    [Required(ErrorMessage = "A senha é obrigatória.")]
    string Password
);
