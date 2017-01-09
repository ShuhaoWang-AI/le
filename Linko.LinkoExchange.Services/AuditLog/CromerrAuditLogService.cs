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
using NLog;

namespace Linko.LinkoExchange.Services.AuditLog
{ 
    public class CromerrAuditLogService : ICromerrAuditLogService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IRequestCache _requestCache;
        private readonly IMapHelper _mapHelper;
        private readonly IHttpContextService _httpContext;
        private readonly ILogger _logService;

        public CromerrAuditLogService(LinkoExchangeContext linkoExchangeContext, IRequestCache requestCache,
            IMapHelper mapHelper, IHttpContextService httpContext, ILogger logService)
        {
            _dbContext = linkoExchangeContext;
            _requestCache = requestCache;
            _mapHelper = mapHelper;
            _httpContext = httpContext;
            _logService = logService;
        }

        public async Task Log(CromerrEvent eventType, CromerrAuditLogEntryDto dto, IDictionary<string, string> contentReplacements)
        {
            var auditLogTemplate = await GetAuditLogTemplate(eventType);
            if (auditLogTemplate == null) return;

            string comment = await GetComment(auditLogTemplate, contentReplacements);

            CromerrAuditLog cromerrAuditLogEntry = _mapHelper.GetCromerrAuditLogFromCromerrAuditLogEntryDto(dto);
            cromerrAuditLogEntry.AuditLogTemplateId = auditLogTemplate.AuditLogTemplateId;
            cromerrAuditLogEntry.Comment = comment;
            cromerrAuditLogEntry.LogDateTimeUtc = DateTime.UtcNow;

            this._dbContext.CromerrAuditLogs.Add(cromerrAuditLogEntry);

            try
            {
                this._dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logService.Info($"CromerrAuditLogService.Log. eventType={eventType}.");
                throw;
            }

        }

        public async Task SimpleLog(CromerrEvent eventType, OrganizationRegulatoryProgramDto orgRegProgram, UserDto user)
        {
            var auditLogTemplate = await GetAuditLogTemplate(eventType);
            if (auditLogTemplate == null) return;

            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
            cromerrAuditLogEntryDto.RegulatoryProgramId = orgRegProgram.RegulatoryProgramId;
            cromerrAuditLogEntryDto.OrganizationId = orgRegProgram.OrganizationId;
            cromerrAuditLogEntryDto.RegulatorOrganizationId = orgRegProgram.RegulatorOrganizationId;
            cromerrAuditLogEntryDto.UserProfileId = user.UserProfileId;
            cromerrAuditLogEntryDto.UserName = user.UserName;
            cromerrAuditLogEntryDto.UserFirstName = user.FirstName;
            cromerrAuditLogEntryDto.UserLastName = user.LastName;
            cromerrAuditLogEntryDto.UserEmailAddress = user.Email;
            cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
            cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();

            var contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add("organizationName", orgRegProgram.OrganizationDto.OrganizationName);
            contentReplacements.Add("firstName", user.FirstName);
            contentReplacements.Add("lastName", user.LastName);
            contentReplacements.Add("userName", user.UserName);
            contentReplacements.Add("emailAddress", user.Email);

            string comment = await GetComment(auditLogTemplate, contentReplacements);

            CromerrAuditLog cromerrAuditLogEntry = _mapHelper.GetCromerrAuditLogFromCromerrAuditLogEntryDto(cromerrAuditLogEntryDto);
            cromerrAuditLogEntry.AuditLogTemplateId = auditLogTemplate.AuditLogTemplateId;
            cromerrAuditLogEntry.Comment = comment;
            cromerrAuditLogEntry.LogDateTimeUtc = DateTime.UtcNow;

            this._dbContext.CromerrAuditLogs.Add(cromerrAuditLogEntry);
            this._dbContext.SaveChanges();

        }

        private Task<AuditLogTemplate> GetAuditLogTemplate(CromerrEvent eventType)
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
                comment = comment.Replace(pair.Key, pair.Value);
            }

            return Task.FromResult(comment);
        }
    }
}
