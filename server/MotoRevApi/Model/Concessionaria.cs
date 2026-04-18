namespace MotoRevApi.Model;

public class Concessionaria
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Cnpj { get; set; }
    public required string Tel { get; set; }
    public List<Endereco> Enderecos { get; set; } = new();
    public required string UsuarioId { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
