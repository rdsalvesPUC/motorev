using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record UpdateConcessionariaRequest(
    [property: Required(ErrorMessage = "O nome ou Razão Social é obrigatório.")] 
    string Nome,
    
    [property: Required(ErrorMessage = "O e-mail é obrigatório.")]
    [property: EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
    [property: RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "O e-mail deve conter '@' e '.'")]
    string Email
);
