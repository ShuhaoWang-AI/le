using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.Account
{
    [Validator(typeof(ResetPasswordValidator))]
    public class ResetPasswordViewModel : PasswordViewModel
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

        [ScaffoldColumn(false)]
        public string Token
        {
            get; set;
        }

        [ScaffoldColumn(false)]
        public int UserProfileId
        {
            get;set;
        }

        public string OwinUserId
        {
            get;set;
        }

        [ScaffoldColumn(false)]
        public int FailedCount
        {
            get; set;
        }
    }

    public partial class ResetPasswordValidator : LinkExchangePasswordValidator<ResetPasswordViewModel>
    {
        public ResetPasswordValidator()
        {
            // Email
            RuleFor(x => x.Answer).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
        }
    }
}