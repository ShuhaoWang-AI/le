using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.Account
{
    [Validator(typeof(ResetPasswordValidator))]
    public class ResetPasswordViewModel
    {
        [ScaffoldColumn(false)]
        public int Id
        {
            get; set;
        }

        public string Question
        {
            get; set;
        }

        public string Answer
        {
            get; set;
        }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password
        {
            get; set;
        }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")] 
        public string ConfirmPassword
        {
            get; set;
        }

        [ScaffoldColumn(false)]
        public string Token
        {
            get; set;
        }

        [ScaffoldColumn(false)]
        public int FailedCount
        {
            get; set;
        }
    }

    public partial class ResetPasswordValidator : AbstractValidator<ResetPasswordViewModel>
    {
        public ResetPasswordValidator()
        {
            // Email
            RuleFor(x => x.Answer).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");

            RuleFor(x => x.Password).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.Password.Length).LessThanOrEqualTo(valueToCompare: 50).WithMessage(errorMessage: "{PropertyName} must be less than or equal to 50 characters long.");

            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage(errorMessage: "{PropertyName} does not match.");
        }
    }
}