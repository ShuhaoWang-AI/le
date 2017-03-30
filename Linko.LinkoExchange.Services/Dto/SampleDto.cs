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
        public string Name { get; set; }
        public int MonitoringPointId { get; set; }
        public string MonitoringPointName { get; set; }
        public int CtsEventTypeId { get; set; }
        public string CtsEventTypeName { get; set; }
        public string CtsEventCategoryName { get; set; }
        public DateTime SampleStartDateLocal { get; set; }
        public DateTime SampleEndDateLocal { get; set; }
        public int CollectionMethodId { get; set; }
        public string CollectionMethodName { get; set; }
        public DateTime LastModificationDateTimeLocal { get; set; }
        public string LastModifierFullName { get; set; }
        public int SampleStatusId { get; set; }
        public string SampleStatusName { get; set; }
        public string LabSampleId { get; set; }
		public double? MassLoadingConversionFactor { get; set; }
        public int MassLoadingUnitId { get; set; }
        public IEnumerable<SampleResultDto> SampleResults { get; set; }
    }
}

