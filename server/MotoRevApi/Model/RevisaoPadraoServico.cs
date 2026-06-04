namespace MotoRevApi.Model;

// Tabela de relacionamento N:N entre RevisaoPadrao e Servico
public class RevisaoPadraoServico
{
    public int RevisaoPadraoId { get; set; }
    public virtual RevisaoPadrao RevisaoPadrao { get; set; } = null!;
    
    public int ServicoId { get; set; }
    public virtual Servico Servico { get; set; } = null!;
}
