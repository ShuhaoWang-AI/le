using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    [Validator(typeof(PendingUserApprovalValidator))]
    public class PendingUserApprovalViewModel
    {
        [ScaffoldColumn(false)]
        [Display(Name = "Id")]
        public int Id
        {
            get; set;
        }

        [ScaffoldColumn(false)]
        [Display(Name = "PId")]
        public int PId
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Registered For")]
        public string RegisteredOrgName
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Organization Type")]
        public string Type
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "User Name")]
        public string UserName
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
        [Display(Name = "Organization Name")]
        public string BusinessName
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Title/Role")]
        public string TitleRole
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Address line 1")]
        public string AddressLine1
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Address line 2")]
        public string AddressLine2
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "City")]
        public string CityName
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "State")]
        public string State
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Zip Code")]
        public string ZipCode
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Address")]
        public string Address
        {
            get
            {
                return string.Format(format: "{0} {1}, {2}, {3} {4}", args: new object[] { AddressLine1, AddressLine2, CityName, State, ZipCode });
            }
        }

        [Editable(false)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email
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
        [Display(Name = "Registration Date")]
        public DateTime? DateRegistered
        {
            get; set;
        }
        
        [Display(Name = "Role")]
        public int? Role
        {
            get; set;
        }

        [Display(Name = "Role")]
        public string RoleText
        {
            get; set;
        }

        [Display(Name ="Is Signatory")]
        public bool IsSignatory
        {
            get; set;
        }

        public IList<SelectListItem> AvailableRoles
        {
            get; set;
        }        
    }

    public partial class PendingUserApprovalValidator : AbstractValidator<PendingUserApprovalViewModel>
    {
        public PendingUserApprovalValidator()
        {
            //Role
            RuleFor(x => x.Role).NotEmpty().GreaterThan(valueToCompare: 0).WithMessage(errorMessage: "{PropertyName} is required.");
        }
    }
}