 
namespace Linko.LinkoExchange.Services.Dto
{
    //TODO: define more COROMMER log entry
    public class CrommerAuditLogEntryDto : IAuditLogEntry
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string EventName { get; set; }
        public string Comments {get;set;}

        public int AuditLogTemplateId
        {
            get;set;
        }
    }
}
