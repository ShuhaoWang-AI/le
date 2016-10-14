using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class EmailAuditLogEntryDto : IAuditLogEntry
    { 
        public int AuditLogTemplateId
        {
            get;set;
        }

        public int SenderRegulatorId
        {
            get;set;
        }

        public int SenderRegulateeId
        {
            get; set;
        }

        public int SenderRegulatoryProgramId
        {
            get; set;
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

        public int RecipientRegulatorId
        {
            get; set;
        }

        public int RecipientRegulateeId
        {
            get; set;
        }
        public int RecipientRegulatoryProgramId
        {
            get; set;
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
        public DateTime SentDateTimeUtc
        {
            get; set;
        }
    }
}