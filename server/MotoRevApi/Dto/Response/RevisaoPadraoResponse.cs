namespace MotoRevApi.Dto.Response;

public record RevisaoPadraoResponse(
    int Id,
    string Nome,
    int Ordem,
    int ModeloMotoId,
    string NomeModeloMoto,
    List<ServicoResponse> Servicos
    // TODO: Adicionar a lista de Peças quando o catálogo de peças estiver implementado
);
