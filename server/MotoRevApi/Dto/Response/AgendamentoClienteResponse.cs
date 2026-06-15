namespace MotoRevApi.Dto.Response;

public record AgendamentoClienteResponse(
    int? AgendamentoId,
    int RevisaoMotoId,
    int MotoId,
    string Marca,
    string Modelo,
    string Placa,
    int NumeroRevisao,
    string NomeRevisao,
    string Status,
    DateTime DataIdeal,
    DateTime DataMinima,
    DateTime DataLimite,
    DateTime? DataAgendada,
    int? LojaId,
    string? NomeLoja,
    string? CidadeLoja,
    int QuantidadePecas,
    int QuantidadeServicos,
    string PrazoTexto,
    string? MensagemRecusa = null,
    DateTime? DataRecusa = null
);
