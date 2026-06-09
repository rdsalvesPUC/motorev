using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Authorization;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;

namespace MotoRevApi.Controller;

/// <summary>
/// API controller para gerenciamento de concessionárias.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Concessionárias")]
public class ConcessionariaController : ControllerBase
{
    private readonly ConcessionariaService _concessionariaService;

    public ConcessionariaController(ConcessionariaService concessionariaService)
    {
        _concessionariaService = concessionariaService;
    }

    /// <summary>
    /// Registrar uma nova concessionária.
    /// </summary>
    /// <param name="request">Os dados para registrar a nova concessionária.</param>
    /// <response code="201">Retorna a concessionária recém-criada.</response>
    /// <response code="400">Se os dados fornecidos forem inválidos.</response>
    /// <response code="409">Se já existir uma concessionária com o mesmo CNPJ ou um usuário com o mesmo email.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ConcessionariaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterConcessionariaRequest request)
    {
        var response = await _concessionariaService.RegisterAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Obter uma concessionária específica pelo ID.
    /// </summary>
    /// <param name="id">O ID da concessionária.</param>
    /// <response code="200">Retorna os dados da concessionária.</response>
    /// <response code="404">Se a concessionária não for encontrada.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ConcessionariaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _concessionariaService.GetByIdAsync(id);
        return Ok(response);
    }

