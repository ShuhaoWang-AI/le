using Linko.LinkoExchange.Services.Dto;
using System;
using System.Web;
using Linko.LinkoExchange.Data;
using System.Threading.Tasks;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Core.Enum;
using System.Collections.Generic;
using System.Linq;

namespace Linko.LinkoExchange.Services.AuditLog
{ 
    public class CromerrAuditLogService : ICromerrAuditLogService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IRequestCache _requestCache;
        private readonly IMapHelper _mapHelper;

        public CromerrAuditLogService(LinkoExchangeContext linkoExchangeContext, IRequestCache requestCache,
            IMapHelper mapHelper)
        {
            _dbContext = linkoExchangeContext;
            _requestCache = requestCache;
            _mapHelper = mapHelper;
        }

        public async Task Log(EmailType eventType, CromerrAuditLogEntryDto dto, IDictionary<string, string> contentReplacements)
        {
            var auditLogTemplate = await GetAuditLogTemplate(eventType);
            if (auditLogTemplate == null) return;

            string comment = await GetComment(auditLogTemplate, contentReplacements);

            CromerrAuditLog cromerrAuditLogEntry = _mapHelper.GetCromerrAuditLogFromCromerrAuditLogEntryDto(dto);
            cromerrAuditLogEntry.AuditLogTemplateId = auditLogTemplate.AuditLogTemplateId;
            cromerrAuditLogEntry.Comment = comment;
            cromerrAuditLogEntry.LogDateTimeUtc = DateTime.UtcNow;

            this._dbContext.CromerrAuditLogs.Add(cromerrAuditLogEntry);
            this._dbContext.SaveChanges();

        }

        private Task<AuditLogTemplate> GetAuditLogTemplate(EmailType eventType)
        {
            var auditLogTemplateName = string.Format("CromerrEvent_{0}", eventType.ToString());
            return Task.FromResult(_dbContext.AuditLogTemplates.First(i => i.Name == auditLogTemplateName));
        }

        private Task<string> GetComment(AuditLogTemplate commentTemplate,
           IDictionary<string, string> replacements)
        {
            var keyValues = replacements.Select(i =>
            {
                return new KeyValuePair<string, string>("{" + i.Key + "}", i.Value);
            });

            replacements = keyValues.ToDictionary(i => i.Key, i => i.Value);

            var comment = commentTemplate.MessageTemplate;
            foreach (var pair in replacements)
            {
                comment.Replace(pair.Key, pair.Value);
            }

            return Task.FromResult(comment);
        }
    }
}
