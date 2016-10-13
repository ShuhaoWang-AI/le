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
    public static class AutoMapperServicesConfiguration
    {
        public static void Configure()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile(new UserMapProfile());
            });

            Mapper.AssertConfigurationIsValid();
        }
    }

    public class UserMapProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<UserProfile, UserDto>();
            CreateMap<OrganizationRegulatoryProgramUser, UserDto>();
        }
    }
}
