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
    /// Lista todas as motos do cliente autenticado.
    /// </summary>
    /// <remarks>
    /// O filtro do cliente é feito internamente através do token JWT.
    /// </remarks>
    /// <response code="200">Lista de motos retornada com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem a role 'Cliente'.</response>
    [HttpGet]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(typeof(List<MotoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarMinhasMotos()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var response = await _motoService.ListarMotosClienteAsync(userId);
        return Ok(response);
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

    /// <summary>
    /// Obter detalhes de uma moto específica do cliente autenticado.
    /// </summary>
    /// <remarks>
    /// Retorna somente motos que pertencem ao cliente identificado pelo token JWT.
    /// </remarks>
    /// <param name="id">ID da moto.</param>
    /// <response code="200">Dados da moto retornados com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem a role 'Cliente'.</response>
    /// <response code="404">Moto não encontrada ou não pertence ao cliente autenticado.</response>
    [HttpGet("{id}")]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(typeof(MotoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterMoto(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var response = await _motoService.GetByIdAsync(id, userId);
        return Ok(response);
    }

    /// <summary>
    /// Atualizar os dados editáveis de uma moto do cliente autenticado.
    /// </summary>
    /// <remarks>
    /// Os campos <c>Placa</c>, <c>Cor</c> e <c>KilometragemAtual</c> são atualizados.
    /// Os campos <c>Chassi</c>, <c>ModeloMotoId</c> e <c>Ano</c> são imutáveis e ignorados nesta operação,
    /// mesmo que sejam enviados no corpo da requisição.
    /// A <c>KilometragemAtual</c> não pode ser inferior à quilometragem atual do veículo no banco de dados.
    /// </remarks>
    /// <param name="id">ID da moto a ser atualizada.</param>
    /// <param name="request">Dados editáveis da moto (Placa, Cor e KilometragemAtual).</param>
    /// <response code="200">Moto atualizada com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem a role 'Cliente'.</response>
    /// <response code="404">Moto não encontrada ou não pertence ao cliente.</response>
    /// <response code="409">Placa já está em uso por outro veículo.</response>
    /// <response code="422">A quilometragem não pode ser reduzida.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(typeof(MotoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AtualizarMoto(int id, [FromBody] MotoUpdateRequest request)
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

        var response = await _motoService.AtualizarMotoAsync(id, request, userId);
        return Ok(response);
    }

    /// <summary>
    /// Inativa uma moto do cliente autenticado (Soft Delete).
    /// </summary>
    /// <remarks>
    /// A moto é desativada logicamente (Ativo = false), mantendo o histórico de revisões.
    /// Não é possível inativar se houver agendamentos pendentes.
    /// </remarks>
    /// <param name="id">ID da moto a ser inativada.</param>
    /// <response code="204">Moto inativada com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário autenticado mas sem a role 'Cliente'.</response>
    /// <response code="404">Moto não encontrada ou não pertence ao cliente.</response>
    /// <response code="422">A moto possui agendamentos pendentes.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> InativarMoto(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _motoService.InativarMotoAsync(id, userId);
        return NoContent();
    }
}
