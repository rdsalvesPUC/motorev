namespace MotoRevApi.Model;

public class Endereco
{
    public int Id { get; set; }
    public required string Cep { get; set; }
    public required string Logradouro { get; set; }
    public required string Numero { get; set; }
    public string? Complemento { get; set; }
    public required string Bairro { get; set; }
    public required string Cidade { get; set; }
    public required string Uf { get; set; }
}
