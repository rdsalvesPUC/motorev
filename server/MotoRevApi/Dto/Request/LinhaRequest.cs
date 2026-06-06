using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record LinhaRequest(
    [Required(ErrorMessage = "O nome da linha é obrigatório.")]
    string Nome,
    string? Descricao
);
