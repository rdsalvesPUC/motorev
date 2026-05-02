using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;

namespace MotoRevApi.Controller;

/// <summary>
/// API controller para gerenciamento de serviços.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Serviços")]
public class ServicoController : ControllerBase
{
    private readonly ServicoService _servicoService;

    public ServicoController(ServicoService servicoService)
    {
        _servicoService = servicoService;
    }

    /// <summary>
    /// Cadastrar um novo serviço.
    /// </summary>
    /// <remarks>
    /// Endpoint disponível apenas para Concessionárias.
    /// </remarks>
    /// <param name="request">Os dados para registrar o novo serviço.</param>
    /// <response code="201">Retorna o serviço recém-criado.</response>
    /// <response code="400">Se os dados fornecidos forem inválidos.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão (não é concessionária).</response>
    /// <response code="404">Se a concessionária não for encontrada.</response>
    /// <response code="409">Se já existir um serviço com o mesmo nome e categoria.</response>
    [HttpPost]
    [Authorize(Roles = "Concessionaria")]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] ServicoRequest request)
    {
        var response = await _servicoService.CreateAsync(request);
        return CreatedAtAction(nameof(Create), new { id = response.Id }, response);
    }
}
