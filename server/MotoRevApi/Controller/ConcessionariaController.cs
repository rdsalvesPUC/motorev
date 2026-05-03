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
    private readonly EnderecoService _enderecoService;

    /// <summary>
    /// Construtor do controller de concessionárias.
    /// </summary>
    public ConcessionariaController(ConcessionariaService concessionariaService, EnderecoService enderecoService)
    {
        _concessionariaService = concessionariaService;
        _enderecoService = enderecoService;
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
    /// Obter uma concessionária específica pelo ID.
    /// </summary>
    /// <param name="id">O ID da concessionária.</param>
    /// <response code="200">Retorna os dados da concessionária e seus endereços.</response>
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
    /// <response code="200">Retorna os dados da concessionária e seus endereços.</response>
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
    /// Adicionar um novo endereço ao perfil da concessionária logada.
    /// </summary>
    /// <param name="request">Dados do endereço.</param>
    /// <response code="201">Retorna o endereço recém-criado.</response>
    /// <response code="400">Dados de endereço inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão de 'Concessionaria'.</response>
    [HttpPost("me/enderecos")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(EnderecoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AdicionarEndereco([FromBody] EnderecoRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var concessionaria = await _concessionariaService.GetByUserIdAsync(userId);

        var response = await _enderecoService.AdicionarEnderecoAsync(concessionaria.Id, request);
        return Created("", response); // Pode-se criar um endpoint GetEnderecoById no futuro para usar CreatedAtAction
    }

    /// <summary>
    /// Remover um endereço do perfil da concessionária logada.
    /// </summary>
    /// <param name="enderecoId">ID do endereço a ser removido.</param>
    /// <response code="200">Endereço removido com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão de 'Concessionaria'.</response>
    /// <response code="404">Endereço não encontrado ou não pertence a esta concessionária.</response>
    [HttpDelete("me/enderecos/{enderecoId}")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverEndereco(int enderecoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var concessionaria = await _concessionariaService.GetByUserIdAsync(userId);

        await _enderecoService.RemoverEnderecoAsync(enderecoId, concessionaria.Id);
        
        return Ok(new { message = "Endereço removido com sucesso." });
    }
}
