using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record EnderecoRequest(
    [property: Required(ErrorMessage = "O CEP é obrigatório.")]
    [property: RegularExpression(@"^\d{8}$", ErrorMessage = "O CEP deve conter exatamente 8 dígitos numéricos.")]
    string Cep,
    
    [property: Required(ErrorMessage = "O Logradouro é obrigatório.")]
    string Logradouro,
    
    [property: Required(ErrorMessage = "O Número é obrigatório.")]
    string Numero,
    
    string? Complemento,
    
    [property: Required(ErrorMessage = "O Bairro é obrigatório.")]
    string Bairro,
    
    [property: Required(ErrorMessage = "A Cidade é obrigatória.")]
    string Cidade,
    
    [property: Required(ErrorMessage = "O Estado é obrigatório.")]
    [property: StringLength(2, MinimumLength = 2, ErrorMessage = "O Estado deve conter 2 caracteres (Ex: SP).")]
    string Estado
);
