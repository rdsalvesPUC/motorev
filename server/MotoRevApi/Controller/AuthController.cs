using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
    /// <response code="200">Login bem-sucedido. Retorna o access token e o refresh token.</response>
    /// <response code="400">Credenciais inválidas.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }
    
    /// <summary>
    /// Renovar o access token usando um refresh token válido.
    /// </summary>
    /// <param name="request">O access token expirado e o refresh token atual.</param>
    /// <response code="200">Retorna um novo access token e um novo refresh token.</response>
    /// <response code="400">Tokens inválidos ou expirados.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        return Ok(response);
    }
    
    /// <summary>
    /// Realizar o logout do usuário, invalidando o seu refresh token.
    /// </summary>
    /// <response code="200">Logout bem-sucedido.</response>
    /// <response code="401">Usuário não autenticado.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        await _authService.LogoutAsync(userId);
        return Ok(new { message = "Logout realizado com sucesso." });
    }
}
