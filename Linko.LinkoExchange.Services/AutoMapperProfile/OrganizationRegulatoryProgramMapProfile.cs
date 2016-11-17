using AutoMapper;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{

    public class OrganizationRegulatoryProgramMapProfile : Profile
    {
        public OrganizationRegulatoryProgramMapProfile()
        {
            CreateMap<Core.Domain.OrganizationRegulatoryProgram, OrganizationRegulatoryProgramDto>()
                .ForMember(d => d.RegulatoryProgramDto, o => o.MapFrom(s => s.RegulatoryProgram))
                .ForMember(d => d.OrganizationDto, o => o.MapFrom(s => s.Organization))
                .ForMember(d => d.HasSignatory, o => o.Ignore())
                .ForMember(d => d.HasAdmin, o => o.Ignore())
            .ReverseMap()
            .ForMember(d => d.PermissionGroups, o => o.Ignore())
            .ForMember(d => d.OrganizationRegulatoryProgramUsers, o => o.Ignore())
            .ForMember(d => d.OrganizationRegulatoryProgramSettings, o => o.Ignore())
            .ForMember(d=>d.OrganizationRegulatoryProgramModules, o=>o.Ignore());

        }
    }
}
