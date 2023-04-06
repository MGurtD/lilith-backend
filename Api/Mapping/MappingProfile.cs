using Api.Mapping.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Api.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Enterprise, EnterpriseDto>().ReverseMap();
        }
    }
}
