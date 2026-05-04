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
            .Map(dest => dest.Email, src => src.Usuario.Email) // Mapeia o e-mail do Identity para o DTO
            .Map(dest => dest.Enderecos, src => src.Enderecos);
    }
}
