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
        if (await _context.Clientes.AnyAsync(c => c.Cpf == request.Cpf))
        {
            throw new DuplicateDataException($"Já existe um cliente com o CPF {request.Cpf}.");
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

            await _userManager.AddToRoleAsync(user, Roles.Cliente);

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

    public async Task<ClienteResponse> GetByUserIdAsync(string userId)
    {
        var cliente = await _context.Clientes
            .Where(c => c.UsuarioId == userId && c.IsActive)
            .ProjectTo<ClienteResponse>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        return cliente ?? throw new NotFoundException($"Cliente não encontrado.");
    }

    public async Task UpdateAsync(string userId, UpdateClienteRequest request)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == userId && c.IsActive);
        if (cliente == null)
        {
            throw new NotFoundException("Cliente não encontrado ou inativo.");
        }

        _mapper.Map(request, cliente);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string userId)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == userId && c.IsActive);
        if (cliente == null)
        {
            throw new NotFoundException("Cliente não encontrado ou inativo.");
        }

        cliente.IsActive = false;
        cliente.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
