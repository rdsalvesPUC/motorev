using MotoRevApi.Enums;

namespace MotoRevApi.Model;

public class Peca
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public string? Descricao { get; set; }
    public decimal? Valor { get; set; }
    public enum StatusCadastro;
}