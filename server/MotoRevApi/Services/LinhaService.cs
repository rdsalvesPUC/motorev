using Mapster;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;
using System.Collections.Generic;
using System.Linq;

namespace MotoRevApi.Services;

public class LinhaService
{
    private readonly AppDbContext _context;

    public LinhaService() { } // Construtor para Moq

    public LinhaService(AppDbContext context)
    {
        _context = context;
    }

    public virtual LinhaResponse CadastrarLinha(LinhaRequest request)
    {
        var nomeNormalizado = request.Nome.Trim();
        var existe = _context.Linhas.Any(l => l.Nome.ToLower() == nomeNormalizado.ToLower());
        if (existe)
        {
            throw new DuplicateDataException("Já existe uma linha cadastrada com este nome.");
        }

        var linha = request.Adapt<Linha>();
        linha.Nome = nomeNormalizado;
        _context.Linhas.Add(linha);
        _context.SaveChanges();

        return linha.Adapt<LinhaResponse>();
    }

    public virtual LinhaResponse ObterLinha(int id)
    {
        var linha = _context.Linhas.Find(id);
        if (linha == null)
        {
            throw new NotFoundException("Linha não encontrada.");
        }
        return linha.Adapt<LinhaResponse>();
    }

    public virtual List<LinhaResponse> ListarLinhas(bool apenasAtivos = true)
    {
        IQueryable<Linha> query = _context.Linhas;
        if (apenasAtivos)
        {
            query = query.Where(l => l.Ativo);
        }
        return query.ToList().Adapt<List<LinhaResponse>>();
    }

    public virtual LinhaResponse AtualizarLinha(int id, LinhaRequest request)
    {
        var linha = _context.Linhas.Find(id);
        if (linha == null)
        {
            throw new NotFoundException("Linha não encontrada.");
        }

        var nomeNormalizado = request.Nome.Trim();
        var existeOutra = _context.Linhas.Any(l => l.Id != id && l.Nome.ToLower() == nomeNormalizado.ToLower());
        if (existeOutra)
        {
            throw new DuplicateDataException("Já existe outra linha cadastrada com este nome.");
        }

        request.Adapt(linha);
        linha.Nome = nomeNormalizado;
        _context.SaveChanges();

        return linha.Adapt<LinhaResponse>();
    }

    public virtual LinhaResponse InativarLinha(int id)
    {
        var linha = _context.Linhas.Find(id);
        if (linha == null)
        {
            throw new NotFoundException("Linha não encontrada.");
        }

        var temModelos = _context.ModelosMotos.Any(m => m.LinhaId == id && m.Ativo);
        if (temModelos)
        {
            throw new BusinessRuleException("Não é possível desativar uma linha com modelos de motos vinculados.");
        }

        linha.Ativo = false;
        _context.SaveChanges();

        return linha.Adapt<LinhaResponse>();
    }

    public virtual LinhaResponse AlternarStatus(int id)
    {
        var linha = _context.Linhas.Find(id);
        if (linha == null)
        {
            throw new NotFoundException("Linha não encontrada.");
        }

        if (linha.Ativo)
        {
            var temModelos = _context.ModelosMotos.Any(m => m.LinhaId == id && m.Ativo);
            if (temModelos)
            {
                throw new BusinessRuleException("Não é possível desativar uma linha com modelos de motos vinculados.");
            }
        }

        linha.Ativo = !linha.Ativo;
        _context.SaveChanges();

        return linha.Adapt<LinhaResponse>();
    }
}
