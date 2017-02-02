using Linko.LinkoExchange.Core.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    public class InviteViewModel
    {
        public string DisplayMessage {  get; set; }

        [Required(AllowEmptyStrings = false)]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        public int OrgRegProgramId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Organization Name")]
        public string BusinessName { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public bool IsExistingProgramUser { get; set; }

        public ICollection<InviteExistingUserViewModel> ExistingUsers { get; set; }

        public InvitationType InvitationType { get; set; }

    }

    public class InviteExistingUserViewModel
    {
        public int? OrgRegProgramUserId { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BusinessName { get; set; }
        public string PhoneNumber { get; set; }
    }
}