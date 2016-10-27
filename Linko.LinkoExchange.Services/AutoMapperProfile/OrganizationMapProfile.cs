﻿using AutoMapper;
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
            CreateMap<Core.Domain.Organization, OrganizationDto>() //Map all properties in the destination where names are the same 
            .ForMember(d => d.OrganizationName, o => o.MapFrom(s => s.Name)) //Need to explicitly map b/c mismatched naming 
            .ForMember(i => i.State, o => o.Ignore())
            .ReverseMap();

            CreateMap<OrganizationType, OrganizationTypeDto>()
                .ReverseMap();


            CreateMap<Core.Domain.Organization, AuthorityDto>()
            .ForMember(d => d.OrganizationName, o => o.MapFrom(s => s.Name))
            .ForMember(i => i.EmailContactInfoName, o => o.Ignore())
            .ForMember(i => i.EmailContactInfoPhone, o => o.Ignore())
            .ForMember(i => i.EmailContactInfoEmailAddress, o => o.Ignore())
            .ForMember(i => i.RegulatoryProgramId, o => o.Ignore())
            .ForMember(i => i.OrganizationRegulatoryProgramId, o => o.Ignore());

        }
    }

}
