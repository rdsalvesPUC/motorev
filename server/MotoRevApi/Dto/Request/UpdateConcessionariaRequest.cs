using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record UpdateConcessionariaRequest(
    [Required(ErrorMessage = "O nome ou Razão Social é obrigatório.")] 
    string Nome,
    
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "O e-mail deve conter '@' e '.'")]
    string Email
);
