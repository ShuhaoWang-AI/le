using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a permitted regulatory program for a particular organization type.
    /// </summary>
    public partial class OrganizationTypeRegulatoryProgram
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int OrganizationTypeRegulatoryProgramId { get; set; }

        public int RegulatoryProgramId { get; set; }
        public virtual RegulatoryProgram RegulatoryProgram { get; set; }

        public int OrganizationTypeId { get; set; }
        public virtual OrganizationType OrganizationType { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }


        // Reverse navigation
        public virtual ICollection<Module> Modules { get; set; }

        public virtual ICollection<PermissionGroupTemplate> PermissionGroupTemplates { get; set; }
    }
}
