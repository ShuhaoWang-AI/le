using System;
using System.Collections.Generic;
using Linko.LinkoExchange.Core.Domain;

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

        public ICollection<DataSourceTranslationDto> DataSourceCollectionMethods { get; set; }
        public ICollection<DataSourceTranslationDto> DataSourceMonitoringPoints { get; set; }
        public ICollection<DataSourceTranslationDto> DataSourceSampleTypes { get; set; }
        public ICollection<DataSourceTranslationDto> DataSourceParameters { get; set; }
        public ICollection<DataSourceTranslationDto> DataSourceUnits { get; set; }

        #endregion
    }

    public class DataSourceTranslationDto
    {
        #region public properties
        public int? Id { get; set; }
        public int DataSourceId { get; set; }
        public string DataSourceTerm { get; set; }
        public DataSourceTranslationItemDto TranslationItem { get; set; }
        #endregion
    }

    public class DataSourceTranslationItemDto
    {
        #region public properties
        public int TranslationId { get; set; }
        public DataSourceTranslationType TranslationType { get; set; }
        public string TranslationName { get; set; }
        #endregion
    }
}
