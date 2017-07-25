using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Can be used in multiple scenarios that all share the same content replacement items in the
        /// comment field:
        /// - "organizationName"
        /// - "firstName"
        /// - "lastName"
        /// - "userName"
        /// - "emailAddress"
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="orgRegProgram">Target Org Reg Program of the event</param>
        /// <param name="user">Target user of the event</param>
        /// <returns></returns>
        Task SimpleLog(CromerrEvent eventType, OrganizationRegulatoryProgramDto orgRegProgram, UserDto user);

        /// <summary>
        /// Returns Cromerr audit log entries that match search parameters
        /// </summary>
        /// <param name="organizationRegulatoryProgramId">The currently logged in the user's org reg program</param>
        /// <param name="page">Which page being requested</param>
        /// <param name="pageSize">Rows per page</param>
        /// <param name="sortColumn">Which column to sort by</param>
        /// <param name="isSortAscending">Sort direction</param>
        /// <param name="dateRangeStart">For date range search</param>
        /// <param name="dateRangeEnd">For date range search</param>
        /// <param name="dateToExclude">Exclusion date search</param>
        /// <param name="eventCategoryContains">Search parameter</param>
        /// <param name="eventTypeContains">Search parameter</param>
        /// <param name="emailAddressContains">Search parameter</param>
        /// <param name="totalCount">Integer passed out for kendo grid requirement</param>
        /// <returns></returns>
        ICollection<CromerrAuditLogEntryDto> GetCromerrAuditLogEntries(int organizationRegulatoryProgramId,
            int page, int pageSize, string sortColumn, bool isSortAscending,
            DateTime? dateRangeStart, DateTime? dateRangeEnd, DateTime? dateToExclude, 
            string eventCategoryContains, string eventTypeContains, string emailAddressContains,
            out int totalCount);

        /// <summary>
        /// Returns a single Cromerr log entry
        /// </summary>
        /// <param name="cromerrAuditLogId">tCromerrAuditLog.CromerrAuditLogId</param>
        /// <returns></returns>
        CromerrAuditLogEntryDto GetCromerrAuditLogEntry(int cromerrAuditLogId);
    }
}
