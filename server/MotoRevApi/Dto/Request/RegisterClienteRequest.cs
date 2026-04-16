using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record RegisterClienteRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    [Required] string Nome,
    [Required] string Cpf,
    [Required] string Cel,
    string? Tel
);
