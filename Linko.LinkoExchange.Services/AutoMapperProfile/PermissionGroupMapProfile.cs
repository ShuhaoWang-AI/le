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
                .ForMember(d => d.OrganizationRegulatoryProgram, o => o.Ignore())
                .ForMember(d => d.LastModifierUserId, o => o.Ignore())
                .ForMember(d => d.OrganizationRegulatoryProgramUsers, o => o.Ignore())
                .ForMember(d => d.PermissionGroupPermissions, o => o.Ignore())
                .ReverseMap(); 
        }
    }
}
