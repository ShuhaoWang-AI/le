using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents the information of an invitation.
    /// </summary>
    public partial class Invitation
    {
        /// <summary>
        /// Primary key.
        /// Guid. So that it can be used in the link whenever applicable inside each email.
        /// </summary>
        public string InvitationId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string EmailAddress { get; set; }

        public DateTimeOffset InvitationDateTimeUtc { get; set; }

        /// <summary>
        /// OrganizationRegulatoryProgram of the sender.
        /// Typical usage: to determine which portal the invitation should be shown.
        /// </summary>
        public int SenderOrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram SenderOrganizationRegulatoryProgram { get; set; }

        /// <summary>
        /// OrganizationRegulatoryProgram where the recipient is invited into.
        /// </summary>
        public int RecipientOrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram RecipientOrganizationRegulatoryProgram { get; set; }

        /// <summary>
        /// True: this is a user profile reset invitation. False, otherwise.
        /// </summary>
        public bool IsResetInvitation { get; set; }
    }
}