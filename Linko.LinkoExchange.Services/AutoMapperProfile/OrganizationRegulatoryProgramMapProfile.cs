using AutoMapper;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{

    public class OrganizationRegulatoryProgramMapProfile : Profile
    {
        public OrganizationRegulatoryProgramMapProfile()
        {
            CreateMap<Core.Domain.OrganizationRegulatoryProgram, OrganizationRegulatoryProgramDto>()
                .ForMember(i => i.RegulatoryProgramDto, o => o.MapFrom(s => s.RegulatoryProgram))
                .ForMember(i => i.OrganizationDto, o => o.MapFrom(s => s.Organization))
                .ForMember(i => i.HasSignatory, o => o.Ignore())
                .ForMember(i => i.HasAdmin, o => o.Ignore())
            .ReverseMap();

        }
    }
}
