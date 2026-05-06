using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record PecaRequest(
    [Required(ErrorMessage = "O nome é obrigatório.")]
    string Nome,
    string? Descricao,
    [Required(ErrorMessage = "O valor é obrigatório.")]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "O valor deve ser maior que zero.")]
    decimal? Valor
    );