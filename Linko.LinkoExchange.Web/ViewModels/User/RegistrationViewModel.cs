using FluentValidation;
using FluentValidation.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.User
{
    [Validator(typeof(RegistrationValidator))]
    public class RegistrationViewModel : UserViewModel
    {
        public string Token { get; set; } 

        [Display(Name = "Agree To Terms And Conditions")]
        public bool AgreeTermsAndConditions { get; set; }
    }

    public partial class RegistrationValidator : AbstractValidator<RegistrationViewModel>
    {
        public RegistrationValidator()
        {
            RuleFor(x => x.Token).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.AgreeTermsAndConditions).Equal(toCompare: true).NotEmpty().WithMessage(errorMessage: "You must agree terms and conditions.");
        } 
    } 
}