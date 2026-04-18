using System.Collections.Generic;
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
    [HttpPost("criar")]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(typeof(MotoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult AdicionarMoto([FromBody] MotoRequest request)
    {
        var response = _motoService.CadastrarMoto(request);
        return CreatedAtAction(nameof(ObterMoto), new { id = response.Id }, response);
    }
    
    /// <summary>
    /// Obter os dados de uma moto específica pelo ID.
    /// </summary>
    /// <param name="id">O ID da moto a ser obtida.</param>
    /// <response code="200">Retorna os dados da moto.</response>
    /// <response code="404">Moto não encontrada.</response>
    [HttpGet("id/{id}")]
    [ProducesResponseType(typeof(MotoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<MotoResponse> ObterMoto(int id)
    {
        var response = _motoService.ObterMoto(id);
        return Ok(response);
    }
    
    /// <summary>
    /// Listar todas as motos cadastradas no sistema.
    /// </summary>
    /// <response code="200">Retorna a lista de motos.</response>
    [HttpGet("listar")]
    [ProducesResponseType(typeof(List<MotoResponse>), StatusCodes.Status200OK)]
    public ActionResult<List<MotoResponse>> ObterMotos()
    {
        var response = _motoService.ListarMotos();
        return Ok(response);
    }
}
