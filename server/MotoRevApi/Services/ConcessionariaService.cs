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

public class ConcessionariaService
{
    private readonly AppDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    public ConcessionariaService() { } // Construtor para Moq

    public ConcessionariaService(AppDbContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public virtual async Task<ConcessionariaResponse> RegisterAsync(RegisterConcessionariaRequest request)
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

            var roleResult = await _userManager.AddToRoleAsync(user, Roles.Concessionaria);

            if(!roleResult.Succeeded) throw new RegistrationException(roleResult.Errors);

            var concessionaria = request.Adapt<Concessionaria>();
            concessionaria.UsuarioId = user.Id;

            _context.Concessionarias.Add(concessionaria);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return concessionaria.Adapt<ConcessionariaResponse>();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public virtual async Task<ConcessionariaResponse> GetByIdAsync(int id)
    {
        var concessionaria = await _context.Concessionarias
            .ProjectToType<ConcessionariaResponse>()
            .FirstOrDefaultAsync();

        return concessionaria ?? throw new NotFoundException($"Concessionária com ID {id} não encontrada.");
    }
    
    public virtual async Task<ConcessionariaResponse> GetByUserIdAsync(string userId)
    {
        var concessionaria = await _context.Concessionarias
            .Where(c => c.UsuarioId == userId)
            .ProjectToType<ConcessionariaResponse>()
            .FirstOrDefaultAsync();

        return concessionaria ?? throw new NotFoundException($"Concessionária não encontrada.");
    }
}
