
using Linko.LinkoExchange.Services.Dto;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.AuditLog
{
    public interface IAuditLogService
    {
        /// <summary>
        /// Write log
        /// </summary>
        /// <param name="logEntry">The log information</param>
        Task Log(IAuditLogEntry logEntry);
    }
}
