using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Transactions;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Extensions;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;

namespace Linko.LinkoExchange.Services.Sync
{
    public class SyncService : ISyncService
    {
        #region private member variables

        private readonly ILogger _logger;
        private readonly IHttpContextService _httpContextService;
        private readonly ISettingService _settingService;
        private readonly IReportPackageService _reportPackageService;
        private readonly ITimeZoneService _timeZoneService;

        #endregion

        #region public methods

        public SyncService(ILogger logger, IHttpContextService httpContextService, ISettingService settingService, IReportPackageService reportPackageService,
                           ITimeZoneService timeZoneService)
        {
            _logger = logger;
            _httpContextService = httpContextService;
            _settingService = settingService;
            _reportPackageService = reportPackageService;
            _timeZoneService = timeZoneService;
        }

        /// <summary>
        ///     Sends submitted report package to LinkoCTS.
        /// </summary>
        /// <param name="reportPackageId"> Report Package Id. </param>
        public void SendSubmittedReportPackageToCts(int reportPackageId)
        {
            _logger.Info(message:$"Enter SyncService.SendSubmittedReportPackageToCts. ReportPackageId: {reportPackageId}");

            var validationIssues = new List<RuleViolation>();

            // Get ReportPackageDto
            var reportPackageDto = _reportPackageService.GetReportPackage(reportPackageId:reportPackageId, isIncludeAssociatedElementData:true);

            // Only Report Packages with an Cts Event Type assigned in LinkoExchange can be sent to CTS.
            // Additionally, only Report Elements with an Event Type assigned are included in the export. 
            // For a Report Element to be included in the export, the Report Package must also be included in the export.
            if (!reportPackageDto.CtsEventTypeId.HasValue || reportPackageDto.ReportPackageElementCategories == null || !reportPackageDto.ReportPackageElementCategories.Any())
            {
                validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:"Nothing to send."));

                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
            }

