using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.Account
{
    [Validator(typeof(ForgotPasswordValidator))]
    public class ForgotPasswordViewModel
    {
        [Display(Name = "UserName", ResourceType = typeof(Core.Resources.Label))]
        public string UserName
        {
            get; set;
        }
    }

    public partial class ForgotPasswordValidator : AbstractValidator<ForgotPasswordViewModel>
    {
        public ForgotPasswordValidator()
        {
            //UserName
            RuleFor(x => x.UserName).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.UserName.Length).LessThanOrEqualTo(valueToCompare: 256).WithMessage(errorMessage: "{PropertyName} must be less than or equal to 256 characters long.");
        }
    }
}