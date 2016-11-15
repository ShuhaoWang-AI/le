using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
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
             .ForMember(d => d.TemplateName, op => op.ResolveUsing(o => MapSettingType(o.SettingTemplate.Name)))
             .ForMember(d => d.OrgTypeName, o => o.MapFrom(s => MapOrganizationType(s.SettingTemplate.OrganizationType.Name)))
             .ForMember(d => d.DefaultValue, o => o.MapFrom(s => s.SettingTemplate.DefaultValue));

            CreateMap<OrganizationRegulatoryProgramSetting, SettingDto>()
               .ForMember(d => d.TemplateName, o => o.MapFrom(s => s.SettingTemplate.Name))
               .ForMember(d => d.OrgTypeName, o => o.MapFrom(s => MapOrganizationType(s.OrganizationRegulatoryProgram.Organization.OrganizationType.Name)))
               .ForMember(d => d.DefaultValue, o => o.MapFrom(s => s.SettingTemplate.DefaultValue));
        }

        public static SettingType MapSettingType(string settingType)
        {
            return (SettingType)Enum.Parse(typeof(SettingType), settingType);
        }
        public static OrganizationTypeName MapOrganizationType(string OrganizationTypeName)
        {
            return (OrganizationTypeName)Enum.Parse(typeof(OrganizationTypeName), OrganizationTypeName);
        }

    }
}
