namespace MotoRevApi.Model;

public class Loja
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public string Tipo { get; set; } = "Filial";
    public required string Cnpj { get; set; }
    public required string Cep { get; set; }
    public required string Logradouro { get; set; }
    public required string Numero { get; set; }
    public required string Bairro { get; set; }
    public required string Cidade { get; set; }
    public required string Uf { get; set; }
    public bool Ativo { get; set; } = true;
    public int ConcessionariaId { get; set; }
    public virtual Concessionaria Concessionaria { get; set; } = null!;
}
