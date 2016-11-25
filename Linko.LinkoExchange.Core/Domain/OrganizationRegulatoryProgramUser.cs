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

        /// <summary>
        /// OrganizationRegulatoryProgram of the inviter.
        /// Typical usage: to determine which portal the pending registration should be shown. 
        /// The value is expected to be the same as tInvitation.SenderOrganizationRegulatoryProgramId.
        /// </summary>
        public int InviterOrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram InviterOrganizationRegulatoryProgram { get; set; }

        /// <summary>
        /// The user is not assigned any permission until the registration is approved.
        /// </summary>
        public int? PermissionGroupId { get; set; }
        public virtual PermissionGroup PermissionGroup { get; set; }

        public DateTimeOffset RegistrationDateTimeUtc { get; set; }

        public bool IsRegistrationApproved { get; set; }

        /// <summary>
        /// Typical usage: to prevent showing denied registrants in the registration pending list.
        /// </summary>
        public bool IsRegistrationDenied { get; set; }

        public bool IsEnabled { get; set; }

        /// <summary>
        /// Soft delete the user in order to keep all the history intact.
        /// </summary>
        public bool IsRemoved { get; set; }

        public bool IsSignatory { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }


        // Reverse navigation
        public virtual ICollection<SignatoryRequest> SignatoryRequests { get; set; }
    }
}