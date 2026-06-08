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
    /// Lista as revisões padrão cadastradas pela concessionária logada.
    /// </summary>
    /// <remarks>
    /// Opcionalmente, os resultados podem ser filtrados por um modelo de moto específico.
    /// </remarks>
    /// <param name="modeloMotoId">ID opcional do modelo de moto para filtrar a listagem.</param>
    /// <param name="linhaId">ID opcional da linha para filtrar a listagem.</param>
    /// <response code="200">Retorna a lista de revisões (pode estar vazia).</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não for do tipo Concessionaria.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<RevisaoPadraoListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get([FromQuery] int? modeloMotoId, [FromQuery] int? linhaId)
    {
        var concessionariaId = await ObterConcessionariaIdAsync();
        if (concessionariaId == null)
        {
            return Unauthorized();
        }

        var revisoes = await _revisaoPadraoService.ListarRevisoesAsync(concessionariaId.Value, modeloMotoId, linhaId);
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
        var concessionariaId = await ObterConcessionariaIdAsync();
        if (concessionariaId == null)
        {
            return Unauthorized();
        }

        var revisao = await _revisaoPadraoService.GetByIdAsync(id, concessionariaId.Value);
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
        var concessionariaId = await ObterConcessionariaIdAsync();
        if (concessionariaId == null)
        {
            return Unauthorized();
        }

        var response = await _revisaoPadraoService.CadastrarRevisaoAsync(request, concessionariaId.Value);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response); // Atualizado para apontar para o GetById
    }

    /// <summary>
    /// Atualiza os dados de uma revisão padrão existente.
    /// </summary>
    /// <param name="id">ID da revisão padrão a ser atualizada.</param>
    /// <param name="request">Novos dados da revisão padrão.</param>
    /// <response code="200">Revisão padrão atualizada com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão.</response>
    /// <response code="404">Revisão padrão não encontrada.</response>
    /// <response code="409">Conflito de dados (ex: ordem duplicada).</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RevisaoPadraoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Put(int id, [FromBody] RevisaoPadraoUpdateRequest request)
    {
        var concessionariaId = await ObterConcessionariaIdAsync();
        if (concessionariaId == null)
        {
            return Unauthorized();
        }

        var response = await _revisaoPadraoService.AtualizarRevisaoAsync(id, request, concessionariaId.Value);
        return Ok(response);
    }


    /// <summary>
    /// Cadastrar revisões padrão para todos os modelos ativos de uma linha.
    /// </summary>
    /// <param name="request">Dados das revisões padrão por linha.</param>
    /// <response code="201">Revisões criadas com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão.</response>
    /// <response code="404">Linha, modelo, serviço ou peça não encontrado.</response>
    /// <response code="409">Já existe revisão com alguma ordem informada.</response>
    [HttpPost("por-linha")]
    [ProducesResponseType(typeof(List<RevisaoPadraoResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PostPorLinha([FromBody] RevisaoPadraoLinhaRequest request)
    {
        var concessionariaId = await ObterConcessionariaIdAsync();
        if (concessionariaId == null)
        {
            return Unauthorized();
        }

        var response = await _revisaoPadraoService.CadastrarRevisoesPorLinhaAsync(request, concessionariaId.Value);
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
    [ProducesResponseType(typeof(List<RevisaoPadraoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutPorLinha(int linhaId, [FromBody] RevisaoPadraoLinhaRequest request)
    {
        var concessionariaId = await ObterConcessionariaIdAsync();
        if (concessionariaId == null)
        {
            return Unauthorized();
        }

        var response = await _revisaoPadraoService.AtualizarRevisoesPorLinhaAsync(linhaId, request, concessionariaId.Value);
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
    [ProducesResponseType(typeof(List<RevisaoPadraoListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlternarStatusPorLinha(int linhaId)
    {
        var concessionariaId = await ObterConcessionariaIdAsync();
        if (concessionariaId == null)
        {
            return Unauthorized();
        }

        var response = await _revisaoPadraoService.AlternarStatusPorLinhaAsync(linhaId, concessionariaId.Value);
        return Ok(response);
    }

    private async Task<int?> ObterConcessionariaIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return null;
        }

        var concessionaria = await _concessionariaService.GetByUserIdAsync(userId);
        return concessionaria.Id;
    }
}
