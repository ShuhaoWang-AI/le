using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class EmailAuditLogEntryDto : IAuditLogEntry
    {
        public int EmailAuditLogId
        {
            get; set;
        }

        public int AuditLogTemplateId
        {
            get;set;
        }

        public int? SenderRegulatoryProgramId
        {
            get; set;
        }

        public int? SenderOrganizationId
        {
            get; set;
        }

        public int? SenderRegulatorOrganizationId
        {
            get; set;
        }

        public int SenderUserProfileId
        {
            get;set;
        }

        public string SenderUserName
        {
            get; set;
        }

        public string SenderFirstName
        {
            get; set;
        }

        public string SenderLastName
        {
            get; set;
        }

        public string SenderEmailAddress
        {
            get; set;
        }

        public int? RecipientOrganizationId
        {
            get; set;
        }

        public int? RecipientRegulatorOrganizationId
        {
            get; set;
        }

        public int? RecipientRegulatoryProgramId
        {
            get; set;
        }

        public int? RecipientUserProfileId
        {
            get;set;
        }

        public string RecipientUserName
        {
            get; set;
        }
        public string RecipientFirstName
        {
            get; set;
        }
        public string RecipientLastName
        {
            get; set;
        }
        public string RecipientEmailAddress
        {
            get; set;
        }
        public string Subject
        {
            get; set;
        }
        public string Body
        {
            get; set;
        }
        public DateTimeOffset SentDateTimeUtc
        {
            get; set;
        }

        public string Token
        {
            get;set;
        }
    }
}