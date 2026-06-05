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

    public PecaResponse AtualizarPeca(int id, PecaUpdateRequest request)
    {
        ValidarRequest(request);

        var peca = _context.Pecas.Find(id);
        if (peca == null)
        {
            throw new NotFoundException($"Peça com ID {id} não encontrada.");
        }

        ValidarCodigoDuplicado(request.Codigo, id);

        peca.Codigo = request.Codigo.Trim().ToUpperInvariant();
        peca.Nome = request.Nome.Trim();
        peca.Categoria = request.Categoria!.Value;
        peca.Preco = request.Preco!.Value;
        peca.Estoque = request.Estoque!.Value;
        peca.Status = request.Status!.Value;

        _context.SaveChanges();

        return MapToResponse(peca);
    }

    public PecaResponse AtualizarStatusPeca(int id, PecaStatusRequest request)
    {
        ValidarRequest(request);

        var peca = _context.Pecas.Find(id);
        if (peca == null)
        {
            throw new NotFoundException($"Peça com ID {id} não encontrada.");
        }

        peca.Status = request.Status!.Value;

        _context.SaveChanges();

        return MapToResponse(peca);
    }
    
    public List<PecaResponse> ListarPecas(StatusCadastro? status = null)
    {
        var query = _context.Pecas.AsQueryable();

        if (status is not null)
        {
            query = query.Where(peca => peca.Status == status);
        }

        return query
            .OrderBy(peca => peca.Nome)
            .Select(MapToResponse)
            .ToList();
    }

    private static void ValidarRequest(PecaRequest request)
    {
        var validationContext = new ValidationContext(request);
        Validator.ValidateObject(request, validationContext, true);
    }

    private static void ValidarRequest(PecaUpdateRequest request)
    {
        var validationContext = new ValidationContext(request);
        Validator.ValidateObject(request, validationContext, true);
    }

    private static void ValidarRequest(PecaStatusRequest request)
    {
        var validationContext = new ValidationContext(request);
        Validator.ValidateObject(request, validationContext, true);
    }

    private void ValidarCodigoDuplicado(string codigo, int? idIgnorado = null)
    {
        var codigoNormalizado = codigo.Trim().ToUpperInvariant();
        var codigoJaExiste = _context.Pecas
            .AsEnumerable()
            .Any(peca =>
                (!idIgnorado.HasValue || peca.Id != idIgnorado.Value) &&
                string.Equals(
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
