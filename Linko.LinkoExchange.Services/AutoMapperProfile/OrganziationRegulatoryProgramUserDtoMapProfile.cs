using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    public class OrganziationRegulatoryProgramUserDtoMapProfile : Profile
    {
        public OrganziationRegulatoryProgramUserDtoMapProfile()
        {
            CreateMap<OrganizationRegulatoryProgramUserDto, OrganizationRegulatoryProgramUser>()
                .ForAllMembers(opts => opts.Ignore()); //We should be explicitly ignoring each property we don't map to prevent silent failures!

            CreateMap<OrganizationRegulatoryProgramUser, OrganizationRegulatoryProgramUserDto>()
                .ForAllMembers(opts => opts.Ignore()); //We should be explicitly ignoring each property we don't map to prevent silent failures!

        }
    }
}