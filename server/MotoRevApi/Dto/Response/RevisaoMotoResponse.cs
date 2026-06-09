namespace MotoRevApi.Dto.Response;

public record RevisaoMotoResponse(
    int Id,
    int RevisaoPadraoId,
    string Nome,
    int Ordem,
    int Quilometragem,
    int TempoMeses,
    DateTime DataPrevista,
    string Status,
    List<ServicoResponse> Servicos,
    List<RevisaoPadraoPecaResponse> Pecas,
    DateTime? DataAgendamento = null,
    int? LojaId = null,
    string? NomeLoja = null,
    string? CidadeLoja = null,
    string? UfLoja = null
);
