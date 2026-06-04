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

        TypeAdapterConfig<RevisaoPadrao, RevisaoPadraoResponse>
            .NewConfig()
            .Map(dest => dest.NomeModeloMoto, src => src.ModeloMoto != null ? src.ModeloMoto.NomeModelo : string.Empty)
            .Map(dest => dest.Servicos, src => src.Servicos.Select(s => s.Servico));
    }
}
