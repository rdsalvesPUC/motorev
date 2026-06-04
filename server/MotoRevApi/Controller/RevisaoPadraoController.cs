using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;
using System.Security.Claims;

namespace MotoRevApi.Controller;

[ApiController]
[Route("api/revisao-padrao")]
[Tags("Revisões Padrão")]
[Authorize(Roles = "Concessionaria")]
public class RevisaoPadraoController : ControllerBase
{
    private readonly RevisaoPadraoService _revisaoPadraoService;

    public RevisaoPadraoController(RevisaoPadraoService revisaoPadraoService)
    {
        _revisaoPadraoService = revisaoPadraoService;
    }

    /// <summary>
    /// Lista as revisões padrão cadastradas pela concessionária logada.
    /// </summary>
    /// <remarks>
    /// Opcionalmente, os resultados podem ser filtrados por um modelo de moto específico.
    /// </remarks>
    /// <param name="modeloMotoId">ID opcional do modelo de moto para filtrar a listagem.</param>
    /// <response code="200">Retorna a lista de revisões (pode estar vazia).</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não for do tipo Concessionaria.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<RevisaoPadraoListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get([FromQuery] int? modeloMotoId)
    {
        var concessionariaId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var revisoes = await _revisaoPadraoService.ListarRevisoesAsync(concessionariaId, modeloMotoId);
        return Ok(revisoes);
    }

    /// <summary>
    /// Cadastrar uma nova revisão padrão.
    /// </summary>
    /// <param name="request">Dados da revisão padrão.</param>
    /// <response code="201">Revisão criada com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão.</response>
    /// <response code="404">Modelo de moto ou serviço não encontrado.</response>
    /// <response code="409">Já existe uma revisão com esta ordem para o modelo selecionado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(RevisaoPadraoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Post([FromBody] RevisaoPadraoRequest request)
    {
        var concessionariaId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var response = await _revisaoPadraoService.CadastrarRevisaoAsync(request, concessionariaId);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }
}
