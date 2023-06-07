using Api.Mapping.Dtos;
using Api.Mapping.Dtos.Authentication;
using Application.Dtos;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;

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
            CreateMap<Role, RoleDto>().ReverseMap();
            CreateMap<User, UserRegisterRequest>().ReverseMap();
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<SupplierType, SupplierTypeDto>().ReverseMap();
            CreateMap<CustomerType, CustomerTypeDto>().ReverseMap();
        }
    }
}
