using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
using MotoRevApi.Services;
using MotoRevApi.Authorization;

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
    /// <response code="409">Se já existir um serviço com o mesmo código ou com o mesmo nome e categoria.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] ServicoRequest request)
    {
        var response = await _servicoService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Listar serviços disponíveis.
    /// </summary>
    /// <remarks>
    /// Endpoint público — não requer autenticação. Aceita filtro opcional por categoria.
    /// </remarks>
    /// <param name="categoria">Filtro opcional por categoria do serviço.</param>
    /// <response code="200">Retorna a lista de serviços encontrados ou lista vazia.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ServicoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] CategoriaServico? categoria)
    {
        var response = await _servicoService.GetAllAsync(categoria);
        return Ok(response);
    }
    
    /// <summary>
    /// Consultar um serviço pelo ID.
    /// </summary>
    /// <remarks>
    /// Endpoint público — não requer autenticação.
    /// </remarks>
    /// <param name="id">ID do serviço.</param>
    /// <response code="200">Retorna o serviço encontrado.</response>
    /// <response code="404">Se o serviço não for encontrado.</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _servicoService.GetByIdAsync(id);
        return Ok(response);
    }

    /// <summary>
    /// Atualizar os dados de um serviço existente.
    /// </summary>
    /// <remarks>
    /// Endpoint disponível apenas para Concessionárias.
    /// </remarks>
    /// <param name="id">ID do serviço a ser atualizado.</param>
    /// <param name="request">Novos dados do serviço.</param>
    /// <response code="200">Retorna o serviço atualizado.</response>
    /// <response code="400">Se os dados fornecidos forem inválidos.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão (não é concessionária).</response>
    /// <response code="404">Se o serviço não for encontrado.</response>
    /// <response code="409">Se já existir outro serviço com o mesmo código ou com o mesmo nome e categoria.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] ServicoUpdateRequest request)
    {
        var response = await _servicoService.UpdateAsync(id, request);
        return Ok(response);
    }

    /// <summary>
    /// Inativar um serviço existente.
    /// </summary>
    /// <remarks>
    /// Endpoint disponível apenas para Concessionárias. Marca o serviço como inativo (exclusão lógica).
    /// </remarks>
    /// <param name="id">ID do serviço a ser inativado.</param>
    /// <response code="204">Operação concluída com sucesso (sem conteúdo de retorno).</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão (não é concessionária).</response>
    /// <response code="404">Se o serviço não for encontrado.</response>
    [HttpPatch("{id}/inativar")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inactivate(int id)
    {
        await _servicoService.InactivateAsync(id);
        return NoContent();
    }
}