using MotoRevApi.Enums;

namespace MotoRevApi.Model;

public class Agendamento
{
    public int Id { get; set; }

    public int RevisaoMotoId { get; set; }
    public virtual RevisaoMoto RevisaoMoto { get; set; } = null!;

    public int LojaId { get; set; }
    public virtual Loja Loja { get; set; } = null!;

    public DateTime DataAgendada { get; set; }
    public StatusAgendamento Status { get; set; } = StatusAgendamento.AguardandoConfirmacao;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
}
