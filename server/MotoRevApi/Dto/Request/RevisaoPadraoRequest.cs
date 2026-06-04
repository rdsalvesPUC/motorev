using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

/// <summary>
/// DTO de requisição para cadastro de Revisão Padrão.
/// </summary>
public record RevisaoPadraoRequest(
    [Required(ErrorMessage = "O nome da revisão é obrigatório.")]
    string Nome,
    
    [Required(ErrorMessage = "A ordem da revisão é obrigatória.")]
    [Range(1, int.MaxValue, ErrorMessage = "A ordem deve ser um número positivo.")]
    int Ordem,
    
    [Required(ErrorMessage = "O ID do modelo de moto é obrigatório.")]
    int ModeloMotoId,
    
    [Required(ErrorMessage = "É necessário informar ao menos um serviço.")]
    [MinLength(1, ErrorMessage = "A revisão deve conter pelo menos um serviço.")]
    List<int> ServicosIds
    
    // TODO: Adicionar a lista de PeçasIds quando o catálogo de peças estiver implementado
); 
