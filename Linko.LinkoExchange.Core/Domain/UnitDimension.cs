using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a Unit Dimension.
    /// </summary>
    public class UnitDimension
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int UnitDimensionId { get; set; }

        /// <summary>
        ///     Alternate Key
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
        public ICollection<SystemUnit> SystemUnits { get; set; }

        #endregion
    }
}