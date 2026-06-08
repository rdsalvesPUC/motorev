using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record ConcessionariaAlterarSenhaRequest(
    [Required] string SenhaAtual,
    [Required] string NovaSenha,
    [Required] string ConfirmarNovaSenha
);
