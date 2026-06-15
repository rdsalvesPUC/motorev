using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record AgendarRevisaoRequest(
    [Required] int LojaId,
    [Required] DateTime DataAgendada);
