using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a Sample.
    /// </summary>
    public class Sample
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int SampleId { get; set; }

        public string Name { get; set; }

        public int MonitoringPointId { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string MonitoringPointName { get; set; }

        public int CtsEventTypeId { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string CtsEventTypeName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string CtsEventCategoryName { get; set; }

        public int CollectionMethodId { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string CollectionMethodName { get; set; }

        public string LabSampleIdentifier { get; set; }

        public DateTimeOffset StartDateTimeUtc { get; set; }

        public DateTimeOffset EndDateTimeUtc { get; set; }

        /// <summary>
        ///     True: the sample is system generated either through overnight process or some other processes.
        /// </summary>
        public bool IsSystemGenerated { get; set; }

        public bool IsReadyToReport { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string FlowUnitValidValues { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string ResultQualifierValidValues { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public double? MassLoadingConversionFactorPounds { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public int? MassLoadingCalculationDecimalPlaces { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public bool IsMassLoadingResultToUseLessThanSign { get; set; }

        /// <summary>
        ///     Which organization who takes this sample,
        ///     i.e. it will be populated with Authority OrganizationRegulatoryProgramId if it is the Authority who takes it or
        ///     Industry OrganizationRegulatoryProgramId if it is the Industry who takes it.
        ///     Typical value: Industry Regulatory Program id.May be the Authority in the future.
        /// </summary>
        public int ByOrganizationRegulatoryProgramId { get; set; }

        public virtual OrganizationRegulatoryProgram ByOrganizationRegulatoryProgram { get; set; }

        /// <summary>
        ///     For which organization this sample is submitted on behalf of.
        ///     Typical value: Industry Regulatory Program id.
        /// </summary>
        public int ForOrganizationRegulatoryProgramId { get; set; }

        public virtual OrganizationRegulatoryProgram ForOrganizationRegulatoryProgram { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        // Reverse navigation
        public virtual ICollection<SampleResult> SampleResults { get; set; }

        public virtual ICollection<ReportSample> ReportSamples { get; set; }

        #endregion
    }
}