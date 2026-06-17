using MotoRevApi.Enums;

namespace MotoRevApi.Dto.Response;

public record AlertaResponse(
    int Id,
    TipoAlerta Tipo,
    string UsuarioId,
    int? MotoId,
    int? AgendamentoId,
    int? Quilometragem,
    bool Lido,
    DateTime CriadoEm,
    int? OrdemRevisao = null,
    string? ModeloMotoNome = null,
    string? MarcaMoto = null
);
