using AutoMapper;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Web.ViewModels.User;

namespace Linko.LinkoExchange.Web.AutoMapperProfile
{

    public class UserProfileViewModelProfile : Profile
    {
        public UserProfileViewModelProfile()
        {
            CreateMap<UserProfileViewModel, UserDto>()
                  .ForMember(d => d.AgreeTermsAndConditions, o => o.Ignore())
                  .ForMember(d => d.IsAccountResetRequired, o => o.Ignore())
                  .ForMember(d => d.IsIdentityProofed, o => o.Ignore())
                  .ForMember(d => d.IsInternalAccount, o => o.Ignore())
                  .ForMember(d => d.CreationDateTimeUtc, o => o.Ignore())
                  .ForMember(d => d.LastModificationDateTimeUtc, o => o.Ignore())
                  .ForMember(d => d.OldEmailAddress, o => o.Ignore())
                  .ForMember(d => d.LastModificationDateTimeUtc, o => o.Ignore())
                  .ForMember(d => d.Password, o => o.Ignore())
                  .ForMember(d => d.PasswordHash, o => o.Ignore())
                  .ForMember(d => d.LockoutEnabled, o => o.Ignore())
                  .ForMember(d => d.LockoutEndDateUtc, o => o.Ignore())
                  .ForMember(d => d.IsAccountLocked, o => o.Ignore()) 
                  .ForMember(d => d.EmailConfirmed, o => o.Ignore()) 

                  .ReverseMap() 
                  .ForMember(d => d.Password, o => o.Ignore());
        } 
    }
}