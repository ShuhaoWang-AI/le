using AutoMapper;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Web.ViewModels.User;

namespace Linko.LinkoExchange.Web.AutoMapperProfile
{
    public class QuestionAnswerPairViewModelProfile : Profile
    {
        public QuestionAnswerPairViewModelProfile()
        {
            CreateMap<QuestionAnswerPairViewModel, QuestionAnswerPairDto>()
                  .ReverseMap();
        }
    }
}