    /// <summary>
    /// Obter a concessionária autenticada.
    /// </summary>
    /// <remarks>
    /// O ID da concessionária é extraído automaticamente do token JWT.
    /// </remarks>
    /// <response code="200">Retorna os dados da concessionária.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão de 'Concessionaria'.</response>
    /// <response code="404">Se a concessionária não for encontrada.</response>
    [HttpGet("me")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(ConcessionariaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _concessionariaService.GetByUserIdAsync(userId);
        return Ok(response);
    }

    /// <summary>
    /// Atualizar os dados da concessionaria matriz autenticada.
    /// </summary>
    /// <param name="request">Dados atualizados da matriz.</param>
    /// <response code="200">Retorna os dados atualizados da concessionaria.</response>
    /// <response code="400">Se os dados fornecidos forem invalidos.</response>
    /// <response code="401">Se o usuario nao estiver autenticado.</response>
    /// <response code="403">Se o usuario nao tiver permissao de concessionaria.</response>
    /// <response code="409">Se o CNPJ ja estiver em uso.</response>
    [HttpPut("me")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(ConcessionariaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateMe([FromBody] ConcessionariaPerfilRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _concessionariaService.UpdatePerfilAsync(userId, request);
        return Ok(response);
    }

    /// <summary>
    /// Alterar a senha da concessionaria autenticada.
    /// </summary>
    /// <param name="request">Senha atual, nova senha e confirmacao.</param>
    /// <response code="204">Senha alterada com sucesso.</response>
    /// <response code="400">Se a senha atual ou a nova senha forem invalidas.</response>
    /// <response code="401">Se o usuario nao estiver autenticado.</response>
    /// <response code="403">Se o usuario nao tiver permissao de concessionaria.</response>
    [HttpPut("me/senha")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AlterarSenha([FromBody] ConcessionariaAlterarSenhaRequest request)
    {
        var userId = ObterUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        await _concessionariaService.AlterarSenhaAsync(userId, request);
        return NoContent();
    }

    /// <summary>
    /// Obter todas as concessionárias cadastradas.
    /// </summary>
    /// <response code="200">Retorna a lista de concessionárias.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ConcessionariaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var response = await _concessionariaService.GetAllAsync();
        return Ok(response);
    }

    /// <summary>
    /// Obter lojas ativas disponiveis para clientes.
    /// </summary>
    /// <response code="200">Retorna matriz e filiais ativas.</response>
    [HttpGet("lojas/ativas")]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(typeof(IEnumerable<LojaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLojasAtivas()
    {
        var response = await _concessionariaService.GetLojasAtivasAsync();
        return Ok(response);
    }

    /// <summary>
    /// Cadastrar uma loja filial para a concessionaria matriz autenticada.
    /// </summary>
    /// <remarks>
    /// O ID da concessionária matriz é extraído automaticamente do token JWT.
    /// </remarks>
    /// <param name="request">Os dados da nova loja filial.</param>
    /// <response code="201">Retorna a loja recém-criada.</response>
    /// <response code="400">Se os dados fornecidos forem inválidos.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão de 'Concessionaria'.</response>
    /// <response code="404">Se a concessionária matriz não for encontrada.</response>
    /// <response code="409">Se já existir uma matriz ou filial com o mesmo CNPJ.</response>
    [HttpPost("me/lojas")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(LojaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddMinhaLoja([FromBody] LojaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = ObterUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _concessionariaService.AddLojaAsync(userId, request);
        return CreatedAtAction(nameof(GetMinhaLojaById), new { lojaId = response.Id }, response);
    }

    /// <summary>
    /// Obter todas as lojas da concessionaria matriz autenticada.
    /// </summary>
    /// <remarks>
    /// Retorna tanto a matriz quanto as filiais vinculadas ao usuário autenticado.
    /// </remarks>
    /// <response code="200">Retorna a lista de lojas da concessionária.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão de 'Concessionaria'.</response>
    /// <response code="404">Se a concessionária matriz não for encontrada.</response>
    [HttpGet("me/lojas")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(IEnumerable<LojaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMinhasLojas()
    {
        var userId = ObterUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _concessionariaService.GetLojasAsync(userId);
        return Ok(response);
    }

    /// <summary>
    /// Obter uma loja específica da concessionaria matriz autenticada.
    /// </summary>
    /// <param name="lojaId">O ID da loja (matriz ou filial).</param>
    /// <response code="200">Retorna os dados da loja.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão de 'Concessionaria'.</response>
    /// <response code="404">Se a loja não for encontrada ou não pertencer à concessionária do usuário.</response>
    [HttpGet("me/lojas/{lojaId}")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(LojaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMinhaLojaById(int lojaId)
    {
        var userId = ObterUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _concessionariaService.GetLojaByIdAsync(userId, lojaId);
        return Ok(response);
    }

    /// <summary>
    /// Atualizar os dados de uma loja filial da concessionaria matriz autenticada.
    /// </summary>
    /// <param name="lojaId">O ID da loja a ser atualizada.</param>
    /// <param name="request">Os novos dados da loja.</param>
    /// <response code="200">Retorna a loja atualizada.</response>
    /// <response code="400">Se os dados fornecidos forem inválidos.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão de 'Concessionaria'.</response>
    /// <response code="404">Se a loja não for encontrada ou não pertencer à concessionária do usuário.</response>
    /// <response code="409">Se o CNPJ informado já estiver em uso por outra loja.</response>
    [HttpPut("me/lojas/{lojaId}")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(LojaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateMinhaLoja(int lojaId, [FromBody] LojaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = ObterUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _concessionariaService.UpdateLojaAsync(userId, lojaId, request);
        return Ok(response);
    }

    /// <summary>
    /// Alternar status ativo/inativo de uma loja da concessionaria matriz autenticada.
    /// </summary>
    /// <param name="lojaId">O ID da loja.</param>
    /// <response code="200">Retorna a loja com status atualizado.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão de 'Concessionaria'.</response>
    /// <response code="404">Se a loja não for encontrada ou não pertencer à concessionária do usuário.</response>
    [HttpPatch("me/lojas/{lojaId}/alternar-status")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(LojaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlternarStatusMinhaLoja(int lojaId)
    {
        var userId = ObterUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _concessionariaService.AlternarStatusLojaAsync(userId, lojaId);
        return Ok(response);
    }

    /// <summary>
    /// Cadastrar uma loja filial para uma concessionaria matriz específica.
    /// </summary>
    /// <remarks>
    /// Requer que o usuário autenticado seja o proprietário da concessionária matriz informada.
    /// </remarks>
    /// <param name="id">O ID da concessionária matriz.</param>
    /// <param name="request">Os dados da nova loja filial.</param>
    /// <response code="201">Retorna a loja recém-criada.</response>
    /// <response code="400">Se os dados fornecidos forem inválidos.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não for o proprietário da matriz.</response>
    /// <response code="404">Se a concessionária matriz não for encontrada.</response>
    /// <response code="409">Se já existir uma matriz ou filial com o mesmo CNPJ.</response>
    [HttpPost("{id}/lojas")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(LojaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddLoja(int id, [FromBody] LojaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ownershipResult = await ValidarOwnershipAsync(id);
        if (ownershipResult != null)
        {
            return ownershipResult;
        }

        var response = await _concessionariaService.AddLojaAsync(id, request);
        return CreatedAtAction(nameof(GetLojaById), new { id, lojaId = response.Id }, response);
    }

    /// <summary>
    /// Obter todas as lojas filiais de uma concessionaria matriz específica.
    /// </summary>
    /// <remarks>
    /// Requer que o usuário autenticado seja o proprietário da concessionária matriz informada.
    /// </remarks>
    /// <param name="id">O ID da concessionária matriz.</param>
    /// <response code="200">Retorna a lista de lojas da concessionária.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não for o proprietário da matriz.</response>
    /// <response code="404">Se a concessionária matriz não for encontrada.</response>
    [HttpGet("{id}/lojas")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(IEnumerable<LojaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLojas(int id)
    {
        var ownershipResult = await ValidarOwnershipAsync(id);
        if (ownershipResult != null)
        {
            return ownershipResult;
        }

        var response = await _concessionariaService.GetLojasAsync(id);
        return Ok(response);
    }

    /// <summary>
    /// Obter uma loja filial específica de uma concessionaria matriz.
    /// </summary>
    /// <remarks>
    /// Requer que o usuário autenticado seja o proprietário da concessionária matriz informada.
    /// </remarks>
    /// <param name="id">O ID da concessionária matriz.</param>
    /// <param name="lojaId">O ID da loja filial.</param>
    /// <response code="200">Retorna os dados da loja.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não for o proprietário da matriz.</response>
    /// <response code="404">Se a loja não for encontrada.</response>
    [HttpGet("{id}/lojas/{lojaId}")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(LojaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLojaById(int id, int lojaId)
    {
        var ownershipResult = await ValidarOwnershipAsync(id);
        if (ownershipResult != null)
        {
            return ownershipResult;
        }

        var response = await _concessionariaService.GetLojaByIdAsync(id, lojaId);
        return Ok(response);
    }

    /// <summary>
    /// Atualizar uma loja filial de uma concessionaria matriz específica.
    /// </summary>
    /// <remarks>
    /// Requer que o usuário autenticado seja o proprietário da concessionária matriz informada.
    /// </remarks>
    /// <param name="id">O ID da concessionária matriz.</param>
    /// <param name="lojaId">O ID da loja filial a ser atualizada.</param>
    /// <param name="request">Os novos dados da loja.</param>
    /// <response code="200">Retorna a loja atualizada.</response>
    /// <response code="400">Se os dados fornecidos forem inválidos.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não for o proprietário da matriz.</response>
    /// <response code="404">Se a loja não for encontrada.</response>
    /// <response code="409">Se o CNPJ informado já estiver em uso por outra loja.</response>
    [HttpPut("{id}/lojas/{lojaId}")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(LojaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateLoja(int id, int lojaId, [FromBody] LojaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ownershipResult = await ValidarOwnershipAsync(id);
        if (ownershipResult != null)
        {
            return ownershipResult;
        }

        var response = await _concessionariaService.UpdateLojaAsync(id, lojaId, request);
        return Ok(response);
    }

    /// <summary>
    /// Alternar status ativo/inativo de uma loja filial específica.
    /// </summary>
    /// <remarks>
    /// Requer que o usuário autenticado seja o proprietário da concessionária matriz informada.
    /// </remarks>
    /// <param name="id">O ID da concessionária matriz.</param>
    /// <param name="lojaId">O ID da loja filial.</param>
    /// <response code="200">Retorna a loja com status atualizado.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não for o proprietário da matriz.</response>
    /// <response code="404">Se a loja não for encontrada.</response>
    [HttpPatch("{id}/lojas/{lojaId}/alternar-status")]
    [Authorize(Roles = Roles.Concessionaria)]
    [ProducesResponseType(typeof(LojaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlternarStatusLoja(int id, int lojaId)
    {
        var ownershipResult = await ValidarOwnershipAsync(id);
        if (ownershipResult != null)
        {
            return ownershipResult;
        }

        var response = await _concessionariaService.AlternarStatusLojaAsync(id, lojaId);
        return Ok(response);
    }

    private string? ObterUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private async Task<IActionResult?> ValidarOwnershipAsync(int concessionariaId)
    {
        var userId = ObterUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        return await _concessionariaService.ConcessionariaPertenceAoUsuarioAsync(userId, concessionariaId)
            ? null
            : Forbid();
    }
}
