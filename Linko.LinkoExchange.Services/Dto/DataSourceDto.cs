using System;

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
}