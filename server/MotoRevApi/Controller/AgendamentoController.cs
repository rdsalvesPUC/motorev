using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Authorization;
using MotoRevApi.Dto.Request;
using MotoRevApi.Services;

namespace MotoRevApi.Controller;

[ApiController]
[Route("api/[controller]")]
public class AgendamentoController : ControllerBase
{
    private readonly AgendamentoService _agendamentoService;

    public AgendamentoController(AgendamentoService agendamentoService)
    {
        _agendamentoService = agendamentoService;
    }

    [HttpGet("cliente")]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<IActionResult> ListarAgendamentosCliente()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var response = await _agendamentoService.ListarAgendamentosClienteAsync(userId);
        return Ok(response);
    }

    [HttpPatch("cliente/{agendamentoId:int}/cancelar")]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<IActionResult> CancelarAgendamentoCliente(int agendamentoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _agendamentoService.CancelarAgendamentoClienteAsync(agendamentoId, userId);
        return NoContent();
    }

    [HttpPost("cliente/{agendamentoId:int}/remarcar")]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<IActionResult> RemarcarAgendamentoCliente(
        int agendamentoId,
        [FromBody] RemarcarAgendamentoRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _agendamentoService.RemarcarAgendamentoClienteAsync(agendamentoId, userId, request);
        return NoContent();
    }

    [HttpPost("cliente/revisoes/{revisaoMotoId:int}/agendar")]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<IActionResult> AgendarRevisaoCliente(
        int revisaoMotoId,
        [FromBody] AgendarRevisaoRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _agendamentoService.AgendarRevisaoClienteAsync(revisaoMotoId, userId, request);
        return NoContent();
    }

    [HttpPatch("cliente/{agendamentoId:int}/recusa/visualizar")]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<IActionResult> VisualizarRecusaCliente(int agendamentoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _agendamentoService.VisualizarRecusaClienteAsync(agendamentoId, userId);
        return NoContent();
    }

    [HttpGet("concessionaria")]
    [Authorize(Roles = Roles.Concessionaria)]
    public async Task<IActionResult> ListarAgendamentosConcessionaria()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var response = await _agendamentoService.ListarAgendamentosConcessionariaAsync(userId);
        return Ok(response);
    }

    [HttpPatch("concessionaria/{agendamentoId:int}/aceitar")]
    [Authorize(Roles = Roles.Concessionaria)]
    public async Task<IActionResult> AceitarSolicitacaoConcessionaria(int agendamentoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _agendamentoService.AceitarSolicitacaoConcessionariaAsync(agendamentoId, userId);
        return NoContent();
    }

    [HttpPost("concessionaria/{agendamentoId:int}/recusar")]
    [Authorize(Roles = Roles.Concessionaria)]
    public async Task<IActionResult> RecusarSolicitacaoConcessionaria(
        int agendamentoId,
        [FromBody] RecusarAgendamentoRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _agendamentoService.RecusarSolicitacaoConcessionariaAsync(agendamentoId, userId, request);
        return NoContent();
    }
}
