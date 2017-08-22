using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using NLog;

namespace Linko.LinkoExchange.Services.AuditLog
{
    /// <summary>
    ///     Email audit log service implementation
    /// </summary>
    public class EmailAuditLogService : IAuditLogService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;
        private readonly IRequestCache _requestCache;

        #endregion

        #region constructors and destructor

        public EmailAuditLogService(
            LinkoExchangeContext linkoExchangeContext,
            IRequestCache requestCache,
            IMapHelper mapHelper,
            ILogger logger)
        {
            _dbContext = linkoExchangeContext;
            _requestCache = requestCache;
            _mapHelper = mapHelper;
            _logger = logger;
        }

        #endregion

        #region interface implementations

        /// <summary>
        ///     Write email audit log
        /// </summary>
        /// <param name="logEntry"> Email audit log entry </param>
        public void Log(IAuditLogEntry logEntry)
        {
            var emailLogEntryDto = logEntry as EmailAuditLogEntryDto;
            if (emailLogEntryDto == null)
            {
                return;
            }

            var emailLogEntry = _mapHelper.GetEmailAuditLogFromEmailAuditLogEntryDto(dto:emailLogEntryDto);
            emailLogEntry.Token = _requestCache.GetValue(key:CacheKey.Token) as string;

            emailLogEntry = _dbContext.EmailAuditLogs.Add(entity:emailLogEntry);
            _dbContext.SaveChanges();

            emailLogEntryDto.EmailAuditLogId = emailLogEntry.EmailAuditLogId;

            _logger.Info(message:$"EmailAuditLogService.Log succeed. Email Audit Log ID: {emailLogEntry.EmailAuditLogId}, Recipient User name:{emailLogEntry.RecipientUserName}, "
                                 + $"Recipient Email Address:{emailLogEntry.RecipientEmailAddress}, Subject:{emailLogEntry.Subject}");
        }

        #endregion
    }
}