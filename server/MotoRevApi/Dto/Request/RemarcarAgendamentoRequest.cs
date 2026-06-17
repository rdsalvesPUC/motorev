using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record RemarcarAgendamentoRequest(
    [Required] DateTime NovaData
);
