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
            // TODO: Adicionar .Include() para Peças quando implementado
            .FirstOrDefaultAsync(rp => rp.Id == id && rp.ConcessionariaId == concessionariaId && rp.Ativo);

        if (revisao == null)
        {
            throw new NotFoundException($"Revisão padrão com ID {id} não encontrada ou não pertence a esta concessionária.");
        }

        return revisao.Adapt<RevisaoPadraoResponse>();
    }

    public virtual async Task<RevisaoPadraoResponse> CadastrarRevisaoAsync(RevisaoPadraoRequest request, int concessionariaId)
    {
        // 1. Validar se o Modelo de Moto existe
        var modeloExiste = await _context.ModelosMotos.AnyAsync(m => m.Id == request.ModeloMotoId);
        if (!modeloExiste)
        {
            throw new NotFoundException($"O modelo de moto com ID {request.ModeloMotoId} não existe ou está inativo.");
        }

        // 2. Validar se a ordem já existe para este modelo nesta concessionária
        var ordemDuplicada = await _context.RevisoesPadrao
            .AnyAsync(rp => rp.ModeloMotoId == request.ModeloMotoId 
                         && rp.Ordem == request.Ordem 
                         && rp.ConcessionariaId == concessionariaId);
                         
        if (ordemDuplicada)
        {
            throw new DuplicateDataException($"Já existe uma revisão cadastrada com a ordem {request.Ordem} para este modelo de moto.");
        }

        // 3. Validar se os serviços existem
        var servicosExistentes = await _context.Servicos
            .Where(s => request.ServicosIds.Contains(s.Id))
            .ToListAsync();

        if (servicosExistentes.Count != request.ServicosIds.Count)
        {
            // Descobre quais IDs não foram encontrados
            var idsNaoEncontrados = request.ServicosIds.Except(servicosExistentes.Select(s => s.Id)).ToList();
            throw new NotFoundException($"Os seguintes serviços não foram encontrados ou estão inativos: {string.Join(", ", idsNaoEncontrados)}");
        }

        // 4. Iniciar a transação para salvar a revisão e as relações
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
            await _context.SaveChangesAsync(); // Salva para gerar o ID da RevisaoPadrao

            // 5. Vincular os serviços à revisão
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

            // Carrega os relacionamentos para retornar o DTO completo
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
        // 1. Validar se a revisão existe
        var revisao = await _context.RevisoesPadrao
            .Include(rp => rp.Servicos) // Carrega as relações N:N para podermos alterá-las
            .Include(rp => rp.ModeloMoto)
            .FirstOrDefaultAsync(rp => rp.Id == id && rp.ConcessionariaId == concessionariaId);

        if (revisao == null)
        {
            throw new NotFoundException($"Revisão padrão com ID {id} não encontrada ou não pertence a esta concessionária.");
        }

        // 2. Validar ordem duplicada APENAS se a ordem for alterada
        if (revisao.Ordem != request.Ordem)
        {
            var ordemDuplicada = await _context.RevisoesPadrao
                .AnyAsync(rp => rp.ModeloMotoId == revisao.ModeloMotoId 
                             && rp.Ordem == request.Ordem 
                             && rp.ConcessionariaId == concessionariaId 
                             && rp.Id != id); // Ignora a própria revisão em edição
                             
            if (ordemDuplicada)
            {
                throw new DuplicateDataException($"Já existe uma revisão cadastrada com a ordem {request.Ordem} para este modelo de moto.");
            }
        }

        // 3. Validar se os novos serviços existem
        var servicosExistentes = await _context.Servicos
            .Where(s => request.ServicosIds.Contains(s.Id))
            .ToListAsync();

        if (servicosExistentes.Count != request.ServicosIds.Count)
        {
            var idsNaoEncontrados = request.ServicosIds.Except(servicosExistentes.Select(s => s.Id)).ToList();
            throw new NotFoundException($"Os seguintes serviços não foram encontrados ou estão inativos: {string.Join(", ", idsNaoEncontrados)}");
        }

        // 4. Iniciar a transação
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 4.1 Atualizar os campos básicos
            revisao.Nome = request.Nome;
            revisao.Ordem = request.Ordem;

            // 4.2 Sincronizar Serviços (N:N)
            // Remover os serviços que não estão mais na nova lista
            var servicosParaRemover = revisao.Servicos
                .Where(rs => !request.ServicosIds.Contains(rs.ServicoId))
                .ToList();

            foreach (var s in servicosParaRemover)
            {
                revisao.Servicos.Remove(s);
            }

            // Adicionar os serviços que estão na nova lista, mas não estavam na antiga
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

            // 5. Retornar os dados atualizados com todas as referências carregadas
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
}
