using Mapster;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
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
            .Include(rp => rp.Pecas)
                .ThenInclude(rp => rp.Peca)
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

        // 3. Validar se os serviços existem e estao ativos
        if (request.ServicosIds.Distinct().Count() != request.ServicosIds.Count)
        {
            throw new DuplicateDataException("A revisão não pode conter serviços duplicados.");
        }

        var servicosExistentes = await _context.Servicos
            .Where(s => request.ServicosIds.Contains(s.Id) && s.Ativo)
            .ToListAsync();

        if (servicosExistentes.Count != request.ServicosIds.Count)
        {
            var idsNaoEncontrados = request.ServicosIds.Except(servicosExistentes.Select(s => s.Id)).ToList();
            throw new NotFoundException($"Os seguintes serviços não foram encontrados ou estão inativos: {string.Join(", ", idsNaoEncontrados)}");
        }

        // 4. Validar pecas quando informadas
        var pecasRequest = request.Pecas ?? new List<RevisaoPadraoPecaRequest>();
        if (pecasRequest.Any(p => p.Quantidade <= 0))
        {
            throw new ValidationException("A quantidade de cada peça deve ser maior que zero.");
        }

        if (pecasRequest.Select(p => p.PecaId).Distinct().Count() != pecasRequest.Count)
        {
            throw new DuplicateDataException("A revisão não pode conter peças duplicadas.");
        }

        var pecasIds = pecasRequest.Select(p => p.PecaId).ToList();
        var pecasExistentes = await _context.Pecas
            .Where(p => pecasIds.Contains(p.Id) && p.Status == StatusCadastro.Ativo)
            .ToListAsync();

        if (pecasExistentes.Count != pecasIds.Count)
        {
            var idsNaoEncontrados = pecasIds.Except(pecasExistentes.Select(p => p.Id)).ToList();
            throw new NotFoundException($"As seguintes peças não foram encontradas ou estão inativas: {string.Join(", ", idsNaoEncontrados)}");
        }

        // 5. Iniciar a transação para salvar a revisão e as relações
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

            // 6. Vincular os serviços à revisão
            foreach (var servicoId in request.ServicosIds)
            {
                _context.RevisaoPadraoServicos.Add(new RevisaoPadraoServico
                {
                    RevisaoPadraoId = revisao.Id,
                    ServicoId = servicoId
                });
            }

            foreach (var peca in pecasRequest)
            {
                _context.RevisaoPadraoPecas.Add(new RevisaoPadraoPeca
                {
                    RevisaoPadraoId = revisao.Id,
                    PecaId = peca.PecaId,
                    Quantidade = peca.Quantidade
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Carrega os relacionamentos para retornar o DTO completo
            var revisaoCriada = await _context.RevisoesPadrao
                .Include(rp => rp.ModeloMoto)
                .Include(rp => rp.Servicos)
                    .ThenInclude(rs => rs.Servico)
                .Include(rp => rp.Pecas)
                    .ThenInclude(rp => rp.Peca)
                .FirstAsync(rp => rp.Id == revisao.Id);

            return revisaoCriada.Adapt<RevisaoPadraoResponse>();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
