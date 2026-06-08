namespace MotoRevApi.Model;

public class Concessionaria
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Cnpj { get; set; }
    public required string Telefone { get; set; }
    public string Tipo { get; set; } = "Matriz";
    public required string UsuarioId { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual ICollection<Loja> Lojas { get; set; } = [];
}
