using System.ComponentModel.DataAnnotations;
using MotoRevApi.Enums;

namespace MotoRevApi.Dto.Request;

public record PecaStatusRequest(
    [Required(ErrorMessage = "O status é obrigatório.")]
    StatusCadastro? Status
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Status is null)
        {
            yield return new ValidationResult("O status é obrigatório.", [nameof(Status)]);
        }
    }
}
