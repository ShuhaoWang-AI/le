using AutoMapper;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    public class RegulatoryProgramMapperProfile : Profile
    {
        public RegulatoryProgramMapperProfile()
        {
            CreateMap<Core.Domain.RegulatoryProgram, ProgramDto>()
                .ReverseMap();

        }
    }
}
