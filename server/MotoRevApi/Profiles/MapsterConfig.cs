using Mapster;
using MotoRevApi.Dto.Response;
using MotoRevApi.Model;

namespace MotoRevApi.Profiles;

public static class MapsterConfig
{
    private static bool _isRegistered = false;
    private static readonly object _lock = new object();

    public static void RegisterMapsterConfiguration()
    {
        lock (_lock)
        {
            if (_isRegistered) return;
            _isRegistered = true;

            TypeAdapterConfig<Moto, MotoResponse>.NewConfig()
                .Map(dest => dest.NomeModelo, src => src.ModeloMoto.NomeModelo)
                .Map(dest => dest.Marca, src => src.ModeloMoto.Marca)
                .Map(dest => dest.NomeConcessionaria, src => src.Concessionaria != null ? src.Concessionaria.Nome : null)
                .Map(dest => dest.Linha, src => src.ModeloMoto.Linha != null ? src.ModeloMoto.Linha.Nome : string.Empty)
                .Map(dest => dest.Cilindrada, src => src.ModeloMoto.Cilindrada ?? string.Empty)
                .Map(dest => dest.Ano, src => src.ModeloMoto.Ano ?? 0);
          
          TypeAdapterConfig<Moto, MotoResponse>.NewConfig()
            .Map(dest => dest.NomeModelo, src => src.ModeloMoto.NomeModelo)
            .Map(dest => dest.Marca, src => src.ModeloMoto.Marca)
            .Map(dest => dest.NomeConcessionaria, src => src.Concessionaria != null ? src.Concessionaria.Nome : null)
            .Map(dest => dest.Linha, src => src.ModeloMoto.Linha != null ? src.ModeloMoto.Linha.Nome : string.Empty)
            .Map(dest => dest.Cilindrada, src => src.ModeloMoto.Cilindrada ?? string.Empty)
            .Map(dest => dest.Ano, src => src.ModeloMoto.Ano ?? 0);

        TypeAdapterConfig<RevisaoPadrao, RevisaoPadraoResponse>
            .NewConfig()
            .Map(dest => dest.NomeModeloMoto, src => src.ModeloMoto != null ? src.ModeloMoto.NomeModelo : string.Empty)
            .Map(dest => dest.Servicos, src => src.Servicos.Select(s => s.Servico))
            .Map(dest => dest.Pecas, src => src.Pecas.Select(p => new RevisaoPadraoPecaResponse(
                p.Peca.Id,
                p.Peca.Codigo,
                p.Peca.Nome,
                p.Peca.Categoria.ToString(),
                p.Peca.Preco,
                p.Peca.Estoque,
                p.Peca.Status.ToString(),
                p.Quantidade
            )));
        }
    }
}
