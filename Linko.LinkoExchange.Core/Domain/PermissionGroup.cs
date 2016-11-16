using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a permission group within a regulatory program for a particular organization.
    /// </summary>
    public partial class PermissionGroup
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int PermissionGroupId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int OrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        // Reverse navigation
        public virtual ICollection<OrganizationRegulatoryProgramUser> OrganizationRegulatoryProgramUsers { get; set; }

        public virtual ICollection<PermissionGroupPermission> PermissionGroupPermissions { get; set; }
    }
}
