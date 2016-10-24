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
               .ForMember(d => d.Password, o => o.Ignore()); 
        }
    }
}
