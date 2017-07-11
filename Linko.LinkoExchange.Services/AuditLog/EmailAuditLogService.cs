using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;

namespace Linko.LinkoExchange.Services.AuditLog
{
    /// <summary>
    /// Email audit log service implementation
    /// </summary>
    public class EmailAuditLogService : IAuditLogService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IRequestCache _requestCache;
        private readonly IMapHelper _mapHelper;

        public EmailAuditLogService(LinkoExchangeContext linkoExchangeContext, IRequestCache requestCache,
            IMapHelper mapHelper)
        { 
            _dbContext = linkoExchangeContext;
            _requestCache = requestCache;
            _mapHelper = mapHelper;
        }

        /// <summary>
        /// Write email audit log
        /// </summary>
        /// <param name="logEntry">Email audit log entry</param>
        public void Log(IAuditLogEntry logEntry)
        {
            var emailLogEntryDto = logEntry as EmailAuditLogEntryDto;
            if (emailLogEntryDto == null)
                return;

            var emailLogEntry = _mapHelper.GetEmailAuditLogFromEmailAuditLogEntryDto(emailLogEntryDto);
            emailLogEntry.Token = _requestCache.GetValue(CacheKey.Token) as string;

            this._dbContext.EmailAuditLogs.Add(emailLogEntry);
            this._dbContext.SaveChanges();
        }

    }
}