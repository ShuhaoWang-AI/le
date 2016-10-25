using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    public class PermissionGroupMapProfile : Profile
    {
        public PermissionGroupMapProfile()
        {
            CreateMap<PermissionGroupDto, PermissionGroup>()
                .ForMember(i => i.OrganizationRegulatoryProgram, o => o.Ignore())
                .ReverseMap(); 
        }
    }
}
