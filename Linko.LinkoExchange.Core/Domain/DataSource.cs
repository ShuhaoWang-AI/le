using System;
using System.Collections.Generic;

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

        // Reverse navigation
        public virtual ICollection<DataSourceCollectionMethod> DataSourceCollectionMethods { get; set; }
        public virtual ICollection<DataSourceMonitoringPoint> DataSourceMonitoringPoints { get; set; }
        public virtual ICollection<DataSourceCtsEventType> DataSourceCtsEventTypes { get; set; }
        public virtual ICollection<DataSourceParameter> DataSourceParameters { get; set; }
        public virtual ICollection<DataSourceUnit> DataSourceUnits { get; set; }

        #endregion
    }
}