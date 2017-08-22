using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     ExchangeReportSync_1.0.
    ///     Represents a simple “flat” export format representing one LinkoExchange report package containing any number of
    ///     report elements,
    ///     but excluding samples and results.
    /// </summary>
    public class LEReportPackageParsedData
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public long LEReportPackageParsedDataID { get; set; }

        /// <summary>
        ///     Assigned by Linko.  This is the version number of this export format.
        /// </summary>
        public string LinkoVersionNo { get; set; }

        /// <summary>
        ///     Assigned by Linko to uniquely identify a client.
        ///     This will be the same number assigned in ts_GlobalDefaultValue.ConfigName =
        ///     LEOrganizationRegulatoryProgramIDForIPP.
        ///     This is the OrganizationRegulatoryProgramID in LE.
        /// </summary>
        public int LinkoClientID { get; set; }

        /// <summary>
        ///     A unique identifier for the Industry for which the Report Package is for,
        ///     it will contain Industry Number (tOrganizationRegulatoryProgram.ReferenceNumber) from LinkoExchange.
        /// </summary>
        public string LEPermitNo { get; set; }

        /// <summary>
        ///     A unique identifier for the Report Package from LinkoExchange.
        /// </summary>
        public int LEReportPackageID { get; set; }

        /// <summary>
        ///     The Report Package name from LinkoExchange.
        /// </summary>
        public string LEReportPackageName { get; set; }

        /// <summary>
        ///     The type of Generic event to be created in Linko; such 1st Quarter PCR, Annual PCR, etc.
        ///     This is taken from the Report Package as configured by the Authority.
        /// </summary>
        public string LEReportPackageCTSEventTypeName { get; set; }

        /// <summary>
        ///     The category of the event to be created in Linko (Generic).
        ///     This is taken from the Report Package as configured by the Authority.
        /// </summary>
        public string LEReportPackageCTSEventCategoryName { get; set; }

        /// <summary>
        ///     The Start Date of the reporting period.
        /// </summary>
        public DateTime LEReportPeriodStartDate { get; set; }

        /// <summary>
        ///     The End Date of the reporting period.
        /// </summary>
        public DateTime LEReportPeriodEndDate { get; set; }

        /// <summary>
        ///     A concatenated and ordered list of Elements that are supposed to be in the Report Package.
        ///     This is created for the Report Package record, not element records.
        /// </summary>
        public string LEReportPackageElements { get; set; }

        /// <summary>
        ///     The comments entered into the Report Package.
        /// </summary>
        public string LEReportPackageSubmissionComments { get; set; }

        /// <summary>
        ///     A concatenated and ordered list of Parameter sampling that did not meet sampling frequency requirements that are in
        ///     the Report Package.
        /// </summary>
        public string LEReportPackageSampleComplianceSummary { get; set; }

        /// <summary>
        ///     The URL to the report package in LinkoExchange.
        /// </summary>
        public string LEReportPackageURL { get; set; }

        /// <summary>
        ///     Date and time the submission was made.
        ///     This is in the configured Timezone for the Authority.
        /// </summary>
        public DateTime LEReportPackageSubmissionDate { get; set; }

        /// <summary>
        ///     The first and last name of the user who submitted the Report Package including a space.
        /// </summary>
        public string LEReportPackageSubmissionBy { get; set; }

        /// <summary>
        ///     Date and time the report package was marked as reviewed.
        ///     This is in the configured Timezone for the Authority.
        /// </summary>
        public DateTime? LEReportPackageReviewedDate { get; set; }

        /// <summary>
        ///     The abbreviation of the Authority user who marked the Report Package reviewed including a space.
        /// </summary>
        public string LEReportPackageReviewedBy { get; set; }

        /// <summary>
        ///     The comments entered by the Authority user who marked the report as reviewed.
        /// </summary>
        public string LEReportPackageReviewedByComments { get; set; }

        /// <summary>
        ///     Date and time the report package was last sent to LinkoCTS.
        ///     This is in the configured Timezone for the Authority.
        /// </summary>
        public DateTime? LEReportPackageLastSentDate { get; set; }

        /// <summary>
        ///     The abbreviation of the Authority user who last sent the Report Package to LinkoCTS including a space.
        /// </summary>
        public string LEReportPackageLastSentToCTSBy { get; set; }

        /// <summary>
        ///     A unique identifier for the Report Package Element from LinkoExchange.
        /// </summary>
        public int? LEReportPackageElementTypeID { get; set; }

        /// <summary>
        ///     The type of event to be created in Linko; such TTO Cert, Lab Analysis Report, etc.
        ///     This is taken from the Report Package Element as configured by the Authority.
        /// </summary>
        public string LEReportPackageElementCTSEventTypeName { get; set; }

        /// <summary>
        ///     The category of the event to be created in Linko (Generic).
        ///     This is taken from the Report Package Element as configured by the Authority.
        /// </summary>
        public string LEReportPackageElementCTSEventCategoryName { get; set; }

        /// <summary>
        ///     One of 2 values that describes the type of report element. Or empty if it is a Report Package.
        /// </summary>
        public string LEReportPackageElementCategoryName { get; set; }

        /// <summary>
        ///     The Report Package Element name from LinkoExchange.
        /// </summary>
        public string LEReportPackageElementTypeName { get; set; }

        /// <summary>
        ///     Whether the Report Package was repudiated by the Industry (use 1 for True or 0 for False).
        /// </summary>
        public bool LEReportPackageIsRepudiated { get; set; }

        /// <summary>
        ///     Date and time the report package was repudiated.
        ///     This is in the configured Timezone for the Authority.
        /// </summary>
        public DateTime? LEReportPackageRepudiatedDate { get; set; }

        /// <summary>
        ///     The name of the user who repudiated the Report Package including a space.
        /// </summary>
        public string LEReportPackageRepudiatedBy { get; set; }

        /// <summary>
        ///     The reason entered by the user who repudiated the report.
        /// </summary>
        public string LEReportPackageRepudiatedByReason { get; set; }

        /// <summary>
        ///     The comments entered by the user who repudiated the report.
        /// </summary>
        public string LEReportPackageRepudiatedByComments { get; set; }

        /// <summary>
        ///     Date and time the repudiated report package was marked as reviewed.
        ///     This is in the configured Timezone for the Authority.
        /// </summary>
        public DateTime? LEReportPackageRepudiationReviewedByDate { get; set; }

        /// <summary>
        ///     The abbreviation of the Authority user who marked the repudiated Report Package as reviewed including a space.
        /// </summary>
        public string LEReportPackageRepudiationReviewedBy { get; set; }

        /// <summary>
        ///     The comments entered by the Authority user who reviewed the repudiated report.
        /// </summary>
        public string LEReportPackageRepudiatedReviewedByComments { get; set; }

        #endregion
    }
}