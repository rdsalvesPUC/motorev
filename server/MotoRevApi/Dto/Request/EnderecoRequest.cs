using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record EnderecoRequest(
    [Required(ErrorMessage = "O CEP é obrigatório.")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "O CEP deve conter exatamente 8 dígitos numéricos.")]
    string Cep,
    
    [Required(ErrorMessage = "O Logradouro é obrigatório.")]
    string Logradouro,
    
    [Required(ErrorMessage = "O Número é obrigatório.")]
    string Numero,
    
    string? Complemento,
    
    [Required(ErrorMessage = "O Bairro é obrigatório.")]
    string Bairro,
    
    [Required(ErrorMessage = "A Cidade é obrigatória.")]
    string Cidade,
    
    [Required(ErrorMessage = "O Estado é obrigatório.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "O Estado deve conter 2 caracteres (Ex: SP).")]
    string Estado
);
