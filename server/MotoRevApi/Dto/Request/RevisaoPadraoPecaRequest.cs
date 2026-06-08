using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record RevisaoPadraoPecaRequest(
    [Required(ErrorMessage = "O ID da peca e obrigatorio.")]
    int PecaId,

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade da peca deve ser maior que zero.")]
    int Quantidade
);
