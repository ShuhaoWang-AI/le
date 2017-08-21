using System;
using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Report
{
    public interface IReportPackageService
    {
        void SignAndSubmitReportPackage(int reportPackageId);
        CopyOfRecordDto GetCopyOfRecordByReportPackageId(int reportPackageId, ReportPackageDto reportPackageDto = null);
        CopyOfRecordValidationResultDto VerifyCopyOfRecord(int reportPackageId);

        /// <summary>
        ///     Get ReportPackageDto object by report package Id.
        /// </summary>
        /// <param name="reportPackageId">ReportPackageId</param>
        /// <param name="isIncludeAssociatedElementData">Indicating attachments binary data is included or not</param>
        /// <returns>ReportPackageDto Object</returns>
        ReportPackageDto GetReportPackage(int reportPackageId, bool isIncludeAssociatedElementData, bool isAuthorizationRequired = false);

        /// <summary>
        ///     The file 'Copy of Record Data.xml' contains below:
        ///     1.  The raw 'text' data including samples and results data, certification name and text, comments,
        ///     List of files included in the .zip file,  and copy of record receipt details
        ///     2.  Copy of Record receipt, including UserProfile Details,  Organization Details, and ReportDetails
        ///     3.  Copy of Record file Manifest: A list of each file in the .zip
        /// </summary>
        /// <param name="reportPackageDto">The ReportPackageDto containing all detailed information </param>
        /// <returns>CopyOfRecordDataXmlFileDto</returns>
        CopyOfRecordDataXmlFileDto GetReportPackageCopyOfRecordDataXmlFile(ReportPackageDto reportPackageDto);

        /// <summary>
        ///     System generated formatted PDF containing(nearly all) data in the Copy of Record Data.xml,
        ///     combined into a single formatted PDF. This is a human readable and printable report.
        ///     Format to match COR Standard Report Format.docx
        /// </summary>
        /// <param name="reportPackageDto">The ReportPackageDto containing all detailed information </param>
        /// <returns>CopyOfRecordPdfFileDto</returns>
        CopyOfRecordPdfFileDto GetReportPackageCopyOfRecordPdfFile(ReportPackageDto reportPackageDto);

        /// <summary>
        /// Get the pdf file for the Copy of Record
        /// </summary>
        /// <param name="reportPackageId">ReportPackageId</param>
        /// <returns>CopyOfRecordPdfFileDto</returns>
        CopyOfRecordPdfFileDto GetReportPackageCopyOfRecordPdfFile(int reportPackageId);

        /// <summary>
        ///     *WARNING: NO VALIDATION CHECK -- CASCADE DELETE*
        ///     Hard delete of row from tReportPackage table associated with passed in parameter.
        ///     Programatically cascade deletes rows in the following associated tables:
        ///     - tReportPackageElementCategory (via ReportPackageId)
        ///     - tReportPackageElementType (via ReportPackageElementCategoryId)
        ///     - tReportSample (via ReportPackageElementTypeId)
        ///     - tReportFile (via ReportPackageElementTypeId)
        ///     - tCopyofRecord (via ReportPackageId)
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        ReportPackageDto DeleteReportPackage(int reportPackageId);

        /// <summary>
        ///     To be called after a User selects a template and date range but before
        ///     the User clicks the "Save Draft" button (no reportPackageDto to save yet)
        /// </summary>
        /// <param name="reportPackageTemplateId"></param>
        /// <param name="startDateTimeLocal"></param>
        /// <param name="endDateTimeLocal"></param>
        /// <returns>The newly created tReportPackage.ReportPackageId</returns>
        int CreateDraft(int reportPackageTemplateId, DateTime startDateTimeLocal, DateTime endDateTimeLocal);

        /// <summary>
        ///     Cannot be used to CREATE, only to UPDATE. Use "CreateDraft" to create.
        ///     reportPackageDto.ReportPackageId must exist or exception thrown
        /// </summary>
        /// <param name="reportPackageDto">Existing Report Package to update</param>
        /// <param name="isUseTransaction">If true, runs within transaction object</param>
        /// <returns>Existing ReportPackage.ReportPackageId</returns>
        int SaveReportPackage(ReportPackageDto reportPackageDto, bool isUseTransaction);

        /// <summary>
        ///     Performs validation to ensure only allowed state transitions are occur,
        ///     throw RuleViolationException otherwise. Does NOT enter any corresponding values into the Report Package row.
        /// </summary>
        /// <param name="reportPackageId"></param>
        /// <param name="reportStatus">Intended target state</param>
        /// <param name="isUseTransaction">If true, runs within transaction object</param>
        void UpdateStatus(int reportPackageId, ReportStatusName reportStatus, bool isUseTransaction);

        /// <summary>
        ///     Gets a collection of FileStoreDto's that are eligible to be added this Report Package -- also indicate which are already associated.
        /// </summary>
        /// <param name="reportPackageElementTypeId">tReportPackage.ReportPackageId</param>
        /// <returns>Collection of FileStoreDto objects</returns>
        ICollection<FileStoreDto> GetFilesForSelection(int reportPackageElementTypeId);

        /// <summary>
        ///     Gets a collection of SampleDto's that are eligible to be added this Report Package -- also indicate which are already associated.
        /// </summary>
        /// <param name="reportPackageElementTypeId">ReportPackageElementTypeId</param>
        /// <returns>Collection of SampleDto objects</returns>
        ICollection<SampleDto> GetSamplesForSelection(int reportPackageElementTypeId);

        /// <summary>
        /// Used to generically fetch a ReportPackageElementType object from the database. Can be used to fetch "content elements" (such as certifications)
        /// from the database.
        /// </summary>
        /// <param name="reportPackageElementTypeId">tReportPackageElementType.ReportPackageElementTypeId</param>
        /// <returns></returns>
        ReportPackageElementTypeDto GetReportReportPackageElementType(int reportPackageElementTypeId);

        /// <summary>
        /// Get counts for different report package status 
        ///  1. Authority portal: return new submitted report package count and new repudiation report package count
        ///  2. Industry portal: return draft count, and ready to submit report package count
        /// </summary>
        /// <returns></returns>
        List<ReportPackageStatusCount> GetReportPackageStatusCounts();

        /// <summary>
        ///     Gets Report Package information (without children element data) for displaying in a grid.
        /// </summary>
        /// <param name="reportStatusName">Fetches report packages of this status only</param>
        /// <returns>Collection of ReportPackageDto objects (without children element data)</returns>
        IEnumerable<ReportPackageDto> GetReportPackagesByStatusName(ReportStatusName reportStatusName);

        /// <summary>
        ///     Gets items to populate a dropdown list of reasons to repudiate a report package (for a specific Org Reg Program Id)
        /// </summary>
        /// <returns></returns>
        IEnumerable<RepudiationReasonDto> GetRepudiationReasons();

        /// <summary>
        ///     Performs various validation checks before putting a report package into "Repudiated" status.
        ///     Also logs action and emails stakeholders.
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        /// <param name="repudiationReasonId">tRepudiationReason.RepudiationReasonId</param>
        /// <param name="repudiationReasonName">Snapshot of tRepudiationReason.Name</param>
        /// <param name="comments">Optional field to accompany "other reason"</param>
        void RepudiateReport(int reportPackageId, int repudiationReasonId, string repudiationReasonName, string comments = null);

        /// <summary>
        ///     Meant to be called when user has reviewed a report submission. Updates the corresponding fields in the Report
        ///     Package row.
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        /// <param name="comments">Optional field</param>
        void ReviewSubmission(int reportPackageId, string comments = null);

        /// <summary>
        ///     Meant to be called when user has reviewed a report repudiation. Updates the corresponding fields in the Report
        ///     Package row.
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        /// <param name="comments">Required field</param>
        void ReviewRepudiation(int reportPackageId, string comments);

        /// <summary>
        /// Iterates through all required element types for a given report package where content is not provided and 
        /// ensures there is at least one "sample & results" or "file" associated with the report package
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        /// <returns>True if there is an association for all required element types where content is not provided</returns>
        bool IsRequiredReportPackageElementTypesIncluded(int reportPackageId);

        /// <summary>
        /// Updates the LastSentDateTimeUtc value of a row in table tReportPackage.
        /// Also optionally updates the "last sender" details.
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        /// <param name="sentDateTime">eg. DateTimeOffset.Now</param>
        /// <param name="lastSenderUserId">Optional: tUserProfile.UserProfileId</param>
        /// <param name="lastSenderFirstName">Optional: tUserProfile.FirstName</param>
        /// <param name="lastSenderLastName">Optional: tUserProfile.LastName</param>
        void UpdateLastSentDateTime(int reportPackageId, DateTimeOffset sentDateTime, int? lastSenderUserId = null, string lastSenderFirstName = null, string lastSenderLastName = null);

        /// <summary>
        /// Identifies if a newer Report Package (same industry, template name and same Reporting Period)
        /// exists with a newer SubmissionDateTimeUtc.
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId of the ReportPackage we want to compare the submission date/time with</param>
        /// <returns>True if a Report Package with a later submission date/time exists</returns>
        bool IsSimilarReportPackageSubmittedAfter(int reportPackageId);

        /// <summary>
        /// True if the current date/time is still within the "max days after report period end date" for repudiation
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        /// <returns></returns>
        bool CanRepudiateReportPackage(int reportPackageId);
    }
}