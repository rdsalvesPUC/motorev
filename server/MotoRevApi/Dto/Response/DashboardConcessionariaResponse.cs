namespace MotoRevApi.Dto.Response;

public record DashboardConcessionariaResponse(
    DateTime Data,
    int TotalMecanicos,
    int TotalRevisoes,
    int RevisoesAgendadas,
    int RevisoesEmExecucao,
    int RevisoesConcluidas,
    List<FilaMecanicoResponse> Filas);

public record FilaMecanicoResponse(
    string MecanicoId,
    string Nome,
    string Especialidade,
    int CapacidadeDiaria,
    List<ItemFilaRevisaoResponse> Itens);

public record ItemFilaRevisaoResponse(
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
    int PosicaoFila,
    int Quilometragem,
    int QuantidadePecas,
    int QuantidadeServicos,
    int TotalItens,
    int ItensConcluidos,
    int ProgressoPercentual);
