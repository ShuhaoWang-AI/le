using FluentValidation.Attributes;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using FluentValidation;

namespace Linko.LinkoExchange.Web.ViewModels.User
{

    [Validator(typeof(UserProfileViewModelValidator))]
    public class UserProfileViewModel
    {

        [ScaffoldColumn(false)]
        [Editable(false)]
        [Display(Name = "UserProfileId")]
        public int UserProfileId
        {
            get; set;
        }

        // For permission role 
        [Editable(false)] 
        [Display(Name ="Role")]
        public string Role
        {
            get;set;
        }

        [Editable(false)]
        [Display(Name ="Signatory")] 
        public bool HasSigntory
        {
            get;set;
        }

        [Editable(false)]
        [Display(Name = "Signatory")] 
        public string HasSigntoryText
        {
            get { return HasSigntory ? "Yes" : "No"; }
        }

        [Display(Name = "Title/Role")]
        public string TitleRole
        {
            get; set;
        }
        
        [Display(Name = "First Name")]
        public string FirstName
        {
            get; set;
        }
         
        [Display(Name = "Last Name")]
        public string LastName
        {
            get; set;
        } 

        [Display(Name = "Organization Name")]
        public string BusinessName
        {
            get; set;
        }

        [Required]
        [Editable(false)]
        public int OrganizationId
        {
            get; set;
        }

        [Display(Name = "Address Line 1")]
        public string AddressLine1
        {
            get; set;
        }

        [Display(Name = "Address Line 2")]
        public string AddressLine2
        {
            get; set;
        }
         
        [Display(Name = "City")]
        public string CityName
        {
            get; set;
        }
         
        [Display(Name = "State")]
        public int JurisdictionId
        {
            get; set;
        } 
         
        [Display(Name = "Zip Code")]
        public string ZipCode
        {
            get; set;
        }         
         
        [Display(Name = "Phone")]
        [Phone]
        public string PhoneNumber
        {
            get; set;
        }
         
        [Display(Name = "Ext")]
        public int? PhoneExt
        {
            get; set;
        } 
  
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email
        {
            get; set;
        }
         
        [Display(Name = "User Name")] 
        public string UserName
        {
            get; set;
        }
         
        [Display(Name = "Password")] 
       
        public string Password
        {
            get; set;
        }
         
        public List<JurisdictionViewModel> StateList
        {
            get;set;
        }
    }

    public partial class UserProfileViewModelValidator : AbstractValidator<UserProfileViewModel>
    {
        public UserProfileViewModelValidator()
        {
            var regexp = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,16}$";
            RuleFor(x => x.FirstName).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.FirstName.Length).LessThanOrEqualTo(valueToCompare: 50).WithMessage(errorMessage: "{PropertyName} is not more than 50 characters.");
            RuleFor(x => x.LastName).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.LastName.Length).LessThanOrEqualTo(valueToCompare: 50).WithMessage(errorMessage: "{PropertyName} is not more than 50 characters.");

            RuleFor(x => x.BusinessName).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.BusinessName.Length).LessThanOrEqualTo(valueToCompare: 100).WithMessage(errorMessage: "{PropertyName} is not more than 100 characters.");

            RuleFor(x => x.AddressLine1).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.AddressLine1.Length).LessThanOrEqualTo(valueToCompare: 100).WithMessage(errorMessage: "{PropertyName} is not more than 100 characters.");

            RuleFor(x => x.CityName).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.CityName.Length).LessThanOrEqualTo(valueToCompare: 100).WithMessage(errorMessage: "{PropertyName} is not more than 100 characters.");
            
            RuleFor(x => x.ZipCode).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.ZipCode.Length).LessThanOrEqualTo(valueToCompare: 50).WithMessage(errorMessage: "{PropertyName} is not more than 50 characters.");

            RuleFor(x => x.Email).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");

            RuleFor(x => x.UserName).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.UserName.Length).LessThanOrEqualTo(valueToCompare: 256).WithMessage(errorMessage: "{PropertyName} is not more than 256 characters.");
             
            RuleFor(x => x.Password).Matches(regexp).WithMessage(errorMessage: "{PropertyName} must be at least 8 characters, have at least one digit ('0'-'9'), and have at least one lowercase ('a'-'z') and one uppercase ('A'-'Z') letter. ");
            RuleFor(x => x.Password).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.Password.Length).LessThanOrEqualTo(valueToCompare: 16).WithMessage(errorMessage: "{PropertyName} must be more than 8 and less than or equal to 16 characters long.");
            RuleFor(x => x.Password.Length).GreaterThanOrEqualTo(valueToCompare: 8).WithMessage(errorMessage: "{PropertyName} must be more than 8 and less than or equal to 16 characters long.");

           //// RuleFor(x => x.AgreeTermsAndConditions).Equal(toCompare: true).NotEmpty().WithMessage(errorMessage: "You must agree terms and conditions.");
        }
     }
}