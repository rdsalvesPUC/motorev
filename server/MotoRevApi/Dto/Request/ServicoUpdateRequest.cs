using System.ComponentModel.DataAnnotations;
using MotoRevApi.Enums;

namespace MotoRevApi.Dto.Request;

public record ServicoUpdateRequest(
    [Required(ErrorMessage = "O nome do serviço é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "A categoria do serviço é obrigatória.")]
    CategoriaServico Categoria,

    [Required(ErrorMessage = "O tempo estimado é obrigatório.")]
    [Range(1, 480, ErrorMessage = "O tempo estimado deve estar entre 1 e 480 minutos.")]
    int TempoEstimado
);
