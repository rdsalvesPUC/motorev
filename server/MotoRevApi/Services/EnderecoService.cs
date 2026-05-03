using Mapster;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;

namespace MotoRevApi.Services;

/// <summary>
/// Serviço para gerenciar endereços de concessionárias.
/// </summary>
public class EnderecoService
{
#pragma warning disable CS8618
    private readonly AppDbContext _context;

    public EnderecoService() { } // Construtor para Moq
#pragma warning restore CS8618

    /// <summary>
    /// Inicializa uma nova instância de <see cref="EnderecoService"/>.
    /// </summary>
    public EnderecoService(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Adiciona um novo endereço para uma concessionária.
    /// </summary>
    public virtual async Task<EnderecoResponse> AdicionarEnderecoAsync(int concessionariaId, EnderecoRequest request)
    {
        var concessionariaExists = await _context.Concessionarias.AnyAsync(c => c.Id == concessionariaId);
        if (!concessionariaExists)
        {
            throw new NotFoundException("Concessionária não encontrada.");
        }

        var endereco = request.Adapt<Endereco>();
        endereco.ConcessionariaId = concessionariaId;

        _context.Enderecos.Add(endereco);
        await _context.SaveChangesAsync();

        return endereco.Adapt<EnderecoResponse>();
    }

    /// <summary>
    /// Remove um endereço existente se pertencer à concessionária forneçada.
    /// </summary>
    public virtual async Task RemoverEnderecoAsync(int enderecoId, int concessionariaId)
    {
        var endereco = await _context.Enderecos
            .FirstOrDefaultAsync(e => e.Id == enderecoId && e.ConcessionariaId == concessionariaId);

        if (endereco == null)
        {
            throw new NotFoundException("Endereço não encontrado ou não pertence a esta concessionária.");
        }

        _context.Enderecos.Remove(endereco);
        await _context.SaveChangesAsync();
    }
}
