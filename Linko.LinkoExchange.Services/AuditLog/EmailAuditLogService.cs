using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using System;

namespace Linko.LinkoExchange.Services.AuditLog
{
    /// <summary>
    /// Email audit log service implementation
    /// </summary>
    public class EmailAuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _linkoExchangeDbContext = new ApplicationDbContext();

        /// <summary>
        /// Write email audit log
        /// </summary>
        /// <param name="logEntry">Email audit log entry</param>
        public void Log(IAuditLogEntry logEntry)
        {
            var emailLogEntryDto = logEntry as EmailAuditLogEntryDto;
            if (emailLogEntryDto == null)
                return;

            var emailLogEntry = AutoMapper.Mapper.Map<EmailAuditLog>(emailLogEntryDto) ;

            this._linkoExchangeDbContext.EmailAuditLog.Add(emailLogEntry);
            this._linkoExchangeDbContext.SaveChangesAsync();
        }
    }
}