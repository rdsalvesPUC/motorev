using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Authorization;
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

    public ClienteService() { } // Construtor para Moq

    public ClienteService(AppDbContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public virtual async Task<ClienteResponse> RegisterAsync(RegisterClienteRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) != null)
        {
            throw new DuplicateDataException($"O email {request.Email} já está em uso.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new Usuario { UserName = request.Email, Email = request.Email };
            var identityResult = await _userManager.CreateAsync(user, request.Password);

            if (!identityResult.Succeeded) throw new RegistrationException(identityResult.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, Roles.Cliente);

            if (!roleResult.Succeeded) throw new RegistrationException(roleResult.Errors);

            var cliente = request.Adapt<Cliente>();
            cliente.UsuarioId = user.Id;

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return cliente.Adapt<ClienteResponse>();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public virtual async Task<ClienteResponse> GetByUserIdAsync(string userId)
    {
        var cliente = await _context.Clientes
            .Where(c => c.UsuarioId == userId)
            .ProjectToType<ClienteResponse>()
            .FirstOrDefaultAsync();

        return cliente ?? throw new NotFoundException($"Cliente não encontrado.");
    }
}
