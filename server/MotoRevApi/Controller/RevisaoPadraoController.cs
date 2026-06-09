using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Authorization;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;
using System.Security.Claims;

namespace MotoRevApi.Controller;

[ApiController]
[Route("api/revisao-padrao")]
[Tags("Revisões Padrão")]
[Authorize]
public class RevisaoPadraoController : ControllerBase
{
    private readonly RevisaoPadraoService _revisaoPadraoService;

    public RevisaoPadraoController(RevisaoPadraoService revisaoPadraoService)
    {
        _revisaoPadraoService = revisaoPadraoService;
    }

    /// <summary>
    /// Lista as revisões padrão.
    /// </summary>
    /// <remarks>
    /// Opcionalmente, os resultados podem ser filtrados por um modelo de moto ou linha específica.
    /// Acessível por qualquer usuário autenticado.
    /// </remarks>
    /// <param name="modeloMotoId">ID opcional do modelo de moto para filtrar a listagem.</param>
    /// <param name="linhaId">ID opcional da linha para filtrar a listagem.</param>
    /// <response code="200">Retorna a lista de revisões (pode estar vazia).</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<RevisaoPadraoListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get([FromQuery] int? modeloMotoId, [FromQuery] int? linhaId)
    {
        var revisoes = await _revisaoPadraoService.ListarRevisoesAsync(modeloMotoId, linhaId);
        return Ok(revisoes);
    }

    /// <summary>
    /// Obtém os detalhes de uma revisão padrão específica pelo ID.
    /// </summary>
    /// <remarks>
    /// Acessível por qualquer usuário autenticado.
    /// </remarks>
    /// <param name="id">O ID da revisão padrão a ser detalhada.</param>
    /// <response code="200">Retorna os detalhes da revisão.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="404">Se a revisão não existir.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RevisaoPadraoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var revisao = await _revisaoPadraoService.GetByIdAsync(id);
        return Ok(revisao);
    }

    /// <summary>
    /// Cadastrar revisões padrão para uma linha.
    /// </summary>
    /// <param name="request">Dados das revisões padrão por linha.</param>
    /// <response code="201">Revisões criadas com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão.</response>
    /// <response code="404">Linha, serviço ou peça não encontrado.</response>
    /// <response code="409">Já existe revisão com alguma ordem informada.</response>
    [HttpPost("por-linha")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(List<RevisaoPadraoResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PostPorLinha([FromBody] RevisaoPadraoLinhaRequest request)
    {
        var response = await _revisaoPadraoService.CadastrarRevisoesPorLinhaAsync(request);
        return CreatedAtAction(nameof(Get), new { linhaId = request.LinhaId }, response);
    }

    /// <summary>
    /// Atualiza as revisões padrão de uma linha (substituindo todas as antigas).
    /// </summary>
    /// <param name="linhaId">ID da linha.</param>
    /// <param name="request">Novos dados das revisões padrão por linha.</param>
    /// <response code="200">Revisões atualizadas com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão.</response>
    /// <response code="404">Linha, serviço ou peça não encontrado.</response>
    [HttpPut("por-linha/{linhaId}")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(List<RevisaoPadraoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutPorLinha(int linhaId, [FromBody] RevisaoPadraoLinhaRequest request)
    {
        var response = await _revisaoPadraoService.AtualizarRevisoesPorLinhaAsync(linhaId, request);
        return Ok(response);
    }

    /// <summary>
    /// Alternar status ativo/inativo das revisões padrão de uma linha.
    /// </summary>
    /// <param name="linhaId">ID da linha.</param>
    /// <response code="200">Retorna as revisões com status atualizado.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão.</response>
    /// <response code="404">Nenhuma revisão encontrada para a linha.</response>
    [HttpPatch("linha/{linhaId}/alternar-status")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(List<RevisaoPadraoListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlternarStatusPorLinha(int linhaId)
    {
        var response = await _revisaoPadraoService.AlternarStatusPorLinhaAsync(linhaId);
        return Ok(response);
    }
}
