using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents an audit log for email.
    /// </summary>
    public partial class EmailAuditLog
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int EmailAuditLogId { get; set; }

        public int AuditLogTemplateId { get; set; }
        public virtual AuditLogTemplate AuditLogTemplate { get; set; }

        public int? SenderRegulatoryProgramId { get; set; }

        public int? SenderOrganizationId { get; set; }

        public int? SenderRegulatorOrganizationId { get; set; }

        public int? SenderUserProfileId { get; set; }

        public string SenderUserName { get; set; }

        public string SenderFirstName { get; set; }

        public string SenderLastName { get; set; }

        public string SenderEmailAddress { get; set; }

        public int? RecipientRegulatoryProgramId { get; set; }

        public int? RecipientOrganizationId { get; set; }

        public int? RecipientRegulatorOrganizationId { get; set; }

        public int? RecipientUserProfileId { get; set; }

        public string RecipientUserName { get; set; }

        public string RecipientFirstName { get; set; }

        public string RecipientLastName { get; set; }

        public string RecipientEmailAddress { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        /// <summary>
        /// Guid. So that it can be used in the link whenever applicable inside each email.
        /// </summary>
        public string Token { get; set; }

        public DateTimeOffset SentDateTimeUtc { get; set; }
    }

}