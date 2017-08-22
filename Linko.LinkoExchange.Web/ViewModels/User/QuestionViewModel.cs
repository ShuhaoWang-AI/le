using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Web.ViewModels.User
{
    public class QuestionViewModel
    {
        #region public properties

        public int? QuestionId { get; set; }
        public QuestionTypeName QuestionType { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; }

        #endregion
    }
}