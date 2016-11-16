using System;

namespace Linko.LinkoExchange.Core.Domain
{

    /// <summary>
    /// Represents a message template for an audit log.
    /// </summary>
    public partial class AuditLogTemplate
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int AuditLogTemplateId { get; set; }

        /// <summary>
        /// Format: TemplateType_EventCategory_EventType. 
        /// It exists to assist the business layer to refer a single row because there are many non-existent combination of the three values.
        /// </summary>
        public string Name { get; set; }

        public string TemplateType { get; set; }

        public string EventCategory { get; set; }

        public string EventType { get; set; }

        /// <summary>
        /// Used by Email TemplateType.
        /// </summary>
        public string SubjectTemplate { get; set; }

        public string MessageTemplate { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }

}