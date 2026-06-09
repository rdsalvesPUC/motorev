using MotoRevApi.Enums;

namespace MotoRevApi.Model;

public class Alerta
{
    public int Id { get; set; }
    public TipoAlerta Tipo { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public bool Lido { get; set; } = false;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public int? MotoId { get; set; }
    public int? AgendamentoId { get; set; }
    public int? Quilometragem { get; set; }
}
