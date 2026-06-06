namespace MotoRevApi.Model;

public class ModeloMoto
{
    public int Id { get; set; }
    public required string NomeModelo { get; set; }
    public required string Marca { get; set; }
    public string? Categoria { get; set; }
    public int LinhaId { get; set; }
    public Linha Linha { get; set; } = null!;
    public string? Cilindrada { get; set; }
    public int? Ano { get; set; }
    public bool Ativo { get; set; } = true;
}
