using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    public class OrganizationMapProfile : Profile
    {
        public OrganizationMapProfile()
        {
            CreateMap<Core.Domain.Organization, OrganizationDto>()
            .ForMember(d => d.OrganizationName, o => o.MapFrom(s => s.Name))
            .ForMember(d => d.OrganizationType, o => o.Ignore());

            CreateMap<OrganizationDto, Core.Domain.Organization>()
            .ForMember(d => d.Name, o => o.MapFrom(s => s.OrganizationName))
            .ForMember(d => d.OrganizationType, o => o.Ignore())
            .ForMember(d => d.OrganizationSettings, o => o.Ignore()); 

            CreateMap<OrganizationType, OrganizationTypeDto>()
                .ReverseMap();


        }
    }
}
