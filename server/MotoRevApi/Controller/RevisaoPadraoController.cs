using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;
using System.Security.Claims;

namespace MotoRevApi.Controller;

/// <summary>
/// API controller para gerenciamento de Revisões Padrão.
/// </summary>
[ApiController]
[Route("api/revisao-padrao")]
[Tags("Revisões Padrão")]
[Authorize(Roles = "Concessionaria")]
public class RevisaoPadraoController : ControllerBase
{
    private readonly RevisaoPadraoService _revisaoPadraoService;

    /// <summary>
    /// Construtor do controller.
    /// </summary>
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
    /// Obtém os detalhes de uma revisão padrão específica pelo ID.
    /// </summary>
    /// <param name="id">O ID da revisão padrão a ser detalhada.</param>
    /// <response code="200">Retorna os detalhes da revisão.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não for do tipo Concessionaria.</response>
    /// <response code="404">Se a revisão não existir ou não pertencer à concessionária logada.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RevisaoPadraoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var concessionariaId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var revisao = await _revisaoPadraoService.GetByIdAsync(id, concessionariaId);
        return Ok(revisao);
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
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response); // Atualizado para apontar para o GetById
    }

    /// <summary>
    /// Atualiza os dados e vínculos (Peças/Serviços) de uma revisão padrão existente.
    /// </summary>
    /// <remarks>
    /// O modelo de moto associado à revisão não pode ser alterado.
    /// </remarks>
    /// <param name="id">ID da revisão a ser atualizada.</param>
    /// <param name="request">Novos dados da revisão (nome, ordem e lista de serviços).</param>
    /// <response code="200">Revisão atualizada com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão.</response>
    /// <response code="404">Revisão ou serviço não encontrado.</response>
    /// <response code="409">Já existe uma revisão com esta ordem para o modelo de moto.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RevisaoPadraoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Put(int id, [FromBody] RevisaoPadraoUpdateRequest request)
    {
        var concessionariaId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var response = await _revisaoPadraoService.AtualizarRevisaoAsync(id, request, concessionariaId);
        return Ok(response);
    }
}
