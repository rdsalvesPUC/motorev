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
/// API controller para gerenciamento de Peças.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Tags("Peças")]
public class PecaController : ControllerBase
{
    private readonly PecaService _pecaService;
    public PecaController(PecaService pecaService)
    {
        _pecaService = pecaService;
    }
    
    /// <summary>
    /// Adicionar uma nova Peça ao sistema.
    /// </summary>
    /// <remarks>
    /// Apenas usuários com a role 'Concessionária' podem adicionar Peças.
    /// </remarks>
    /// <param name="request">Dados da Peça a ser criada.</param>
    /// <response code="201">Peça criada com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão para criar Peças.</response>
    /// <response code="409">Já existe uma Peça cadastrada com os dados informados.</response>
    [HttpPost("criar")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public IActionResult AdicionarPeca([FromBody] PecaRequest request)
    {
        var response = _pecaService.CadastrarPeca(request);
        return CreatedAtAction(nameof(ObterPeca), new { id = response.Id }, response);
    }
    
    /// <summary>
    /// Obter os dados de uma Peça específica pelo ID.
    /// </summary>
    /// <param name="id">O ID da Peça a ser obtida.</param>
    /// <response code="200">Retorna os dados da Peça.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão para consultar Peças.</response>
    /// <response code="404">Peça não encontrada.</response>
    [HttpGet("id/{id}")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<PecaResponse> ObterPeca(int id)
    {
        var response = _pecaService.ObterPeca(id);
        return Ok(response);
    }
    
    /// <summary>
    /// Listar todas as Peças cadastradas no sistema.
    /// </summary>
    /// <response code="200">Retorna a lista de Peças.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão para consultar Peças.</response>
    [HttpGet("listar")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(List<PecaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<List<PecaResponse>> ObterPecas()
    {
        var response = _pecaService.ListarPecas();
        return Ok(response);
    }
    
}
