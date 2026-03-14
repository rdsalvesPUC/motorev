using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record MotoRequest(
    [Required(ErrorMessage = "O modelo é obrigatório.")]
    string Modelo,
    string? Cor,
    [Required(ErrorMessage = "O Ano de fabricação é obrigatório.")]
    string Ano
);