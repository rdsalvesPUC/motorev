using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record RecusarAgendamentoRequest(
    [StringLength(500)] string? Motivo);
