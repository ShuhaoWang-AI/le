using FluentValidation;
using FluentValidation.Attributes; 
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Web.ViewModels.User
{
    [Validator(validatorType:typeof(OneQuestionAnswerViewModelValidator))]
    public class QuestionAnswerViewModel
    {
        public QuestionTypeName QuestionTypeName {get;set; }
        public string QuestionLabel {get;set; }
        public string AnswerLabel {get;set; }
        public int QuestionAnswerId {get;set;} 
        public int QuestionId {get;set; }
        public string Content {get;set; }
    }

    public class OneQuestionAnswerViewModelValidator : AbstractValidator<QuestionAnswerViewModel>
    {
        #region constructors and destructor

        public OneQuestionAnswerViewModelValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");
        }

        #endregion
    }

}