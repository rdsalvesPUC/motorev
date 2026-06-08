using MotoRevApi.Enums;

namespace MotoRevApi.Model;

public class Peca
{
    public int Id { get; set; }
    public required string Codigo { get; set; }
    public required string Nome { get; set; }
    public CategoriaPeca Categoria { get; set; }
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
    public StatusCadastro Status { get; set; }
    public virtual ICollection<RevisaoPadraoPeca> RevisoesPadrao { get; set; } = new List<RevisaoPadraoPeca>();
}
