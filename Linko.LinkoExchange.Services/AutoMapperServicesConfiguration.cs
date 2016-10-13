using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services
{
    //public static class AutoMapperServicesConfiguration
    //{
    //    public static void Configure()
    //    {
    //        Mapper.Initialize(cfg =>
    //        {
    //            cfg.AddProfile(new UserMapProfile());
    //        });

    //        Mapper.AssertConfigurationIsValid();
    //    }
    //}

    //These profiles are now loaded when Unity is configured
    public class UserMapProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<UserProfile, UserDto>()
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
                .ForAllOtherMembers(opts => opts.Ignore()); //We should be explicitly ignoring each property we don't map to prevent silent failures!
            CreateMap<OrganizationRegulatoryProgramUser, UserDto>()
                .ForAllOtherMembers(opts => opts.Ignore());
        }
    }
}
