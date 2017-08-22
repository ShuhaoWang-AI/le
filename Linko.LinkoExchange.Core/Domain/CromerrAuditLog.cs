using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents an audit log for Cromerr.
    /// </summary>
    public class CromerrAuditLog
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int CromerrAuditLogId { get; set; }

        public int AuditLogTemplateId { get; set; }
        public virtual AuditLogTemplate AuditLogTemplate { get; set; }

        public int? RegulatoryProgramId { get; set; }

        public int? OrganizationId { get; set; }

        public int? RegulatorOrganizationId { get; set; }

        public int? UserProfileId { get; set; }

        public string UserName { get; set; }

        public string UserFirstName { get; set; }

        public string UserLastName { get; set; }

        public string UserEmailAddress { get; set; }

        public string IPAddress { get; set; }

        public string HostName { get; set; }

        public string Comment { get; set; }

        public DateTimeOffset LogDateTimeUtc { get; set; }

        #endregion
    }
}