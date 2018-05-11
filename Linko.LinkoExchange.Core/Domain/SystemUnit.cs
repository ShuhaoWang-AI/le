using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a System Unit.
    /// </summary>
    public class SystemUnit
    {
        public SystemUnit()
        {
            ConversionFactor = 1.0; // Default value
            AdditiveFactor = 0.0; // Default value
        }

        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int SystemUnitId { get; set; }

        /// <summary>
        ///     Alternate Key
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public int UnitDimensionId { get; set; }

        public virtual UnitDimension UnitDimension { get; set; }

        public double ConversionFactor { get; set; }
        public double AdditiveFactor { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        #endregion
    }
}