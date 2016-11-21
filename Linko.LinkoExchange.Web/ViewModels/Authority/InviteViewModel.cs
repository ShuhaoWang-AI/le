using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Linko.LinkoExchange.Web.ViewModels.Authority
{
    public class InviteViewModel
    {
        public string DisplayMessage {  get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        //public int OrgRegProgramUserId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Facility Name")]
        public string FacilityName { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public bool IsExistingProgramUser { get; set; }

        //[Required]
        //[Display(Name = "First Name")]
        //public string NewFirstName { get; set; }

        //[Required]
        //[Display(Name = "Last Name")]
        //public string NewLastName { get; set; }

    }
}