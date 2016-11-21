using FluentValidation;
using FluentValidation.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.Account
{
    [Validator(typeof(ChangeEmailValidator))]
    public class ChangeEmailViewModel
    {
       
        [Display(Name = "Old Email")] 
        public string OldEmail
        {
            get; set;
        } 
     
        [Display(Name = "New Email")] 
        [EmailAddress]
        public string NewEmail
        {
            get; set;

        }
    }

    public partial class ChangeEmailValidator : AbstractValidator<ChangeEmailViewModel>
    {
        public ChangeEmailValidator()
        {
            RuleFor(x => x.NewEmail).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.NewEmail).NotEqual(a=>a.OldEmail).WithMessage(errorMessage: "Old email address the new email address can not be the same.");
        }
    }
}