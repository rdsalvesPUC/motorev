namespace MotoRevApi.Dto.Response;

public record RevisaoPadraoListResponse(
    int Id,
    string Nome,
    int ModeloMotoId,
    string NomeModeloMoto,
    int LinhaId,
    string NomeLinha,
    int Ordem,
    int Quilometragem,
    int TempoMeses,
    bool Ativo
);
