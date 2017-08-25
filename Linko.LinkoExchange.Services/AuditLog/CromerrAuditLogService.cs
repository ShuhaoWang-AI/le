using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using NLog;

namespace Linko.LinkoExchange.Services.AuditLog
{
    public class CromerrAuditLogService : ICromerrAuditLogService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContext;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;

        #endregion

        #region constructors and destructor

        public CromerrAuditLogService(
            LinkoExchangeContext linkoExchangeContext,
            IMapHelper mapHelper,
            IHttpContextService httpContext,
            ILogger logger)
        {
            _dbContext = linkoExchangeContext;
            _mapHelper = mapHelper;
            _httpContext = httpContext;
            _logger = logger;
        }

        #endregion

        #region interface implementations

        public async Task Log(CromerrEvent eventType, CromerrAuditLogEntryDto dto, IDictionary<string, string> contentReplacements)
        {
            var auditLogTemplate = await GetAuditLogTemplate(eventType:eventType);
            if (auditLogTemplate == null)
            {
                return;
            }
            
            var comment = await GetComment(commentTemplate:auditLogTemplate, replacements:contentReplacements);

            var cromerrAuditLogEntry = _mapHelper.GetCromerrAuditLogFromCromerrAuditLogEntryDto(dto:dto);
            cromerrAuditLogEntry.AuditLogTemplateId = auditLogTemplate.AuditLogTemplateId;
            cromerrAuditLogEntry.Comment = comment;
            cromerrAuditLogEntry.LogDateTimeUtc = DateTimeOffset.Now;

            _dbContext.CromerrAuditLogs.Add(entity:cromerrAuditLogEntry);
            
            try
            {
                _dbContext.SaveChanges();
            }
            catch
            {
                _logger.Info(message:$"CromerrAuditLogService.Log. eventType={eventType}.");
                throw;
            }
        }

        public async Task SimpleLog(CromerrEvent eventType, OrganizationRegulatoryProgramDto orgRegProgram, UserDto user)
        {
            var auditLogTemplate = await GetAuditLogTemplate(eventType:eventType);
            if (auditLogTemplate == null)
            {
                return;
            }

            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                          {
                                              RegulatoryProgramId = orgRegProgram.RegulatoryProgramId,
                                              OrganizationId = orgRegProgram.OrganizationId,
                                              RegulatorOrganizationId = orgRegProgram.RegulatorOrganizationId ?? orgRegProgram.OrganizationId,
                                              UserProfileId = user.UserProfileId,
                                              UserName = user.UserName,
                                              UserFirstName = user.FirstName,
                                              UserLastName = user.LastName,
                                              UserEmailAddress = user.Email,
                                              IPAddress = _httpContext.CurrentUserIPAddress(),
                                              HostName = _httpContext.CurrentUserHostName()
                                          };

            var contentReplacements = new Dictionary<string, string>
                                      {
                                          {"organizationName", orgRegProgram.OrganizationDto.OrganizationName},
                                          {"firstName", user.FirstName},
                                          {"lastName", user.LastName},
                                          {"userName", user.UserName},
                                          {"emailAddress", user.Email}
                                      };

            var comment = await GetComment(commentTemplate:auditLogTemplate, replacements:contentReplacements);

            var cromerrAuditLogEntry = _mapHelper.GetCromerrAuditLogFromCromerrAuditLogEntryDto(dto:cromerrAuditLogEntryDto);
            cromerrAuditLogEntry.AuditLogTemplateId = auditLogTemplate.AuditLogTemplateId;
            cromerrAuditLogEntry.Comment = comment;
            cromerrAuditLogEntry.LogDateTimeUtc = DateTimeOffset.Now;

            _dbContext.CromerrAuditLogs.Add(entity:cromerrAuditLogEntry);
            _dbContext.SaveChanges();
        }

