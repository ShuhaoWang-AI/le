using FluentValidation;
using FluentValidation.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.Account
{
    [Validator(typeof(KbqChanlengeViewModelValidator))]
    public class KbqChallengeViewModel
    {
        [Display(Name = "Question")]
        public string Question
        {
            get; set;
        }

        public int QuestionAnswerId
        {
            get;set;
        }

        [Display(Name = "Answer")]
        public string Answer
        {
            get; set;

        }

        [ScaffoldColumn(false)]
        public int FailedCount
        {
            get; set;
        }
    }
     
    public partial class KbqChanlengeViewModelValidator : AbstractValidator<KbqChallengeViewModel>
    {
        public KbqChanlengeViewModelValidator()
        { 
            RuleFor(x => x.Answer).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required."); 
        }
    }
}