namespace MotoRevApi.Dto.Response;

public record ModeloMotoResponse(
    int Id,
    string NomeModelo,
    string Marca,
    int LinhaId,
    string? Cilindrada,
    int? Ano,
    bool Ativo
);
