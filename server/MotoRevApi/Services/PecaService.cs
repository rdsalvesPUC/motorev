using Mapster;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
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
        var peca = request.Adapt<Model.Peca>();
        _context.Pecas.Add(peca);
        _context.SaveChanges();
        
        return peca.Adapt<PecaResponse>();
    }
}   