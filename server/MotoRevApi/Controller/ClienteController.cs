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
/// API controller para gerenciamento de clientes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Clientes")]
public class ClienteController : ControllerBase
{
    private readonly ClienteService _clienteService;

    public ClienteController(ClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    /// <summary>
    /// Registrar um novo cliente.
    /// </summary>
    /// <param name="request">Os dados para registrar o novo cliente.</param>
    /// <response code="201">Retorna o cliente recém-criado.</response>
    /// <response code="400">Se os dados fornecidos forem inválidos.</response>
    /// <response code="409">Se já existir um cliente com o mesmo CPF ou um usuário com o mesmo email.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterClienteRequest request)
    {
        var response = await _clienteService.RegisterAsync(request);
        // Changed to use the default 'Get' route since GetById now uses user context
        return CreatedAtAction(nameof(Get), response);
    }

    /// <summary>
    /// Obter os dados do cliente autenticado.
    /// </summary>
    /// <remarks>
    /// O ID do cliente é extraído automaticamente do token JWT.
    /// </remarks>
    /// <response code="200">Retorna os dados do cliente.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="404">Se o cliente não for encontrado.</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _clienteService.GetByUserIdAsync(userId);
        return Ok(response);
    }

    /// <summary>
    /// Atualizar as informações de um cliente.
    /// </summary>
    /// <remarks>
    /// O ID do cliente é extraído automaticamente do token JWT do usuário autenticado.
    /// </remarks>
    /// <param name="request">Os novos dados do cliente.</param>
    /// <response code="204">Cliente atualizado com sucesso.</response>
    /// <response code="400">Se os dados fornecidos forem inválidos.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão de 'Cliente'.</response>
    /// <response code="404">Se o cliente não for encontrado.</response>
    [HttpPut]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromBody] UpdateClienteRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
        
        await _clienteService.UpdateAsync(userId, request);
        return NoContent();
    }

    /// <summary>
    /// Deletar um cliente.
    /// </summary>
    /// <remarks>
    /// O ID do cliente a ser deletado é extraído automaticamente do token JWT.
    /// </remarks>
    /// <response code="204">Cliente deletado com sucesso.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão de 'Cliente'.</response>
    /// <response code="404">Se o cliente não for encontrado.</response>
    [HttpDelete]
    [Authorize(Roles = Roles.Cliente)]
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

        await _clienteService.DeleteAsync(userId);
        return NoContent();
    }
}
