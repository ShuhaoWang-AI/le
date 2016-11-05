using System;
using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.Authority
{
    public class IndustryUserViewModel
    {
        [ScaffoldColumn(false)]
        [Display(Name = "ID")]
        public int ID
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
        public int PhoneExt
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

        [Editable(false)]
        [Display(Name = "Date Registered")]
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
                return Status ? "Locked" : "No";
            }
        }

        [Editable(false)]
        [Display(Name = "Role")]
        public string Role
        {
            get; set;
        }
        
        [Display(Name = "Is Signatory")]
        public bool IsSignatory
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
}