using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Core.Domain
{
     public class EmailAuditLog
     {
        [Key]
        public int EmailAuditLogId
        {
            get;set;
        }
        public int AuditLogTemplateId
        {
            get; set;
        }

        public int SenderRegulatorId
        {
            get; set;
        }

        public int SenderRegulateeId
        {
            get; set;
        }

        public int SenderRegulatoryProgramId
        {
            get; set;
        }

        public int SenderUserName
        {
            get; set;
        }

        public int SenderFirstName
        {
            get; set;
        }

        public int SenderLastName
        {
            get; set;
        }

        public int SenderEmailAddress
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
        public int RecipientUserName
        {
            get; set;
        }
        public int RecipientFirstName
        {
            get; set;
        }
        public int RecipientLastName
        {
            get; set;
        }
        public int RecipientEmailAddress
        {
            get; set;
        }
        public int Subject
        {
            get; set;
        }
        public int Body
        {
            get; set;
        }
        public int SentDateTimeUtc
        {
            get; set;
        }
    }
}
