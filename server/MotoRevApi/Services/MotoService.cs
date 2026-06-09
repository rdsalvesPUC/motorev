using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Domain.Revisoes;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;

namespace MotoRevApi.Services;

public class MotoService
{
    private readonly AppDbContext _context;

    public MotoService() { } // Construtor para Moq

    public MotoService(AppDbContext context)
    {
        _context = context;
    }

    public virtual async Task<List<MotoResponse>> ListarMotosClienteAsync(string userId)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.UsuarioId == userId);
        if (cliente == null)
        {
            throw new NotFoundException("Cliente não encontrado.");
        }

        var motos = await _context.Motos
            .Include(m => m.ModeloMoto)
                .ThenInclude(mm => mm.Linha)
            .Include(m => m.Concessionaria)
            .Include(m => m.RevisoesPlanejadas)
                .ThenInclude(rm => rm.Loja)
            .Include(m => m.RevisoesPlanejadas)
                .ThenInclude(rm => rm.RevisaoPadrao)
                    .ThenInclude(rp => rp.Servicos)
                        .ThenInclude(rps => rps.Servico)
            .Include(m => m.RevisoesPlanejadas)
                .ThenInclude(rm => rm.RevisaoPadrao)
                    .ThenInclude(rp => rp.Pecas)
                        .ThenInclude(rpp => rpp.Peca)
            .AsSplitQuery()
            .Where(m => m.ClienteId == cliente.Id && m.Ativo)
            .ToListAsync();

        return motos.Adapt<List<MotoResponse>>();
    }

    public virtual async Task<MotoResponse> CadastrarMotoAsync(MotoRequest request, string userId)
    {
        var placaUpper = request.Placa.ToUpper().Replace("-", "");
        var chassiUpper = request.Chassi.ToUpper();

        // Validar se Placa ou Chassi já estão vinculados a outra moto ativa
        var motoExistente = await _context.Motos
            .AnyAsync(m => m.Ativo && (m.Placa == placaUpper || m.Chassi == chassiUpper));

        if (motoExistente)
        {
            throw new DuplicateDataException("Veículo com esta placa ou chassi já cadastrado.");
        }

        // Validar a existência do Modelo de Moto no banco de dados
        var modelo = await _context.ModelosMotos
            .FirstOrDefaultAsync(m => m.Id == request.ModeloMotoId && m.Ativo);
        if (modelo == null)
        {
            throw new NotFoundException("Modelo de moto não encontrado.");
        }

        // Buscar o cliente a partir do User ID
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.UsuarioId == userId);
        if (cliente == null)
        {
            throw new NotFoundException("Cliente não encontrado.");
        }

        var revisoesPadrao = await _context.RevisoesPadrao
            .Where(rp => rp.LinhaId == modelo.LinhaId && rp.Ativo)
            .OrderBy(rp => rp.Ordem)
            .ToListAsync();

        if (revisoesPadrao.Count == 0)
        {
            throw new BusinessRuleException(
                "Este modelo de moto ainda não possui um modelo de revisão ativo vinculado à sua linha.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var moto = request.Adapt<Moto>();
            moto.Placa = placaUpper;
            moto.Chassi = chassiUpper;
            moto.ClienteId = cliente.Id;
            moto.Ativo = true;

            foreach (var revisaoPadrao in revisoesPadrao)
            {
                moto.RevisoesPlanejadas.Add(new RevisaoMoto
                {
                    RevisaoPadraoId = revisaoPadrao.Id,
                    Nome = revisaoPadrao.Nome,
                    Ordem = revisaoPadrao.Ordem,
                    Quilometragem = revisaoPadrao.Quilometragem,
                    TempoMeses = revisaoPadrao.TempoMeses,
                    DataPrevista = moto.DataVenda.AddMonths(revisaoPadrao.TempoMeses),
                    Status = StatusRevisaoMoto.Planejada,
                });
            }

            _context.Motos.Add(moto);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Retorna o response carregando os dados associados ao modelo e ao plano de revisões.
            var savedMoto = await _context.Motos
                .Include(m => m.ModeloMoto)
                    .ThenInclude(mm => mm.Linha)
                .Include(m => m.Concessionaria)
                .Include(m => m.RevisoesPlanejadas)
                    .ThenInclude(rm => rm.Loja)
                .Include(m => m.RevisoesPlanejadas)
                    .ThenInclude(rm => rm.RevisaoPadrao)
                        .ThenInclude(rp => rp.Servicos)
                            .ThenInclude(rps => rps.Servico)
                .Include(m => m.RevisoesPlanejadas)
                    .ThenInclude(rm => rm.RevisaoPadrao)
                        .ThenInclude(rp => rp.Pecas)
                            .ThenInclude(rpp => rpp.Peca)
                .AsSplitQuery()
                .FirstAsync(m => m.Id == moto.Id);

            return savedMoto.Adapt<MotoResponse>();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public virtual async Task<MotoResponse> GetByIdAsync(int id, string userId)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.UsuarioId == userId);
        if (cliente == null)
        {
            throw new NotFoundException("Cliente não encontrado.");
        }

        var moto = await _context.Motos
            .Include(m => m.ModeloMoto)
                .ThenInclude(mm => mm.Linha)
            .Include(m => m.Concessionaria)
            .Include(m => m.RevisoesPlanejadas)
                .ThenInclude(rm => rm.Loja)
            .Include(m => m.RevisoesPlanejadas)
                .ThenInclude(rm => rm.RevisaoPadrao)
                    .ThenInclude(rp => rp.Servicos)
                        .ThenInclude(rps => rps.Servico)
            .Include(m => m.RevisoesPlanejadas)
                .ThenInclude(rm => rm.RevisaoPadrao)
                    .ThenInclude(rp => rp.Pecas)
                        .ThenInclude(rpp => rpp.Peca)
            .AsSplitQuery()
            .FirstOrDefaultAsync(m => m.Id == id && m.ClienteId == cliente.Id && m.Ativo);

        if (moto == null)
        {
            throw new NotFoundException("Moto não encontrada.");
        }

        return moto.Adapt<MotoResponse>();
    }

    public virtual async Task<MotoResponse> AtualizarMotoAsync(int id, MotoUpdateRequest request, string userId)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.UsuarioId == userId);
        if (cliente == null)
        {
            throw new NotFoundException("Cliente não encontrado.");
        }

        var moto = await _context.Motos
            .FirstOrDefaultAsync(m => m.Id == id && m.ClienteId == cliente.Id && m.Ativo);

        if (moto == null)
        {
            throw new NotFoundException("Moto não encontrada.");
        }

        var placaUpper = request.Placa.ToUpper().Replace("-", "");

        // Validar se a nova placa já está em uso por outra moto ativa (exceto a atual)
        var placaEmUso = await _context.Motos
            .AnyAsync(m => m.Ativo && m.Id != id && m.Placa == placaUpper);

        if (placaEmUso)
        {
            throw new DuplicateDataException("Outro veículo com esta placa já cadastrado.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Valida que a nova quilometragem não é inferior à atual
            if (request.KilometragemAtual < moto.KilometragemAtual)
            {
                throw new BusinessRuleException(
                    $"A quilometragem não pode ser reduzida. Valor atual: {moto.KilometragemAtual} km.");
            }

            // Atualiza apenas os campos editáveis (Chassi, ModeloMotoId e Ano são imutáveis)
            moto.Placa = placaUpper;
            moto.Cor = request.Cor;
            moto.KilometragemAtual = request.KilometragemAtual;

            _context.Motos.Update(moto);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var updatedMoto = await _context.Motos
                .Include(m => m.ModeloMoto)
                    .ThenInclude(mm => mm.Linha)
                .Include(m => m.Concessionaria)
                .Include(m => m.RevisoesPlanejadas)
                    .ThenInclude(rm => rm.Loja)
                .Include(m => m.RevisoesPlanejadas)
                    .ThenInclude(rm => rm.RevisaoPadrao)
                        .ThenInclude(rp => rp.Servicos)
                            .ThenInclude(rps => rps.Servico)
                .Include(m => m.RevisoesPlanejadas)
                    .ThenInclude(rm => rm.RevisaoPadrao)
                        .ThenInclude(rp => rp.Pecas)
                            .ThenInclude(rpp => rpp.Peca)
                .AsSplitQuery()
                .FirstAsync(m => m.Id == moto.Id);

            return updatedMoto.Adapt<MotoResponse>();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public virtual async Task<RevisaoMotoResponse> SolicitarAgendamentoRevisaoAsync(
        int revisaoMotoId,
        AgendamentoRevisaoRequest request,
        string userId)
    {
        var revisao = await ObterRevisaoMotoDoClienteAsync(revisaoMotoId, userId);
        var statusAtual = RevisaoMotoStatusPolicy.ObterStatusEfetivo(
            revisao.Status,
            revisao.DataPrevista,
            revisao.DataAgendamento,
            DateTime.Today);

        if (!RevisaoMotoStatusPolicy.PodeSolicitarAgendamento(statusAtual))
        {
            throw new BusinessRuleException("Esta revisão não está disponível para agendamento.");
        }

        await ValidarLojaEJanelaAgendamentoAsync(revisao, request);

        revisao.LojaId = request.LojaId;
        revisao.DataAgendamento = request.DataAgendamento.Date;
        revisao.Status = StatusRevisaoMoto.AguardandoConfirmacao;

        await _context.SaveChangesAsync();
        return await ObterRevisaoMotoResponseAsync(revisaoMotoId);
    }

    public virtual async Task<RevisaoMotoResponse> RemarcarAgendamentoRevisaoAsync(
        int revisaoMotoId,
        AgendamentoRevisaoRequest request,
        string userId)
    {
        var revisao = await ObterRevisaoMotoDoClienteAsync(revisaoMotoId, userId);
        var statusAtual = RevisaoMotoStatusPolicy.ObterStatusEfetivo(
            revisao.Status,
            revisao.DataPrevista,
            revisao.DataAgendamento,
            DateTime.Today);

        if (!RevisaoMotoStatusPolicy.PodeRemarcar(statusAtual))
        {
            throw new BusinessRuleException("Apenas revisões agendadas podem ser remarcadas.");
        }

        await ValidarLojaEJanelaAgendamentoAsync(revisao, request);

        revisao.LojaId = request.LojaId;
        revisao.DataAgendamento = request.DataAgendamento.Date;
        revisao.Status = StatusRevisaoMoto.AguardandoConfirmacao;

        await _context.SaveChangesAsync();
        return await ObterRevisaoMotoResponseAsync(revisaoMotoId);
    }

    public virtual async Task<RevisaoMotoResponse> CancelarAgendamentoRevisaoAsync(int revisaoMotoId, string userId)
    {
        var revisao = await ObterRevisaoMotoDoClienteAsync(revisaoMotoId, userId);
        var statusAtual = RevisaoMotoStatusPolicy.ObterStatusEfetivo(
            revisao.Status,
            revisao.DataPrevista,
            revisao.DataAgendamento,
            DateTime.Today);

        if (!RevisaoMotoStatusPolicy.PodeCancelar(statusAtual))
        {
            throw new BusinessRuleException("Esta revisão não possui agendamento cancelável.");
        }

        revisao.LojaId = null;
        revisao.DataAgendamento = null;
        revisao.Status = DateTime.Today > revisao.DataPrevista.Date.AddDays(15)
            ? StatusRevisaoMoto.Perdida
            : StatusRevisaoMoto.AguardandoAgendamento;

        await _context.SaveChangesAsync();
        return await ObterRevisaoMotoResponseAsync(revisaoMotoId);
    }

    public virtual Task<bool> TemAgendamentosPendentesAsync(int motoId)
    {
        return _context.RevisoesMotos.AnyAsync(rm =>
            rm.MotoId == motoId &&
            (rm.Status == StatusRevisaoMoto.AguardandoConfirmacao ||
             rm.Status == StatusRevisaoMoto.Agendada ||
             rm.Status == StatusRevisaoMoto.EmExecucao));
    }

    private async Task<RevisaoMoto> ObterRevisaoMotoDoClienteAsync(int revisaoMotoId, string userId)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.UsuarioId == userId);
        if (cliente == null)
        {
            throw new NotFoundException("Cliente não encontrado.");
        }

        var revisao = await _context.RevisoesMotos
            .Include(rm => rm.Moto)
            .Include(rm => rm.Loja)
            .FirstOrDefaultAsync(rm =>
                rm.Id == revisaoMotoId &&
                rm.Moto.ClienteId == cliente.Id &&
                rm.Moto.Ativo);

        if (revisao == null)
        {
            throw new NotFoundException("Revisão da moto não encontrada.");
        }

        return revisao;
    }

    private async Task<RevisaoMotoResponse> ObterRevisaoMotoResponseAsync(int revisaoMotoId)
    {
        var revisao = await _context.RevisoesMotos
            .Include(rm => rm.Loja)
            .Include(rm => rm.RevisaoPadrao)
                .ThenInclude(rp => rp.Servicos)
                    .ThenInclude(rps => rps.Servico)
            .Include(rm => rm.RevisaoPadrao)
                .ThenInclude(rp => rp.Pecas)
                    .ThenInclude(rpp => rpp.Peca)
            .AsSplitQuery()
            .FirstAsync(rm => rm.Id == revisaoMotoId);

        return revisao.Adapt<RevisaoMotoResponse>();
    }

    private async Task ValidarLojaEJanelaAgendamentoAsync(
        RevisaoMoto revisao,
        AgendamentoRevisaoRequest request)
    {
        var lojaExiste = await _context.Lojas.AnyAsync(l => l.Id == request.LojaId && l.Ativo);
        if (!lojaExiste)
        {
            throw new NotFoundException("Loja não encontrada.");
        }

        var hoje = DateTime.Today;
        var dataAgendamento = request.DataAgendamento.Date;
        var dataMinima = revisao.DataPrevista.Date.AddDays(-15);
        var dataLimite = revisao.DataPrevista.Date.AddDays(15);

        if (hoje < dataMinima || hoje > dataLimite)
        {
            throw new BusinessRuleException(
                $"Esta revisão só pode ser agendada entre {dataMinima:dd/MM/yyyy} e {dataLimite:dd/MM/yyyy}.");
        }

        if (dataAgendamento < hoje)
        {
            throw new BusinessRuleException("A data do agendamento não pode estar no passado.");
        }

        if (dataAgendamento < dataMinima || dataAgendamento > dataLimite)
        {
            throw new BusinessRuleException(
                $"A data do agendamento deve estar entre {dataMinima:dd/MM/yyyy} e {dataLimite:dd/MM/yyyy}.");
        }
    }

    public virtual async Task InativarMotoAsync(int id, string userId)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.UsuarioId == userId);
        if (cliente == null)
        {
            throw new NotFoundException("Cliente não encontrado.");
        }

        var moto = await _context.Motos
            .FirstOrDefaultAsync(m => m.Id == id && m.ClienteId == cliente.Id && m.Ativo);

        if (moto == null)
        {
            throw new NotFoundException("Moto não encontrada.");
        }

        if (await TemAgendamentosPendentesAsync(id))
        {
            throw new BusinessRuleException("Não é possível inativar uma moto com agendamentos pendentes.");
        }

        moto.Ativo = false;
        _context.Motos.Update(moto);
        await _context.SaveChangesAsync();
    }
}
