using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    public class OrganziationRegulatoryProgramUserDtoMapProfile : Profile
    {
        public OrganziationRegulatoryProgramUserDtoMapProfile()
        {
            CreateMap<OrganizationRegulatoryProgramUserDto, OrganizationRegulatoryProgramUser>();
            CreateMap<OrganizationRegulatoryProgramUser, OrganizationRegulatoryProgramUserDto>();  
        }
    }
}