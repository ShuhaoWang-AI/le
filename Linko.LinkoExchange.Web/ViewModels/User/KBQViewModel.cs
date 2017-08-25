using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.User
{
    [Validator(validatorType:typeof(OneKbqValidator))]
    public class KbqViewModel
    {
        public string QuestionLabel {get;set; }
        public string AnswerLabel {get;set; }
        public int QuestionAnswerId {get;set;} 
        public int QuestionId {get;set; }
        public string Content {get;set; }
    }

    public class OneKbqValidator : AbstractValidator<KbqViewModel>
    {
        #region constructors and destructor

        public OneKbqValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");
        }

        #endregion
    }

}