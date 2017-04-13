using Linko.LinkoExchange.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class SampleDto
    {
        public int? SampleId { get; set; }
        public string Name { get; set; } //UI should only read this as it is automatically set based on settings
        public int MonitoringPointId { get; set; }
        public string MonitoringPointName { get; set; }
        public int CtsEventTypeId { get; set; }
        public string CtsEventTypeName { get; set; }
        public string CtsEventCategoryName { get; set; }
        public int CollectionMethodId { get; set; }
        public string CollectionMethodName { get; set; }
        public string LabSampleIdentifier { get; set; }
        public DateTime StartDateTimeLocal { get; set; }
        public DateTime EndDateTimeLocal { get; set; }
        public bool IsCalculated { get; set; }
        public bool IsReadyToReport { get; set; }
        public SampleStatusName SampleStatusName { get; set; }
        //public int OrganizationTypeId { get; set; }
        //public int OrganizationRegulatoryProgramId { get; set; }
        public DateTime LastModificationDateTimeLocal { get; set; }
        public string LastModifierFullName { get; set; }
        
        public double? FlowValue { get; set; }
        public int? FlowValueDecimalPlaces { get; set; }
        public int? FlowUnitId { get; set; }
        public string FlowUnitName { get; set; }
        public IEnumerable<SampleResultDto> SampleResults { get; set; }
    }
}

