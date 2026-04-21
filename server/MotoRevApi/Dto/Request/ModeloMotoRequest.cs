using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record ModeloMotoRequest(
    [Required(ErrorMessage = "O nome do modelo é obrigatório.")]
    string NomeModelo,
    [Required(ErrorMessage = "A marca é obrigatória.")]
    string Marca,
    string? Categoria
);
