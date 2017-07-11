using System;
using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.Authority
{
    public class AuditLogViewModel
    {
        [ScaffoldColumn(false)]
        public int CromerrAuditLogId
        {
            get; set;
        }

        [Display(Name = "Event Code")]
        public int AuditLogTemplateId
        {
            get; set;
        }

        [ScaffoldColumn(false)]
        public int RegulatoryProgramId
        {
            get; set;
        }

        [Display(Name = "Regulatory Program")]
        public string RegulatoryProgramName
        {
            get; set;
        }

        [Display(Name = "Facility Number")]
        public int OrganizationId
        {
            get; set;
        }

        [Display(Name = "Facility")]
        public string OrganizationName
        {
            get; set;
        }

        [ScaffoldColumn(false)]
        public int RegulatorOrganizationId
        {
            get; set;
        }

        [Display(Name = "Authority")]
        public string RegulatorName
        {
            get; set;
        }

        [Display(Name = "User ID")]
        public string UserProfileIdDisplay
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
        [Display(Name = "IP Name")]
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
        [Display(Name = "Date and Time")]
        public DateTime LogDateTimeUtc
        {
            get; set;
        }

        [ScaffoldColumn(false)]
        public string LogDateTimeUtcDetailString
        {
            get
            {
                return this.LogDateTimeUtc.ToString("MM/dd/yyyy h:mm tt");
            }
        }
    }
}