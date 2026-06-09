using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record ModeloMotoRequest(
    [Required(ErrorMessage = "O nome do modelo é obrigatório.")]
    string NomeModelo,
    [Required(ErrorMessage = "A marca é obrigatória.")]
    string Marca,
    [Required(ErrorMessage = "A linha é obrigatória.")]
    [Range(1, int.MaxValue, ErrorMessage = "Código da linha inválido.")]
    int LinhaId,
    string? Cilindrada,
    int? Ano
);
