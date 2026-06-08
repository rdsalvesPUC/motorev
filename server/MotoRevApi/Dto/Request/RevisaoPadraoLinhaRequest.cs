using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record RevisaoPadraoLinhaRequest(
    [Required(ErrorMessage = "O nome do modelo de revisão é obrigatório.")]
    string Nome,

    [Required(ErrorMessage = "O ID da linha é obrigatório.")]
    int LinhaId,

    [Required(ErrorMessage = "É necessário informar ao menos uma revisão.")]
    [MinLength(1, ErrorMessage = "O modelo deve conter pelo menos uma revisão.")]
    List<RevisaoPadraoLinhaItemRequest> Revisoes
);
