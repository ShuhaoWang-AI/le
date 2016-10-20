using FluentValidation;
using FluentValidation.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.Account
{
    [Validator(typeof(SigInValidator))]
    public class SignInViewModel
    {
        [Display(Name = "UserName", ResourceType = typeof(Core.Resources.Label))]
        public string UserName
        {
            get; set;
        }
        
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Core.Resources.Label))]
        public string Password
        {
            get; set;
        }

        [Display(Name = "RememberMe", ResourceType = typeof(Core.Resources.Label))]
        public bool RememberMe
        {
            get; set;
        }
    }

    public partial class SigInValidator : AbstractValidator<SignInViewModel>
    {
        public SigInValidator()
        {
            //Email
            RuleFor(x => x.UserName).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.UserName.Length).LessThanOrEqualTo(valueToCompare: 256).WithMessage(errorMessage: "{PropertyName} must be less than or equal to 256 characters long.");

            //Password
            RuleFor(x => x.Password).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.Password.Length).LessThanOrEqualTo(valueToCompare: 50).WithMessage(errorMessage: "{PropertyName} must be less than or equal to 50 characters long.");
        }
    }
}