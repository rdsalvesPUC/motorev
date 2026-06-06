using System.ComponentModel.DataAnnotations;
using MotoRevApi.Enums;

namespace MotoRevApi.Dto.Request;

public record PecaUpdateRequest(
    [Required(ErrorMessage = "O código é obrigatório.")]
    string Codigo,
    [Required(ErrorMessage = "O nome é obrigatório.")]
    string Nome,
    [Required(ErrorMessage = "A categoria é obrigatória.")]
    CategoriaPeca? Categoria,
    [Required(ErrorMessage = "O preço é obrigatório.")]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "O preço deve ser maior que zero.")]
    decimal? Preco,
    [Required(ErrorMessage = "O estoque é obrigatório.")]
    [Range(0, int.MaxValue, ErrorMessage = "O estoque não pode ser negativo.")]
    int? Estoque,
    [Required(ErrorMessage = "O status é obrigatório.")]
    StatusCadastro? Status
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Codigo))
        {
            yield return new ValidationResult("O código é obrigatório.", [nameof(Codigo)]);
        }
        else if (Codigo.Trim().Length is < 2 or > 20)
        {
            yield return new ValidationResult("O código deve ter entre 2 e 20 caracteres.", [nameof(Codigo)]);
        }

        if (string.IsNullOrWhiteSpace(Nome))
        {
            yield return new ValidationResult("O nome é obrigatório.", [nameof(Nome)]);
        }
        else if (Nome.Trim().Length is < 3 or > 150)
        {
            yield return new ValidationResult("O nome deve ter entre 3 e 150 caracteres.", [nameof(Nome)]);
        }

        if (Categoria is null)
        {
            yield return new ValidationResult("A categoria é obrigatória.", [nameof(Categoria)]);
        }

        if (Preco is null)
        {
            yield return new ValidationResult("O preço é obrigatório.", [nameof(Preco)]);
        }
        else if (Preco <= 0)
        {
            yield return new ValidationResult("O preço deve ser maior que zero.", [nameof(Preco)]);
        }
        else if (Preco.Value * 100 != decimal.Truncate(Preco.Value * 100))
        {
            yield return new ValidationResult("O preço deve ter no máximo 2 casas decimais.", [nameof(Preco)]);
        }

        if (Estoque is null)
        {
            yield return new ValidationResult("O estoque é obrigatório.", [nameof(Estoque)]);
        }
        else if (Estoque < 0)
        {
            yield return new ValidationResult("O estoque não pode ser negativo.", [nameof(Estoque)]);
        }

        if (Status is null)
        {
            yield return new ValidationResult("O status é obrigatório.", [nameof(Status)]);
        }
    }
}
