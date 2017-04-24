using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;
using System;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Report
{
    public interface IReportPackageService
    {
        void SignAndSubmitReportPackage(int reportPackageId);
        CopyOfRecordDto GetCopyOfRecordByReportPackageId(int reportPackageId, ReportPackageDto reportPackageDto = null);
        CopyOfRecordDto CreateCopyOfRecordForReportPackage(ReportPackageDto reportPackageDto);
        CopyOfRecordValidationResultDto VerififyCopyOfRecord(int reportPackageId);

        /// <summary>
        /// Get ReportPackageDto object by report package Id. 
        /// </summary>
        /// <param name="reportPackageId">ReportPackageId</param>
        /// <param name="incldingAttachmentFileData">Indicating attachments binary data is included or not</param>
        /// <returns>ReportPackageDto Object</returns> 
        ReportPackageDto GetReportPackage(int reportPackageId, bool incldingAttachmentFileData);

        /// <summary>
        /// The file 'Copy of Record Data.xml' contains below:
        /// 1.  The raw 'text' data including samples and results data, certification name and text, coments,
        ///     List of files included in the .zip file,  and copy of record receipt details
        /// 2.  Copy of Record receit, including UserProfile Details,  Organization Details, and ReportDetails
        /// 3.  Copy of Record file Manifest: A list of earch file in the .zip      
        /// </summary>
        /// <param name="reportPackageDto">The ReportPackageDto containing all detailed information </param>
        /// <returns>CopyOfRecordDataXmlFileDto</returns>
        CopyOfRecordDataXmlFileDto GetReportPackageCopyOfRecordDataXmlFile(ReportPackageDto reportPackageDto);

        /// <summary>
        /// System generated formatted PDF containing(nearly all) data in the Copy of Record Data.xml, 
        /// combined into a single formatted PDF. This is a human readable and printable report.
        /// Format to match COR Standard Report Format.docx
        /// </summary>
        /// <param name="reportPackageDto">The ReportPackageDto containing all detailed information </param>
        /// <returns>CopyOfRecordPdfFileDto</returns>
        CopyOfRecordPdfFileDto GetReportPackageCopyOfRecordPdfFile(ReportPackageDto reportPackageDto);

        /// <summary>
        /// *WARNING: NO VALIDATION CHECK -- CASCADE DELETE*
        /// Hard delete of row from tReportPackage table associated with passed in parameter.
        /// Programatically cascade deletes rows in the following associated tables:
        /// - tReportPackageElementCategory (via ReportPackageId)
        /// - tReportPackageElementType (via ReportPackageElementCategoryId)
        /// - tReportSample (via ReportPackageElementTypeId)
        /// - tReportFile (via ReportPackageElementTypeId)
        /// - tCopyofRecord (via ReportPackageId)
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        void DeleteReportPackage(int reportPackageId);

        /// <summary>
        /// To be called after a User selects a template and date range but before 
        /// the User clicks the "Save Draft" button (no reportPackageDto to save yet)
        /// </summary>
        /// <param name="reportPackageTemplateId"></param>
        /// <param name="startDateTimeLocal"></param>
        /// <param name="endDateTimeLocal"></param>
        /// <returns>The newly created tReportPackage.ReportPackageId</returns>

        int CreateDraft(int reportPackageTemplateId, DateTime startDateTimeLocal, DateTime endDateTimeLocal);

        /// <summary>
        /// Cannot be used to CREATE, only to UPDATE. Use "CreateDraft" to create.
        /// reportPackageDto.ReportPackageId must exist or exception thrown
        /// </summary>
        /// <param name="reportPackageDto">Existing Report Package to update</param>
        /// <returns>Existing ReportPackage.ReportPackageId</returns>
        int SaveReportPackage(ReportPackageDto reportPackageDto);

        /// <summary>
        /// Performs validation to ensure only allowed state transitions are occur,
        /// throw RuleViolationException otherwise
        /// </summary>
        /// <param name="reportStatus">Intended target state</param>
        void UpdateStatus(int reportPackageId, ReportStatusName reportStatus);
    }
}