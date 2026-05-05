namespace MotoRevApi.Model;

public class Concessionaria
{
    public int Id { get; set; }
    public required string Nome { get; set; } // Razão Social ou Nome Fantasia
    public required string Cnpj { get; set; } // Novo campo adicionado
    public required string UsuarioId { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
    
    // Controle de Soft Delete
    public bool Ativo { get; set; } = true;
    
    // Relacionamento com Endereco (1:N)
    public virtual ICollection<Endereco> Enderecos { get; set; } = new List<Endereco>();
}
