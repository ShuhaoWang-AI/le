using System;

namespace Linko.LinkoExchange.Services.Dto
{
    /// <summary>
    ///     Represents a System Unit.
    /// </summary>
    public class SystemUnitDto
    {
        public SystemUnitDto()
        {
            ConversionFactor = 1.0; // Default value
            AdditiveFactor = 0.0; // Default value
        }

        #region public properties
        public int SystemUnitId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int UnitDimensionId { get; set; }
        public UnitDimensionDto UnitDimension { get; set; }
        public double ConversionFactor { get; set; }
        public double AdditiveFactor { get; set; }
        public DateTimeOffset CreationDateTimeUtc { get; set; }
        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }
        public int? LastModifierUserId { get; set; }
        public string LastModifierFullName { get; set; }

        #endregion
    }
}