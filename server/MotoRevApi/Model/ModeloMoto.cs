namespace MotoRevApi.Model;

public class ModeloMoto
{
    public int Id { get; set; }
    public required string NomeModelo { get; set; }
    public required string Marca { get; set; }
    public string? Categoria { get; set; }
    public bool Ativo { get; set; } = true;
}
