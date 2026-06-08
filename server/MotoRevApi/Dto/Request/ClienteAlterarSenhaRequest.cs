using System.ComponentModel.DataAnnotations;

namespace MotoRevApi.Dto.Request;

public record ClienteAlterarSenhaRequest(
    [Required] string SenhaAtual,
    [Required] string NovaSenha,
    [Required] string ConfirmarNovaSenha
);
