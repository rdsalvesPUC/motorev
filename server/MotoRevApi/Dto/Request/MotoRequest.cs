using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record MotoRequest
{
    [Required(ErrorMessage = "A placa é obrigatória.")]
    [RegularExpression(@"^[a-zA-Z]{3}-?[0-9][a-zA-Z0-9][0-9]{2}$", 
        ErrorMessage = "A placa deve estar no formato convencional (ABC-1234) ou Mercosul (ABC1D23).")]
    public string Placa { get; init; } = null!;

    [Required(ErrorMessage = "O chassi é obrigatório.")]
    [RegularExpression(@"^[a-zA-Z0-9]{17}$", 
        ErrorMessage = "O chassi deve conter exatamente 17 caracteres alfanuméricos.")]
    public string Chassi { get; init; } = null!;

    [Required(ErrorMessage = "O modelo da moto é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "Código do modelo inválido.")]
    public int ModeloMotoId { get; init; }

    public int? ConcessionariaId { get; init; }

    public string? Foto { get; init; }

    [Required(ErrorMessage = "O ano é obrigatório.")]
    public int Ano { get; init; }

    [Required(ErrorMessage = "A cor é obrigatória.")]
    public string Cor { get; init; } = null!;

    [Required(ErrorMessage = "A quilometragem atual é obrigatória.")]
    public int KilometragemAtual { get; init; }

    [Required(ErrorMessage = "A data de venda é obrigatória.")]
    public DateTime DataVenda { get; init; }

    public string? Linha { get; init; }

    public string? Cilindrada { get; init; }

    public MotoRequest() { }

    public MotoRequest(string placa, string chassi, int modeloMotoId, int ano, string cor, int kilometragemAtual, DateTime dataVenda, string? linha, string? cilindrada, int? concessionariaId = null, string? foto = null)
    {
        Placa = placa;
        Chassi = chassi;
        ModeloMotoId = modeloMotoId;
        Ano = ano;
        Cor = cor;
        KilometragemAtual = kilometragemAtual;
        DataVenda = dataVenda;
        Linha = linha;
        Cilindrada = cilindrada;
        ConcessionariaId = concessionariaId;
        Foto = foto;
    }
}