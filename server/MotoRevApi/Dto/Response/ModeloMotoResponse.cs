namespace MotoRevApi.Dto.Response;

public record ModeloMotoResponse(
    int Id,
    string NomeModelo,
    string Marca,
    string? Categoria,
    string? Linha,
    string? Cilindrada,
    int? Ano,
    bool Ativo
);
