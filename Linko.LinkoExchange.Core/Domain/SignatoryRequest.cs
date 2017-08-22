using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents the information related to a request for signatory.
    /// </summary>
    public class SignatoryRequest
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int SignatoryRequestId { get; set; }

        public DateTimeOffset RequestDateTimeUtc { get; set; }

        /// <summary>
        ///     Signatory Status=Granted: it is the date and time the signatory right request is granted.
        ///     Signatory Status=Denied: it is the date and time the signatory right request is denied.
        /// </summary>
        public DateTimeOffset GrantDenyDateTimeUtc { get; set; }

        public DateTimeOffset RevokeDateTimeUtc { get; set; }

        public int OrganizationRegulatoryProgramUserId { get; set; }
        public virtual OrganizationRegulatoryProgramUser OrganizationRegulatoryProgramUser { get; set; }

        public int SignatoryRequestStatusId { get; set; }
        public virtual SignatoryRequestStatus SignatoryRequestStatus { get; set; }

        #endregion
    }
}