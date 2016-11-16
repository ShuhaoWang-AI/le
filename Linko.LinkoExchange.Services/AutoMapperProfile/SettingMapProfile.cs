using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;
using System;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    public class SettingMapProfile : Profile
    {
        public SettingMapProfile()
        {
            CreateMap<OrganizationSetting, SettingDto>()
             .ForMember(d => d.TemplateName, o => o.ResolveUsing(s => MapSettingType(s.SettingTemplate.Name)))
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

        public static OrganizationTypeName MapOrganizationType(string organizationTypeName)
        {
            return (OrganizationTypeName)Enum.Parse(typeof(OrganizationTypeName), organizationTypeName);
        }

    }
}
