using System.ComponentModel.DataAnnotations;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    public class InviteViewModel
    {
        #region public properties

        public string DisplayMessage { get; set; }

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
        [Phone]
        public string PhoneNumber { get; set; }

        public bool IsUserActiveInSameProgram { get; set; }

        public InvitationType InvitationType { get; set; }

        #endregion
    }
}