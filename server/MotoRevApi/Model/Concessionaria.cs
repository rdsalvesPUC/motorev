namespace MotoRevApi.Model;

public class Concessionaria
{
    public int Id { get; set; }
    public required string Nome { get; set; } // Razão Social ou Nome Fantasia
    public required string Cnpj { get; set; } // Novo campo adicionado
    public required string UsuarioId { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
}
