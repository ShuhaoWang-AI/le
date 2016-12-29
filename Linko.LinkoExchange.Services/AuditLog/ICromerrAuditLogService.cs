using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.AuditLog
{
    public interface ICromerrAuditLogService
    {
        /// <summary>
        /// Creates a CROMERR audit log entry in the database
        /// </summary>
        /// <param name="logEntry"></param>
        Task Log(EmailType eventType, CromerrAuditLogEntryDto dto, IDictionary<string, string> contentReplacements);
    }
}
