using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Core.Enum;
using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.User
{
    [Validator(typeof(RegistrationValidator))]
    public class RegistrationViewModel : UserViewModel
    {
        public string Token { get; set; }

        [Display(Name = "Agree To Terms And Conditions")]
        public bool AgreeTermsAndConditions { get; set; }

        [Display(Name = "Email")]
        public string InvitationEmail { get; set; }

        [Display(Name = "Program")]
        public string ProgramName { get; set; }

        [Display(Name = "Authority")]
        public string AuthorityName { get; set; }

        [Display(Name = "Facility")]
        public string IndustryName { get; set; }
        public RegistrationType RegistrationType { get; set; }
    }

    public partial class RegistrationValidator : AbstractValidator<RegistrationViewModel>
    {
        public RegistrationValidator()
        {
            RuleFor(x => x.Token).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.AgreeTermsAndConditions).Equal(toCompare: false).NotEmpty().WithMessage(errorMessage: "You must agree terms and conditions.");
        }
    }
}