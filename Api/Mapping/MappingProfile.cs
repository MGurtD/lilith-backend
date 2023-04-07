using Api.Mapping.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Api.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Enterprise, EnterpriseDto>()
                .ForMember(dest => dest.Sites,
                   opts => opts.MapFrom(src => src.Sites.Select(ci => ci).ToList())
                   )
                .ReverseMap();
            CreateMap<Site, SiteDto>().ReverseMap();
        }
    }
}
