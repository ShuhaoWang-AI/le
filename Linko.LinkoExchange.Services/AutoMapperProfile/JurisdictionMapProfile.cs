using AutoMapper;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    public class JurisdictionMapProfile : Profile
    {
        public JurisdictionMapProfile()
        {
            CreateMap<Core.Domain.Jurisdiction, JurisdictionDto>()
            .ReverseMap();
        }
    }

}
