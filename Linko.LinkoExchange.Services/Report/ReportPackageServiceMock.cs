using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Report
{
    public class ReportPackageServiceMock : IReportPackageService
    {
        public IList<FileStoreDto> GetReportPackageAttachments(int reportPackageId)
        {
            throw new System.NotImplementedException();
        }

        public IList<ReportPackageELementTypeDto> GetReportPackageCertifications(int reportPackageId)
        {
            throw new System.NotImplementedException();
        }

        public CorPreviewFileDto GetReportPackageSampleFormData(int reportPackageId)
        {
            throw new System.NotImplementedException();
        }

        public CorManifestFileDato GetReportPackageManefestData(int reportPackageId)
        {
            throw new System.NotImplementedException();
        }
    }
}