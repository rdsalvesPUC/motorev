using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MotoRevApi.Enums;

namespace MotoRevApi.Dto.Request;

public record ServicoRequest
{
    [Required(ErrorMessage = "O código do serviço é obrigatório.")]
    [RegularExpression(@"^[A-Z0-9-]{3,20}$", ErrorMessage = "O código deve ter entre 3 e 20 caracteres e conter apenas letras maiúsculas, números e hífens.")]
    public string Codigo { get; init; }

    [Required(ErrorMessage = "O nome do serviço é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Nome { get; init; }

    [Required(ErrorMessage = "A descrição do serviço é obrigatória.")]
    [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres.")]
    public string Descricao { get; init; }

    [Required(ErrorMessage = "A categoria do serviço é obrigatória.")]
    [EnumDataType(typeof(CategoriaServico), ErrorMessage = "Categoria de serviço inválida.")]
    [JsonRequired]
    public CategoriaServico Categoria { get; init; }

    [Required(ErrorMessage = "O tempo estimado é obrigatório.")]
    [Range(1, 480, ErrorMessage = "O tempo estimado deve estar entre 1 e 480 minutos.")]
    [JsonRequired]
    public int TempoEstimado { get; init; }

    [Required(ErrorMessage = "O custo é obrigatório.")]
    [Range(0, 100000, ErrorMessage = "O custo deve ser um valor positivo.")]
    [JsonRequired]
    public decimal Custo { get; init; }

    public ServicoRequest() { }

    public ServicoRequest(string codigo, string nome, string descricao, CategoriaServico categoria, int tempoEstimado, decimal custo)
    {
        Codigo = codigo;
        Nome = nome;
        Descricao = descricao;
        Categoria = categoria;
        TempoEstimado = tempoEstimado;
        Custo = custo;
    }
}

