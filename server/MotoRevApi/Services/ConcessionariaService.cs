using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    private readonly IMapper _mapper;

    public ConcessionariaService(AppDbContext context, UserManager<Usuario> userManager, IMapper mapper)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<ConcessionariaResponse> RegisterAsync(RegisterConcessionariaRequest request)
    {
        if (await _context.Concessionarias.AnyAsync(c => c.Cnpj == request.Cnpj))
        {
            throw new DuplicateDataException($"Já existe uma concessionária com o CNPJ {request.Cnpj}.");
        }

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

            await _userManager.AddToRoleAsync(user, Roles.Concessionaria);

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

    public async Task<List<ConcessionariaResponse>> GetAllAsync()
    {
        return await _context.Concessionarias
            .Where(c => c.IsActive)
            .ProjectTo<ConcessionariaResponse>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ConcessionariaResponse> GetByIdAsync(int id)
    {
        var concessionaria = await _context.Concessionarias
            .Where(c => c.Id == id && c.IsActive)
            .ProjectTo<ConcessionariaResponse>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        return concessionaria ?? throw new NotFoundException($"Concessionária com ID {id} não encontrada.");
    }
    
    public async Task<ConcessionariaResponse> GetByUserIdAsync(string userId)
    {
        var concessionaria = await _context.Concessionarias
            .Where(c => c.UsuarioId == userId && c.IsActive)
            .ProjectTo<ConcessionariaResponse>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        return concessionaria ?? throw new NotFoundException($"Concessionária não encontrada.");
    }

    public async Task UpdateAsync(string userId, UpdateConcessionariaRequest request)
    {
        var concessionaria = await _context.Concessionarias.FirstOrDefaultAsync(c => c.UsuarioId == userId && c.IsActive);
        if (concessionaria == null)
        {
            throw new NotFoundException("Concessionária não encontrada ou inativa.");
        }

        _mapper.Map(request, concessionaria);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string userId)
    {
        var concessionaria = await _context.Concessionarias.FirstOrDefaultAsync(c => c.UsuarioId == userId && c.IsActive);
        if (concessionaria == null)
        {
            throw new NotFoundException("Concessionária não encontrada ou inativa.");
        }

        concessionaria.IsActive = false;
        concessionaria.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
