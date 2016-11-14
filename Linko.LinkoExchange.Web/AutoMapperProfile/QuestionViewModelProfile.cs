using AutoMapper;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Web.ViewModels.User;

namespace Linko.LinkoExchange.Web.AutoMapperProfile
{
    public class QuestionViewModelProfile : Profile
    {
        public QuestionViewModelProfile()
        {
            CreateMap<QuestionViewModel, QuestionDto>()
                  .ReverseMap();
        }
    }
}