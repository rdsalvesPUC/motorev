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
        
        // Responses
        CreateMap<Moto, MotoResponse>();
    }
}