        public ICollection<CromerrAuditLogEntryDto> GetCromerrAuditLogEntries(int organizationRegulatoryProgramId, int page, int pageSize, string sortColumn, bool isSortAscending,
                                                                              DateTime? dateRangeStart, DateTime? dateRangeEnd, DateTime? dateToExclude,
                                                                              string eventCategoryContains, string eventTypeContains, string emailAddressContains,
                                                                              out int totalCount)
        {
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms
                                          .Include(path:"RegulatoryProgram")
                                          .Single(o => o.OrganizationRegulatoryProgramId == organizationRegulatoryProgramId);
            var authorityOrganizationId = orgRegProgram.RegulatorOrganizationId ?? orgRegProgram.OrganizationId;
            var authorityName = _dbContext.Organizations.Single(o => o.OrganizationId == authorityOrganizationId).Name;
            var dtos = new List<CromerrAuditLogEntryDto>();
            var entries = _dbContext.CromerrAuditLogs.Where(l => l.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId
                                                                 && l.RegulatorOrganizationId == orgRegProgram.OrganizationId);

            //Sort is required by EF before paging operations
            if (isSortAscending)
            {
                switch (sortColumn)
                {
                    case "LogDateTimeUtc":
                        entries = entries.OrderBy(entry => entry.LogDateTimeUtc);
                        break;
                    default:
                        entries = entries.OrderBy(entry => entry.LogDateTimeUtc);
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "LogDateTimeUtc":
                        entries = entries.OrderByDescending(entry => entry.LogDateTimeUtc);
                        break;
                    default:
                        entries = entries.OrderByDescending(entry => entry.LogDateTimeUtc);
                        break;
                }
            }

            if (dateRangeStart.HasValue)
            {
                entries = entries.Where(e => e.LogDateTimeUtc > dateRangeStart.Value);
            }

            if (dateRangeEnd.HasValue)
            {
                entries = entries.Where(e => e.LogDateTimeUtc < dateRangeEnd.Value);
            }

            if (dateToExclude.HasValue)
            {
                var endOfDateToExclude = dateToExclude.Value.AddDays(value:1);
                entries = entries.Where(e => e.LogDateTimeUtc < dateToExclude || e.LogDateTimeUtc > endOfDateToExclude);
            }

            if (!string.IsNullOrEmpty(value:eventCategoryContains))
            {
                // ReSharper disable once ArgumentsStyleNamedExpression
                entries = entries.Where(e => e.AuditLogTemplate.EventCategory.Contains(eventCategoryContains));
            }

            if (!string.IsNullOrEmpty(value:eventTypeContains))
            {
                // ReSharper disable once ArgumentsStyleNamedExpression
                entries = entries.Where(e => e.AuditLogTemplate.EventType.Contains(eventTypeContains));
            }

            if (!string.IsNullOrEmpty(value:emailAddressContains))
            {
                // ReSharper disable once ArgumentsStyleNamedExpression
                entries = entries.Where(e => e.UserEmailAddress.Contains(emailAddressContains));
            }

            //Apply paging
            totalCount = entries.Count();
            entries = entries.Skip(count:(page - 1) * pageSize);
            entries = entries.Take(count:pageSize);

            foreach (var entry in entries)
            {
                var dto = _mapHelper.GetCromerrAuditLogDtoFromCromerrAuditLog(cromerrAuditLog:entry);
                dto.OrganizationName = _dbContext.Organizations.Single(o => o.OrganizationId == dto.OrganizationId).Name;
                dto.RegulatorOrganizationName = authorityName;
                dto.RegulatoryProgramName = orgRegProgram.RegulatoryProgram.Name;

                dtos.Add(item:dto);
            }

            return dtos;
        }

        public CromerrAuditLogEntryDto GetCromerrAuditLogEntry(int cromerrAuditLogId)
        {
            var entry = _dbContext.CromerrAuditLogs.Single(l => l.CromerrAuditLogId == cromerrAuditLogId);
            return _mapHelper.GetCromerrAuditLogDtoFromCromerrAuditLog(cromerrAuditLog:entry);
        }

        #endregion

        private Task<AuditLogTemplate> GetAuditLogTemplate(CromerrEvent eventType)
        {
            var auditLogTemplateName = string.Format(format:"CromerrEvent_{0}", arg0:eventType);
            return Task.FromResult(result:_dbContext.AuditLogTemplates.First(i => i.Name == auditLogTemplateName));
        }

        private Task<string> GetComment(AuditLogTemplate commentTemplate,
                                        IDictionary<string, string> replacements)
        {
            var keyValues = replacements.Select(i => new KeyValuePair<string, string>(key:"{" + i.Key + "}", value:i.Value));

            replacements = keyValues.ToDictionary(i => i.Key, i => i.Value);

            var comment = commentTemplate.MessageTemplate;
            foreach (var pair in replacements)
            {
                comment = comment.Replace(oldValue:pair.Key, newValue:pair.Value);
            }

            return Task.FromResult(result:comment);
        }
    }
}