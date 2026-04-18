using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Authorization;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;

namespace MotoRevApi.Controller;

/// <summary>
/// API controller para gerenciamento de concessionárias.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Concessionárias")]
public class ConcessionariaController : ControllerBase
{
    private readonly ConcessionariaService _concessionariaService;

    public ConcessionariaController(ConcessionariaService concessionariaService)
    {
        _concessionariaService = concessionariaService;
    }

    /// <summary>
    /// Registrar uma nova concessionária.
    /// </summary>
    /// <param name="request">Os dados para registrar a nova concessionária.</param>
    /// <response code="201">Retorna a concessionária recém-criada.</response>
    /// <response code="400">Se os dados fornecidos forem inválidos.</response>
    /// <response code="409">Se já existir uma concessionária com o mesmo CNPJ ou um usuário com o mesmo email.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ConcessionariaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterConcessionariaRequest request)
    {
        var response = await _concessionariaService.RegisterAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Obter todas as concessionárias cadastradas.
    /// </summary>
    /// <response code="200">Retorna a lista de concessionárias.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ConcessionariaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var response = await _concessionariaService.GetAllAsync();
        return Ok(response);
    }

    /// <summary>
    /// Obter uma concessionária específica pelo ID.
    /// </summary>
    /// <param name="id">O ID da concessionária.</param>
    /// <response code="200">Retorna os dados da concessionária.</response>
    /// <response code="404">Se a concessionária não for encontrada.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ConcessionariaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _concessionariaService.GetByIdAsync(id);
        return Ok(response);
    }

    /// <summary>
    /// Obter a concessionária autenticada.
    /// </summary>
    /// <remarks>
    /// O ID da concessionária é extraído automaticamente do token JWT.
    /// </remarks>
    /// <response code="200">Retorna os dados da concessionária.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão de 'Concessionaria'.</response>
    /// <response code="404">Se a concessionária não for encontrada.</response>
    [HttpGet("me")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(ConcessionariaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _concessionariaService.GetByUserIdAsync(userId);
        return Ok(response);
    }

    /// <summary>
    /// Atualizar as informações de uma concessionária.
    /// </summary>
    /// <remarks>
    /// O ID da concessionária é extraído automaticamente do token JWT do usuário autenticado.
    /// </remarks>
    /// <param name="request">Os novos dados da concessionária.</param>
    /// <response code="204">Concessionária atualizada com sucesso.</response>
    /// <response code="400">Se os dados fornecidos forem inválidos.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão de 'Concessionaria'.</response>
    /// <response code="404">Se a concessionária não for encontrada.</response>
    [HttpPut]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromBody] UpdateConcessionariaRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        await _concessionariaService.UpdateAsync(userId, request);
        return NoContent();
    }

    /// <summary>
    /// Deletar uma concessionária.
    /// </summary>
    /// <remarks>
    /// O ID da concessionária a ser deletada é extraído automaticamente do token JWT.
    /// </remarks>
    /// <response code="204">Concessionária deletada com sucesso.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão de 'Concessionaria'.</response>
    /// <response code="404">Se a concessionária não for encontrada.</response>
    [HttpDelete]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        await _concessionariaService.DeleteAsync(userId);
        return NoContent();
    }
}
