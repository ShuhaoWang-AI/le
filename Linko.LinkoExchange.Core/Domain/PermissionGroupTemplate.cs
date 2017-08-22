using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a system-default permission group.
    /// </summary>
    public class PermissionGroupTemplate
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int PermissionGroupTemplateId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int OrganizationTypeRegulatoryProgramId { get; set; }
        public virtual OrganizationTypeRegulatoryProgram OrganizationTypeRegulatoryProgram { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        // Reverse navigation
        public virtual ICollection<PermissionGroupTemplatePermission> PermissionGroupTemplatePermissions { get; set; }

        #endregion
    }
}