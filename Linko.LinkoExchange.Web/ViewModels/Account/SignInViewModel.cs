using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Core.Resources;

namespace Linko.LinkoExchange.Web.ViewModels.Account
{
    [Validator(validatorType:typeof(SigInValidator))]
    public class SignInViewModel
    {
        #region public properties

        [Display(Name = "UserName", ResourceType = typeof(Label))]
        public string UserName { get; set; }

        [DataType(dataType:DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Label))]
        public string Password { get; set; }

        #endregion
    }

    public class SigInValidator : AbstractValidator<SignInViewModel>
    {
        #region constructors and destructor

        public SigInValidator()
        {
            //UserName
            RuleFor(x => x.UserName).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");
            RuleFor(x => x.UserName.Length).LessThanOrEqualTo(valueToCompare:256).WithMessage(errorMessage:"{PropertyName} must be less than or equal to 256 characters long.");

            //Password
            RuleFor(x => x.Password).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");
            RuleFor(x => x.Password.Length).LessThanOrEqualTo(valueToCompare:50).WithMessage(errorMessage:"{PropertyName} must be less than or equal to 50 characters long.");
        }

        #endregion
    }
}