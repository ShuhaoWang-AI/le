using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a repudiation reason.
    /// </summary>
    public class RepudiationReason
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int RepudiationReasonId { get; set; }

        /// <summary>
        ///     Unique within a particular OrganizationRegulatoryProgramId.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        ///     Typical value: Authority Regulatory Program id.
        /// </summary>
        public int OrganizationRegulatoryProgramId { get; set; }

        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        #endregion
    }
}