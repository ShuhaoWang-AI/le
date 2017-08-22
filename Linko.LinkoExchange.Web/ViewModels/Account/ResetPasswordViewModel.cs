using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.Account
{
    [Validator(validatorType:typeof(ResetPasswordValidator))]
    public class ResetPasswordViewModel : PasswordViewModel
    {
        #region public properties

        [ScaffoldColumn(scaffold:false)]
        public int Id { get; set; }

        public string Question { get; set; }

        public string Answer { get; set; }

        [ScaffoldColumn(scaffold:false)]
        public string Token { get; set; }

        [ScaffoldColumn(scaffold:false)]
        public int UserProfileId { get; set; }

        public string OwinUserId { get; set; }

        [ScaffoldColumn(scaffold:false)]
        public int FailedCount { get; set; }

        [DataType(dataType:DataType.Password)]
        [Display(Name = "New Password")]
        public override string Password { get; set; }

        [DataType(dataType:DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public override string ConfirmPassword { get; set; }

        #endregion
    }

    public class ResetPasswordValidator : LinkExchangePasswordValidator<ResetPasswordViewModel>
    {
        #region constructors and destructor

        public ResetPasswordValidator()
        {
            // Email
            RuleFor(x => x.Answer).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");
        }

        #endregion
    }
}