using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record AgendamentoRevisaoRequest(
    [Required] int LojaId,
    [Required] DateTime DataAgendamento
);
