namespace MotoRevApi.Dto.Response;

public record RevisaoPadraoResponse(
    int Id,
    string Nome,
    int Ordem,
    int Quilometragem,
    int TempoMeses,
    int ModeloMotoId,
    string NomeModeloMoto,
    List<ServicoResponse> Servicos,
    List<RevisaoPadraoPecaResponse> Pecas
);
