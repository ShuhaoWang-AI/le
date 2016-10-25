using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.Account
{
    [Validator(typeof(ForgotUserNameValidator))]
    public class ForgotUserNameViewModel
    {
        [Display(Name = "Email", ResourceType = typeof(Core.Resources.Label))]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress
        {
            get; set;
        }
    }

    public partial class ForgotUserNameValidator : AbstractValidator<ForgotUserNameViewModel>
    {
        public ForgotUserNameValidator()
        {
            RuleFor(x => x.EmailAddress).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.EmailAddress).EmailAddress().WithMessage(errorMessage: "Invalid {PropertyName}.");
            RuleFor(x => x.EmailAddress).Length(0, 254).WithMessage(errorMessage: "{PropertyName} must be less than or equal to 254 characters long.");
        }
    }
}