namespace Linko.LinkoExchange.Services.AuditLog
{
    //TODO: define more Crommer log entry
    public class CrommerAuditLogEntry : IAuditLogEntry
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string EventName { get; set; }
        public string Comments {get;set;}
    }
}
