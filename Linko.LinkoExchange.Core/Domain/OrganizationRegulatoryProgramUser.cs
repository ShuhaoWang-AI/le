using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a user within a regulatory program for a particular organization.
    /// </summary>
    public partial class OrganizationRegulatoryProgramUser
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int OrganizationRegulatoryProgramUserId { get; set; }

        public int UserProfileId { get; set; }
        //public virtual UserProfile UserProfile { get; set; }

        public int OrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public int? PermissionGroupId { get; set; }
        public virtual PermissionGroup PermissionGroup { get; set; }

        public DateTimeOffset RegistrationDateTimeUtc { get; set; }

        public bool IsRegistrationApproved { get; set; }

        public bool IsRegistrationDenied { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsRemoved { get; set; }

        public bool IsSignatory { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }


        // Reverse navigation
        public virtual ICollection<SignatoryRequest> SignatoryRequests { get; set; }
    }
}