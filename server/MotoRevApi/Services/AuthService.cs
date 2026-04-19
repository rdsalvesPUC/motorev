using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace MotoRevApi.Services;

public class AuthService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly TokenService _tokenService;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public AuthService(
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager,
        TokenService tokenService,
        AppDbContext context,
        IConfiguration configuration,
        IMapper mapper)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _context = context;
        _configuration = configuration;
        _mapper = mapper;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new NotFoundException("Email ou senha inválidos.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            throw new NotFoundException("Email ou senha inválidos.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);
        
        var refreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtSettings:RefreshTokenExpirationInDays"] ?? "7"));
        await _userManager.UpdateAsync(user);
        
        var role = roles.FirstOrDefault();
        UserData userData = null;

        if (role == "Cliente")
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);
            if (cliente != null)
            {
                userData = new UserData(cliente.Id, cliente.Nome);
            }
        }
        else if (role == "Concessionaria")
        {
            var concessionaria = await _context.Concessionarias.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);
            if (concessionaria != null)
            {
                userData = new UserData(concessionaria.Id, concessionaria.Nome);
            }
        }

        return new LoginResponse(token, refreshToken, role, userData);
    }
    
    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            throw new SecurityTokenException("O Access token ou o Refresh token estão inválidos");
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId ?? "");

        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
        {
            throw new SecurityTokenException("O Access token ou o Refresh token estão inválidos");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.GenerateToken(user, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        await _userManager.UpdateAsync(user);
        
        var role = roles.FirstOrDefault();
        UserData userData = null;

        if (role == "Cliente")
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);
            if (cliente != null)
            {
                userData = new UserData(cliente.Id, cliente.Nome);
            }
        }
        else if (role == "Concessionaria")
        {
            var concessionaria = await _context.Concessionarias.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);
            if (concessionaria != null)
            {
                userData = new UserData(concessionaria.Id, concessionaria.Nome);
            }
        }

        return new LoginResponse(newAccessToken, newRefreshToken, role, userData);
    }
    
    public async Task LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return;

        user.RefreshToken = null;
        await _userManager.UpdateAsync(user);
    }
}
