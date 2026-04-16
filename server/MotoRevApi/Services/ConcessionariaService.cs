using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
    private readonly IMapper _mapper;

    public ConcessionariaService(AppDbContext context, UserManager<Usuario> userManager, IMapper mapper)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<ConcessionariaResponse> RegisterAsync(RegisterConcessionariaRequest request)
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

            await _userManager.AddToRoleAsync(user, "Concessionaria");

            var concessionaria = _mapper.Map<Concessionaria>(request);
            concessionaria.UsuarioId = user.Id;

            _context.Concessionarias.Add(concessionaria);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return _mapper.Map<ConcessionariaResponse>(concessionaria);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
