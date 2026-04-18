namespace MotoRevApi.Model;

public class Cliente
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Cpf { get; set; }
    public required string Email { get; set; }
    public string? Tel { get; set; }
    public required string Cel { get; set; }
    public Endereco? Endereco { get; set; }
    public required string UsuarioId { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
