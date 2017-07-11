using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Settings;
using NLog;
using System.Configuration;

namespace Linko.LinkoExchange.Services.Sync
{
    public class SyncService : ISyncService
    {
        #region private member variables

        private readonly ILogger _logger;
        private readonly IHttpContextService _httpContextService;
        private readonly ISettingService _settingService;
        private readonly IReportPackageService _reportPackageService;

        #endregion


        #region public methods

        public SyncService(ILogger logger, IHttpContextService httpContextService, ISettingService settingService, IReportPackageService reportPackageService)
        {
            _logger = logger;
            _httpContextService = httpContextService;
            _settingService = settingService;
            _reportPackageService = reportPackageService;
        }

        /// <summary>
        /// Sends submitted report package to LinkoCTS.
        /// </summary>
        /// <param name="reportPackageId">Report Package Id.</param>
        public void SendSubmittedReportPackageToCts(int reportPackageId)
        {
            _logger.Info($"Enter SyncService.SendSubmittedReportPackageToCts. ReportPackageId: {reportPackageId}");

            List<RuleViolation> validationIssues = new List<RuleViolation>();

            // Get ReportPackageDto
            ReportPackageDto reportPackageDto = _reportPackageService.GetReportPackage(reportPackageId: reportPackageId, isIncludeAssociatedElementData: true);

            // Only Report Packages with an Cts Event Type assigned in LinkoExchange can be sent to CTS.
            // Additionally, only Report Elements with an Event Type assigned are included in the export. 
            // For a Report Element to be included in the export, the Report Package must also be included in the export.
            if (!reportPackageDto.CtsEventTypeId.HasValue || reportPackageDto.ReportPackageElementCategories == null || !reportPackageDto.ReportPackageElementCategories.Any())
            {
                validationIssues.Add(new RuleViolation(string.Empty, null, "Nothing to send."));

                throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
            }

            // SyncContext is only used here then there is little reason to put it in Unity
            using (var syncContext = new SyncContext(GetOrganizationRegulatoryProgramConnectionString(reportPackageDto.RecipientOrganizationRegulatoryProgramId)))
            {
                // Verify connection to CTS
                try
                {
                    syncContext.Database.Connection.Open();
                    syncContext.Database.Connection.Close();
                }
                catch (Exception ex)
                {
                    var errors = new List<string>() { ex.Message };

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(ex.Message);
                    }

                    _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                    validationIssues.Add(new RuleViolation(string.Empty, null, "Send to CTS Failed. Unable to establish a connection to CTS. Please contact Linko for assistance."));

                    throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                }

                using (var transaction = syncContext.Database.BeginTransaction())
                {
                    try
                    {
                        List<LEReportPackageParsedData> reportPackageParsedDatas = new List<LEReportPackageParsedData>();

                        // LEReportPackageParsedData: Report Package record
                        var reportPackageRecord = new LEReportPackageParsedData();
                        PopulateCommonReportPackageInfo(reportPackageParsedData: reportPackageRecord, reportPackageDto: reportPackageDto);
                        reportPackageParsedDatas.Add(reportPackageRecord);

                        // LEReportPackageParsedData: Report Package Element records
                        // If an element in a Report Package does not have data created for it,
                        // e.g. a Lab Analysis Report attachment was supposed to be included in the report but it was not added to the report 
                        //      then that element is not included in the data export. 
                        // No event would be created for that Element in LinkoCTS even if it had an Event Type assigned.

                        StringBuilder reportPackageElements = new StringBuilder("Report Element Type\tRequired?\tPresent in Report?\r\n");
                        var isElementTypePresent = false;
                        foreach (var categoryName in reportPackageDto.ReportPackageElementCategories)
                        {
                            // Attachments
                            if (categoryName.Equals(ReportElementCategoryName.Attachments) && reportPackageDto.AttachmentTypes != null)
                            {
                                foreach (var attachmentType in reportPackageDto.AttachmentTypes)
                                {
                                    isElementTypePresent = attachmentType.FileStores != null && attachmentType.FileStores.Any();
                                    if (attachmentType.CtsEventTypeId != null && isElementTypePresent)
                                    {
                                        var reportPackageElementTypeRecord = new LEReportPackageParsedData();
                                        PopulateCommonReportPackageInfo(reportPackageParsedData: reportPackageElementTypeRecord, reportPackageDto: reportPackageDto);
                                        PopulateReportPackageElementTypeInfo(reportPackageParsedData: reportPackageElementTypeRecord, reportPackageElementTypeDto: attachmentType,
                                                                                reportElementCategoryName: categoryName);

                                        reportPackageParsedDatas.Add(reportPackageElementTypeRecord);
                                    }
                                    else
                                    {
                                        // No record to send. See the general comment for LEReportPackageParsedData above.
                                    }

                                    reportPackageElements.Append($"{attachmentType.ReportElementTypeName}\t{(attachmentType.IsRequired ? "Yes" : "No")}\t{(isElementTypePresent ? "Yes" : "No")}\r\n");
                                }
                            }

                            // Certifications
                            if (categoryName.Equals(ReportElementCategoryName.Certifications) && reportPackageDto.CertificationTypes != null)
                            {
                                foreach (var certificationType in reportPackageDto.CertificationTypes)
                                {
                                    isElementTypePresent = certificationType.ReportElementTypeIsContentProvided;
                                    if (certificationType.CtsEventTypeId != null && isElementTypePresent)
                                    {
                                        var reportPackageElementTypeRecord = new LEReportPackageParsedData();
                                        PopulateCommonReportPackageInfo(reportPackageParsedData: reportPackageElementTypeRecord, reportPackageDto: reportPackageDto);
                                        PopulateReportPackageElementTypeInfo(reportPackageParsedData: reportPackageElementTypeRecord, reportPackageElementTypeDto: certificationType,
                                                                                reportElementCategoryName: categoryName);

                                        reportPackageParsedDatas.Add(reportPackageElementTypeRecord);
                                    }
                                    else
                                    {
                                        // No record to send. See the general comment for LEReportPackageParsedData above.
                                    }

                                    reportPackageElements.Append($"{certificationType.ReportElementTypeName}\t{(certificationType.IsRequired ? "Yes" : "No")}\t{(isElementTypePresent ? "Yes" : "No")}\r\n");
                                }
                            }
                        }

                        // Populate LEReportPackageElements field for all LEReportPackageParsedData records
                        foreach (var record in reportPackageParsedDatas)
                        {
                            record.LEReportPackageElements = reportPackageElements.ToString();
                        }

                        syncContext.LEReportPackageParsedDatas.AddRange(reportPackageParsedDatas);
                        syncContext.SaveChanges();


                        // LESampleResultParsedData
                        if (reportPackageDto.SamplesAndResultsTypes != null)
                        {
                            foreach (var samplesAndResultsType in reportPackageDto.SamplesAndResultsTypes)
                            {
                                foreach (var sample in samplesAndResultsType.Samples)
                                {
                                    foreach (var sampleResult in sample.SampleResults)
                                    {
                                        var sampleResultParsedData = new LESampleResultParsedData();
                                        PopulateSampleResultParsedData(sampleResultParsedData: sampleResultParsedData, reportPackageDto: reportPackageDto, sampleDto: sample, sampleResultDto: sampleResult);
                                        syncContext.LESampleResultParsedDatas.Add(sampleResultParsedData);
                                    }
                                }
                            }

                            syncContext.SaveChanges();
                        }

                        // Update ReportPackage last sent related info
                        _reportPackageService.UpdateLastSentDateTime(reportPackageId, 
                                                                     DateTimeOffset.Now, 
                                                                     int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId)), 
                                                                     _httpContextService.GetClaimValue(CacheKey.FirstName), 
                                                                     _httpContextService.GetClaimValue(CacheKey.LastName));

                        transaction.Commit();
                    }
                    catch (DbEntityValidationException ex)
                    {
                        transaction.Rollback();

                        var errors = new List<string>() { ex.Message };
                        foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                        {
                            DbEntityEntry entry = item.Entry;
                            string entityTypeName = entry.Entity.GetType().Name;

                            foreach (DbValidationError subItem in item.ValidationErrors)
                            {
                                string message = $"Error {subItem.ErrorMessage} occurred in {entityTypeName} at {subItem.PropertyName}";
                                errors.Add(message);
                            }
                        }

                        _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                        validationIssues.Add(new RuleViolation(string.Empty, null, "Send to CTS Failed. An unknown error occured. Please contact Linko for assistance."));

                        throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        var errors = new List<string>() { ex.Message };

                        while (ex.InnerException != null)
                        {
                            ex = ex.InnerException;
                            errors.Add(ex.Message);
                        }

                        _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                        validationIssues.Add(new RuleViolation(string.Empty, null, "Send to CTS Failed. An unknown error occured. Please contact Linko for assistance."));

                        throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                    }
                }
            }

            _logger.Info($"Leave SyncService.SendSubmittedReportPackageToCts. ReportPackageId: {reportPackageId}");
        }

        #endregion


        #region private methods

        /// <summary>
        /// Populates the LEReportPackageParsedData with the common report package information.
        /// </summary>
        /// <param name="reportPackageParsedData">LEReportPackageParsedData.</param>
        /// <param name="reportPackageDto">ReportPackageDto.</param>
        private void PopulateCommonReportPackageInfo(LEReportPackageParsedData reportPackageParsedData, ReportPackageDto reportPackageDto)
        {
            reportPackageParsedData.LinkoVersionNo = "ExchangeReportSync_1.0";

            reportPackageParsedData.LinkoClientID = reportPackageDto.RecipientOrganizationRegulatoryProgramId;
            reportPackageParsedData.LEPermitNo = reportPackageDto.OrganizationReferenceNumber;
            reportPackageParsedData.LEReportPackageID = reportPackageDto.ReportPackageId;
            reportPackageParsedData.LEReportPackageName = reportPackageDto.Name;
            reportPackageParsedData.LEReportPackageCTSEventTypeName = reportPackageDto.CtsEventTypeName;
            reportPackageParsedData.LEReportPackageCTSEventCategoryName = reportPackageDto.CtsEventCategoryName;
            reportPackageParsedData.LEReportPeriodStartDate = reportPackageDto.PeriodStartDateTimeLocal;
            reportPackageParsedData.LEReportPeriodEndDate = reportPackageDto.PeriodEndDateTimeLocal;
            
            reportPackageParsedData.LEReportPackageSubmissionComments = reportPackageDto.Comments;
            reportPackageParsedData.LEReportPackageSampleComplianceSummary = null;

            reportPackageParsedData.LEReportPackageURL = $"{_httpContextService.GetRequestBaseUrl()}ReportPackage/{reportPackageDto.ReportPackageId}/Details";

            reportPackageParsedData.LEReportPackageSubmissionDate = reportPackageDto.SubmissionDateTimeLocal.Value; // the package is expected to have been submitted
            reportPackageParsedData.LEReportPackageSubmissionBy = $"{reportPackageDto.SubmitterFirstName} {reportPackageDto.SubmitterLastName}";

            reportPackageParsedData.LEReportPackageReviewedDate = reportPackageDto.SubmissionReviewDateTimeLocal;
            reportPackageParsedData.LEReportPackageReviewedBy = $"{reportPackageDto.SubmissionReviewerFirstName} {reportPackageDto.SubmissionReviewerLastName}";
            reportPackageParsedData.LEReportPackageReviewedByComments = reportPackageDto.SubmissionReviewComments;

            reportPackageParsedData.LEReportPackageLastSentDate = reportPackageDto.LastSentDateTimeLocal;
            reportPackageParsedData.LEReportPackageLastSentToCTSBy = $"{reportPackageDto.LastSenderFirstName} {reportPackageDto.LastSenderLastName}";

            reportPackageParsedData.LEReportPackageIsRepudiated = reportPackageDto.ReportStatusName.Equals(ReportStatusName.Repudiated);
            reportPackageParsedData.LEReportPackageRepudiatedDate = reportPackageDto.RepudiationDateTimeLocal;
            reportPackageParsedData.LEReportPackageRepudiatedBy = $"{reportPackageDto.RepudiatorFirstName} {reportPackageDto.RepudiatorLastName}";
            reportPackageParsedData.LEReportPackageRepudiatedByReason = reportPackageDto.RepudiationReasonName;
            reportPackageParsedData.LEReportPackageRepudiatedByComments = reportPackageDto.RepudiationComments;

            reportPackageParsedData.LEReportPackageRepudiationReviewedByDate = reportPackageDto.RepudiationReviewDateTimeLocal;
            reportPackageParsedData.LEReportPackageRepudiationReviewedBy = $"{reportPackageDto.RepudiationReviewerFirstName} {reportPackageDto.RepudiationReviewerLastName}";
            reportPackageParsedData.LEReportPackageRepudiatedReviewedByComments = reportPackageDto.RepudiationReviewComments;
        }

        /// <summary>
        /// Populates the LEReportPackageParsedData with the report package element type information except Sample and Results.
        /// </summary>
        /// <param name="reportPackageParsedData">LEReportPackageParsedData.</param>
        /// <param name="reportPackageElementTypeDto">ReportPackageElementTypeDto.</param>
        /// <param name="reportElementCategoryName">ReportElementCategoryName.</param>
        private void PopulateReportPackageElementTypeInfo(LEReportPackageParsedData reportPackageParsedData, ReportPackageElementTypeDto reportPackageElementTypeDto,
                                                          ReportElementCategoryName reportElementCategoryName)
        {
            reportPackageParsedData.LEReportPackageElementTypeID = reportPackageElementTypeDto.ReportPackageElementTypeId;
            reportPackageParsedData.LEReportPackageElementCTSEventTypeName = reportPackageElementTypeDto.CtsEventTypeName;
            reportPackageParsedData.LEReportPackageElementCTSEventCategoryName = reportPackageElementTypeDto.CtsEventCategoryName;
            reportPackageParsedData.LEReportPackageElementCategoryName = reportElementCategoryName.ToString();
            reportPackageParsedData.LEReportPackageElementTypeName = reportPackageElementTypeDto.ReportElementTypeName;
        }

        /// <summary>
        /// Populates the LESampleResultParsedData with the sample and sample result info.
        /// </summary>
        /// <param name="sampleResultParsedData">LESampleResultParsedData.</param>
        /// <param name="reportPackageDto">ReportPackageDto.</param>
        private void PopulateSampleResultParsedData(LESampleResultParsedData sampleResultParsedData, ReportPackageDto reportPackageDto, SampleDto sampleDto, SampleResultDto sampleResultDto)
        {
            
            sampleResultParsedData.LinkoVersionNo = "ExchangeLabSync_1.0";
            sampleResultParsedData.LinkoClientID = reportPackageDto.RecipientOrganizationRegulatoryProgramId;

            // Sample Info

            // LabPermitMPID max length: 250 characters
            var labPermitMpid = $"{reportPackageDto.OrganizationName} - {sampleDto.MonitoringPointName}";
            sampleResultParsedData.LabPermitMPID = labPermitMpid.Substring(0, labPermitMpid.Length > 250 ? 250 :labPermitMpid.Length);

            sampleResultParsedData.LabSampleName = sampleDto.Name;
            sampleResultParsedData.LabDateSampled = sampleDto.StartDateTimeLocal.Date;
            sampleResultParsedData.LabStartDateTimeSampled = sampleDto.StartDateTimeLocal;
            sampleResultParsedData.LabStopDateTimeSampled = sampleDto.EndDateTimeLocal;
            sampleResultParsedData.LabSampleID = sampleDto.LabSampleIdentifier;
            sampleResultParsedData.LabSampleEventType = sampleDto.CtsEventTypeName;
            sampleResultParsedData.LabCollectMethod = sampleDto.CollectionMethodName;
            sampleResultParsedData.LabSampler = null;


            // Sample result info

            sampleResultParsedData.LabAnalysisDate = sampleResultDto.AnalysisDateTimeLocal;
            sampleResultParsedData.LabAnalysisMethod = sampleResultDto.AnalysisMethod;
            sampleResultParsedData.LabParamName = sampleResultDto.ParameterName;

            // it is assumed that whenever that qualifier and value are matched and stored correctly in the DB,
            // i.e. "ND and NF" will not have value component, "<" will have corresponding value component, and numeric value only (no qualifier).
            sampleResultParsedData.LabResult = $"{sampleResultDto.Qualifier}{sampleResultDto.EnteredValue}";

            double labNumResult = default(double);
            sampleResultParsedData.LabNumResult = double.TryParse(sampleResultDto.EnteredValue, out labNumResult) ? labNumResult : default(double?);

            sampleResultParsedData.LabResultFlag = null;
            sampleResultParsedData.LabResultUnits = sampleResultDto.UnitName;
            sampleResultParsedData.LabRepLimit = null;
            sampleResultParsedData.LabMDL = sampleResultDto.EnteredMethodDetectionLimit;
            sampleResultParsedData.IsLabQCResult = false;
            sampleResultParsedData.LabReportedDate = reportPackageDto.LastSentDateTimeLocal;
            sampleResultParsedData.LabSampleComments = null;
            sampleResultParsedData.LabResultComments = null;
            sampleResultParsedData.LabStatus = "Approved";
            sampleResultParsedData.LabSampleMatchID = null;
            sampleResultParsedData.IsResultRepudiated = reportPackageDto.ReportStatusName.Equals(ReportStatusName.Repudiated);
            sampleResultParsedData.LEIsValidEPAMethod = sampleResultDto.IsApprovedEPAMethod;
            sampleResultParsedData.LEReportPackageID = reportPackageDto.ReportPackageId;
            sampleResultParsedData.LESampleID = sampleDto.SampleId.Value;
        }

        /// <summary>
        /// Gets the connection string for a given organization regulatory program id.
        /// </summary>
        /// <param name="organizationRegulatoryProgramId">Authority OrganizationRegulatoryProgramId.</param>
        /// <returns>Connection string.</returns>
        private string GetOrganizationRegulatoryProgramConnectionString(int organizationRegulatoryProgramId)
        {
            string dbServerName = _settingService.GetOrgRegProgramSettingValue(organizationRegulatoryProgramId, SettingType.SendToCtsDatabaseServerName);
            string dbName = _settingService.GetOrgRegProgramSettingValue(organizationRegulatoryProgramId, SettingType.SendToCtsDatabaseName);
            string dbUserName = ConfigurationManager.AppSettings["SendToCtsDatabaseUserName"]?.ToString();
            string dbPassword = ConfigurationManager.AppSettings["SendToCtsDatabasePassword"]?.ToString();
            if (string.IsNullOrEmpty(dbServerName) || string.IsNullOrEmpty(dbName))
            {
                List<RuleViolation> validationIssues = new List<RuleViolation>();
                validationIssues.Add(new RuleViolation(string.Empty, null, "Send to CTS Failed. This feature is not configured. Please contact Linko for assistance."));

                throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
            }
            else
            {
                string connectionString = $"data source={dbServerName};initial catalog={dbName};user id={dbUserName};password={dbPassword};MultipleActiveResultSets=True;App=LinkoExchange";
                return connectionString;
            }
        }

        #endregion
    }
}
