namespace MotoRevApi.Dto.Response;

public record MotoResponse(
    int Id,
    string Placa,
    string Chassi,
    int ModeloMotoId,
    string NomeModelo,
    string Marca,
    int ClienteId,
    int? ConcessionariaId,
    string? NomeConcessionaria,
    string? Foto
);