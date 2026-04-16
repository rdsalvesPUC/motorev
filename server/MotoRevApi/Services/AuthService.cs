using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;

namespace MotoRevApi.Services;

public class AuthService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly TokenService _tokenService;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public AuthService(
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager,
        TokenService tokenService,
        AppDbContext context,
        IMapper mapper)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _context = context;
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

        return new LoginResponse(token, role, userData);
    }
}
