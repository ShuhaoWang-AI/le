using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    public class OrganizationMapProfile : Profile
    {
        public OrganizationMapProfile()
        {
            CreateMap<Core.Domain.Organization, OrganizationDto>() //Map all properties in the destination where names are the same 
            .ForMember(d => d.OrganizationName, o => o.MapFrom(s => s.Name)) //Need to explicitly map b/c mismatched naming 
            .ForMember(d => d.State, o => o.MapFrom(s => s.Jurisdiction.Code))
            .ReverseMap();

            CreateMap<OrganizationType, OrganizationTypeDto>()
                .ReverseMap();


            CreateMap<Core.Domain.Organization, AuthorityDto>()
            .ForMember(d => d.OrganizationName, o => o.MapFrom(s => s.Name))
            .ForMember(d => d.EmailContactInfoName, o => o.Ignore())
            .ForMember(d => d.EmailContactInfoPhone, o => o.Ignore())
            .ForMember(d => d.EmailContactInfoEmailAddress, o => o.Ignore())
            .ForMember(d => d.RegulatoryProgramId, o => o.Ignore())
            .ForMember(d => d.State, o => o.Ignore())
            .ForMember(d => d.OrganizationRegulatoryProgramId, o => o.Ignore());

        }
    }

}
