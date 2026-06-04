using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Authorization;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;

namespace MotoRevApi.Controller;

[ApiController]
[Route("api/[controller]")]
[Tags("Revisões Padrão")]
[Authorize(Roles = Roles.Concessionaria)]
public class RevisaoPadraoController : ControllerBase
{
    private readonly RevisaoPadraoService _revisaoPadraoService;
    private readonly ConcessionariaService _concessionariaService;

    public RevisaoPadraoController(RevisaoPadraoService revisaoPadraoService, ConcessionariaService concessionariaService)
    {
        _revisaoPadraoService = revisaoPadraoService;
        _concessionariaService = concessionariaService;
    }

    /// <summary>
    /// Cadastrar uma nova revisão padrão para um modelo de moto.
    /// </summary>
    /// <remarks>
    /// Apenas concessionárias podem cadastrar revisões. A revisão é vinculada à concessionária logada.
    /// </remarks>
    /// <param name="request">Dados da revisão padrão.</param>
    /// <response code="201">Revisão criada com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão.</response>
    /// <response code="404">Modelo de moto ou serviço não encontrado.</response>
    /// <response code="409">Já existe uma revisão com esta ordem para o modelo de moto selecionado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(RevisaoPadraoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CadastrarRevisao([FromBody] RevisaoPadraoRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var concessionaria = await _concessionariaService.GetByUserIdAsync(userId);
        
        var response = await _revisaoPadraoService.CadastrarRevisaoAsync(request, concessionaria.Id);
        
        // Idealmente, teríamos um endpoint GetById para retornar a URL correta
        return CreatedAtAction(null, new { id = response.Id }, response);
    }
}
