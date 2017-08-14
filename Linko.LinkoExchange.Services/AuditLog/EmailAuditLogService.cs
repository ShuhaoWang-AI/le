using System;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using NLog;

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
        private readonly ILogger _logger;
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

        /// <summary>
        /// Write email audit log
        /// </summary>
        /// <param name="logEntry">Email audit log entry</param>
        public void Log(IAuditLogEntry logEntry)
        { 
            var emailLogEntryDto = logEntry as EmailAuditLogEntryDto;
            if (emailLogEntryDto == null)
            {
                return;
            }    

            var emailLogEntry = _mapHelper.GetEmailAuditLogFromEmailAuditLogEntryDto(emailLogEntryDto);
            emailLogEntry.Token = _requestCache.GetValue(CacheKey.Token) as string;

            emailLogEntry = _dbContext.EmailAuditLogs.Add(emailLogEntry);
            _dbContext.SaveChanges(); 

            emailLogEntryDto.EmailAuditLogId = emailLogEntry.EmailAuditLogId; 

            _logger.Info($"EmailAuditLogService.Log succeed. Email Audit Log ID: {emailLogEntry.EmailAuditLogId}, Recipient Username:{emailLogEntry.RecipientUserName}, Recipient Email Address:{emailLogEntry.RecipientEmailAddress}, Subject:{emailLogEntry.Subject}");
        } 
    }
}