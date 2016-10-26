﻿using Linko.LinkoExchange.Core.Common;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using System;

namespace Linko.LinkoExchange.Services.AuditLog
{
    /// <summary>
    /// Email audit log service implementation
    /// </summary>
    public class EmailAuditLogService : IAuditLogService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IRequestCache _requestCache; 

        public EmailAuditLogService(LinkoExchangeContext linkoExchangeContext, IRequestCache requestCache)
        { 
            _dbContext = linkoExchangeContext;
            _requestCache = requestCache; 
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

            var emailLogEntry = AutoMapper.Mapper.Map<EmailAuditLog>(emailLogEntryDto);
            emailLogEntry.Token = _requestCache.GetValue(CacheKey.Token) as string;
            emailLogEntry.SenderUserProfileId = _requestCache.GetValue(CacheKey.EmailSenderUserProfileId) == null ? null : (int?)(ValueParser.TryParseInt(_requestCache.GetValue(CacheKey.EmailSenderUserProfileId) as string, 0));
            emailLogEntry.RecipientUserProfileId = _requestCache.GetValue(CacheKey.EmailRecipientUserProfileId) == null ? null : (int?)(ValueParser.TryParseInt(_requestCache.GetValue(CacheKey.EmailRecipientUserProfileId) as string, 0));

            this._dbContext.EmailAuditLog.Add(emailLogEntry);
            this._dbContext.SaveChangesAsync();
        }
    }
}