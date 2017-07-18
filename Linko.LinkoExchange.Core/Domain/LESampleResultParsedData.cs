using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// ExchangeLabSync_1.0.
    /// Represents a simple “flat” export format representing one or more LinkoExchange Report Packages 
    ///  containing any number of samples and results, but excluding other report package elements.
    /// </summary>
    public partial class LESampleResultParsedData
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public long LESampleResultParsedDataID { get; set; }

        /// <summary>
        /// Assigned by Linko. 
        /// This is the version number of this export format.
        /// </summary>
        public string LinkoVersionNo { get; set; }

        /// <summary>
        /// Assigned by Linko to uniquely identify a client.
        /// This will be the same number assigned in ts_GlobalDefaultValue.ConfigName = LEOrganizationRegulatoryProgramIDForIPP.
        /// This is the OrganizationRegulatoryProgramID in LE.
        /// </summary>
        public int LinkoClientID { get; set; }

        /// <summary>
        /// A unique identifier for the location from which the sample was taken, 
        ///  it will contain Industry Name and Monitoring Point Name (Abbrv).
        /// </summary>
        public string LabPermitMPID { get; set; }

        /// <summary>
        /// Sample name is taken from the Sample and is dynamically created based on Authority configurations 
        ///  and is not visible to the Industry during data entry.
        /// </summary>
        public string LabSampleName { get; set; }

        /// <summary>
        /// Date of sample collection. 
        /// This is taken from the SampleStartDateTimeUtc but without a time. 
        /// This is in the configured Timezone for the Authority.
        /// </summary>
        public DateTime LabDateSampled { get; set; }

        /// <summary>
        /// Date and time sample collection was started.
        /// This is taken from the SampleStartDateTimeUtc. 
        /// This is in the configured Timezone for the Authority.
        /// </summary>
        public DateTime? LabStartDateTimeSampled { get; set; }

        /// <summary>
        /// Date and time sample collection was completed.
        /// This is taken from the SampleEndDateTimeUtc.
        /// This is in the configured Timezone for the Authority.
        /// </summary>
        public DateTime? LabStopDateTimeSampled { get; set; }

        /// <summary>
        /// LabSampleIdentifier taken from the Sample as entered by the Industry.
        /// </summary>
        public string LabSampleID { get; set; }

        /// <summary>
        /// The type of sample to be created in Linko; such IU_SAMPLE or AUTH_SAMPLE. 
        /// This is taken from Sample CTSEventTypeName as selected by the Industry during data entry.
        /// </summary>
        public string LabSampleEventType { get; set; }

        /// <summary>
        /// Sample Collection Method. 
        /// This is taken from the Sample CollectionMethodName as selected by the Industry during data entry.
        /// </summary>
        public string LabCollectMethod { get; set; }

        /// <summary>
        /// Person or Entity who collected the sample from the sampling point. 
        /// Since we are not capturing this from Industries, it will always be NULL.
        /// </summary>
        public string LabSampler { get; set; }

        /// <summary>
        /// Date and/or date and time sample was analyzed by the lab. 
        /// Since we are not capturing this from Industries, it will always be NULL.
        /// This is in the configured Timezone for the Authority.
        /// </summary>
        public DateTime? LabAnalysisDate { get; set; }

        /// <summary>
        /// EPA analysis method. 
        /// This is taken from Sample Result AnalysisMethod as entered by the Industry during data entry.
        /// </summary>
        public string LabAnalysisMethod { get; set; }

        /// <summary>
        /// Analyte/Pollutant/Parameter name. 
        /// This is taken from Sample Result ParameterName as selected by the Industry during data entry.
        /// </summary>
        public string LabParamName { get; set; }

        /// <summary>
        /// Concentration Analytical Result and/or Qualifier (includes "ND", "NA",  "(less than sign) 200").
        /// This is taken from Sample Result Qualifier and/or Value as entered by the Industry during data entry.
        /// Mass Results are always calculated in LabSync.
        /// </summary>
        public string LabResult { get; set; }

        /// <summary>
        /// Numeric equivalent of the LabResult; must be a number.
        /// This is taken from Sample Result Value as entered by the Industry during data entry.
        /// </summary>
        public double? LabNumResult { get; set; }

        /// <summary>
        /// Lab flag which qualifies LabResult field. 
        /// Since we are not capturing this from Industries, it will always be NULL.
        /// </summary>
        public string LabResultFlag { get; set; }

        /// <summary>
        /// Units of measure for Result, RepLim and MDL.  
        /// This is taken from Sample Result UnitName as entered by the Industry during data entry.
        /// </summary>
        public string LabResultUnits { get; set; }

        /// <summary>
        /// Reporting Limit for LabParamName.
        /// Since we are not capturing this from Industries, it will always be NULL.
        /// </summary>
        public string LabRepLimit { get; set; }

        /// <summary>
        /// Method Detection Limit.
        /// This is taken from Sample Result MethodDetectionLimit as entered by the Industry during data entry.
        /// </summary>
        public string LabMDL { get; set; }

        /// <summary>
        /// Whether the LabResult is a QC result (Blank) or not (use 1 for True or 0 for False).
        /// Since we are not capturing this from Industries, it will always be 0.
        /// </summary>
        public bool? IsLabQCResult { get; set; }

        /// <summary>
        /// Date & Time the data was sent to CTS. 
        /// This is taken from Report Package LastSentDateTimeUtc.
        /// This is in the configured Timezone for the Authority.
        /// </summary>
        public DateTime? LabReportedDate { get; set; }

        /// <summary>
        /// Free form comments that show in the Sample Comments field in Linko.
        /// Since we are not capturing this from Industries, it will always be NULL.
        ///
        /// </summary>
        public string LabSampleComments { get; set; }

        /// <summary>
        /// Free form comments that show in the Result Comments field in Linko.
        /// Since we are not capturing this from Industries, it will always be NULL.
        /// </summary>
        public string LabResultComments { get; set; }

        /// <summary>
        /// Indicates if the data is approved by the lab and can be used in Linko Compliance calcs. 
        /// All data from LinkoExchange will be “Approved” data and “Approved” will be in the data export.
        /// </summary>
        public string LabStatus { get; set; }

        /// <summary>
        /// The ID that relates a Grab and Composite sample together for calculating TTO’s.
        /// Since we are not capturing this from Industries, it will always be NULL.
        /// </summary>
        public string LabSampleMatchID { get; set; }

        /// <summary>
        /// Whether the Result has been repudiated by the Industry or not (use 1 for True or 0 for False). 
        /// If blank, 0 is used. This is true if Report Package RepudiationReviewDateTimeUtc is not null. 
        /// </summary>
        public bool? IsResultRepudiated { get; set; }

        /// <summary>
        /// Whether the LabResult was tested using a valid EPA Method and can be used in Compliance (use 1 for True or 0 for False). 
        /// If blank, 1 is used. 
        /// This is taken from Sample Result IsApprovedMethod as entered by the Industry during data entry.
        /// </summary>
        public bool? LEIsValidEPAMethod { get; set; }

        /// <summary>
        /// A unique identifier for the Report Package from LinkoExchange. 
        /// This is taken from ReportPackage.ReportPackageID
        /// </summary>
        public int LEReportPackageID { get; set; }

        /// <summary>
        /// A unique identifier for a Sample in LinkoExchange. 
        /// This is taken from ReportSample.SampleID
        /// </summary>
        public int LESampleID { get; set; }
    }
}

