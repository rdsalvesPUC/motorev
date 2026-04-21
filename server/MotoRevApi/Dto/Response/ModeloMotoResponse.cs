namespace MotoRevApi.Dto.Response;

public record ModeloMotoResponse(
    int Id,
    string NomeModelo,
    string Marca,
    string? Categoria,
    bool Ativo
);
