using AutoMapper;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Model;

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
        var modeloMoto = _mapper.Map<ModeloMoto>(request);
        _context.ModelosMotos.Add(modeloMoto);
        _context.SaveChanges();
        
        return _mapper.Map<ModeloMotoResponse>(modeloMoto);
    }
    
    public ModeloMotoResponse? ObterModeloMoto(int id)
    {
        var modeloMoto = _context.ModelosMotos.Find(id);
        return _mapper.Map<ModeloMotoResponse>(modeloMoto);
    }
    
    public List<ModeloMotoResponse> ListarModelosMotos()
    {
        return _mapper.Map<List<ModeloMotoResponse>>(_context.ModelosMotos.ToList());
    }
    
    public ModeloMotoResponse? AtualizarModeloMoto(int id, ModeloMotoRequest request)
    {
        var modeloMoto = _context.ModelosMotos.Find(id);
        if (modeloMoto == null)
        {
            return null;
        }
        
        // Mapeia os dados do request para a entidade que já existe e está sendo rastreada pelo EF
        _mapper.Map(request, modeloMoto);
        
        _context.SaveChanges();
        
        return _mapper.Map<ModeloMotoResponse>(modeloMoto);
    }
}
