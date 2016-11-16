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
    public class TimeZoneMapProfile : Profile
    {
        public TimeZoneMapProfile()
        {
            CreateMap<Core.Domain.TimeZone, TimeZoneDto>()
               .ReverseMap();
        }
    }
}
