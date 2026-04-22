using Mapster;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Model;

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
        var modeloMoto = request.Adapt<ModeloMoto>();
        _context.ModelosMotos.Add(modeloMoto);
        _context.SaveChanges();
        
        return modeloMoto.Adapt<ModeloMotoResponse>();
    }
    
    public virtual ModeloMotoResponse? ObterModeloMoto(int id)
    {
        var modeloMoto = _context.ModelosMotos.Find(id);
        if (modeloMoto == null)
        {
            return null;
        }
        return modeloMoto.Adapt<ModeloMotoResponse>();
    }
    
    public virtual List<ModeloMotoResponse> ListarModelosMotos()
    {
        return _context.ModelosMotos.ToList().Adapt<List<ModeloMotoResponse>>();
    }
    
    public virtual ModeloMotoResponse? AtualizarModeloMoto(int id, ModeloMotoRequest request)
    {
        var modeloMoto = _context.ModelosMotos.Find(id);
        if (modeloMoto == null)
        {
            return null;
        }
        
        // Mapeia os dados do request para a entidade que já existe e está sendo rastreada pelo EF
        request.Adapt(modeloMoto);
        
        _context.SaveChanges();
        
        return modeloMoto.Adapt<ModeloMotoResponse>();
    }
    
    public virtual ModeloMotoResponse? AlternarStatus(int id)
    {
        var modeloMoto = _context.ModelosMotos.Find(id);
        if (modeloMoto == null)
        {
            return null;
        }

        modeloMoto.Ativo = !modeloMoto.Ativo; // Inverte o status atual
        _context.SaveChanges();

        return modeloMoto.Adapt<ModeloMotoResponse>();
    }
}
