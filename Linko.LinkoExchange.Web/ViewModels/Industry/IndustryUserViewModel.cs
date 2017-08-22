using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.Industry
{
    [Validator(validatorType:typeof(IndustryUserValidator))]
    public class IndustryUserViewModel
    {
        #region public properties

        [ScaffoldColumn(scaffold:false)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [ScaffoldColumn(scaffold:false)]
        [Display(Name = "PId")]
        public int PId { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Ext")]
        public int? PhoneExt { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        public string ResetEmail { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Registration Date")]
        public DateTime? DateRegistered { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Status")]
        public bool Status { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Status")]
        public string StatusText => Status ? "Active" : "Inactive";

        [Editable(allowEdit:false)]
        public string StatusButtonText => Status ? "Disable" : "Enable";

        [Editable(allowEdit:false)]
        [Display(Name = "Account Locked")]
        public bool AccountLocked { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Account Locked")]
        public string AccountLockedText => AccountLocked ? "Locked" : "No";

        [Editable(allowEdit:false)]
        public string AccountLockedButtonText => AccountLocked ? "Unlock" : "Lock";

        [Display(Name = "Role")]
        public int Role { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Role")]
        public string RoleText { get; set; }

        public IList<SelectListItem> AvailableRoles { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Is Signatory")]
        public bool IsSignatory { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Security Question 1")]
        public string SecurityQuestion1 { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Answer")]
        public string Answer1 { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Security Question 2")]
        public string SecurityQuestion2 { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Answer")]
        public string Answer2 { get; set; }

        #endregion
    }

    public class IndustryUserValidator : AbstractValidator<IndustryUserViewModel>
    {
        #region constructors and destructor

        public IndustryUserValidator()
        {
            //ResetEmail
            RuleFor(x => x.ResetEmail).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");
            RuleFor(x => x.ResetEmail.Length).LessThanOrEqualTo(valueToCompare:256).WithMessage(errorMessage:"{PropertyName} must be less than or equal to 256 characters long.");
        }

        #endregion
    }
}