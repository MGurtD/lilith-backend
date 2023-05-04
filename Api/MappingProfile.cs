using Api.Mapping.Dtos;
using Application.Dtos;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Api
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
            CreateMap<IdentityRole, RoleDto>().ReverseMap();
            CreateMap<Role, RoleDto>().ReverseMap();
            CreateMap<User, UserDto>().ReverseMap();
        }
    }
}
