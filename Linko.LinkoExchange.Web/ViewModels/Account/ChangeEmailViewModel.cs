using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.Account
{
    [Validator(validatorType:typeof(ChangeEmailValidator))]
    public class ChangeEmailViewModel
    {
        #region public properties

        [Display(Name = "Old Email")]
        public string OldEmail { get; set; }

        [Display(Name = "New Email")]
        [EmailAddress]
        public string NewEmail { get; set; }

        #endregion
    }

    public class ChangeEmailValidator : AbstractValidator<ChangeEmailViewModel>
    {
        #region constructors and destructor

        public ChangeEmailValidator()
        {
            RuleFor(x => x.NewEmail).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");
            RuleFor(x => x.NewEmail).NotEqual(a => a.OldEmail).WithMessage(errorMessage:"Old email address and the new email address cannot be the same.");
        }

        #endregion
    }
}