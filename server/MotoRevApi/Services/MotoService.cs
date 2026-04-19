using Mapster;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Exceptions;

namespace MotoRevApi.Services;

public class MotoService
{
    private readonly AppDbContext _context;
    
    public MotoService(AppDbContext context)
    {
        _context = context;
    }
    
    public MotoResponse CadastrarMoto (MotoRequest request)
    {
        var moto = request.Adapt<Model.Moto>();
        _context.Motos.Add(moto);
        _context.SaveChanges();
        
        return moto.Adapt<MotoResponse>();
    }
    
    public MotoResponse ObterMoto(int id)
    {
        var moto = _context.Motos.Find(id)?.Adapt<MotoResponse>();
        if (moto == null)
        {
            throw new NotFoundException($"Moto com ID {id} não encontrada.");
        }
        return moto;
    }
    
    public List<MotoResponse> ListarMotos()
    {
        return _context.Motos.ToList().Adapt<List<MotoResponse>>();
    }
}