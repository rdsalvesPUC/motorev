using MotoRevApi.Enums;

namespace MotoRevApi.Dto.Response;

public record ServicoResponse(
    int Id,
    string Codigo,
    string Nome,
    string Descricao,
    CategoriaServico Categoria,
    int TempoEstimado,
    decimal Custo,
    bool Ativo,
    string? StatusExecucao = null
);
