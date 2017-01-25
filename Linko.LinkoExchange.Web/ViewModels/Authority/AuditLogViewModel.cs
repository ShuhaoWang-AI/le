using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Linko.LinkoExchange.Web.ViewModels.Authority
{
    public class AuditLogViewModel
    {
        [ScaffoldColumn(false)]
        [Display(Name = "CromerrAuditLogId")]
        public int CromerrAuditLogId
        {
            get; set;
        }

        [ScaffoldColumn(false)]
        [Display(Name = "AuditLogTemplateId")]
        public int AuditLogTemplateId
        {
            get; set;
        }

        [ScaffoldColumn(false)]
        [Display(Name = "RegulatoryProgramId")]
        public int RegulatoryProgramId
        {
            get; set;
        }

        [ScaffoldColumn(false)]
        [Display(Name = "RegulatoryProgramName")]
        public string RegulatoryProgramName
        {
            get; set;
        }

        [Display(Name = "Industry Number")]
        public int OrganizationId
        {
            get; set;
        }

        [Display(Name = "Industry Name")]
        public string OrganizationName
        {
            get; set;
        }

        [ScaffoldColumn(false)]
        [Display(Name = "RegulatorOrganizationId")]
        public int RegulatorOrganizationId
        {
            get; set;
        }

        [ScaffoldColumn(false)]
        [Display(Name = "RegulatorName")]
        public string RegulatorName
        {
            get; set;
        }

        [ScaffoldColumn(false)]
        [Display(Name = "UserProfileId")]
        public int UserProfileId
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Event Category")]
        public string EventCategory
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Event Type")]
        public string EventType
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
        [Display(Name = "Email Address")]
        public string EmailAddress
        {
            get; set;
        }
        [Editable(false)]
        [Display(Name = "IP Address")]
        public string IPAddress
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Host Name")]
        public string HostName
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Comment")]
        public string Comment
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Timestamp")]
        public DateTimeOffset LogDateTimeUtc
        {
            get; set;
        }
    }
}