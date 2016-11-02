using AutoMapper;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    public class InvitationMapProfile : Profile
    {
        public InvitationMapProfile()
        {
            CreateMap<Core.Domain.Invitation, InvitationDto>()
                .ForMember(i => i.ExpiryDateTimeUtc, o => o.Ignore());

            CreateMap<InvitationDto, Core.Domain.Invitation>();
        }
    }
}