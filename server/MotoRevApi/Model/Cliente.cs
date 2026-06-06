namespace MotoRevApi.Model;

public class Cliente
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public string? Cpf { get; set; }
    public string? Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Uf { get; set; }
    public required string UsuarioId { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
}
