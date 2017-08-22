using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.Account
{
    [Validator(validatorType:typeof(KbqChanlengeViewModelValidator))]
    public class KbqChallengeViewModel
    {
        #region public properties

        [Display(Name = "Question")]
        public string Question { get; set; }

        public int QuestionAnswerId { get; set; }

        [Display(Name = "Answer")]
        public string Answer { get; set; }

        [ScaffoldColumn(scaffold:false)]
        public int FailedCount { get; set; }

        #endregion
    }

    public class KbqChanlengeViewModelValidator : AbstractValidator<KbqChallengeViewModel>
    {
        #region constructors and destructor

        public KbqChanlengeViewModelValidator()
        {
            RuleFor(x => x.Answer).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");
        }

        #endregion
    }
}