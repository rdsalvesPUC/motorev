using Mapster;
using MotoRevApi.Dto.Response;
using MotoRevApi.Model;

namespace MotoRevApi.Profiles;

public static class MapsterConfig
{
    public static void RegisterMapsterConfiguration()
    {
        TypeAdapterConfig<Concessionaria, ConcessionariaResponse>
            .NewConfig()
            .Map(dest => dest.Enderecos, src => src.Enderecos);
    }
}
