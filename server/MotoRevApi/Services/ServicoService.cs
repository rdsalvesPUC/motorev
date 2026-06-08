using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;

namespace MotoRevApi.Services;

public class ServicoService
{
    private readonly AppDbContext _context;

    public ServicoService() { } // Construtor para Moq

    public ServicoService(AppDbContext context)
    {
        _context = context;
    }

    public virtual async Task<ServicoResponse> CreateAsync(ServicoRequest request)
    {
        if (await ServicoAtivoDuplicadoAsync(request.Codigo, request.Nome, request.Categoria))
        {
            throw new DuplicateDataException(
                $"Já existe um serviço ativo com o código '{request.Codigo}' ou com o nome '{request.Nome}' nesta categoria.");
        }

        var servico = request.Adapt<Servico>();

        _context.Servicos.Add(servico);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateDataException(
                $"Já existe um serviço ativo com o código '{request.Codigo}' ou com o nome '{request.Nome}' nesta categoria.");
        }

        return servico.Adapt<ServicoResponse>();
    }

    public virtual async Task<IEnumerable<ServicoResponse>> GetAllAsync(CategoriaServico? categoria = null)
    {
        var query = _context.Servicos.AsNoTracking().Where(s => s.Ativo);

        if (categoria.HasValue)
        {
            query = query.Where(s => s.Categoria == categoria.Value);
        }

        var servicos = await query
            .OrderBy(s => s.Categoria)
            .ThenBy(s => s.Nome)
            .ToListAsync();

        return servicos.Adapt<IEnumerable<ServicoResponse>>();
    }

    public virtual async Task<IEnumerable<ServicoResponse>> GetCatalogoAsync(CategoriaServico? categoria = null, bool? ativo = null)
    {
        var query = _context.Servicos.AsNoTracking().AsQueryable();

        if (categoria.HasValue)
        {
            query = query.Where(s => s.Categoria == categoria.Value);
        }

        if (ativo.HasValue)
        {
            query = query.Where(s => s.Ativo == ativo.Value);
        }

        var servicos = await query
            .OrderByDescending(s => s.Ativo)
            .ThenBy(s => s.Categoria)
            .ThenBy(s => s.Nome)
            .ToListAsync();

        return servicos.Adapt<IEnumerable<ServicoResponse>>();
    }
    
    public virtual async Task<ServicoResponse> GetByIdAsync(int id)
    {
        var servico = await _context.Servicos
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.Ativo);

        if (servico == null)
        {
            throw new NotFoundException($"Serviço com ID {id} não encontrado.");
        }

        return servico.Adapt<ServicoResponse>();
    }

    public virtual async Task<ServicoResponse> UpdateAsync(int id, ServicoUpdateRequest request)
    {
        var servico = await _context.Servicos.FindAsync(id);
        if (servico == null)
        {
            throw new NotFoundException($"Serviço com ID {id} não encontrado.");
        }

        if (await ServicoAtivoDuplicadoAsync(request.Codigo, request.Nome, request.Categoria, id))
        {
            throw new DuplicateDataException(
                $"Já existe outro serviço ativo com o código '{request.Codigo}' ou com o nome '{request.Nome}' nesta categoria.");
        }

        servico.Codigo = request.Codigo;
        servico.Nome = request.Nome;
        servico.Descricao = request.Descricao;
        servico.Categoria = request.Categoria;
        servico.TempoEstimado = request.TempoEstimado;
        servico.Custo = request.Custo;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateDataException(
                $"Já existe outro serviço ativo com o código '{request.Codigo}' ou com o nome '{request.Nome}' nesta categoria.");
        }

        return servico.Adapt<ServicoResponse>();
    }

    public virtual async Task<ServicoResponse> AlternarStatusAsync(int id)
    {
        var servico = await _context.Servicos.FindAsync(id);
        if (servico == null)
        {
            throw new NotFoundException($"Serviço com ID {id} não encontrado.");
        }

        if (!servico.Ativo && await ServicoAtivoDuplicadoAsync(servico.Codigo, servico.Nome, servico.Categoria, id))
        {
            throw new DuplicateDataException(
                $"Já existe outro serviço ativo com o código '{servico.Codigo}' ou com o nome '{servico.Nome}' nesta categoria.");
        }

        servico.Ativo = !servico.Ativo;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateDataException(
                $"Já existe outro serviço ativo com o código '{servico.Codigo}' ou com o nome '{servico.Nome}' nesta categoria.");
        }

        return servico.Adapt<ServicoResponse>();
    }

    public virtual async Task InactivateAsync(int id)
    {
        var servico = await _context.Servicos.FindAsync(id);
        if (servico == null)
        {
            throw new NotFoundException($"Serviço com ID {id} não encontrado.");
        }

        if (servico.Ativo)
        {
            servico.Ativo = false;
            await _context.SaveChangesAsync();
        }
    }

    private Task<bool> ServicoAtivoDuplicadoAsync(
        string codigo,
        string nome,
        CategoriaServico categoria,
        int? idIgnorado = null)
    {
        return _context.Servicos
            .AsNoTracking()
            .AnyAsync(s =>
                s.Ativo &&
                (!idIgnorado.HasValue || s.Id != idIgnorado.Value) &&
                (s.Codigo == codigo || (s.Nome == nome && s.Categoria == categoria)));
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
    }
}
