namespace MotoRevApi.Dto.Response;

public record AgendamentoConcessionariaResponse(
    int AgendamentoId,
    int RevisaoMotoId,
    int MotoId,
    int LojaId,
    string LojaNome,
    string ClienteNome,
    string Marca,
    string Modelo,
    string Placa,
    int NumeroRevisao,
    string NomeRevisao,
    string Status,
    DateTime DataIdeal,
    DateTime DataAgendada,
    int Quilometragem,
    int QuantidadePecas,
    int QuantidadeServicos,
    string? MensagemRecusa,
    DateTime? DataRecusa);
