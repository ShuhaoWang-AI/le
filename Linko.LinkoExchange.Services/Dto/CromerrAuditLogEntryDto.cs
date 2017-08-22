using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class CromerrAuditLogEntryDto : IAuditLogEntry
    {
        #region interface implementations

        public int AuditLogTemplateId { get; set; }

        #endregion

        #region public properties

        public int CromerrAuditLogId { get; set; }
        public int? RegulatoryProgramId { get; set; }
        public string RegulatoryProgramName { get; set; }
        public int? OrganizationId { get; set; }
        public int? RegulatorOrganizationId { get; set; }
        public string RegulatorOrganizationName { get; set; }
        public int? UserProfileId { get; set; }
        public string EventCategory { get; set; }
        public string EventType { get; set; }
        public string UserName { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserEmailAddress { get; set; }
        public string IPAddress { get; set; }
        public string HostName { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset LogDateTimeUtc { get; set; }
        public string OrganizationName { get; set; }

        #endregion
    }
}