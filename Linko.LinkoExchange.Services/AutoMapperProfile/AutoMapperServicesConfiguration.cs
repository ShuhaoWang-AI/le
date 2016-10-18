using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    /// <summary>
    /// These mapping profiles are loaded individually 
    /// where Unity is configured
    /// </summary>
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
               .ForMember(d => d.IsEnabled, o => o.Ignore());

            CreateMap<Organization, OrganizationDto>() //Map all properties in the destination where names are the same 
            .ForMember(d => d.OrganizationName, o => o.MapFrom(s => s.Name)) //Need to explicitly map b/c mismatched naming
            .ReverseMap();
        }
    } 
}
