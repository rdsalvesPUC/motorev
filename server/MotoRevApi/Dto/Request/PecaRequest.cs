using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record PecaRequest(
    [property: Required(ErrorMessage = "O nome é obrigatório.")]
    string Nome,
    string? Descricao,
    [property: Required(ErrorMessage = "O valor é obrigatório.")]
    [property: Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "O valor deve ser maior que zero.")]
    decimal? Valor
    ) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Nome))
        {
            yield return new ValidationResult("O nome é obrigatório.", [nameof(Nome)]);
        }
        else if (Nome.Trim().Length < 3)
        {
            yield return new ValidationResult("O nome deve ter pelo menos 3 caracteres.", [nameof(Nome)]);
        }

        if (Descricao is { Length: > 1024 })
        {
            yield return new ValidationResult("A descrição deve ter no máximo 1024 caracteres.", [nameof(Descricao)]);
        }

        if (Valor.HasValue && Valor.Value * 100 != decimal.Truncate(Valor.Value * 100))
        {
            yield return new ValidationResult("O valor deve ter no máximo 2 casas decimais.", [nameof(Valor)]);
        }
    }
}
