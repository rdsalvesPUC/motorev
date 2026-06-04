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
}
