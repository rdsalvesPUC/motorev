using AutoMapper;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;

namespace MotoRevApi.Services;

public class ModeloMotoService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    
    public ModeloMotoService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public ModeloMotoResponse CadastrarModeloMoto(ModeloMotoRequest request)
    {
        var modeloMoto = _mapper.Map<Model.ModeloMoto>(request);
        _context.ModelosMotos.Add(modeloMoto);
        _context.SaveChanges();
        
        return _mapper.Map<ModeloMotoResponse>(modeloMoto);
    }
    
    public ModeloMotoResponse? ObterModeloMoto(int id)
    {
        var modeloMoto = _context.ModelosMotos.Find(id);
        if (modeloMoto == null)
        {
            return null;
        }
        return _mapper.Map<ModeloMotoResponse>(modeloMoto);
    }
    
    public List<ModeloMotoResponse> ListarModelosMotos()
    {
        return _mapper.Map<List<ModeloMotoResponse>>(_context.ModelosMotos.ToList());
    }
}
