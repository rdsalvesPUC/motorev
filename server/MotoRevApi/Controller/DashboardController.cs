using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Authorization;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;

namespace MotoRevApi.Controller;

/// <summary>
/// API controller para dashboards operacionais do MotoRev.
/// </summary>
/// <remarks>
/// Centraliza indicadores e visões consolidadas que dependem de múltiplos cadastros
/// e fluxos do sistema.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardConcessionariaService _dashboardConcessionariaService;

    /// <summary>
    /// Inicializa uma nova instância do controller de dashboard.
    /// </summary>
    /// <param name="dashboardConcessionariaService">Serviço responsável pelo dashboard da Concessionária.</param>
    public DashboardController(DashboardConcessionariaService dashboardConcessionariaService)
    {
        _dashboardConcessionariaService = dashboardConcessionariaService;
    }

    /// <summary>
    /// Retorna as filas de revisões da concessionária autenticada para um dia.
    /// </summary>
    /// <remarks>
    /// Monta automaticamente as filas a partir dos agendamentos confirmados do dia.
    /// São considerados os agendamentos das lojas da Concessionária autenticada nos status
    /// <c>Agendada</c>, <c>EmExecucao</c> e <c>Concluida</c>.
    /// Enquanto o sistema não possui cadastro real de mecânicos, a distribuição usa quatro
    /// mecânicos mockados e aplica round-robin pela ordem de chegada: data agendada, criação
    /// do agendamento e ID.
    /// </remarks>
    /// <param name="data">Data desejada no formato yyyy-MM-dd. Se omitida, usa a data atual.</param>
    /// <response code="200">Dashboard de filas retornado com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário autenticado sem a role 'Concessionaria'.</response>
    /// <response code="404">Concessionária não encontrada para o usuário autenticado.</response>
    [HttpGet("concessionaria/revisoes")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(DashboardConcessionariaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterDashboardRevisoesConcessionaria([FromQuery] DateTime? data = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var response = await _dashboardConcessionariaService.ObterDashboardRevisoesAsync(userId, data);
        return Ok(response);
    }
}
