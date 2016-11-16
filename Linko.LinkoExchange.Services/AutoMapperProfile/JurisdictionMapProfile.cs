using AutoMapper;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    public class JurisdictionMapProfile : Profile
    {
        public JurisdictionMapProfile()
        {
            CreateMap<Core.Domain.Jurisdiction, JurisdictionDto>()
            .ReverseMap();
        }
    }

}
