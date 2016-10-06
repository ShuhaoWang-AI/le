namespace Linko.LinkoExchange.Services.AuditLog
{
    public interface IAuditLogService
    {
        /// <summary>
        /// Write log
        /// </summary>
        /// <param name="logEntry">The log information</param>
        void Log(IAuditLogEntry logEntry);
    }
}
