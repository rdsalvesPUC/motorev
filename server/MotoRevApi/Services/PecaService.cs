using System.ComponentModel.DataAnnotations;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Enums;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;

namespace MotoRevApi.Services;

public class PecaService
{
    private readonly AppDbContext _context;
    
    public PecaService(AppDbContext context)
    {
        _context = context;
    }
    
    public PecaResponse CadastrarPeca(PecaRequest request)
    {
        ValidarRequest(request);
        ValidarCodigoDuplicado(request.Codigo);

        var peca = new Peca
        {
            Codigo = request.Codigo.Trim().ToUpperInvariant(),
            Nome = request.Nome.Trim(),
            Categoria = request.Categoria!.Value,
            Preco = request.Preco!.Value,
            Estoque = request.Estoque!.Value,
            Status = StatusCadastro.Ativo
        };

        _context.Pecas.Add(peca);
        _context.SaveChanges();
        
        return MapToResponse(peca);
    }
    
    public PecaResponse ObterPeca(int id)
    {
        var peca = _context.Pecas.Find(id);
        if (peca == null)
        {
            throw new NotFoundException($"Peça com ID {id} não encontrada.");
        }

        return MapToResponse(peca);
    }
    
    public List<PecaResponse> ListarPecas()
    {
        return _context.Pecas
            .OrderBy(peca => peca.Nome)
            .Select(MapToResponse)
            .ToList();
    }

    private static void ValidarRequest(PecaRequest request)
    {
        var validationContext = new ValidationContext(request);
        Validator.ValidateObject(request, validationContext, true);
    }

    private void ValidarCodigoDuplicado(string codigo)
    {
        var codigoNormalizado = codigo.Trim().ToUpperInvariant();
        var codigoJaExiste = _context.Pecas
            .AsEnumerable()
            .Any(peca => string.Equals(
                peca.Codigo.Trim(),
                codigoNormalizado,
                StringComparison.OrdinalIgnoreCase));

        if (codigoJaExiste)
        {
            throw new DuplicateDataException($"Já existe uma peça cadastrada com o código {codigoNormalizado}.");
        }
    }

    private static PecaResponse MapToResponse(Peca peca)
    {
        return new PecaResponse(
            peca.Id,
            peca.Codigo,
            peca.Nome,
            ObterNomeCategoria(peca.Categoria),
            peca.Preco,
            peca.Estoque,
            peca.Status.ToString());
    }

    private static string ObterNomeCategoria(CategoriaPeca categoria)
    {
        return categoria switch
        {
            CategoriaPeca.Transmissao => "Transmissão",
            CategoriaPeca.Eletrica => "Elétrica",
            _ => categoria.ToString()
        };
    }
}   
