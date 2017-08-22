using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class PermissionGroupDto
    {
        #region public properties

        public int? PermissionGroupId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int OrganizationRegulatoryProgramId { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        #endregion
    }
}