using AutoMapper;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    public class InvitationMapProfile : Profile
    {
        public InvitationMapProfile()
        {
            CreateMap<Core.Domain.Invitation, InvitationDto>()
                .ForMember(d => d.ProgramName, o => o.Ignore())
                .ForMember(d => d.AuthorityName, o => o.Ignore())
                .ForMember(d => d.IndustryName, o => o.Ignore())
                .ForMember(d => d.ExpiryDateTimeUtc, o => o.Ignore());

            CreateMap<InvitationDto, Core.Domain.Invitation>()
                
                .ForMember(d => d.SenderOrganizationRegulatoryProgram, o => o.Ignore())
                .ForMember(d => d.RecipientOrganizationRegulatoryProgram, o => o.Ignore());
        }
    }
}