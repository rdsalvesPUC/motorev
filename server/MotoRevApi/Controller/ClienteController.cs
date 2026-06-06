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
/// API controller para gerenciamento de clientes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Clientes")]
public class ClienteController : ControllerBase
{
    private readonly ClienteService _clienteService;

    public ClienteController(ClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    /// <summary>
    /// Registrar um novo cliente.
    /// </summary>
    /// <param name="request">Os dados para registrar o novo cliente.</param>
    /// <response code="201">Retorna o cliente recém-criado.</response>
    /// <response code="400">Se os dados fornecidos forem inválidos.</response>
    /// <response code="409">Se já existir um cliente com o mesmo CPF ou um usuário com o mesmo email.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterClienteRequest request)
    {
        var response = await _clienteService.RegisterAsync(request);
        return CreatedAtAction(nameof(Get), response);
    }

    /// <summary>
    /// Obter os dados do cliente autenticado.
    /// </summary>
    /// <remarks>
    /// O ID do cliente é extraído automaticamente do token JWT.
    /// </remarks>
    /// <response code="200">Retorna os dados do cliente.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="404">Se o cliente não for encontrado.</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _clienteService.GetByUserIdAsync(userId);
        return Ok(response);
    }

    /// <summary>
    /// Obter o perfil completo do cliente autenticado.
    /// </summary>
    /// <response code="200">Retorna os dados completos do perfil do cliente.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão de cliente.</response>
    /// <response code="404">Se o cliente não for encontrado.</response>
    [HttpGet("me")]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(typeof(ClientePerfilResponse), StatusCodes.Status200OK)]
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

        var response = await _clienteService.GetPerfilByUserIdAsync(userId);
        return Ok(response);
    }

    /// <summary>
    /// Atualizar os dados pessoais do cliente autenticado.
    /// </summary>
    /// <param name="request">Dados pessoais atualizados.</param>
    /// <response code="200">Retorna o perfil atualizado.</response>
    /// <response code="400">Se os dados fornecidos forem inválidos.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão de cliente.</response>
    /// <response code="409">Se o email já estiver em uso.</response>
    [HttpPut("me/dados-pessoais")]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(typeof(ClientePerfilResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateDadosPessoais([FromBody] ClienteDadosPessoaisRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _clienteService.UpdateDadosPessoaisAsync(userId, request);
        return Ok(response);
    }

    /// <summary>
    /// Atualizar o endereço temporário do cliente autenticado.
    /// </summary>
    /// <remarks>
    /// Estrutura simples e temporária até o épico dedicado de endereços.
    /// </remarks>
    /// <param name="request">Endereço atualizado.</param>
    /// <response code="200">Retorna o perfil atualizado.</response>
    /// <response code="400">Se os dados fornecidos forem inválidos.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão de cliente.</response>
    [HttpPut("me/endereco")]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(typeof(ClientePerfilResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateEndereco([FromBody] ClienteEnderecoRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _clienteService.UpdateEnderecoAsync(userId, request);
        return Ok(response);
    }

    /// <summary>
    /// Alterar a senha do cliente autenticado no Meu Perfil.
    /// </summary>
    /// <remarks>
    /// Este endpoint atende apenas o fluxo em que o cliente já está logado e informa a senha atual.
    /// Não atende o fluxo "Esqueci minha senha", que deve ter endpoints próprios para recuperação antes do login.
    /// </remarks>
    /// <param name="request">Senha atual, nova senha e confirmação da nova senha.</param>
    /// <response code="204">Senha alterada com sucesso para o cliente autenticado.</response>
    /// <response code="400">Se a senha atual ou a nova senha forem inválidas.</response>
    /// <response code="401">Se o usuário não estiver autenticado.</response>
    /// <response code="403">Se o usuário não tiver permissão de cliente.</response>
    [HttpPut("me/senha")]
    [Authorize(Roles = Roles.Cliente)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AlterarSenha([FromBody] ClienteAlterarSenhaRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        await _clienteService.AlterarSenhaAsync(userId, request);
        return NoContent();
    }
}
