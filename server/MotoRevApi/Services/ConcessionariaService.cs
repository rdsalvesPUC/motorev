using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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

        if (await TelefoneLojaJaExisteAsync(request.Telefone))
        {
            throw new DuplicateDataException($"O telefone {request.Telefone} ja esta em uso.");
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
                UsuarioId = user.Id
            };

            _context.Concessionarias.Add(concessionaria);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapConcessionariaResponse(concessionaria, user.Email);
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
            .Include(c => c.Usuario)
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
            .Include(c => c.Usuario)
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
            .Include(c => c.Usuario)
            .Include(c => c.Lojas)
            .ToListAsync();

        return concessionarias.Select(c => MapConcessionariaResponse(c));
    }

    public virtual async Task<ConcessionariaResponse> UpdatePerfilAsync(string userId, ConcessionariaPerfilRequest request)
    {
        var concessionaria = await _context.Concessionarias
            .Include(c => c.Usuario)
            .Include(c => c.Lojas)
            .FirstOrDefaultAsync(c => c.UsuarioId == userId);

        if (concessionaria == null)
        {
            throw new NotFoundException("Concessionaria nao encontrada.");
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null && existingUser.Id != concessionaria.UsuarioId)
        {
            throw new DuplicateDataException($"O email {request.Email} ja esta em uso.");
        }

        var telefoneEmUsoPorLoja = await _context.Lojas.AnyAsync(l =>
            l.Telefone == request.Telefone &&
            !(l.ConcessionariaId == concessionaria.Id && l.Tipo == "Matriz"));
        if (telefoneEmUsoPorLoja)
        {
            throw new DuplicateDataException($"O telefone {request.Telefone} ja esta em uso.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            concessionaria.Nome = request.Nome;

            if (!string.Equals(concessionaria.Usuario.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailResult = await _userManager.SetEmailAsync(concessionaria.Usuario, request.Email);
                if (!emailResult.Succeeded) throw new RegistrationException(emailResult.Errors);

                var userNameResult = await _userManager.SetUserNameAsync(concessionaria.Usuario, request.Email);
                if (!userNameResult.Succeeded) throw new RegistrationException(userNameResult.Errors);

                concessionaria.Usuario.Email = request.Email;
                concessionaria.Usuario.UserName = request.Email;
            }

            concessionaria.Telefone = request.Telefone;
            concessionaria.Tipo = "Matriz";
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return MapConcessionariaResponse(concessionaria);
    }

    public virtual async Task AlterarSenhaAsync(string userId, ConcessionariaAlterarSenhaRequest request)
    {
        if (request.NovaSenha != request.ConfirmarNovaSenha)
        {
            throw new ValidationException("A confirmacao da nova senha nao confere.");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("Usuario nao encontrado.");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.SenhaAtual, request.NovaSenha);
        if (!result.Succeeded)
        {
            throw new RegistrationException(result.Errors);
        }
    }

    public virtual async Task<bool> ConcessionariaPertenceAoUsuarioAsync(string userId, int concessionariaId)
    {
        return await _context.Concessionarias
            .AnyAsync(c => c.Id == concessionariaId && c.UsuarioId == userId);
    }

    public virtual async Task<LojaResponse> AddLojaAsync(string userId, LojaRequest request)
    {
        var concessionaria = await ObterConcessionariaPorUsuarioAsync(userId);
        return await AddLojaAsync(concessionaria.Id, request);
    }

    public virtual async Task<LojaResponse> AddLojaAsync(int concessionariaId, LojaRequest request)
    {
        var concessionaria = await _context.Concessionarias
            .FirstOrDefaultAsync(c => c.Id == concessionariaId);
        if (concessionaria == null)
        {
            throw new NotFoundException($"Concessionaria com ID {concessionariaId} nao encontrada.");
        }

        var lojaMatrizAtual = await _context.Lojas
            .FirstOrDefaultAsync(l => l.ConcessionariaId == concessionariaId && l.Tipo == "Matriz");
        var isMatriz = request.IsMatriz || lojaMatrizAtual == null;

        var cnpjEmUsoPorOutraConcessionaria = await _context.Concessionarias.AnyAsync(c => c.Cnpj == request.Cnpj && c.Id != concessionariaId);
        var cnpjEmUsoPorOutraLoja = await _context.Lojas.AnyAsync(l => l.Cnpj == request.Cnpj);

        if (cnpjEmUsoPorOutraConcessionaria || 
            cnpjEmUsoPorOutraLoja || 
            (!isMatriz && request.Cnpj == concessionaria.Cnpj))
        {
            throw new DuplicateDataException($"O CNPJ {request.Cnpj} ja esta em uso.");
        }

        if (await TelefoneLojaJaExisteAsync(request.Telefone))
        {
            throw new DuplicateDataException($"O telefone {request.Telefone} ja esta em uso.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (isMatriz && lojaMatrizAtual != null)
            {
                lojaMatrizAtual.Tipo = "Filial";
                await _context.SaveChangesAsync();
            }

            var loja = new Loja
            {
                Nome = request.Nome,
                Tipo = isMatriz ? "Matriz" : "Filial",
                Cnpj = request.Cnpj,
                Telefone = request.Telefone,
                Cep = request.Cep,
                Logradouro = request.Logradouro,
                Numero = request.Numero,
                Bairro = request.Bairro,
                Cidade = request.Cidade,
                Uf = request.Uf.ToUpperInvariant(),
                Foto = request.Foto,
                Ativo = true,
                ConcessionariaId = concessionariaId
            };

            _context.Lojas.Add(loja);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapLojaResponse(loja);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public virtual async Task<LojaResponse> UpdateLojaAsync(string userId, int lojaId, LojaRequest request)
    {
        var concessionaria = await ObterConcessionariaPorUsuarioAsync(userId);
        return await UpdateLojaAsync(concessionaria.Id, lojaId, request);
    }

    public virtual async Task<LojaResponse> UpdateLojaAsync(int concessionariaId, int lojaId, LojaRequest request)
    {
        var loja = await _context.Lojas
            .FirstOrDefaultAsync(l => l.Id == lojaId && l.ConcessionariaId == concessionariaId);

        if (loja == null)
        {
            throw new NotFoundException($"Loja com ID {lojaId} nao encontrada.");
        }

        var outraMatriz = await _context.Lojas
            .FirstOrDefaultAsync(l => l.ConcessionariaId == concessionariaId && l.Tipo == "Matriz" && l.Id != lojaId);
        var isMatriz = request.IsMatriz || (loja.Tipo == "Matriz" && outraMatriz == null);

        var cnpjEmUsoPorOutraConcessionaria = await _context.Concessionarias.AnyAsync(c =>
            c.Cnpj == request.Cnpj && c.Id != concessionariaId);
        var cnpjEmUsoPorMatriz = !isMatriz && await _context.Concessionarias.AnyAsync(c => c.Cnpj == request.Cnpj);
        var cnpjEmUsoPorOutraLoja = await _context.Lojas.AnyAsync(l => l.Cnpj == request.Cnpj && l.Id != lojaId);
        if (cnpjEmUsoPorOutraConcessionaria || cnpjEmUsoPorMatriz || cnpjEmUsoPorOutraLoja)
        {
            throw new DuplicateDataException($"O CNPJ {request.Cnpj} ja esta em uso.");
        }

        if (await TelefoneLojaJaExisteAsync(request.Telefone, lojaId))
        {
            throw new DuplicateDataException($"O telefone {request.Telefone} ja esta em uso.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (isMatriz && outraMatriz != null)
            {
                outraMatriz.Tipo = "Filial";
                await _context.SaveChangesAsync();
            }

            loja.Nome = request.Nome;
            loja.Tipo = isMatriz ? "Matriz" : "Filial";
            loja.Cnpj = request.Cnpj;
            loja.Telefone = request.Telefone;
            loja.Cep = request.Cep;
            loja.Logradouro = request.Logradouro;
            loja.Numero = request.Numero;
            loja.Bairro = request.Bairro;
            loja.Cidade = request.Cidade;
            loja.Uf = request.Uf.ToUpperInvariant();
            loja.Foto = request.Foto;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapLojaResponse(loja);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public virtual async Task<LojaResponse> AlternarStatusLojaAsync(string userId, int lojaId)
    {
        var concessionaria = await ObterConcessionariaPorUsuarioAsync(userId);
        return await AlternarStatusLojaAsync(concessionaria.Id, lojaId);
    }

    public virtual async Task<LojaResponse> AlternarStatusLojaAsync(int concessionariaId, int lojaId)
    {
        var loja = await _context.Lojas
            .FirstOrDefaultAsync(l => l.Id == lojaId && l.ConcessionariaId == concessionariaId);

        if (loja == null)
        {
            throw new NotFoundException($"Loja com ID {lojaId} nao encontrada.");
        }

        if (loja.Tipo == "Matriz")
        {
            throw new BusinessRuleException("A loja matriz nao pode ser desativada.");
        }

        loja.Ativo = !loja.Ativo;
        await _context.SaveChangesAsync();

        return MapLojaResponse(loja);
    }

    public virtual async Task<IEnumerable<LojaResponse>> GetLojasAsync(string userId)
    {
        var concessionaria = await ObterConcessionariaPorUsuarioAsync(userId);
        return await GetLojasAsync(concessionaria.Id);
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
            .OrderByDescending(l => l.Tipo == "Matriz")
            .ThenBy(l => l.Nome)
            .ToListAsync();

        return lojas.Select(MapLojaResponse);
    }

    public virtual async Task<IEnumerable<LojaResponse>> GetLojasAtivasAsync()
    {
        var lojas = await _context.Lojas
            .Where(l => l.Ativo)
            .OrderByDescending(l => l.Tipo == "Matriz")
            .ThenBy(l => l.Cidade)
            .ThenBy(l => l.Nome)
            .ToListAsync();

        return lojas.Select(MapLojaResponse);
    }

    public virtual async Task<LojaResponse> GetLojaByIdAsync(string userId, int lojaId)
    {
        var concessionaria = await ObterConcessionariaPorUsuarioAsync(userId);
        return await GetLojaByIdAsync(concessionaria.Id, lojaId);
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

    private async Task<bool> TelefoneLojaJaExisteAsync(string telefone, int? lojaIdIgnorado = null)
    {
        return await _context.Lojas.AnyAsync(l => l.Telefone == telefone && l.Id != lojaIdIgnorado);
    }

    private async Task<Concessionaria> ObterConcessionariaPorUsuarioAsync(string userId)
    {
        var concessionaria = await _context.Concessionarias
            .FirstOrDefaultAsync(c => c.UsuarioId == userId);

        return concessionaria == null
            ? throw new NotFoundException("Concessionaria nao encontrada.")
            : concessionaria;
    }

    private static ConcessionariaResponse MapConcessionariaResponse(Concessionaria concessionaria, string? email = null)
    {
        var lojaMatriz = concessionaria.Lojas.FirstOrDefault(l => l.Tipo == "Matriz");
        return new ConcessionariaResponse(
            concessionaria.Id,
            concessionaria.Nome,
            email ?? concessionaria.Usuario?.Email ?? string.Empty,
            concessionaria.Cnpj,
            concessionaria.Telefone,
            concessionaria.Tipo,
            lojaMatriz?.Cep ?? string.Empty,
            lojaMatriz?.Logradouro ?? string.Empty,
            lojaMatriz?.Numero ?? string.Empty,
            lojaMatriz?.Bairro ?? string.Empty,
            lojaMatriz?.Cidade ?? string.Empty,
            lojaMatriz?.Uf ?? string.Empty,
            concessionaria.Lojas
                .OrderByDescending(l => l.Tipo == "Matriz")
                .ThenBy(l => l.Nome)
                .Select(MapLojaResponse)
        );
    }

    private static LojaResponse MapLojaResponse(Loja loja)
    {
        return new LojaResponse(
            loja.Id,
            loja.Nome,
            loja.Tipo,
            loja.Cnpj,
            loja.Telefone,
            loja.Cep,
            loja.Logradouro,
            loja.Numero,
            loja.Bairro,
            loja.Cidade,
            loja.Uf,
            loja.ConcessionariaId,
            loja.Ativo,
            loja.Foto
        );
    }
}
