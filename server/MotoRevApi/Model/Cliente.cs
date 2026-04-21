namespace MotoRevApi.Model;

public class Cliente
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string UsuarioId { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
}
