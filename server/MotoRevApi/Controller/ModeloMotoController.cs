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
/// API controller para gerenciamento de modelos de motos.
/// </summary>
/// <remarks>
/// Todos os endpoints deste controller requerem a role 'Concessionaria'.
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Tags("Modelos de Motos")]
[Authorize(Roles = Roles.Concessionaria)] // ← Adicionado aqui em nível de classe
public class ModeloMotoController : ControllerBase
{
    private readonly ModeloMotoService _modeloMotoService;

    public ModeloMotoController(ModeloMotoService modeloMotoService)
    {
        _modeloMotoService = modeloMotoService;
    }

    /// <summary>
    /// Adicionar um novo modelo de moto ao sistema.
    /// </summary>
    /// <param name="request">Dados do modelo de moto a ser criado.</param>
    /// <response code="201">Modelo de moto criado com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão para criar modelos de motos.</response>
    [HttpPost("criar")]
    [ProducesResponseType(typeof(ModeloMotoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult AdicionarModeloMoto([FromBody] ModeloMotoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = _modeloMotoService.CadastrarModeloMoto(request);
        return CreatedAtAction(nameof(ObterModeloMoto), new { id = response.Id }, response);
    }

    /// <summary>
    /// Obter os dados de um modelo de moto específico pelo ID.
    /// </summary>
    /// <param name="id">O ID do modelo de moto a ser obtido.</param>
    /// <response code="200">Retorna os dados do modelo de moto.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão para acessar modelos de motos.</response>
    /// <response code="404">Modelo de moto não encontrado.</response>
    [HttpGet("id/{id}")]
    [ProducesResponseType(typeof(ModeloMotoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<ModeloMotoResponse> ObterModeloMoto(int id)
    {
        var response = _modeloMotoService.ObterModeloMoto(id);
        if (response == null)
        {
            return NotFound(new { message = "Modelo de moto não encontrado." });
        }
        return Ok(response);
    }

    /// <summary>
    /// Listar todos os modelos de motos cadastrados no sistema.
    /// </summary>
    /// <response code="200">Retorna a lista de modelos de motos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão para listar modelos de motos.</response>
    [HttpGet("listar")]
    [ProducesResponseType(typeof(List<ModeloMotoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<List<ModeloMotoResponse>> ObterModelosMotos()
    {
        var response = _modeloMotoService.ListarModelosMotos();
        return Ok(response);
    }

    /// <summary>
    /// Atualiza os dados de um modelo de moto existente.
    /// </summary>
    /// <param name="id">O ID do modelo de moto a ser atualizado.</param>
    /// <param name="request">Novos dados do modelo de moto.</param>
    /// <response code="200">Modelo de moto atualizado com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão para atualizar modelos de motos.</response>
    /// <response code="404">Modelo de moto não encontrado.</response>
    [HttpPut("atualizar/{id}")]
    [ProducesResponseType(typeof(ModeloMotoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult AtualizarModeloMoto(int id, [FromBody] ModeloMotoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = _modeloMotoService.AtualizarModeloMoto(id, request);
        
        if (response == null)
        {
            return NotFound(new { message = "Modelo de moto não encontrado." });
        }

        return Ok(response);
    }

    /// <summary>
    /// Alterna o status (Ativo/Inativo) de um modelo de moto (Soft Delete).
    /// </summary>
    /// <param name="id">O ID do modelo de moto cujo status será alternado.</param>
    /// <response code="200">Status do modelo de moto alternado com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário não tem permissão para alternar status.</response>
    /// <response code="404">Modelo de moto não encontrado.</response>
    [HttpPatch("alternar-status/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult AlternarStatusModeloMoto(int id)
    {
        var response = _modeloMotoService.AlternarStatus(id);

        if (response == null)
        {
            return NotFound(new { message = "Modelo de moto não encontrado." });
        }

        return Ok(new { message = response.Ativo ? "Modelo ativado com sucesso." : "Modelo inativado com sucesso.", modelo = response });
    }
}
