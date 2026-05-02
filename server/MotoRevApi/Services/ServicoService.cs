using Mapster;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Exceptions;
using MotoRevApi.Model;

namespace MotoRevApi.Services;

public class ServicoService
{
    private readonly AppDbContext _context;

    public ServicoService() { } // Construtor para Moq

    public ServicoService(AppDbContext context)
    {
        _context = context;
    }

    public virtual async Task<ServicoResponse> CreateAsync(ServicoRequest request)
    {
        // Verificar duplicidade: mesmo nome e categoria globalmente
        var servicoDuplicado = await _context.Servicos
            .AsNoTracking()
            .AnyAsync(s => s.Nome == request.Nome
                && s.Categoria == request.Categoria);

        if (servicoDuplicado)
        {
            throw new DuplicateDataException(
                $"Já existe um serviço com o nome '{request.Nome}' e categoria '{request.Categoria}'.");
        }

        var servico = request.Adapt<Servico>();

        _context.Servicos.Add(servico);
        await _context.SaveChangesAsync();

        return servico.Adapt<ServicoResponse>();
    }
}
