using MotoRevApi.Enums;

namespace MotoRevApi.Model;

public class Servico
{
    public int Id { get; set; }
    public required string Codigo { get; set; }
    public required string Nome { get; set; }
    public required string Descricao { get; set; }
    public required CategoriaServico Categoria { get; set; }
    public int TempoEstimado { get; set; } // em minutos
    public decimal Custo { get; set; }
    public bool Ativo { get; set; } = true;
}
