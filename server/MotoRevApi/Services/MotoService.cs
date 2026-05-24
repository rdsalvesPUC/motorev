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
}