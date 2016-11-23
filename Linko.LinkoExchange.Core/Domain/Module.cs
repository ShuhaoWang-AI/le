using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a permitted module within a regulatory program for a particular organization type.
    /// </summary>
    public partial class Module
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int ModuleId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int OrganizationTypeRegulatoryProgramId { get; set; }
        public virtual OrganizationTypeRegulatoryProgram OrganizationTypeRegulatoryProgram { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }


        // Reverse navigation
        public virtual ICollection<OrganizationRegulatoryProgramModule> OrganizationRegulatoryProgramModules { get; set; }

        public virtual ICollection<Permission> Permissions { get; set; }
    }
}