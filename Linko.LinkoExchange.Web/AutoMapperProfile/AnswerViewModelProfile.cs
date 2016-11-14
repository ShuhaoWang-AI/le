using AutoMapper;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Web.ViewModels.User;

namespace Linko.LinkoExchange.Web.AutoMapperProfile
{
    public class AnswerViewModelProfile : Profile
    {
        public AnswerViewModelProfile()
        {
            CreateMap<AnswerViewModel, AnswerDto>()
                  .ReverseMap();
        }
    }
}