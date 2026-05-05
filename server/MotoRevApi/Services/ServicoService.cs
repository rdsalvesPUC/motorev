using Mapster;
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
        var servicoDuplicado = await _context.Servicos
            .AsNoTracking()
            .AnyAsync(s => s.Ativo && (s.Nome == request.Nome || s.Codigo == request.Codigo));

        if (servicoDuplicado)
        {
            throw new DuplicateDataException(
                $"Já existe um serviço ativo com o nome '{request.Nome}' ou código '{request.Codigo}'.");
        }

        var servico = request.Adapt<Servico>();

        _context.Servicos.Add(servico);
        await _context.SaveChangesAsync();

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

        var servicoDuplicado = await _context.Servicos
            .AsNoTracking()
            .AnyAsync(s => s.Id != id && s.Ativo 
                && (s.Nome == request.Nome || s.Codigo == request.Codigo));

        if (servicoDuplicado)
        {
            throw new DuplicateDataException(
                $"Já existe outro serviço ativo com o nome '{request.Nome}' ou código '{request.Codigo}'.");
        }

        servico.Codigo = request.Codigo;
        servico.Nome = request.Nome;
        servico.Descricao = request.Descricao;
        servico.Categoria = request.Categoria;
        servico.TempoEstimado = request.TempoEstimado;
        servico.Custo = request.Custo;

        await _context.SaveChangesAsync();

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
}
