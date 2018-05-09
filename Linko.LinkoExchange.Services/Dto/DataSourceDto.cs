using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class DataSourceDto
    {
        #region public properties

        public int? DataSourceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int OrganizationRegulatoryProgramId { get; set; }

        public DateTime LastModificationDateTimeLocal { get; set; }
        public string LastModifierFullName { get; set; }

        #endregion
    }

    public class DataSourceMonitoringPointDto
    {
        #region public properties
        public int? DataSourceMonitoringPointId { get; set; }
        public int DataSourceId { get; set; }
        public string DataSourceTerm { get; set; }
        public int MonitoringPointId { get; set; }
        #endregion
    }

    public class DataSourceCtsEventTypeDto
    {
        #region public properties
        public int? DataSourceCtsEventTypeId { get; set; }
        public int DataSourceId { get; set; }
        public string DataSourceTerm { get; set; }
        public int CtsEventTypeId { get; set; }
        #endregion
    }

    public class DataSourceCollectionMethodDto
    {
        #region public properties
        public int? DataSourceCollectionMethodId { get; set; }
        public int DataSourceId { get; set; }
        public string DataSourceTerm { get; set; }
        public int CollectionMethodId { get; set; }
        #endregion
    }

    public class DataSourceParameterDto
    {
        #region public properties
        public int? DataSourceParameterId { get; set; }
        public int DataSourceId { get; set; }
        public string DataSourceTerm { get; set; }
        public int ParameterId { get; set; }
        #endregion
    }

    public class DataSourceUnitDto
    {
        #region public properties
        public int? DataSourceUnitId { get; set; }
        public int DataSourceId { get; set; }
        public string DataSourceTerm { get; set; }
        public int UnitId { get; set; }
        #endregion
    }

    public class DataSourceTranslationsDto
    {
        #region public properties
        public DataSourceDto DataSource { get; set; }
        public ICollection<DataSourceMonitoringPointDto> MonitoringPoints { get; set; }
        public ICollection<DataSourceCtsEventTypeDto> CtsEventTypes { get; set; }
        public ICollection<DataSourceCollectionMethodDto> CollectionMethods { get; set; }
        public ICollection<DataSourceParameterDto> Parameters { get; set; }
        public ICollection<DataSourceUnitDto> Units { get; set; }
        #endregion
    }
}
