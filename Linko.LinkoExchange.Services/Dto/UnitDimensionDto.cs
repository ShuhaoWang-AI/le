using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    /// <summary>
    ///     Represents a Unit Dimension.
    /// </summary>
    public class UnitDimensionDto
    {
        #region public properties
        public int UnitDimensionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //public DateTimeOffset CreationDateTimeUtc { get; set; }
        //public DateTimeOffset? LastModificationDateTimeUtc { get; set; }
        public int? LastModifierUserId { get; set; }
        //public string LastModifierFullName { get; set; }

        #endregion
    }
}