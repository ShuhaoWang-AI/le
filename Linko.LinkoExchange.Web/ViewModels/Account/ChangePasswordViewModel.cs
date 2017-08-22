using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.Account
{
    [Validator(validatorType:typeof(LinkExchangePasswordValidator<PasswordViewModel>))]
    public class PasswordViewModel
    {
        #region public properties

        [DataType(dataType:DataType.Password)]
        [Display(Name = "Password")]
        public virtual string Password { get; set; }

        [DataType(dataType:DataType.Password)]
        [Display(Name = "Confirm Password")]
        public virtual string ConfirmPassword { get; set; }

        public bool ShowConfirmPassword { get; set; }

        #endregion
    }

    [Validator(validatorType:typeof(LinkExchangePasswordValidator<PasswordViewModel>))]
    public class ChangePasswordViewModel : PasswordViewModel
    {
        #region public properties

        [DataType(dataType:DataType.Password)]
        [Display(Name = "New Password")]
        public override string Password { get; set; }

        [DataType(dataType:DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public override string ConfirmPassword { get; set; }

        #endregion
    }

    public class LinkExchangePasswordValidator<T> : AbstractValidator<T> where T : PasswordViewModel
    {
        #region constructors and destructor

        public LinkExchangePasswordValidator()
        {
            var regexp = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d$@$#!%^&#'*""?,;:&.<>_\-\[\]\{\}\(\)\+\=~\|\\\?/]{8,16}$";

            RuleFor(x => x.Password).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");
            RuleFor(x => x.Password.Length).LessThanOrEqualTo(valueToCompare:16).WithMessage(errorMessage:"{PropertyName} does not meet the password requirements.");
            RuleFor(x => x.Password.Length).GreaterThanOrEqualTo(valueToCompare:8).WithMessage(errorMessage:"{PropertyName} does not meet the password requirements.");
            RuleFor(x => x.Password).Matches(expression:regexp).WithMessage(errorMessage:"{PropertyName} does not meet the password requirements.");

            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage(errorMessage:"The Password and Confirm Password do not match.");
        }

        #endregion
    }
}