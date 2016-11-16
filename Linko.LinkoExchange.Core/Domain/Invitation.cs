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

        public int SenderOrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram SenderOrganizationRegulatoryProgram { get; set; }

        public int RecipientOrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram RecipientOrganizationRegulatoryProgram { get; set; }
    }
}