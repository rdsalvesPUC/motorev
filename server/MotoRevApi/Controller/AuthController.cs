using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Services;

namespace MotoRevApi.Controller;

/// <summary>
/// API controller para autenticação de usuários.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Autenticação")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Realizar o login de um usuário no sistema.
    /// </summary>
    /// <param name="request">Credenciais do usuário.</param>
    /// <response code="200">Login bem-sucedido. Retorna o token de acesso.</response>
    /// <response code="400">Credenciais inválidas.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }
}
