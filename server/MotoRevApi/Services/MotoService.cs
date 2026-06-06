using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
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
            .Include(m => m.Concessionaria)
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

        // Validar a existência opcional da Concessionária
        if (request.ConcessionariaId.HasValue)
        {
            var concessionariaExiste = await _context.Concessionarias
                .AnyAsync(c => c.Id == request.ConcessionariaId.Value);
            if (!concessionariaExiste)
            {
                throw new NotFoundException("Concessionária não encontrada.");
            }
        }

        // Buscar o cliente a partir do User ID
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.UsuarioId == userId);
        if (cliente == null)
        {
            throw new NotFoundException("Cliente não encontrado.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var moto = request.Adapt<Moto>();
            moto.Placa = placaUpper;
            moto.Chassi = chassiUpper;
            moto.ClienteId = cliente.Id;
            moto.Ativo = true;

            _context.Motos.Add(moto);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Retorna o response carregando os dados do modelo e concessionária associados
            var savedMoto = await _context.Motos
                .Include(m => m.ModeloMoto)
                .Include(m => m.Concessionaria)
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
            .Include(m => m.Concessionaria)
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
                .Include(m => m.Concessionaria)
                .FirstAsync(m => m.Id == moto.Id);

            return updatedMoto.Adapt<MotoResponse>();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public virtual Task<bool> TemAgendamentosPendentesAsync(int motoId)
    {
        // Como o fluxo de agendamentos ainda não foi implementado,
        // retorna false por padrão.
        return Task.FromResult(false);
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