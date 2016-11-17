using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    public class OrganizationRegulatoryProgramUserMapProfile : Profile
    {
        public OrganizationRegulatoryProgramUserMapProfile()
        {
            CreateMap<OrganizationRegulatoryProgramUserDto, OrganizationRegulatoryProgramUser>()
                .ForAllMembers(o => o.Ignore()); //We should be explicitly ignoring each property we don't map to prevent silent failures!

            CreateMap<OrganizationRegulatoryProgramUser, OrganizationRegulatoryProgramUserDto>()
                .ForMember(d => d.UserProfileDto, o => o.Ignore())
                .ForMember(d => d.OrganizationRegulatoryProgramDto, o => o.MapFrom(s => s.OrganizationRegulatoryProgram)); 

            CreateMap<OrganizationRegulatoryProgramUser, UserDto>()

                .ForMember(d => d.FirstName, o => o.Ignore())
                .ForMember(d => d.LastName, o => o.Ignore())
                .ForMember(d => d.UserName, o => o.Ignore())
                .ForMember(d => d.Email, o => o.Ignore())
                .ForMember(d => d.TitleRole, o => o.Ignore())
                .ForMember(d => d.Password, o => o.Ignore())
                .ForMember(d => d.CityName, o => o.Ignore())
               .ForMember(d => d.AddressLine1, o => o.Ignore())
               .ForMember(d => d.AddressLine2, o => o.Ignore())
               .ForMember(d => d.ZipCode, o => o.Ignore())
               .ForMember(d => d.PasswordHash, o => o.Ignore())

               .ForMember(d => d.BusinessName, o => o.Ignore())
               .ForMember(d => d.JurisdictionId, o => o.Ignore())
               .ForMember(d => d.PhoneExt, o => o.Ignore())
               .ForMember(d => d.IsAccountLocked, o => o.Ignore())
               .ForMember(d => d.IsAccountResetRequired, o => o.Ignore())
               .ForMember(d => d.IsIdentityProofed, o => o.Ignore())
               .ForMember(d => d.IsInternalAccount, o => o.Ignore())
               .ForMember(d => d.EmailConfirmed, o => o.Ignore())
               .ForMember(d => d.PhoneNumber, o => o.Ignore())
               .ForMember(d => d.LockoutEndDateUtc, o => o.Ignore())
               .ForMember(d => d.LockoutEnabled, o => o.Ignore())
               .ForMember(d => d.OldEmailAddress, o => o.Ignore())
               .ForMember(d=>d.AgreeTermsAndConditions, o => o.Ignore())
                ;
        }
    }
}