using Mapster;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;

namespace MotoRevApi.Services;

public class RevisaoPadraoService
{
    private readonly AppDbContext _context;

    public RevisaoPadraoService() { _context = null!; }

    public RevisaoPadraoService(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public virtual async Task<List<RevisaoPadraoListResponse>> ListarRevisoesAsync(int concessionariaId, int? modeloMotoId = null)
    {
        var query = _context.RevisoesPadrao
            .AsNoTracking()
            .Where(rp => rp.ConcessionariaId == concessionariaId && rp.Ativo);

        if (modeloMotoId.HasValue)
        {
            query = query.Where(rp => rp.ModeloMotoId == modeloMotoId.Value);
        }

        return await query
            .Include(rp => rp.ModeloMoto)
            .Select(rp => new RevisaoPadraoListResponse(
                rp.Id,
                rp.Nome,
                rp.ModeloMoto.NomeModelo
            ))
            .ToListAsync();
    }

    public virtual async Task<RevisaoPadraoResponse> GetByIdAsync(int id, int concessionariaId)
    {
        var revisao = await _context.RevisoesPadrao
            .AsNoTracking()
            .Include(rp => rp.ModeloMoto)
            .Include(rp => rp.Servicos)
                .ThenInclude(rs => rs.Servico)
            .FirstOrDefaultAsync(rp => rp.Id == id && rp.ConcessionariaId == concessionariaId && rp.Ativo);

        if (revisao == null)
        {
            throw new NotFoundException($"Revisão padrão com ID {id} não encontrada ou não pertence a esta concessionária.");
        }

        return revisao.Adapt<RevisaoPadraoResponse>();
    }

    public virtual async Task<RevisaoPadraoResponse> CadastrarRevisaoAsync(RevisaoPadraoRequest request, int concessionariaId)
    {
        var modeloExiste = await _context.ModelosMotos.AnyAsync(m => m.Id == request.ModeloMotoId);
        if (!modeloExiste)
        {
            throw new NotFoundException($"O modelo de moto com ID {request.ModeloMotoId} não existe ou está inativo.");
        }

        var ordemDuplicada = await _context.RevisoesPadrao
            .AnyAsync(rp => rp.ModeloMotoId == request.ModeloMotoId 
                         && rp.Ordem == request.Ordem 
                         && rp.ConcessionariaId == concessionariaId);
                         
        if (ordemDuplicada)
        {
            throw new DuplicateDataException($"Já existe uma revisão cadastrada com a ordem {request.Ordem} para este modelo de moto.");
        }

        var servicosExistentes = await _context.Servicos
            .Where(s => request.ServicosIds.Contains(s.Id))
            .ToListAsync();

        if (servicosExistentes.Count != request.ServicosIds.Count)
        {
            var idsNaoEncontrados = request.ServicosIds.Except(servicosExistentes.Select(s => s.Id)).ToList();
            throw new NotFoundException($"Os seguintes serviços não foram encontrados ou estão inativos: {string.Join(", ", idsNaoEncontrados)}");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var revisao = new RevisaoPadrao
            {
                Nome = request.Nome,
                Ordem = request.Ordem,
                ModeloMotoId = request.ModeloMotoId,
                ConcessionariaId = concessionariaId
            };

            _context.RevisoesPadrao.Add(revisao);
            await _context.SaveChangesAsync(); 

            foreach (var servicoId in request.ServicosIds)
            {
                _context.RevisaoPadraoServicos.Add(new RevisaoPadraoServico
                {
                    RevisaoPadraoId = revisao.Id,
                    ServicoId = servicoId
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var revisaoCriada = await _context.RevisoesPadrao
                .Include(rp => rp.ModeloMoto)
                .Include(rp => rp.Servicos)
                    .ThenInclude(rs => rs.Servico)
                .FirstAsync(rp => rp.Id == revisao.Id);

            return revisaoCriada.Adapt<RevisaoPadraoResponse>();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public virtual async Task<RevisaoPadraoResponse> AtualizarRevisaoAsync(int id, RevisaoPadraoUpdateRequest request, int concessionariaId)
    {
        var revisao = await _context.RevisoesPadrao
            .Include(rp => rp.Servicos)
            .Include(rp => rp.ModeloMoto)
            .FirstOrDefaultAsync(rp => rp.Id == id && rp.ConcessionariaId == concessionariaId);

        if (revisao == null)
        {
            throw new NotFoundException($"Revisão padrão com ID {id} não encontrada ou não pertence a esta concessionária.");
        }

        if (revisao.Ordem != request.Ordem)
        {
            var ordemDuplicada = await _context.RevisoesPadrao
                .AnyAsync(rp => rp.ModeloMotoId == revisao.ModeloMotoId 
                             && rp.Ordem == request.Ordem 
                             && rp.ConcessionariaId == concessionariaId 
                             && rp.Id != id); 
                             
            if (ordemDuplicada)
            {
                throw new DuplicateDataException($"Já existe uma revisão cadastrada com a ordem {request.Ordem} para este modelo de moto.");
            }
        }

        var servicosExistentes = await _context.Servicos
            .Where(s => request.ServicosIds.Contains(s.Id))
            .ToListAsync();

        if (servicosExistentes.Count != request.ServicosIds.Count)
        {
            var idsNaoEncontrados = request.ServicosIds.Except(servicosExistentes.Select(s => s.Id)).ToList();
            throw new NotFoundException($"Os seguintes serviços não foram encontrados ou estão inativos: {string.Join(", ", idsNaoEncontrados)}");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            revisao.Nome = request.Nome;
            revisao.Ordem = request.Ordem;

            var servicosParaRemover = revisao.Servicos
                .Where(rs => !request.ServicosIds.Contains(rs.ServicoId))
                .ToList();

            foreach (var s in servicosParaRemover)
            {
                revisao.Servicos.Remove(s);
            }

            var servicosAtuaisIds = revisao.Servicos.Select(rs => rs.ServicoId).ToList();
            var servicosParaAdicionarIds = request.ServicosIds.Except(servicosAtuaisIds).ToList();

            foreach (var servicoId in servicosParaAdicionarIds)
            {
                revisao.Servicos.Add(new RevisaoPadraoServico
                {
                    RevisaoPadraoId = revisao.Id,
                    ServicoId = servicoId
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var revisaoAtualizada = await _context.RevisoesPadrao
                .Include(rp => rp.ModeloMoto)
                .Include(rp => rp.Servicos)
                    .ThenInclude(rs => rs.Servico)
                .FirstAsync(rp => rp.Id == revisao.Id);

            return revisaoAtualizada.Adapt<RevisaoPadraoResponse>();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public virtual async Task InativarAsync(int id, int concessionariaId)
    {
        // Usa IgnoreQueryFilters para encontrar a revisão mesmo que ela já esteja inativa
        var revisao = await _context.RevisoesPadrao
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(rp => rp.Id == id && rp.ConcessionariaId == concessionariaId);

        if (revisao == null)
        {
            throw new NotFoundException($"Revisão padrão com ID {id} não encontrada ou não pertence a esta concessionária.");
        }

        // Cenário: Inativação de revisão já inativa (Idempotência)
        if (!revisao.Ativo)
        {
            return; // Já está inativa, operação concluída com sucesso sem fazer nada.
        }

        revisao.Ativo = false;
        await _context.SaveChangesAsync();
    }
}
