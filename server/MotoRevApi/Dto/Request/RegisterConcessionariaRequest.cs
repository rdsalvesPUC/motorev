using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record RegisterConcessionariaRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    [Required] string Nome,
    [Required] string Cnpj,
    [Required] string Tel
);
