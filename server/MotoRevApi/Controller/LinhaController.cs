using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Authorization;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;
using System.Collections.Generic;

namespace MotoRevApi.Controller;

/// <summary>
/// API controller para gerenciamento de linhas de motos.
/// </summary>
/// <remarks>
/// Os endpoints de escrita (Criar, Atualizar, Inativar) requerem a role 'Concessionaria'.
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Tags("Linhas de Motos")]
[Authorize]
public class LinhaController : ControllerBase
{
    private readonly LinhaService _linhaService;

    public LinhaController(LinhaService linhaService)
    {
        _linhaService = linhaService;
    }

    /// <summary>
    /// Criar uma nova linha de moto.
    /// </summary>
    /// <param name="request">Dados da linha de moto a ser criada.</param>
    /// <response code="201">Linha de moto criada com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão para criar linhas de motos.</response>
    /// <response code="409">Já existe uma linha cadastrada com este nome.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(LinhaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public IActionResult CriarLinha([FromBody] LinhaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = _linhaService.CadastrarLinha(request);
        return CreatedAtAction(nameof(ObterLinha), new { id = response.Id }, response);
    }

    /// <summary>
    /// Listar linhas de motos.
    /// </summary>
    /// <param name="apenasAtivos">Se true, retorna apenas as linhas ativas. Padrão é true.</param>
    /// <response code="200">Retorna a lista de linhas de motos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<LinhaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<List<LinhaResponse>> ListarLinhas([FromQuery] bool apenasAtivos = true)
    {
        var response = _linhaService.ListarLinhas(apenasAtivos);
        return Ok(response);
    }

    /// <summary>
    /// Obter os dados de uma linha de moto específica pelo ID.
    /// </summary>
    /// <param name="id">O ID da linha de moto a ser obtida.</param>
    /// <response code="200">Retorna os dados da linha de moto.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="404">Linha não encontrada.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LinhaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<LinhaResponse> ObterLinha(int id)
    {
        var response = _linhaService.ObterLinha(id);
        return Ok(response);
    }

    /// <summary>
    /// Atualizar uma linha de moto existente.
    /// </summary>
    /// <param name="id">O ID da linha de moto a ser atualizada.</param>
    /// <param name="request">Novos dados da linha de moto.</param>
    /// <response code="200">Linha de moto atualizada com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão para atualizar linhas de motos.</response>
    /// <response code="404">Linha não encontrada.</response>
    /// <response code="409">Já existe outra linha cadastrada com este nome.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(LinhaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public IActionResult AtualizarLinha(int id, [FromBody] LinhaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = _linhaService.AtualizarLinha(id, request);
        return Ok(response);
    }

    /// <summary>
    /// Inativar uma linha de moto (Soft Delete).
    /// </summary>
    /// <param name="id">O ID da linha de moto a ser inativada.</param>
    /// <response code="200">Linha de moto inativada com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão para inativar linhas de motos.</response>
    /// <response code="404">Linha não encontrada.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(LinhaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult InativarLinha(int id)
    {
        var response = _linhaService.InativarLinha(id);
        return Ok(response);
    }

    /// <summary>
    /// Alternar o status ativo/inativo de uma linha de moto.
    /// </summary>
    /// <param name="id">O ID da linha de moto a ter o status alternado.</param>
    /// <response code="200">Status da linha de moto alternado com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão para alternar o status.</response>
    /// <response code="404">Linha não encontrada.</response>
    [HttpPatch("alternar-status/{id}")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(LinhaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult AlternarStatus(int id)
    {
        var response = _linhaService.AlternarStatus(id);
        return Ok(response);
    }
}
