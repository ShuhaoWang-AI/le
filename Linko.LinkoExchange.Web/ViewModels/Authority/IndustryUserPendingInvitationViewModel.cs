﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.Authority
{
    public class IndustryUserPendingInvitationViewModel
    {
        [ScaffoldColumn(false)]
        [Display(Name = "ID")]
        public string ID
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
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Date Invited")]
        public DateTime? DateInvited
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Invite Expires")]
        public DateTime? InviteExpires
        {
            get; set;
        }

        [Editable(false)]
        public bool CanInvite
        {
            get; set;
        }
    }
}