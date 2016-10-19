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
    public class SettingMapProfile : Profile
    {
        public SettingMapProfile()
        {
            CreateMap<OrganizationSetting, SettingDto>()
               .ForMember(d => d.Name, o => o.MapFrom(s => s.SettingTemplate.Name));

            CreateMap<OrganizationRegulatoryProgramSetting, SettingDto>()
               .ForMember(d => d.Name, o => o.MapFrom(s => s.SettingTemplate.Name));
        }
    }
}