            // SyncContext is only used here then there is little reason to put it in Unity
            using (var syncContext =
                new SyncContext(nameOrConnectionString:GetOrganizationRegulatoryProgramConnectionString(organizationRegulatoryProgramId:reportPackageDto
                                                                                                            .RecipientOrganizationRegulatoryProgramId)))
            {
                // Verify connection to CTS
                try
                {
                    syncContext.Database.Connection.Open();
                    syncContext.Database.Connection.Close();
                }
                catch (Exception ex)
                {
                    var errors = new List<string> {ex.Message};

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(item:ex.Message);
                    }

                    _logger.Error(message:"Error happens {0} ", argument:string.Join(separator:"," + Environment.NewLine, values:errors));

                    validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null,
                                                                errorMessage:"Send to CTS Failed. Unable to establish a connection to CTS. Please contact Linko for assistance."));

                    throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                }

                using (var scope = new TransactionScope())
                {
                    try
                    {
                        var timeZoneId =
                            Convert.ToInt32(value:_settingService.GetOrganizationSettingValue(orgRegProgramId:reportPackageDto.RecipientOrganizationRegulatoryProgramId,
                                                                                              settingType:SettingType.TimeZone));
                        var dateTimeOffsetNow = DateTimeOffset.Now;

                        // This LastSent info is used by LinkoCTS users to determine when the data was sent to CTS.  
                        // Thus, the last sent data should be populated with the time of the "Send to CTS" button click so it can be placed in CTS_IMPORT.
                        // We modify just the DTO early on so that it can be propagated to all fields that use them.
                        // The actual save to the DB is done at the very last step (and with dateTimeOffsetNow value).
                        reportPackageDto.LastSentDateTimeLocal =
                            _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:dateTimeOffsetNow.UtcDateTime, timeZoneId:timeZoneId);
                        reportPackageDto.LastSenderFirstName = _httpContextService.GetClaimValue(claimType:CacheKey.FirstName);
                        reportPackageDto.LastSenderLastName = _httpContextService.GetClaimValue(claimType:CacheKey.LastName);

                        var reportPackageParsedDatas = new List<LEReportPackageParsedData>();

                        // LEReportPackageParsedData: Report Package record
                        var reportPackageRecord = new LEReportPackageParsedData();
                        PopulateCommonReportPackageInfo(reportPackageParsedData:reportPackageRecord, reportPackageDto:reportPackageDto);
                        reportPackageParsedDatas.Add(item:reportPackageRecord);

                        // --------------------- LEReportPackageParsedData: Report Package Element records ---------------------
                        // If an element in a Report Package does not have data created for it,
                        // e.g. a Lab Analysis Report attachment was supposed to be included in the report but it was not added to the report 
                        //      then that element is not included in the data export. 
                        // No event would be created for that Element in LinkoCTS even if it had an Event Type assigned.

                        var reportPackageElements = new StringBuilder(value:"Report Element Type | Present in Report?\r\n");
                        foreach (var categoryName in reportPackageDto.ReportPackageElementCategories)
                        {
                            // Sample and Results
                            bool isElementTypePresent;
                            if (categoryName.Equals(obj:ReportElementCategoryName.SamplesAndResults) && reportPackageDto.SamplesAndResultsTypes != null)
                            {
                                // --------------------- LESampleResultParsedData: Samples and Results records ---------------------
                                // a Samples and Results element category is present only if there is at least one Sample with at least one Result

                                foreach (var samplesAndResultsType in reportPackageDto.SamplesAndResultsTypes)
                                {
                                    isElementTypePresent = false;
                                    if (samplesAndResultsType.Samples != null)
                                    {
                                        foreach (var sample in samplesAndResultsType.Samples)
                                        {
                                            isElementTypePresent |= sample.SampleResults != null && sample.SampleResults.Any();
                                            if (sample.SampleResults != null && sample.SampleResults.Any())
                                            {
                                                if (!string.IsNullOrWhiteSpace(value:sample.FlowParameterName))
                                                {
                                                    var sampleResultParsedData = new LESampleResultParsedData();
                                                    PopulateCommonSampleInfo(sampleResultParsedData:sampleResultParsedData, reportPackageDto:reportPackageDto, sampleDto:sample);
                                                    PopulateSampleFlowResultInfo(sampleResultParsedData:sampleResultParsedData, sampleDto:sample);
                                                    syncContext.LESampleResultParsedDatas.Add(entity:sampleResultParsedData);
                                                }

                                                foreach (var sampleResult in sample.SampleResults)
                                                {
                                                    var sampleResultParsedData = new LESampleResultParsedData();
                                                    PopulateCommonSampleInfo(sampleResultParsedData:sampleResultParsedData, reportPackageDto:reportPackageDto, sampleDto:sample);
                                                    PopulateSampleResultInfo(sampleResultParsedData:sampleResultParsedData, sampleResultDto:sampleResult);
                                                    syncContext.LESampleResultParsedDatas.Add(entity:sampleResultParsedData);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // No record to send
                                    }

                                    reportPackageElements.Append(value:$"   {samplesAndResultsType.ReportElementTypeName} | {(isElementTypePresent ? "Yes" : "No")}\r\n");
                                }

                                // --------------------- End of LESampleResultParsedData ---------------------
                            }

                            // Attachments
                            if (categoryName.Equals(obj:ReportElementCategoryName.Attachments) && reportPackageDto.AttachmentTypes != null)
                            {
                                foreach (var attachmentType in reportPackageDto.AttachmentTypes)
                                {
                                    isElementTypePresent = attachmentType.FileStores != null && attachmentType.FileStores.Any();
                                    if (attachmentType.CtsEventTypeId != null && isElementTypePresent)
                                    {
                                        var reportPackageElementTypeRecord = new LEReportPackageParsedData();
                                        PopulateCommonReportPackageInfo(reportPackageParsedData:reportPackageElementTypeRecord, reportPackageDto:reportPackageDto);
                                        PopulateReportPackageElementTypeInfo(reportPackageParsedData:reportPackageElementTypeRecord, reportPackageElementTypeDto:attachmentType,
                                                                             reportElementCategoryName:categoryName);

                                        reportPackageParsedDatas.Add(item:reportPackageElementTypeRecord);
                                    }
                                    else
                                    {
                                        // No record to send. See the general comment for LEReportPackageParsedData above.
                                    }

                                    reportPackageElements.Append(value:$"   {attachmentType.ReportElementTypeName} | {(isElementTypePresent ? "Yes" : "No")}\r\n");
                                }
                            }

                            // Certifications
                            if (categoryName.Equals(obj:ReportElementCategoryName.Certifications) && reportPackageDto.CertificationTypes != null)
                            {
                                foreach (var certificationType in reportPackageDto.CertificationTypes)
                                {
                                    isElementTypePresent = certificationType.ReportElementTypeIsContentProvided;
                                    if (certificationType.CtsEventTypeId != null && isElementTypePresent)
                                    {
                                        var reportPackageElementTypeRecord = new LEReportPackageParsedData();
                                        PopulateCommonReportPackageInfo(reportPackageParsedData:reportPackageElementTypeRecord, reportPackageDto:reportPackageDto);
                                        PopulateReportPackageElementTypeInfo(reportPackageParsedData:reportPackageElementTypeRecord, reportPackageElementTypeDto:certificationType,
                                                                             reportElementCategoryName:categoryName);

                                        reportPackageParsedDatas.Add(item:reportPackageElementTypeRecord);
                                    }
                                    else
                                    {
                                        // No record to send. See the general comment for LEReportPackageParsedData above.
                                    }

                                    reportPackageElements.Append(value:$"   {certificationType.ReportElementTypeName} | {(isElementTypePresent ? "Yes" : "No")}\r\n");
                                }
                            }
                        }

                        // Populate LEReportPackageElements field for all LEReportPackageParsedData records
                        foreach (var record in reportPackageParsedDatas)
                        {
                            record.LEReportPackageElements = reportPackageElements.ToString();
                        }

                        syncContext.LEReportPackageParsedDatas.AddRange(entities:reportPackageParsedDatas);
                        syncContext.SaveChanges();

                        // Update ReportPackage last sent related info
                        _reportPackageService.UpdateLastSentDateTime(reportPackageId:reportPackageId,
                                                                     sentDateTime:dateTimeOffsetNow,
                                                                     lastSenderUserId:int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId)),
                                                                     lastSenderFirstName:_httpContextService.GetClaimValue(claimType:CacheKey.FirstName),
                                                                     lastSenderLastName:_httpContextService.GetClaimValue(claimType:CacheKey.LastName));

                        // The Complete method commits the transaction. If an exception has been thrown,
                        // Complete is not  called and the transaction is rolled back.
                        scope.Complete();
                    }
                    catch (DbEntityValidationException ex)
                    {
                        var errors = new List<string> {ex.Message};
                        foreach (var item in ex.EntityValidationErrors)
                        {
                            var entry = item.Entry;
                            var entityTypeName = entry.Entity.GetType().Name;

                            foreach (var subItem in item.ValidationErrors)
                            {
                                var message = $"Error {subItem.ErrorMessage} occurred in {entityTypeName} at {subItem.PropertyName}";
                                errors.Add(item:message);
                            }
                        }

                        _logger.Error(message:"Error happens {0} ", argument:string.Join(separator:"," + Environment.NewLine, values:errors));

                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null,
                                                                    errorMessage:"Send to CTS Failed. An unknown error occurred. Please contact Linko for assistance."));

                        throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                    }
                    catch (Exception ex)
                    {
                        var errors = new List<string> {ex.Message};

                        while (ex.InnerException != null)
                        {
                            ex = ex.InnerException;
                            errors.Add(item:ex.Message);
                        }

                        _logger.Error(message:"Error happens {0} ", argument:string.Join(separator:"," + Environment.NewLine, values:errors));

                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null,
                                                                    errorMessage:"Send to CTS Failed. An unknown error occurred. Please contact Linko for assistance."));

                        throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                    }
                }
            }

            _logger.Info(message:$"Leave SyncService.SendSubmittedReportPackageToCts. ReportPackageId: {reportPackageId}");
        }

        #endregion

        #region private methods

        /// <summary>
        ///     Populates the LEReportPackageParsedData with the common report package information.
        /// </summary>
        /// <param name="reportPackageParsedData"> LEReportPackageParsedData. </param>
        /// <param name="reportPackageDto"> ReportPackageDto. </param>
        private void PopulateCommonReportPackageInfo(LEReportPackageParsedData reportPackageParsedData, ReportPackageDto reportPackageDto)
        {
            reportPackageParsedData.LinkoVersionNo = "ExchangeReportSync_1.0";

            reportPackageParsedData.LinkoClientID = reportPackageDto.RecipientOrganizationRegulatoryProgramId;
            reportPackageParsedData.LEPermitNo = reportPackageDto.OrganizationReferenceNumber.GetValueOrNull();
            reportPackageParsedData.LEReportPackageID = reportPackageDto.ReportPackageId;
            reportPackageParsedData.LEReportPackageName = reportPackageDto.Name.GetValueOrNull();
            reportPackageParsedData.LEReportPackageCTSEventTypeName = reportPackageDto.CtsEventTypeName.GetValueOrNull();
            reportPackageParsedData.LEReportPackageCTSEventCategoryName = reportPackageDto.CtsEventCategoryName.GetValueOrNull();
            reportPackageParsedData.LEReportPeriodStartDate = reportPackageDto.PeriodStartDateTimeLocal;
            reportPackageParsedData.LEReportPeriodEndDate = reportPackageDto.PeriodEndDateTimeLocal;

            reportPackageParsedData.LEReportPackageSubmissionComments = reportPackageDto.Comments.GetValueOrNull();
            reportPackageParsedData.LEReportPackageSampleComplianceSummary = null;

            reportPackageParsedData.LEReportPackageURL = $"{_httpContextService.GetRequestBaseUrl()}ReportPackage/{reportPackageDto.ReportPackageId}/Details";

            reportPackageParsedData.LEReportPackageSubmissionDate = reportPackageDto.SubmissionDateTimeLocal.Value; // the package is expected to have been submitted
            reportPackageParsedData.LEReportPackageSubmissionBy = $"{reportPackageDto.SubmitterFirstName} {reportPackageDto.SubmitterLastName}".GetValueOrNull();

            reportPackageParsedData.LEReportPackageReviewedDate = reportPackageDto.SubmissionReviewDateTimeLocal;
            reportPackageParsedData.LEReportPackageReviewedBy = $"{reportPackageDto.SubmissionReviewerFirstName} {reportPackageDto.SubmissionReviewerLastName}".GetValueOrNull();
            reportPackageParsedData.LEReportPackageReviewedByComments = reportPackageDto.SubmissionReviewComments.GetValueOrNull();

            reportPackageParsedData.LEReportPackageLastSentDate = reportPackageDto.LastSentDateTimeLocal;
            reportPackageParsedData.LEReportPackageLastSentToCTSBy = $"{reportPackageDto.LastSenderFirstName} {reportPackageDto.LastSenderLastName}".GetValueOrNull();

            reportPackageParsedData.LEReportPackageIsRepudiated = reportPackageDto.ReportStatusName.Equals(obj:ReportStatusName.Repudiated);
            reportPackageParsedData.LEReportPackageRepudiatedDate = reportPackageDto.RepudiationDateTimeLocal;
            reportPackageParsedData.LEReportPackageRepudiatedBy = $"{reportPackageDto.RepudiatorFirstName} {reportPackageDto.RepudiatorLastName}".GetValueOrNull();
            reportPackageParsedData.LEReportPackageRepudiatedByReason = reportPackageDto.RepudiationReasonName.GetValueOrNull();
            reportPackageParsedData.LEReportPackageRepudiatedByComments = reportPackageDto.RepudiationComments.GetValueOrNull();

            reportPackageParsedData.LEReportPackageRepudiationReviewedByDate = reportPackageDto.RepudiationReviewDateTimeLocal;
            reportPackageParsedData.LEReportPackageRepudiationReviewedBy = $"{reportPackageDto.RepudiationReviewerFirstName} {reportPackageDto.RepudiationReviewerLastName}"
                .GetValueOrNull();
            reportPackageParsedData.LEReportPackageRepudiatedReviewedByComments = reportPackageDto.RepudiationReviewComments.GetValueOrNull();
        }

        /// <summary>
        ///     Populates the LEReportPackageParsedData with the report package element type information except Sample and Results.
        /// </summary>
        /// <param name="reportPackageParsedData"> LEReportPackageParsedData. </param>
        /// <param name="reportPackageElementTypeDto"> ReportPackageElementTypeDto. </param>
        /// <param name="reportElementCategoryName"> ReportElementCategoryName. </param>
        private void PopulateReportPackageElementTypeInfo(LEReportPackageParsedData reportPackageParsedData, ReportPackageElementTypeDto reportPackageElementTypeDto,
                                                          ReportElementCategoryName reportElementCategoryName)
        {
            reportPackageParsedData.LEReportPackageElementTypeID = reportPackageElementTypeDto.ReportPackageElementTypeId;
            reportPackageParsedData.LEReportPackageElementCTSEventTypeName = reportPackageElementTypeDto.CtsEventTypeName.GetValueOrNull();
            reportPackageParsedData.LEReportPackageElementCTSEventCategoryName = reportPackageElementTypeDto.CtsEventCategoryName.GetValueOrNull();
            reportPackageParsedData.LEReportPackageElementCategoryName = reportElementCategoryName.ToString();
            reportPackageParsedData.LEReportPackageElementTypeName = reportPackageElementTypeDto.ReportElementTypeName.GetValueOrNull();
        }

        /// <summary>
        ///     Populates the LESampleResultParsedData with the common sample info.
        /// </summary>
        /// <param name="sampleResultParsedData"> LESampleResultParsedData. </param>
        /// <param name="reportPackageDto"> ReportPackageDto. </param>
        /// <param name="sampleDto"> SampleDto. </param>
        private void PopulateCommonSampleInfo(LESampleResultParsedData sampleResultParsedData, ReportPackageDto reportPackageDto, SampleDto sampleDto)
        {
            sampleResultParsedData.LinkoVersionNo = "ExchangeLabSync_1.0";
            sampleResultParsedData.LinkoClientID = reportPackageDto.RecipientOrganizationRegulatoryProgramId;

            // LabPermitMPID max length: 250 characters
            var labPermitMpid = $"{reportPackageDto.OrganizationName} - {sampleDto.MonitoringPointName}";
            sampleResultParsedData.LabPermitMPID = labPermitMpid.Substring(startIndex:0, length:labPermitMpid.Length > 250 ? 250 : labPermitMpid.Length).GetValueOrNull();

            sampleResultParsedData.LabSampleName = sampleDto.Name.GetValueOrNull();
            sampleResultParsedData.LabDateSampled = sampleDto.StartDateTimeLocal.Date;
            sampleResultParsedData.LabStartDateTimeSampled = sampleDto.StartDateTimeLocal;
            sampleResultParsedData.LabStopDateTimeSampled = sampleDto.EndDateTimeLocal;
            sampleResultParsedData.LabSampleID = sampleDto.LabSampleIdentifier.GetValueOrNull();
            sampleResultParsedData.LabSampleEventType = sampleDto.CtsEventTypeName.GetValueOrNull();
            sampleResultParsedData.LabCollectMethod = sampleDto.CollectionMethodName.GetValueOrNull();
            sampleResultParsedData.LabSampler = null;
            sampleResultParsedData.LabReportedDate = reportPackageDto.LastSentDateTimeLocal;
            sampleResultParsedData.IsResultRepudiated = reportPackageDto.ReportStatusName.Equals(obj:ReportStatusName.Repudiated);
            sampleResultParsedData.LEReportPackageID = reportPackageDto.ReportPackageId;
            sampleResultParsedData.LESampleID = sampleDto.SampleId.Value;
        }

        /// <summary>
        ///     Populates the LESampleResultParsedData with the sample result info.
        /// </summary>
        /// <param name="sampleResultParsedData"> LESampleResultParsedData. </param>
        /// <param name="sampleResultDto"> SampleResultDto. </param>
        private void PopulateSampleResultInfo(LESampleResultParsedData sampleResultParsedData, SampleResultDto sampleResultDto)
        {
            sampleResultParsedData.LabAnalysisDate = sampleResultDto.AnalysisDateTimeLocal;
            sampleResultParsedData.LabAnalysisMethod = sampleResultDto.AnalysisMethod.GetValueOrNull();
            sampleResultParsedData.LabParamName = sampleResultDto.ParameterName.GetValueOrNull();

            // it is assumed that whenever that qualifier and value are matched and stored correctly in the DB,
            // i.e. "ND and NF" will not have value component, "<" will have corresponding value component, and numeric value only (no qualifier).
            sampleResultParsedData.LabResult = $"{sampleResultDto.Qualifier}{sampleResultDto.EnteredValue}".GetValueOrNull();

            sampleResultParsedData.LabNumResult = sampleResultDto.Value;

            sampleResultParsedData.LabResultFlag = null;
            sampleResultParsedData.LabResultUnits = sampleResultDto.UnitName.GetValueOrNull();
            sampleResultParsedData.LabRepLimit = null;
            sampleResultParsedData.LabMDL = sampleResultDto.EnteredMethodDetectionLimit.GetValueOrNull();
            sampleResultParsedData.IsLabQCResult = false;
            sampleResultParsedData.LabSampleComments = null;
            sampleResultParsedData.LabResultComments = null;
            sampleResultParsedData.LabStatus = "Approved";
            sampleResultParsedData.LabSampleMatchID = null;
            sampleResultParsedData.LEIsValidEPAMethod = sampleResultDto.IsApprovedEPAMethod;
        }

        /// <summary>
        ///     Populates the LESampleResultParsedData with the sample flow result info.
        /// </summary>
        /// <param name="sampleResultParsedData"> LESampleResultParsedData. </param>
        /// <param name="sampleDto"> SampleDto. </param>
        private void PopulateSampleFlowResultInfo(LESampleResultParsedData sampleResultParsedData, SampleDto sampleDto)
        {
            sampleResultParsedData.LabAnalysisDate = null;
            sampleResultParsedData.LabAnalysisMethod = null;
            sampleResultParsedData.LabParamName = sampleDto.FlowParameterName;

            // no qualifier for flow
            sampleResultParsedData.LabResult = sampleDto.FlowEnteredValue.GetValueOrNull();

            sampleResultParsedData.LabNumResult = sampleDto.FlowValue;

            sampleResultParsedData.LabResultFlag = null;
            sampleResultParsedData.LabResultUnits = sampleDto.FlowUnitName.GetValueOrNull();
            sampleResultParsedData.LabRepLimit = null;
            sampleResultParsedData.LabMDL = null;
            sampleResultParsedData.IsLabQCResult = false;
            sampleResultParsedData.LabSampleComments = null;
            sampleResultParsedData.LabResultComments = null;
            sampleResultParsedData.LabStatus = "Approved";
            sampleResultParsedData.LabSampleMatchID = null;
            sampleResultParsedData.LEIsValidEPAMethod = true;
        }

        /// <summary>
        ///     Gets the connection string for a given organization regulatory program id.
        /// </summary>
        /// <param name="organizationRegulatoryProgramId"> Authority OrganizationRegulatoryProgramId. </param>
        /// <returns> Connection string. </returns>
        private string GetOrganizationRegulatoryProgramConnectionString(int organizationRegulatoryProgramId)
        {
            var dbServerName = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:organizationRegulatoryProgramId, settingType:SettingType.SendToCtsDatabaseServerName);
            var dbName = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:organizationRegulatoryProgramId, settingType:SettingType.SendToCtsDatabaseName);
            var dbUserName = ConfigurationManager.AppSettings[name:"SendToCtsDatabaseUserName"];
            var dbPassword = ConfigurationManager.AppSettings[name:"SendToCtsDatabasePassword"];
            if (string.IsNullOrEmpty(value:dbServerName) || string.IsNullOrEmpty(value:dbName))
            {
                var validationIssues = new List<RuleViolation>
                                       {
                                           new RuleViolation(propertyName:string.Empty, propertyValue:null,
                                                             errorMessage:"Send to CTS Failed. This feature is not configured. Please contact Linko for assistance.")
                                       };

                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
            }
            else
            {
                var connectionString =
                    $"data source={dbServerName};initial catalog={dbName};user id={dbUserName};password={dbPassword};MultipleActiveResultSets=True;App=LinkoExchange";
                return connectionString;
            }
        }

        #endregion
    }
}