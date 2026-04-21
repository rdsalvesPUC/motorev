using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using MotoRevApi.Authorization;

namespace MotoRevApi.Services;

public class AuthService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly TokenService _tokenService;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly HashService _hashService;

    public AuthService() { } // Construtor para Moq

    public AuthService(
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager,
        TokenService tokenService,
        AppDbContext context,
        IConfiguration configuration,
        HashService hashService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _context = context;
        _configuration = configuration;
        _hashService = hashService;
    }

    public virtual async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new NotFoundException("Email ou senha inválidos.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Email ou senha inválidos.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        
        if (role == null)
        {
            throw new Exception("Usuário não possui um perfil associado.");
        }

        var token = _tokenService.GenerateToken(user, roles);
        
        var refreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshToken = _hashService.HashToken(refreshToken); // Armazena o hash
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["JwtSettings:RefreshTokenExpirationInDays"] ?? "7"));
        
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new Exception("Não foi possível salvar o refresh token.");
        }
        
        UserData? userData = null;

        switch (role)
        {
            case Roles.Cliente:
            {
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);
                if (cliente != null)
                {
                    userData = new UserData(cliente.Id, cliente.Nome);
                }

                break;
            }
            case Roles.Concessionaria:
            {
                var concessionaria = await _context.Concessionarias.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);
                if (concessionaria != null)
                {
                    userData = new UserData(concessionaria.Id, concessionaria.Nome);
                }

                break;
            }
        }

        return userData == null ? throw new Exception("Não foi possível encontrar os dados do perfil do usuário.") : new LoginResponse(token, refreshToken, role, userData); // Retorna o token original
    }
    
    public virtual async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            throw new SecurityTokenException("O Access token ou o Refresh token estão inválidos");
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId ?? "");

        if (user == null || user.RefreshToken == null || !_hashService.VerifyToken(request.RefreshToken, user.RefreshToken) || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new SecurityTokenException("O Access token ou o Refresh token estão inválidos");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        
        if (role == null)
        {
            throw new Exception("Usuário não possui um perfil associado.");
        }

        var newAccessToken = _tokenService.GenerateToken(user, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = _hashService.HashToken(newRefreshToken); // Armazena o novo hash
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new Exception("Não foi possível atualizar o refresh token.");
        }
        
        UserData userData = null;

        switch (role)
        {
            case Roles.Cliente:
            {
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);
                if (cliente != null)
                {
                    userData = new UserData(cliente.Id, cliente.Nome);
                }

                break;
            }
            case Roles.Concessionaria:
            {
                var concessionaria = await _context.Concessionarias.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);
                if (concessionaria != null)
                {
                    userData = new UserData(concessionaria.Id, concessionaria.Nome);
                }

                break;
            }
        }

        return userData == null ? throw new Exception("Não foi possível encontrar os dados do perfil do usuário.") : new LoginResponse(newAccessToken, newRefreshToken, role, userData); // Retorna o novo token original
    }
    
    public virtual async Task LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return;

        user.RefreshToken = null;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new Exception("Não foi possível realizar o logout.");
        }
    }
}
