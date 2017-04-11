using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Report
{
    public interface IReportPackageService
    {
        // TODO: Get sample result....  

        CopyOfRecordDto GetCopyOfRecordByReportPackageId(int reportPackageId);
        CopyOfRecordDto CreateCopyOfRecordForReportPackage(int reportPackageId);

        ReportPackageDto GetReportPackage(int reportPackageId);
        IList<FileStoreDto> GetReportPackageAttachments(int reportPackageId);

        /// <summary>
        /// The file 'Copy of Record Data.xml' contains below:
        /// 1.  The raw 'text' data including samples and results data, certification name and text, coments,
        ///     List of files included in the .zip file,  and copy of record receipt details
        /// 2.  Copy of Record receit, including UserProfile Details,  Organization Details, and ReportDetails
        /// 3.  Copy of Record file Manifest: A list of earch file in the .zip      
        /// </summary>
        /// <param name="reportPackageId"></param>
        /// <returns></returns>
        CopyOfRecordDataXmlFileDto GetReportPackageCopyOfRecordDataXmlFile(int reportPackageId);

        /// <summary>
        /// System generated formatted PDF containing(nearly all) data in the Copy of Record Data.xml, 
        /// combined into a single formatted PDF. This is a human readable and printable report.
        /// Format to match COR Standard Report Format.docx
        /// </summary>
        /// <param name="reportPackageId"></param>
        /// <returns></returns>
        CopyOfRecordPdfFileDto GetReportPackageCopyOfRecordPdfFile(int reportPackageId);
    }
}