namespace MotoRevApi.Model;

// Tabela de relacionamento N:N entre RevisaoPadrao e Peca, com quantidade usada na revisao.
public class RevisaoPadraoPeca
{
    public int RevisaoPadraoId { get; set; }
    public virtual RevisaoPadrao RevisaoPadrao { get; set; } = null!;

    public int PecaId { get; set; }
    public virtual Peca Peca { get; set; } = null!;

    public int Quantidade { get; set; }
}
