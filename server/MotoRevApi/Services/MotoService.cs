using AutoMapper;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;

namespace MotoRevApi.Services;

public class MotoService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    
    public MotoService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public MotoResponse CadastrarMoto (MotoRequest request)
    {
        var moto = _mapper.Map<Model.Moto>(request);
        _context.Motos.Add(moto);
        _context.SaveChanges();
        
        return _mapper.Map<MotoResponse>(moto);
    }
    
    public MotoResponse ObterMoto(int id)
    {
        return _mapper.Map<MotoResponse>(_context.Motos.Find(id));
    }
    
    public List<MotoResponse> ListarMotos()
    {
        return _mapper.Map<List<MotoResponse>>(_context.Motos.ToList());
    }
}