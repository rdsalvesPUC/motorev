using Mapster;
using MotoRevApi.Dto.Request;
using MotoRevApi.Model;

namespace MotoRevApi.Profiles;

public static class MapsterConfig
{
    public static void RegisterMapsterConfiguration()
    {
        TypeAdapterConfig<UpdateClienteRequest, Cliente>
            .NewConfig()
            .IgnoreNullValues(true);
            
        TypeAdapterConfig<UpdateConcessionariaRequest, Concessionaria>
            .NewConfig()
            .IgnoreNullValues(true);
    }
}
