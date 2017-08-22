using System;
using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    public class PendingInvitationViewModel
    {
        #region public properties

        [ScaffoldColumn(scaffold:false)]
        [Display(Name = "Id")]
        public string Id { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Date Invited")]
        public DateTime? DateInvited { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Invite Expires")]
        public DateTime? InviteExpires { get; set; }

        #endregion
    }
}