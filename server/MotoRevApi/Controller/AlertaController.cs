using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Enums;
using MotoRevApi.Services;

namespace MotoRevApi.Controller;

/// <summary>
/// API controller para gerenciamento de alertas e notificações.
/// </summary>
[ApiController]
[Route("api/alertas")]
[Authorize]
[Tags("Alertas")]
public class AlertaController : ControllerBase
{
    private readonly AlertaService _alertaService;

    public AlertaController(AlertaService alertaService)
    {
        _alertaService = alertaService;
    }

    /// <summary>
    /// Lista os alertas do usuário autenticado.
    /// </summary>
    /// <param name="lido">Filtrar por status de leitura.</param>
    /// <param name="tipo">Filtrar por tipo de alerta.</param>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool? lido, [FromQuery] TipoAlerta? tipo)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

        var alertas = await _alertaService.ListarAlertasAsync(usuarioId, lido, tipo);
        return Ok(alertas);
    }

    /// <summary>
    /// Retorna o total de alertas não lidos do usuário.
    /// </summary>
    [HttpGet("nao-lidos/total")]
    public async Task<IActionResult> ContarNaoLidos()
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

        var total = await _alertaService.ContarNaoLidosAsync(usuarioId);
        return Ok(new { total });
    }

    /// <summary>
    /// Marca um alerta específico como lido.
    /// </summary>
    /// <param name="id">ID do alerta.</param>
    [HttpPut("{id}/ler")]
    public async Task<IActionResult> MarcarComoLido(int id)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

        var alerta = await _alertaService.MarcarComoLidoAsync(id, usuarioId);
        if (alerta == null) return NotFound();

        return Ok(alerta);
    }

    /// <summary>
    /// Marca todos os alertas do usuário como lidos.
    /// </summary>
    [HttpPut("ler-todos")]
    public async Task<IActionResult> MarcarTodosComoLidos()
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

        await _alertaService.MarcarTodosComoLidosAsync(usuarioId);
        return NoContent();
    }
}
