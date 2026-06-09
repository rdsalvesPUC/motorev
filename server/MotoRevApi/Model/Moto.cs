namespace MotoRevApi.Model;

public class Moto
{
    public int Id { get; set; }
    public required string Placa { get; set; }
    public required string Chassi { get; set; }

    public int ModeloMotoId { get; set; }
    public virtual ModeloMoto ModeloMoto { get; set; } = null!;

    public int ClienteId { get; set; }
    public virtual Cliente Cliente { get; set; } = null!;

    public int? ConcessionariaId { get; set; }
    public virtual Concessionaria? Concessionaria { get; set; }

    public bool Ativo { get; set; } = true;
    public string? Foto { get; set; }

    public string Cor { get; set; } = null!;
    public int KilometragemAtual { get; set; }
    public DateTime DataVenda { get; set; }

    public virtual ICollection<RevisaoMoto> RevisoesPlanejadas { get; set; } = new List<RevisaoMoto>();
}
