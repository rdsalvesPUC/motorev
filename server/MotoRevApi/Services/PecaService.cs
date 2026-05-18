using System.ComponentModel.DataAnnotations;
using Mapster;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
using MotoRevApi.Exceptions;

namespace MotoRevApi.Services;

public class PecaService
{
    private readonly AppDbContext _context;
    
    public PecaService(AppDbContext context)
    {
        _context = context;
    }
    
    public PecaResponse CadastrarPeca (PecaRequest request)
    {
        ValidarRequest(request);
        ValidarNomeDuplicado(request.Nome);

        var peca = request.Adapt<Model.Peca>();
        peca.Nome = request.Nome.Trim();
        peca.Status = StatusCadastro.Ativo;

        _context.Pecas.Add(peca);
        _context.SaveChanges();
        
        return peca.Adapt<PecaResponse>();
    }
    
    public PecaResponse ObterPeca(int id)
    {
        var peca = _context.Pecas.Find(id)?.Adapt<PecaResponse>();
        if (peca == null)
        {
            throw new NotFoundException($"Peça com ID {id} não encontrada.");
        }
        return peca;
    }
    
    public List<PecaResponse> ListarPecas()
    {
        return _context.Pecas.ToList().Adapt<List<PecaResponse>>();
    }

    private static void ValidarRequest(PecaRequest request)
    {
        var validationContext = new ValidationContext(request);
        Validator.ValidateObject(request, validationContext, true);
    }

    private void ValidarNomeDuplicado(string nome)
    {
        var nomeNormalizado = nome.Trim();
        var nomeJaExiste = _context.Pecas
            .AsEnumerable()
            .Any(peca => string.Equals(
                peca.Nome.Trim(),
                nomeNormalizado,
                StringComparison.OrdinalIgnoreCase));

        if (nomeJaExiste)
        {
            throw new DuplicateDataException($"Já existe uma peça cadastrada com o nome {nomeNormalizado}.");
        }
    }
}   
