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
    
    public virtual async Task<List<RevisaoPadraoListResponse>> ListarRevisoesAsync(int concessionariaId, int? modeloMotoId = null, int? linhaId = null)
    {
        var query = _context.RevisoesPadrao
            .AsNoTracking()
            .Where(rp => rp.ConcessionariaId == concessionariaId);

        if (linhaId.HasValue)
        {
            query = query.Where(rp => rp.LinhaId == linhaId.Value);
        }

        if (modeloMotoId.HasValue)
        {
            var modeloLinhaId = await _context.ModelosMotos
                .Where(m => m.Id == modeloMotoId.Value)
                .Select(m => m.LinhaId)
                .FirstOrDefaultAsync();

            query = query.Where(rp => rp.LinhaId == modeloLinhaId);
        }

        return await query
            .Include(rp => rp.Linha)
            .OrderBy(rp => rp.Linha.Nome)
            .ThenBy(rp => rp.Ordem)
            .Select(rp => new RevisaoPadraoListResponse(
                rp.Id,
                rp.Nome,
                rp.LinhaId,
                rp.Linha.Nome,
                rp.Ordem,
                rp.Quilometragem,
                rp.TempoMeses,
                rp.Ativo
            ))
            .ToListAsync();
    }

    public virtual async Task<RevisaoPadraoResponse> GetByIdAsync(int id, int concessionariaId)
    {
        var revisao = await _context.RevisoesPadrao
            .AsNoTracking()
            .Include(rp => rp.Linha)
            .Include(rp => rp.Servicos)
                .ThenInclude(rs => rs.Servico)
            .Include(rp => rp.Pecas)
                .ThenInclude(rp => rp.Peca)
            .FirstOrDefaultAsync(rp => rp.Id == id && rp.ConcessionariaId == concessionariaId);

        if (revisao == null)
        {
            throw new NotFoundException($"Revisão padrão com ID {id} não encontrada ou não pertence a esta concessionária.");
        }

        return revisao.Adapt<RevisaoPadraoResponse>();
    }

    public virtual async Task<RevisaoPadraoResponse> CadastrarRevisaoAsync(RevisaoPadraoRequest request, int concessionariaId)
    {
        // 1. Validar se o Modelo de Moto existe
        var modelo = await _context.ModelosMotos.FirstOrDefaultAsync(m => m.Id == request.ModeloMotoId && m.Ativo);
        if (modelo == null)
        {
            throw new NotFoundException($"O modelo de moto com ID {request.ModeloMotoId} não existe ou está inativo.");
        }

        // 2. Validar se a ordem já existe para esta linha nesta concessionária
        var ordemDuplicada = await _context.RevisoesPadrao
            .AnyAsync(rp => rp.LinhaId == modelo.LinhaId 
                          && rp.Ordem == request.Ordem 
                          && rp.ConcessionariaId == concessionariaId);
                          
        if (ordemDuplicada)
        {
            throw new DuplicateDataException($"Já existe uma revisão cadastrada com a ordem {request.Ordem} para este modelo de moto.");
        }

        // 3. Validar se os serviços existem e estao ativos
        if (request.ServicosIds.Count == 0)
        {
            throw new ValidationException("A revisão deve conter pelo menos um serviço.");
        }

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
                Quilometragem = request.Quilometragem,
                TempoMeses = request.TempoMeses,
                LinhaId = modelo.LinhaId,
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
                .Include(rp => rp.Linha)
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

    public virtual async Task<RevisaoPadraoResponse> AtualizarRevisaoAsync(int id, RevisaoPadraoUpdateRequest request, int concessionariaId)
    {
        var revisao = await _context.RevisoesPadrao
            .Include(rp => rp.Servicos)
            .Include(rp => rp.Pecas)
            .FirstOrDefaultAsync(rp => rp.Id == id && rp.ConcessionariaId == concessionariaId);

        if (revisao == null)
        {
            throw new NotFoundException($"Revisão padrão com ID {id} não encontrada ou não pertence a esta concessionária.");
        }

        // 1. Validar se a ordem já existe para esta linha nesta concessionária (excluindo a própria revisão)
        var ordemDuplicada = await _context.RevisoesPadrao
            .AnyAsync(rp => rp.LinhaId == revisao.LinhaId 
                          && rp.Ordem == request.Ordem 
                          && rp.Id != id
                          && rp.ConcessionariaId == concessionariaId);
                          
        if (ordemDuplicada)
        {
            throw new DuplicateDataException($"Já existe uma revisão cadastrada com a ordem {request.Ordem} para esta linha.");
        }

        // 2. Validar se os serviços existem e estao ativos
        await ValidarServicosAsync(request.ServicosIds);

        // 3. Validar pecas quando informadas
        var pecasRequest = request.Pecas ?? new List<RevisaoPadraoPecaRequest>();
        await ValidarPecasAsync(pecasRequest);

        // 4. Iniciar a transação
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Atualizar os campos da revisão
            revisao.Nome = request.Nome;
            revisao.Ordem = request.Ordem;
            revisao.Quilometragem = request.Quilometragem;
            revisao.TempoMeses = request.TempoMeses;

            // Remover serviços antigos e adicionar novos
            _context.RevisaoPadraoServicos.RemoveRange(revisao.Servicos);
            revisao.Servicos.Clear();

            foreach (var servicoId in request.ServicosIds)
            {
                revisao.Servicos.Add(new RevisaoPadraoServico
                {
                    RevisaoPadraoId = revisao.Id,
                    ServicoId = servicoId
                });
            }

            // Remover peças antigas e adicionar novas
            _context.RevisaoPadraoPecas.RemoveRange(revisao.Pecas);
            revisao.Pecas.Clear();

            foreach (var peca in pecasRequest)
            {
                revisao.Pecas.Add(new RevisaoPadraoPeca
                {
                    RevisaoPadraoId = revisao.Id,
                    PecaId = peca.PecaId,
                    Quantidade = peca.Quantidade
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Carrega os relacionamentos para retornar o DTO completo
            var revisaoAtualizada = await _context.RevisoesPadrao
                .Include(rp => rp.Linha)
                .Include(rp => rp.Servicos)
                    .ThenInclude(rs => rs.Servico)
                .Include(rp => rp.Pecas)
                    .ThenInclude(rp => rp.Peca)
                .FirstAsync(rp => rp.Id == revisao.Id);

            return revisaoAtualizada.Adapt<RevisaoPadraoResponse>();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


    public virtual async Task<List<RevisaoPadraoResponse>> CadastrarRevisoesPorLinhaAsync(RevisaoPadraoLinhaRequest request, int concessionariaId)
    {
        var linhaExiste = await _context.Linhas
            .AsNoTracking()
            .AnyAsync(l => l.Id == request.LinhaId && l.Ativo);

        if (!linhaExiste)
        {
            throw new NotFoundException($"Linha com ID {request.LinhaId} não encontrada ou inativa.");
        }

        if (request.Revisoes.Select(r => r.Ordem).Distinct().Count() != request.Revisoes.Count)
        {
            throw new DuplicateDataException("O modelo de revisão não pode conter ordens duplicadas.");
        }

        foreach (var revisao in request.Revisoes)
        {
            await ValidarServicosAsync(revisao.ServicosIds);
            await ValidarPecasAsync(revisao.Pecas ?? new List<RevisaoPadraoPecaRequest>());
        }

        var ordens = request.Revisoes.Select(r => r.Ordem).ToList();
        var revisaoDuplicada = await _context.RevisoesPadrao
            .AsNoTracking()
            .AnyAsync(rp => rp.ConcessionariaId == concessionariaId
                            && rp.LinhaId == request.LinhaId
                            && ordens.Contains(rp.Ordem));

        if (revisaoDuplicada)
        {
            throw new DuplicateDataException("Já existe revisão cadastrada para esta linha com uma das ordens informadas.");
        }

        var revisoesCriadasIds = new List<int>();
        var revisoesAdicionadas = new List<RevisaoPadrao>();
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var revisaoRequest in request.Revisoes)
            {
                var revisao = new RevisaoPadrao
                {
                    Nome = revisaoRequest.Nome,
                    Ordem = revisaoRequest.Ordem,
                    Quilometragem = revisaoRequest.Quilometragem,
                    TempoMeses = revisaoRequest.TempoMeses,
                    LinhaId = request.LinhaId,
                    ConcessionariaId = concessionariaId,
                    Servicos = revisaoRequest.ServicosIds.Select(servicoId => new RevisaoPadraoServico
                    {
                        ServicoId = servicoId
                    }).ToList(),
                    Pecas = (revisaoRequest.Pecas ?? new List<RevisaoPadraoPecaRequest>()).Select(peca => new RevisaoPadraoPeca
                    {
                        PecaId = peca.PecaId,
                        Quantidade = peca.Quantidade
                    }).ToList()
                };

                _context.RevisoesPadrao.Add(revisao);
                revisoesAdicionadas.Add(revisao);
            }

            await _context.SaveChangesAsync();
            revisoesCriadasIds.AddRange(revisoesAdicionadas.Select(r => r.Id));
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        var revisoesCriadas = await _context.RevisoesPadrao
            .Include(rp => rp.Linha)
            .Include(rp => rp.Servicos)
                .ThenInclude(rs => rs.Servico)
            .Include(rp => rp.Pecas)
                .ThenInclude(rp => rp.Peca)
            .Where(rp => revisoesCriadasIds.Contains(rp.Id))
            .OrderBy(rp => rp.Linha.Nome)
            .ThenBy(rp => rp.Ordem)
            .ToListAsync();

        return revisoesCriadas.Adapt<List<RevisaoPadraoResponse>>();
    }

    public virtual async Task<List<RevisaoPadraoResponse>> AtualizarRevisoesPorLinhaAsync(int linhaId, RevisaoPadraoLinhaRequest request, int concessionariaId)
    {
        var linhaExiste = await _context.Linhas
            .AsNoTracking()
            .AnyAsync(l => l.Id == linhaId && l.Ativo);

        if (!linhaExiste)
        {
            throw new NotFoundException($"Linha com ID {linhaId} não encontrada ou inativa.");
        }

        if (request.Revisoes.Select(r => r.Ordem).Distinct().Count() != request.Revisoes.Count)
        {
            throw new DuplicateDataException("O modelo de revisão não pode conter ordens duplicadas.");
        }

        foreach (var revisao in request.Revisoes)
        {
            await ValidarServicosAsync(revisao.ServicosIds);
            await ValidarPecasAsync(revisao.Pecas ?? new List<RevisaoPadraoPecaRequest>());
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Remover revisões existentes para esta linha nesta concessionária
            var revisoesExistentes = await _context.RevisoesPadrao
                .Include(rp => rp.Servicos)
                .Include(rp => rp.Pecas)
                .Where(rp => rp.LinhaId == linhaId && rp.ConcessionariaId == concessionariaId)
                .ToListAsync();

            if (revisoesExistentes.Count > 0)
            {
                foreach (var rev in revisoesExistentes)
                {
                    _context.RevisaoPadraoServicos.RemoveRange(rev.Servicos);
                    _context.RevisaoPadraoPecas.RemoveRange(rev.Pecas);
                }
                _context.RevisoesPadrao.RemoveRange(revisoesExistentes);
                await _context.SaveChangesAsync();
            }

            // 2. Adicionar novas revisões
            var revisoesCriadasIds = new List<int>();
            var revisoesAdicionadas = new List<RevisaoPadrao>();

            foreach (var revisaoRequest in request.Revisoes)
            {
                var revisao = new RevisaoPadrao
                {
                    Nome = revisaoRequest.Nome,
                    Ordem = revisaoRequest.Ordem,
                    Quilometragem = revisaoRequest.Quilometragem,
                    TempoMeses = revisaoRequest.TempoMeses,
                    LinhaId = linhaId,
                    ConcessionariaId = concessionariaId,
                    Servicos = revisaoRequest.ServicosIds.Select(servicoId => new RevisaoPadraoServico
                    {
                        ServicoId = servicoId
                    }).ToList(),
                    Pecas = (revisaoRequest.Pecas ?? new List<RevisaoPadraoPecaRequest>()).Select(peca => new RevisaoPadraoPeca
                    {
                        PecaId = peca.PecaId,
                        Quantidade = peca.Quantidade
                    }).ToList()
                };

                _context.RevisoesPadrao.Add(revisao);
                revisoesAdicionadas.Add(revisao);
            }

            await _context.SaveChangesAsync();
            revisoesCriadasIds.AddRange(revisoesAdicionadas.Select(r => r.Id));
            await transaction.CommitAsync();

            var revisoesCriadas = await _context.RevisoesPadrao
                .Include(rp => rp.Linha)
                .Include(rp => rp.Servicos)
                    .ThenInclude(rs => rs.Servico)
                .Include(rp => rp.Pecas)
                    .ThenInclude(rp => rp.Peca)
                .Where(rp => revisoesCriadasIds.Contains(rp.Id))
                .OrderBy(rp => rp.Linha.Nome)
                .ThenBy(rp => rp.Ordem)
                .ToListAsync();

            return revisoesCriadas.Adapt<List<RevisaoPadraoResponse>>();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


    public virtual async Task<List<RevisaoPadraoListResponse>> AlternarStatusPorLinhaAsync(int linhaId, int concessionariaId)
    {
        var revisoes = await _context.RevisoesPadrao
            .Include(rp => rp.Linha)
            .Where(rp => rp.ConcessionariaId == concessionariaId && rp.LinhaId == linhaId)
            .ToListAsync();

        if (revisoes.Count == 0)
        {
            throw new NotFoundException($"Nenhuma revisão padrão encontrada para a linha com ID {linhaId}.");
        }

        var novoStatus = !revisoes.Any(rp => rp.Ativo);
        if (novoStatus == false)
        {
            var temModelosAtivos = await _context.ModelosMotos.AnyAsync(m => m.LinhaId == linhaId && m.Ativo);
            if (temModelosAtivos)
            {
                throw new BusinessRuleException("Não é possível desativar este modelo de revisão porque ele possui modelos de moto ativos vinculados.");
            }
        }

        foreach (var revisao in revisoes)
        {
            revisao.Ativo = novoStatus;
        }

        await _context.SaveChangesAsync();

        return revisoes
            .OrderBy(rp => rp.Ordem)
            .Select(rp => new RevisaoPadraoListResponse(
                rp.Id,
                rp.Nome,
                rp.LinhaId,
                rp.Linha.Nome,
                rp.Ordem,
                rp.Quilometragem,
                rp.TempoMeses,
                rp.Ativo
            ))
            .ToList();
    }

    private async Task ValidarServicosAsync(List<int> servicosIds)
    {
        if (servicosIds.Count == 0)
        {
            throw new ValidationException("A revisão deve conter pelo menos um serviço.");
        }

        if (servicosIds.Distinct().Count() != servicosIds.Count)
        {
            throw new DuplicateDataException("A revisão não pode conter serviços duplicados.");
        }

        var servicosExistentes = await _context.Servicos
            .Where(s => servicosIds.Contains(s.Id) && s.Ativo)
            .ToListAsync();

        if (servicosExistentes.Count != servicosIds.Count)
        {
            var idsNaoEncontrados = servicosIds.Except(servicosExistentes.Select(s => s.Id)).ToList();
            throw new NotFoundException($"Os seguintes serviços não foram encontrados ou estão inativos: {string.Join(", ", idsNaoEncontrados)}");
        }
    }

    private async Task ValidarPecasAsync(List<RevisaoPadraoPecaRequest> pecasRequest)
    {
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
    }
}
