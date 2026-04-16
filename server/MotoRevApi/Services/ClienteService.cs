using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;

namespace MotoRevApi.Services;

public class ClienteService
{
    private readonly AppDbContext _context;
    private readonly UserManager<Usuario> _userManager;
    private readonly IMapper _mapper;

    public ClienteService(AppDbContext context, UserManager<Usuario> userManager, IMapper mapper)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<ClienteResponse> RegisterAsync(RegisterClienteRequest request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new Usuario { UserName = request.Email, Email = request.Email };
            var identityResult = await _userManager.CreateAsync(user, request.Password);

            if (!identityResult.Succeeded)
            {
                throw new RegistrationException(identityResult.Errors);
            }

            await _userManager.AddToRoleAsync(user, "Cliente");

            var cliente = _mapper.Map<Cliente>(request);
            cliente.UsuarioId = user.Id;

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return _mapper.Map<ClienteResponse>(cliente);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
