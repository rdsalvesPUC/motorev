namespace MotoRevApi.Model;

public class Linha
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public string? Descricao { get; set; }
    public bool Ativo { get; set; } = true;
}
