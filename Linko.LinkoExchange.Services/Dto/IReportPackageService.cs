using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services
{
    public interface IReportPackageService
    {
        // TODO: Get sample result....  
        IList<FileStoreDto> GetReportPackageAttachments(int reportPackageId);
        IList<ReportPackageELementTypeDto> GetReportPackageCertifications(int reportPackageId);

        /// <summary>
        /// The function to get sample result into a pdf, and return the binary of the pdf
        /// </summary>
        /// <param name="reportPackageId"></param>
        /// <returns></returns>
        CorPreviewFileDto GetReportPackageSampleFormData(int reportPackageId);
        CorManifestFileDato GetReportPackageManefestData(int reportPackageId);
    }
}