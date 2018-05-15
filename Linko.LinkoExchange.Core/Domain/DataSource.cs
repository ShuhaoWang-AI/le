using System;

namespace Linko.LinkoExchange.Core.Domain
{
    public class DataSource
    {
        #region public properties

        public int DataSourceId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int OrganizationRegulatoryProgramId { get; set; }

        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        #endregion
    }
}