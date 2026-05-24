using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Authorization;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;

namespace MotoRevApi.Controller;

/// <summary>
/// API controller para gerenciamento de motos.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Tags("Motos")]
public class MotoController : ControllerBase
{
    private readonly MotoService _motoService;

    public MotoController(MotoService motoService)
    {
        _motoService = motoService;
    }

    /// <summary>
    /// Adicionar uma nova moto ao sistema.
    /// </summary>
    /// <remarks>
    /// Apenas usuários com a role 'Cliente' podem adicionar motos.
    /// </remarks>
    /// <param name="request">Dados da moto a ser criada.</param>
    /// <response code="201">Moto criada com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão para criar motos.</response>
    /// <response code="409">Veículo com esta placa ou chassi já cadastrado.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(typeof(MotoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AdicionarMoto([FromBody] MotoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var response = await _motoService.CadastrarMotoAsync(request, userId);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
