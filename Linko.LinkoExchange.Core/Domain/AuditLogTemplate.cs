namespace Linko.LinkoExchange.Core.Domain
{
    public class AuditLogTemplate
    { 
        public int AuditLogTemplateId
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

        public string SubjectTempate
        {
            get;set;
        }

        public string MessageTemplate
        {
            set;get;
        } 
    }
}
