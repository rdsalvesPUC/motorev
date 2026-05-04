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

/// <summary>
/// Serviço de gerenciamento de Concessionárias.
/// </summary>
public class ConcessionariaService
{
#pragma warning disable CS8618
    private readonly AppDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    /// <summary>
    /// Construtor vazio utilizado para Mocks em testes unitários.
    /// </summary>
    public ConcessionariaService() { } // Construtor para Moq
#pragma warning restore CS8618

    /// <summary>
    /// Construtor principal.
    /// </summary>
    public ConcessionariaService(AppDbContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Realiza o cadastro de uma nova concessionária com validações de unicidade.
    /// </summary>
    public virtual async Task<ConcessionariaResponse> RegisterAsync(RegisterConcessionariaRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) != null)
        {
            throw new DuplicateDataException($"O email {request.Email} já está em uso.");
        }

        // Nova verificação: Validar se o CNPJ já está cadastrado
        var cnpjExiste = await _context.Concessionarias.AnyAsync(c => c.Cnpj == request.Cnpj);
        if (cnpjExiste)
        {
            throw new DuplicateDataException($"O CNPJ {request.Cnpj} já está cadastrado no sistema.");
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

    /// <summary>
    /// Obtém uma concessionária específica pelo ID.
    /// </summary>
    public virtual async Task<ConcessionariaResponse> GetByIdAsync(int id)
    {
        var concessionaria = await _context.Concessionarias
            .Include(c => c.Usuario) // Carrega o usuário para pegar o e-mail
            .Include(c => c.Enderecos) // Carrega os endereços
            .FirstOrDefaultAsync(c => c.Id == id);

        if (concessionaria == null)
            throw new NotFoundException($"Concessionária com ID {id} não encontrada.");

        var response = concessionaria.Adapt<ConcessionariaResponse>();
        return response;
    }
    
    /// <summary>
    /// Obtém os dados da concessionária logada através do ID do usuário Identity.
    /// </summary>
    public virtual async Task<ConcessionariaResponse> GetByUserIdAsync(string userId)
    {
        var concessionaria = await _context.Concessionarias
            .Include(c => c.Usuario) // Carrega o usuário para pegar o e-mail
            .Include(c => c.Enderecos) // Carrega os endereços
            .FirstOrDefaultAsync(c => c.UsuarioId == userId);

        if (concessionaria == null)
            throw new NotFoundException($"Concessionária não encontrada.");

        return concessionaria.Adapt<ConcessionariaResponse>();
    }

    /// <summary>
    /// Lista e filtra concessionárias por nome, id e/ou cidade.
    /// </summary>
    public virtual async Task<List<ConcessionariaListResponse>> BuscarConcessionariasAsync(string? termoBusca, string? cidade)
    {
        var query = _context.Concessionarias.Include(c => c.Enderecos).AsQueryable();

        // Filtro por Nome ou ID
        if (!string.IsNullOrWhiteSpace(termoBusca))
        {
            if (int.TryParse(termoBusca, out int idBusca))
            {
                query = query.Where(c => c.Id == idBusca || c.Nome.Contains(termoBusca));
            }
            else
            {
                query = query.Where(c => c.Nome.Contains(termoBusca));
            }
        }

        // Filtro adicional por Cidade (que está dentro dos Enderecos)
        if (!string.IsNullOrWhiteSpace(cidade))
        {
            query = query.Where(c => c.Enderecos.Any(e => e.Cidade.Contains(cidade)));
        }

        var concessionariasData = await query.ToListAsync();

        return concessionariasData.Adapt<List<ConcessionariaListResponse>>();
    }

    /// <summary>
    /// Atualiza os dados cadastrais de uma concessionária.
    /// </summary>
    public virtual async Task<ConcessionariaResponse> UpdateAsync(int id, UpdateConcessionariaRequest request)
    {
        var concessionaria = await _context.Concessionarias
            .Include(c => c.Usuario)
            .Include(c => c.Enderecos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (concessionaria == null)
        {
            throw new NotFoundException($"Concessionária com ID {id} não encontrada.");
        }

        // Se o email está sendo alterado, verifica se já existe outro usuário com esse email
        if (concessionaria.Usuario.Email != request.Email)
        {
            var emailJaExiste = await _userManager.FindByEmailAsync(request.Email);
            if (emailJaExiste != null)
            {
                throw new DuplicateDataException($"O email {request.Email} já está em uso.");
            }
            
            // Atualiza o e-mail e username
            concessionaria.Usuario.Email = request.Email;
            concessionaria.Usuario.UserName = request.Email;
            
            var updateResult = await _userManager.UpdateAsync(concessionaria.Usuario);
            if (!updateResult.Succeeded)
            {
                throw new Exception("Falha ao atualizar o e-mail no provedor de autenticação.");
            }
        }

        // Atualiza a Razão Social
        concessionaria.Nome = request.Nome;

        await _context.SaveChangesAsync();

        return concessionaria.Adapt<ConcessionariaResponse>();
    }
}
