using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record RevisaoPadraoLinhaItemRequest(
    [Required(ErrorMessage = "O nome da revisão é obrigatório.")]
    string Nome,

    [Required(ErrorMessage = "A ordem da revisão é obrigatória.")]
    [Range(1, int.MaxValue, ErrorMessage = "A ordem deve ser um número positivo.")]
    int Ordem,

    [Range(0, int.MaxValue, ErrorMessage = "A quilometragem deve ser maior ou igual a zero.")]
    int Quilometragem,

    [Range(0, int.MaxValue, ErrorMessage = "O tempo em meses deve ser maior ou igual a zero.")]
    int TempoMeses,

    [Required(ErrorMessage = "É necessário informar ao menos um serviço.")]
    [MinLength(1, ErrorMessage = "A revisão deve conter pelo menos um serviço.")]
    List<int> ServicosIds,

    List<RevisaoPadraoPecaRequest>? Pecas = null
);
