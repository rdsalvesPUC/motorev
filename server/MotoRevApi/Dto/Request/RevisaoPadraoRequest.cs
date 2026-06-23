using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

/// <summary>
/// DTO de requisição para cadastro de Revisão Padrão.
/// </summary>
public record RevisaoPadraoRequest(
    [property: Required(ErrorMessage = "O nome da revisão é obrigatório.")]
    string Nome,
    
    [property: Required(ErrorMessage = "A ordem da revisão é obrigatória.")]
    [property: Range(1, int.MaxValue, ErrorMessage = "A ordem deve ser um número positivo.")]
    int Ordem,

    [property: Range(0, int.MaxValue, ErrorMessage = "A quilometragem deve ser maior ou igual a zero.")]
    int Quilometragem,

    [property: Range(0, int.MaxValue, ErrorMessage = "O tempo em meses deve ser maior ou igual a zero.")]
    int TempoMeses,
    
    [property: Range(1, int.MaxValue, ErrorMessage = "O ID do modelo de moto deve ser um número positivo.")]
    int ModeloMotoId,
    
    [property: Required(ErrorMessage = "É necessário informar ao menos um serviço.")]
    [property: MinLength(1, ErrorMessage = "A revisão deve conter pelo menos um serviço.")]
    List<int> ServicosIds,

    List<RevisaoPadraoPecaRequest>? Pecas = null
); 
