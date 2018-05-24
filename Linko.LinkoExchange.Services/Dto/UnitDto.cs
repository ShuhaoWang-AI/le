using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class UnitDto
    {
        #region public properties

        public int UnitId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsFlowUnit { get; set; }
        public int OrganizationId { get; set; }
        public bool IsRemoved { get; set; }
        public DateTimeOffset CreationDateTimeUtc { get; set; }
        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }
        public DateTimeOffset? LastModificationDateTimeLocal { get; set; }
        public int? LastModifierUserId { get; set; }
        public string LastModifierFullName { get; set; }
        public int? SystemUnitId { get; set; }
        public SystemUnitDto SystemUnit { get; set; }
        public bool IsAvailableToRegulatee { get; set; }
        public bool IsReviewed { get; set; }

        #endregion
    }
}