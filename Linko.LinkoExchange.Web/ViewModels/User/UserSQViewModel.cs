using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.User
{
    [Validator(validatorType:typeof(SecurityQuestionValidator))]
    public class UserSQViewModel
    {
        #region public properties

        [ScaffoldColumn(scaffold:false)]
        [Editable(allowEdit:false)]
        [Display(Name = "UserProfileId")]
        public int UserProfileId { get; set; }

        public int UserQuestionAnserId_SQ1 { get; set; }

        public int UserQuestionAnserId_SQ2 { get; set; }

        [Display(Name = "Question 1")]
        public int SecurityQuestion1 { get; set; }

        [Display(Name = "Answer 1")]
        public string SecurityQuestionAnswer1 { get; set; }

        [Display(Name = "Question 2")]
        public int SecurityQuestion2 { get; set; }

        [Display(Name = "Answer 2")]
        public string SecurityQuestionAnswer2 { get; set; }

        public List<QuestionViewModel> QuestionPool { get; set; }

        #endregion
    }

    public class SecurityQuestionValidator : AbstractValidator<UserSQViewModel>
    {
        #region constructors and destructor

        public SecurityQuestionValidator()
        {
            RuleFor(x => x.SecurityQuestion1)
                .NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.")
                .NotEqual(a => a.SecurityQuestion2).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.");

            RuleFor(x => x.SecurityQuestion2)
                .NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.")
                .NotEqual(a => a.SecurityQuestion1).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.");

            RuleFor(x => x.SecurityQuestionAnswer1)
                .NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.")
                .NotEqual(a => a.SecurityQuestionAnswer2).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.");

            RuleFor(x => x.SecurityQuestionAnswer2)
                .NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.")
                .NotEqual(a => a.SecurityQuestionAnswer1).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.");
        }

        #endregion
    }
}