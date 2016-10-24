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
    public class UserMapProfile : Profile
    {
        public UserMapProfile()
        {
            CreateMap<UserProfile, UserDto>()
               //.ForAllMembers(opts => opts.Ignore()); //We should be explicitly ignoring each property we don't map to prevent silent failures!
               .ForMember(d => d.OrgRegProgUserId, o => o.Ignore())
               .ForMember(d => d.UserName, o => o.Ignore())
               .ForMember(d => d.AuthorityId, o => o.Ignore())
               .ForMember(d => d.IndustryId, o => o.Ignore())
               .ForMember(d => d.IndustryName, o => o.Ignore())
               .ForMember(d => d.Password, o => o.Ignore())
               .ForMember(d=>d.CityName,o=>o.Ignore())
               .ForMember(d => d.AddressLine1, o => o.Ignore())
               .ForMember(d => d.AddressLine2, o => o.Ignore())
               .ForMember(d => d.ZipCode, o => o.Ignore())
               .ForMember(d => d.IsEnabled, o => o.Ignore());
                

        }
    }
}
