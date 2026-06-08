namespace MotoRevApi.Model;

public class RevisaoPadrao
{
    public int Id { get; set; }
    public required string Nome { get; set; } // Ex: Revisão de 1000km
    public int Ordem { get; set; } // Ordem da revisão (1 = Primeira, 2 = Segunda, etc.)
    public int Quilometragem { get; set; }
    public int TempoMeses { get; set; }
    
    public int LinhaId { get; set; }
    public virtual Linha Linha { get; set; } = null!;
    
    public int ConcessionariaId { get; set; }
    public virtual Concessionaria Concessionaria { get; set; } = null!;
    
    public bool Ativo { get; set; } = true;
    
    public virtual ICollection<RevisaoPadraoServico> Servicos { get; set; } = new List<RevisaoPadraoServico>();

    public virtual ICollection<RevisaoPadraoPeca> Pecas { get; set; } = new List<RevisaoPadraoPeca>();
}
