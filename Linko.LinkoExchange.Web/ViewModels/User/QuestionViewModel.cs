
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Web.ViewModels.User
{
    public class QuestionViewModel
    {
        public int? QuestionId { get; set; }
        public QuestionType QuestionType { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; }
    }

    public class AnswerViewModel
    {
        public int? UserQuestionAnswerId { get; set; }
        public string Content { get; set; }
        public int QuestionId { get; set; }

    }

    public class QuestionAnswerPairViewModel
    {
        public QuestionViewModel Question { get; set; }
        public AnswerViewModel Answer { get; set; }
    } 
}