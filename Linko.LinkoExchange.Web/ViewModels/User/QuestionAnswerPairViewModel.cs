using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Web.ViewModels.User
{
    public class QuestionAnswerPairViewModel
    {
        public QuestionViewModel Question
        {
            get; set;
        }
        public AnswerViewModel Answer
        {
            get; set;
        }
    }
}