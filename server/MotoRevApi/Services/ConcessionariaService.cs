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
            throw new DuplicateDataException($"O email {request.Email} ja esta em uso.");
        }

        if (await CnpjJaExisteAsync(request.Cnpj))
        {
            throw new DuplicateDataException($"O CNPJ {request.Cnpj} ja esta em uso.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new Usuario { UserName = request.Email, Email = request.Email };
            var identityResult = await _userManager.CreateAsync(user, request.Password);

            if (!identityResult.Succeeded) throw new RegistrationException(identityResult.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, Roles.Concessionaria);

            if (!roleResult.Succeeded) throw new RegistrationException(roleResult.Errors);

            var concessionaria = new Concessionaria
            {
                Nome = request.Nome,
                Cnpj = request.Cnpj,
                Telefone = request.Telefone,
                Tipo = "Matriz",
                Cep = request.Cep,
                Logradouro = request.Logradouro,
                Numero = request.Numero,
                Bairro = request.Bairro,
                Cidade = request.Cidade,
                Uf = request.Uf.ToUpperInvariant(),
                UsuarioId = user.Id
            };

            _context.Concessionarias.Add(concessionaria);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapConcessionariaResponse(concessionaria);
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
            .Include(c => c.Lojas)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        return concessionaria == null
            ? throw new NotFoundException($"Concessionaria com ID {id} nao encontrada.")
            : MapConcessionariaResponse(concessionaria);
    }

    public virtual async Task<ConcessionariaResponse> GetByUserIdAsync(string userId)
    {
        var concessionaria = await _context.Concessionarias
            .Include(c => c.Lojas)
            .Where(c => c.UsuarioId == userId)
            .FirstOrDefaultAsync();

        return concessionaria == null
            ? throw new NotFoundException("Concessionaria nao encontrada.")
            : MapConcessionariaResponse(concessionaria);
    }

    public virtual async Task<IEnumerable<ConcessionariaResponse>> GetAllAsync()
    {
        var concessionarias = await _context.Concessionarias
            .Include(c => c.Lojas)
            .ToListAsync();

        return concessionarias.Select(MapConcessionariaResponse);
    }

    public virtual async Task<LojaResponse> AddLojaAsync(int concessionariaId, LojaRequest request)
    {
        var concessionariaExiste = await _context.Concessionarias.AnyAsync(c => c.Id == concessionariaId);
        if (!concessionariaExiste)
        {
            throw new NotFoundException($"Concessionaria com ID {concessionariaId} nao encontrada.");
        }

        if (await CnpjJaExisteAsync(request.Cnpj))
        {
            throw new DuplicateDataException($"O CNPJ {request.Cnpj} ja esta em uso.");
        }

        var loja = new Loja
        {
            Nome = request.Nome,
            Tipo = "Filial",
            Cnpj = request.Cnpj,
            Cep = request.Cep,
            Logradouro = request.Logradouro,
            Numero = request.Numero,
            Bairro = request.Bairro,
            Cidade = request.Cidade,
            Uf = request.Uf.ToUpperInvariant(),
            ConcessionariaId = concessionariaId
        };

        _context.Lojas.Add(loja);
        await _context.SaveChangesAsync();

        return MapLojaResponse(loja);
    }

    public virtual async Task<IEnumerable<LojaResponse>> GetLojasAsync(int concessionariaId)
    {
        var concessionariaExiste = await _context.Concessionarias.AnyAsync(c => c.Id == concessionariaId);
        if (!concessionariaExiste)
        {
            throw new NotFoundException($"Concessionaria com ID {concessionariaId} nao encontrada.");
        }

        var lojas = await _context.Lojas
            .Where(l => l.ConcessionariaId == concessionariaId)
            .ToListAsync();

        return lojas.Select(MapLojaResponse);
    }

    public virtual async Task<LojaResponse> GetLojaByIdAsync(int concessionariaId, int lojaId)
    {
        var loja = await _context.Lojas
            .FirstOrDefaultAsync(l => l.Id == lojaId && l.ConcessionariaId == concessionariaId);

        return loja == null
            ? throw new NotFoundException($"Loja com ID {lojaId} nao encontrada.")
            : MapLojaResponse(loja);
    }

    private async Task<bool> CnpjJaExisteAsync(string cnpj)
    {
        return await _context.Concessionarias.AnyAsync(c => c.Cnpj == cnpj)
            || await _context.Lojas.AnyAsync(l => l.Cnpj == cnpj);
    }

    private static ConcessionariaResponse MapConcessionariaResponse(Concessionaria concessionaria)
    {
        return new ConcessionariaResponse(
            concessionaria.Id,
            concessionaria.Nome,
            concessionaria.Cnpj,
            concessionaria.Telefone,
            concessionaria.Tipo,
            concessionaria.Cep,
            concessionaria.Logradouro,
            concessionaria.Numero,
            concessionaria.Bairro,
            concessionaria.Cidade,
            concessionaria.Uf,
            concessionaria.Lojas.Select(MapLojaResponse)
        );
    }

    private static LojaResponse MapLojaResponse(Loja loja)
    {
        return new LojaResponse(
            loja.Id,
            loja.Nome,
            loja.Tipo,
            loja.Cnpj,
            loja.Cep,
            loja.Logradouro,
            loja.Numero,
            loja.Bairro,
            loja.Cidade,
            loja.Uf,
            loja.ConcessionariaId
        );
    }
}
