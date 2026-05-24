using Mapster;
using MotoRevApi.Dto.Response;
using MotoRevApi.Model;

namespace MotoRevApi.Profiles;

public static class MapsterConfig
{
    public static void RegisterMapsterConfiguration()
    {
        TypeAdapterConfig<Moto, MotoResponse>.NewConfig()
            .Map(dest => dest.NomeModelo, src => src.ModeloMoto.NomeModelo)
            .Map(dest => dest.Marca, src => src.ModeloMoto.Marca)
            .Map(dest => dest.NomeConcessionaria, src => src.Concessionaria != null ? src.Concessionaria.Nome : null);
    }
}
