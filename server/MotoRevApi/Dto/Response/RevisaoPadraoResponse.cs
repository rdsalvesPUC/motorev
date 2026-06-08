namespace MotoRevApi.Dto.Response;

public record RevisaoPadraoResponse(
    int Id,
    string Nome,
    int Ordem,
    int Quilometragem,
    int TempoMeses,
    int LinhaId,
    string NomeLinha,
    List<ServicoResponse> Servicos,
    List<RevisaoPadraoPecaResponse> Pecas
);
