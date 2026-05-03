using MotoRevApi.Enums;

namespace MotoRevApi.Dto.Response;

public record ServicoResponse(
    int Id,
    string Nome,
    CategoriaServico Categoria,
    int TempoEstimado
);
