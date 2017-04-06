using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a type of an organization.
    /// </summary>
    public partial class OrganizationType
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int OrganizationTypeId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }


        // Reverse navigation
        public virtual ICollection<OrganizationTypeRegulatoryProgram> OrganizationTypeRegulatoryPrograms { get; set; }
    }
}
