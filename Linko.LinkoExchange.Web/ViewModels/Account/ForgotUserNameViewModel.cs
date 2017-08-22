using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Core.Resources;

namespace Linko.LinkoExchange.Web.ViewModels.Account
{
    [Validator(validatorType:typeof(ForgotUserNameValidator))]
    public class ForgotUserNameViewModel
    {
        #region public properties

        [Display(Name = "Email", ResourceType = typeof(Label))]
        [DataType(dataType:DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        #endregion
    }

    public class ForgotUserNameValidator : AbstractValidator<ForgotUserNameViewModel>
    {
        #region constructors and destructor

        public ForgotUserNameValidator()
        {
            RuleFor(x => x.EmailAddress).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");
            RuleFor(x => x.EmailAddress).EmailAddress().WithMessage(errorMessage:"Invalid {PropertyName}.");
            RuleFor(x => x.EmailAddress).Length(min:0, max:254).WithMessage(errorMessage:"{PropertyName} must be less than or equal to 254 characters long.");
        }

        #endregion
    }
}