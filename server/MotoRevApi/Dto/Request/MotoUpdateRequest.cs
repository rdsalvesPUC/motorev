using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

/// <summary>
/// DTO para atualização parcial de uma moto.
/// Os campos <c>Placa</c>, <c>Cor</c> e <c>KilometragemAtual</c> são aceitos.
/// Os campos Chassi, ModeloMotoId e Ano são imutáveis e ignorados nesta operação.
/// </summary>
public record MotoUpdateRequest
{
    [Required(ErrorMessage = "A placa é obrigatória.")]
    [RegularExpression(@"^[a-zA-Z]{3}-?[0-9][a-zA-Z0-9][0-9]{2}$",
        ErrorMessage = "A placa deve estar no formato convencional (ABC-1234) ou Mercosul (ABC1D23).")]
    public string Placa { get; init; } = null!;

    [Required(ErrorMessage = "A cor é obrigatória.")]
    [StringLength(30, ErrorMessage = "A cor não pode exceder 30 caracteres.")]
    public string Cor { get; init; } = null!;

    [Required(ErrorMessage = "A quilometragem atual é obrigatória.")]
    [Range(0, int.MaxValue, ErrorMessage = "A quilometragem deve ser um valor positivo.")]
    public int KilometragemAtual { get; init; }

    public MotoUpdateRequest() { }

    public MotoUpdateRequest(string placa, string cor, int kilometragemAtual = 0)
    {
        Placa = placa;
        Cor = cor;
        KilometragemAtual = kilometragemAtual;
    }
}
