using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Authorization;
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
}
