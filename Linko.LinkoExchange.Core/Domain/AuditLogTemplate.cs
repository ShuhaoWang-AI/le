using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Linko.LinkoExchange.Core.Domain
{
    public class AuditLogTemplate
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditLogTemplateId
        {
            get;set;
        }
        
        public string Name
        {
            get;set;
        }
        public string TemplateType
        {
            get;set;
        }

        public string EventCategory
        {
            get;set;
        }

        public string EventType
        {
            get;set;
        }

        public string SubjectTemplate
        {
            get;set;
        }

        public string MessageTemplate
        {
            set;get;
        } 

        public DateTimeOffset CreationDateTimeUtc
        {
            get; set; 
            
        }

        public DateTimeOffset? LastModificationDateTimeUtc
        {
            get; set;
        }

        public int? LastModifierUserId { get; set; }
    }
}
