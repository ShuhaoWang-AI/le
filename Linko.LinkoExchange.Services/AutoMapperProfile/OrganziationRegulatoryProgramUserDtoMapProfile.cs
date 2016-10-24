using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    public class OrganizationRegulatoryProgramUserDtoMapProfile : Profile
    {
        public OrganizationRegulatoryProgramUserDtoMapProfile()
        {
            CreateMap<OrganizationRegulatoryProgramUserDto, OrganizationRegulatoryProgramUser>()
                .ForAllMembers(opts => opts.Ignore()); //We should be explicitly ignoring each property we don't map to prevent silent failures!

            CreateMap<OrganizationRegulatoryProgramUser, OrganizationRegulatoryProgramUserDto>()
                .ForAllMembers(opts => opts.Ignore()); //We should be explicitly ignoring each property we don't map to prevent silent failures!

            CreateMap<OrganizationRegulatoryProgram, OrganizationRegulatoryProgramDto>()
                .ForMember(d => d.HasSignatory, o => o.Ignore());

            CreateMap<OrganizationRegulatoryProgramUser, UserDto>()

                .ForMember(d => d.FirstName, o => o.Ignore())
                .ForMember(d => d.LastName, o => o.Ignore())
                .ForMember(d => d.UserName, o => o.Ignore())
                .ForMember(d => d.Email, o => o.Ignore())
                .ForMember(d => d.Password, o => o.Ignore())
                .ForMember(d => d.CityName, o => o.Ignore())
               .ForMember(d => d.AddressLine1, o => o.Ignore())
               .ForMember(d => d.AddressLine2, o => o.Ignore())
               .ForMember(d => d.ZipCode, o => o.Ignore())
               .ForMember(d => d.PasswordHash, o => o.Ignore())

               .ForMember(d => d.BusinessName, o => o.Ignore())
               .ForMember(d => d.Jurisdictionid, o => o.Ignore())
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
                ;
        }
    }
}