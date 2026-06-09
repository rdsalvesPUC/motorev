using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using System.Collections.Generic;
using System.Linq;

namespace MotoRevApi.Services;

public class ModeloMotoService
{
    private readonly AppDbContext _context;

    public ModeloMotoService(AppDbContext context)
    {
        _context = context;
    }
    
    // Adicionado virtual para permitir o mock pelo Moq
    public virtual ModeloMotoResponse CadastrarModeloMoto(ModeloMotoRequest request)
    {
        ValidarLinhaAtiva(request.LinhaId);
        ValidarNomeDuplicado(request.NomeModelo);

        var modeloMoto = request.Adapt<ModeloMoto>();
        modeloMoto.NomeModelo = request.NomeModelo.Trim();
        modeloMoto.Marca = request.Marca.Trim();
        modeloMoto.Cilindrada = request.Cilindrada?.Trim();
        _context.ModelosMotos.Add(modeloMoto);
        try
        {
            _context.SaveChanges();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateDataException($"Já existe um modelo de moto ativo com o nome '{request.NomeModelo}'.");
        }
        
        return modeloMoto.Adapt<ModeloMotoResponse>();
    }
    
    public virtual ModeloMotoResponse ObterModeloMoto(int id)
    {
        var modeloMoto = _context.ModelosMotos.Find(id);
        if (modeloMoto == null)
        {
            throw new NotFoundException("Modelo de moto não encontrado.");
        }
        return modeloMoto.Adapt<ModeloMotoResponse>();
    }
    
    public virtual List<ModeloMotoResponse> ListarModelosMotos(bool apenasAtivos = true)
    {
        var query = _context.ModelosMotos.AsQueryable();

        if (apenasAtivos)
        {
            query = query.Where(modelo => modelo.Ativo);
        }

        return query
            .OrderBy(modelo => modelo.Marca)
            .ThenBy(modelo => modelo.NomeModelo)
            .ToList()
            .Adapt<List<ModeloMotoResponse>>();
    }

    public virtual List<ModeloMotoResponse> ListarCatalogoModelosMotos(bool? ativo = null)
    {
        var query = _context.ModelosMotos.AsQueryable();

        if (ativo.HasValue)
        {
            query = query.Where(modelo => modelo.Ativo == ativo.Value);
        }

        return query
            .OrderByDescending(modelo => modelo.Ativo)
            .ThenBy(modelo => modelo.Marca)
            .ThenBy(modelo => modelo.NomeModelo)
            .ToList()
            .Adapt<List<ModeloMotoResponse>>();
    }
    
    public virtual ModeloMotoResponse AtualizarModeloMoto(int id, ModeloMotoRequest request)
    {
        var modeloMoto = _context.ModelosMotos.Find(id);
        if (modeloMoto == null)
        {
            throw new NotFoundException("Modelo de moto não encontrado.");
        }

        ValidarLinhaAtiva(request.LinhaId);
        ValidarNomeDuplicado(request.NomeModelo, id);
        
        // Mapeia os dados do request para a entidade que já existe e está sendo rastreada pelo EF
        request.Adapt(modeloMoto);
        modeloMoto.NomeModelo = request.NomeModelo.Trim();
        modeloMoto.Marca = request.Marca.Trim();
        modeloMoto.Cilindrada = request.Cilindrada?.Trim();
        
        try
        {
            _context.SaveChanges();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateDataException($"Já existe outro modelo de moto ativo com o nome '{request.NomeModelo}'.");
        }
        
        return modeloMoto.Adapt<ModeloMotoResponse>();
    }
    
    public virtual ModeloMotoResponse AlternarStatus(int id)
    {
        var modeloMoto = _context.ModelosMotos.Find(id);
        if (modeloMoto == null)
        {
            throw new NotFoundException("Modelo de moto não encontrado.");
        }

        if (!modeloMoto.Ativo)
        {
            ValidarNomeDuplicado(modeloMoto.NomeModelo, id);
        }

        modeloMoto.Ativo = !modeloMoto.Ativo; // Inverte o status atual
        try
        {
            _context.SaveChanges();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateDataException($"Já existe outro modelo de moto ativo com o nome '{modeloMoto.NomeModelo}'.");
        }

        return modeloMoto.Adapt<ModeloMotoResponse>();
    }

    private void ValidarLinhaAtiva(int linhaId)
    {
        var linhaExiste = _context.Linhas.Any(l => l.Id == linhaId && l.Ativo);
        if (!linhaExiste)
        {
            throw new NotFoundException("Linha informada não encontrada.");
        }
    }

    private void ValidarNomeDuplicado(string nomeModelo, int? idIgnorado = null)
    {
        var nomeNormalizado = nomeModelo.Trim();
        var modeloDuplicado = _context.ModelosMotos.Any(modelo =>
            modelo.Ativo &&
            (!idIgnorado.HasValue || modelo.Id != idIgnorado.Value) &&
            modelo.NomeModelo.ToLower() == nomeNormalizado.ToLower());

        if (modeloDuplicado)
        {
            var mensagem = idIgnorado.HasValue
                ? $"Já existe outro modelo de moto ativo com o nome '{nomeModelo}'."
                : $"Já existe um modelo de moto ativo com o nome '{nomeModelo}'.";
            throw new DuplicateDataException(mensagem);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
    }
}
