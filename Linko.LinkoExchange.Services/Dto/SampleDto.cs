using Linko.LinkoExchange.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
[assembly: InternalsVisibleTo("Linko.LinkoExchange.Test")]

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
        public DateTimeOffset StartDateTime { get; set; }
        public DateTimeOffset EndDateTime { get; set; }

        public DateTime StartDateTimeLocal { get; set; }
        public DateTime EndDateTimeLocal { get; set; }
        public bool IsReadyToReport { get; set; }

        public IEnumerable<UnitDto> FlowUnitValidValues { get; set; }
        public string ResultQualifierValidValues { get; set; }
        public double? MassLoadingConversionFactorPounds { get; set; }
        public int? MassLoadingCalculationDecimalPlaces { get; set; }
        public bool IsMassLoadingResultToUseLessThanSign { get; set; }

        public SampleStatusName SampleStatusName { get; set; }
        public DateTime LastModificationDateTimeLocal { get; set; }
        public string LastModifierFullName { get; set; }
        public string FlowValue { get; set; }
        public int? FlowUnitId { get; set; }
        public string FlowUnitName { get; set; }
        public IEnumerable<SampleResultDto> SampleResults { get; set; }
        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgramDto { get; internal set; }
    }
}

