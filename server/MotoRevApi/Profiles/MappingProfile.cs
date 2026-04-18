using AutoMapper;
using MotoRevApi.Dto.Request;
using MotoRevApi.Dto.Response;
using MotoRevApi.Model;

namespace MotoRevApi.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Requests
        CreateMap<MotoRequest, Moto>();
        CreateMap<RegisterClienteRequest, Cliente>();
        CreateMap<RegisterConcessionariaRequest, Concessionaria>();
        
        CreateMap<UpdateClienteRequest, Cliente>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
        CreateMap<UpdateConcessionariaRequest, Concessionaria>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        
        // Responses
        CreateMap<Moto, MotoResponse>();
        CreateMap<Cliente, ClienteResponse>();
        CreateMap<Concessionaria, ConcessionariaResponse>();
    }
}
