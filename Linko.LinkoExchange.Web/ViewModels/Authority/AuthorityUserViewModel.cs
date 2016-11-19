using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.Authority
{
    [Validator(typeof(AuthorityUserViewModelValidator))]
    public class AuthorityUserViewModel
    {
        [ScaffoldColumn(false)]
        [Display(Name = "ID")]
        public int ID
        {
            get; set;
        }
        
        [ScaffoldColumn(false)]
        [Display(Name = "pID")]
        public int PID
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "First Name")]
        public string FirstName
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Last Name")]
        public string LastName
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Ext")]
        public int? PhoneExt
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email
        {
            get; set;
        }

        [Display(Name = "Email")]
        [EmailAddress]
        public string ResetEmail
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Registration Date")]
        public DateTime? DateRegistered
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Status")]
        public bool Status
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Status")]
        public string StatusText
        {
            get
            {
                return Status ? "Active" : "Inactive";
            }
        }

        [Editable(false)]
        [Display(Name = "Account Locked")]
        public bool AccountLocked
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Account Locked")]
        public string AccountLockedText
        {
            get
            {
                return AccountLocked ? "Locked" : "No";
            }
        }

        [Editable(false)]
        public string AccountLockedButtonText
        {
            get
            {
                return AccountLocked ? "Unlock" : "Lock";
            }
        }
                
        [Display(Name = "Role")]
        public int Role
        {
            get; set;
        }
        
        [Display(Name = "Role")]
        public string RoleText
        {
            get; set;
        }

        public IList<SelectListItem> AvailableRoles
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Security Question 1")]
        public string SecurityQuestion1
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Answer")]
        public string Answer1
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Security Question 2")]
        public string SecurityQuestion2
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Answer")]
        public string Answer2
        {
            get; set;
        }
    }
    
    public partial class AuthorityUserViewModelValidator : AbstractValidator<AuthorityUserViewModel>
    {
        public AuthorityUserViewModelValidator()
        {
            //ResetEmail
            RuleFor(x => x.ResetEmail).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.");
            RuleFor(x => x.ResetEmail.Length).LessThanOrEqualTo(valueToCompare: 256).WithMessage(errorMessage: "{PropertyName} must be less than or equal to 256 characters long.");
            
        }
    }
}