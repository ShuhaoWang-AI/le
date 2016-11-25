using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a jurisdiction, which can be a state, a province or a country.
    /// </summary>
    public partial class Jurisdiction
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int JurisdictionId { get; set; }

        /// <summary>
        /// A JurisdictionId that represents the country.
        /// </summary>
        public int CountryId { get; set; }

        /// <summary>
        /// A JurisdictionId that represents the state.
        /// </summary>
        public int StateId { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// A JurisdictionId that the current Jurisdiction belongs to.
        /// If the current Jurisdiction has no parent (0) then it is a country.
        /// </summary>
        public int? ParentId { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }
}
