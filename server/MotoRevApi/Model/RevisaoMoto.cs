namespace MotoRevApi.Model;

public class RevisaoMoto
{
    public int Id { get; set; }

    public int MotoId { get; set; }
    public virtual Moto Moto { get; set; } = null!;

    public int RevisaoPadraoId { get; set; }
    public virtual RevisaoPadrao RevisaoPadrao { get; set; } = null!;

    public required string Nome { get; set; }
    public int Ordem { get; set; }
    public int Quilometragem { get; set; }
    public int TempoMeses { get; set; }
    public DateTime DataPrevista { get; set; }
    public string Status { get; set; } = "Planejada";
}
