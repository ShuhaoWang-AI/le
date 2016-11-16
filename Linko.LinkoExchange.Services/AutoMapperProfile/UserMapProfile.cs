using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    public class UserMapProfile : Profile
    {
        public UserMapProfile()
        {
            CreateMap<UserProfile, UserDto>()
               .ForMember(d => d.Password, o => o.Ignore())
               .ForMember(d => d.AgreeTermsAndConditions, o => o.Ignore());

            CreateMap<UserDto, UserProfile>()
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
                .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
                .ForMember(d => d.TitleRole, o => o.MapFrom(s => s.TitleRole))
                .ForMember(d => d.BusinessName, o => o.MapFrom(s => s.BusinessName))
                .ForMember(d => d.AddressLine1, o => o.MapFrom(s => s.AddressLine1))
                .ForMember(d => d.AddressLine2, o => o.MapFrom(s => s.AddressLine2))
                .ForMember(d => d.CityName, o => o.MapFrom(s => s.CityName))
                .ForMember(d => d.ZipCode, o => o.MapFrom(s => s.ZipCode))
                .ForAllOtherMembers(o => o.Ignore());

        }
    }
}
