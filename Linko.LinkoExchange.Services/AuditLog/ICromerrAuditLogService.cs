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
        /// <param name="eventType">The event that occured that requires logging</param>
        /// <param name="dto">Id fields to populate a row</param>
        /// <param name="contentReplacements">Words to populate comment field</param>
        /// <returns></returns>
        Task Log(CromerrEvent eventType, CromerrAuditLogEntryDto dto, IDictionary<string, string> contentReplacements);

        Task SimpleLog(CromerrEvent eventType, OrganizationRegulatoryProgramDto orgRegProgram, UserDto user);

        ICollection<CromerrAuditLogEntryDto> GetCromerrAuditLogEntries(int organizationRegulatoryProgramId,
            int page, int pageSize, string sortColumn, bool isSortAscending,
            DateTime? dateRangeStart, DateTime? dateRangeEnd, DateTime? dateToExclude, 
            string eventCategoryContains, string eventTypeContains, string emailAddressContains,
            out int totalCount);

        CromerrAuditLogEntryDto GetCromerrAuditLogEntry(int cromerrAuditLogId);
    }
}
