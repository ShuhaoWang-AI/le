using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a status of a request for signature.
    /// </summary>
    public partial class SignatoryRequestStatus
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int SignatoryRequestStatusId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }


        // Reverse navigation
        public virtual ICollection<SignatoryRequest> SignatoryRequests { get; set; }
    }
}