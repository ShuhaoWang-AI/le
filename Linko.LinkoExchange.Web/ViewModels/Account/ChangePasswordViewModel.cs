using FluentValidation;
using FluentValidation.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Linko.LinkoExchange.Web.ViewModels.Account
{

    [Validator(typeof(LinkExchangePasswordValidator<PasswordViewModel>))]
     public class PasswordViewModel
    {
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

        public bool ShowConfirmPassword
        {
            get; set;
        }
    }

    [Validator(typeof(LinkExchangePasswordValidator<PasswordViewModel>))]
    public class ChangePasswordViewModel:PasswordViewModel
    {

    }

    public partial class LinkExchangePasswordValidator<T> : AbstractValidator<T> where T : PasswordViewModel
    {
        public LinkExchangePasswordValidator()
        {
            var regexp = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,16}$";  

            RuleFor(x => x.Password).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.Password.Length).LessThanOrEqualTo(valueToCompare: 16).WithMessage(errorMessage: "{PropertyName} must be more than 8 and less than or equal to 16 characters long.");
            RuleFor(x => x.Password.Length).GreaterThanOrEqualTo(valueToCompare: 8).WithMessage(errorMessage: "{PropertyName} must be more than 8 and less than or equal to 16 characters long.");
            RuleFor(x => x.Password).Matches(regexp).WithMessage(errorMessage: "{PropertyName} must be at least 8 characters, have at least one digit ('0'-'9'), and have at least one lowercase ('a'-'z') and one uppercase ('A'-'Z') letter. ");

            When(x => x.ShowConfirmPassword, () =>
                                           {
                                               RuleFor(x => x.ConfirmPassword)
                                                  .NotEmpty()
                                                  .WithMessage(errorMessage: "{PropertyName} is required.");
                                               RuleFor(x => x.ConfirmPassword)
                                                  .Equal(x => x.Password)
                                                  .WithMessage(errorMessage: "The Password and Confirm Password do not match.");
                                          });
        } 
    }
}