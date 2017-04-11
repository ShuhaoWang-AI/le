using System;
using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Program;

namespace Linko.LinkoExchange.Services.Report
{
    // TODO:
    // Mock service, needs to be replaced with real one.
    public class ReportPackageServiceMock : IReportPackageService
    {
        private readonly IProgramService _programService;
        public ReportPackageServiceMock(IProgramService programService)
        {
            _programService = programService;
        }

        /// <summary>
        ///  Prepare Mock data;
        /// </summary>
        /// <param name="reportPackageId"></param>
        /// <returns></returns>
        public IList<FileStoreDto> GetReportPackageAttachments(int reportPackageId)
        {
            throw new System.NotImplementedException();
        }

        public IList<ReportPackageELementTypeDto> GetReportPackageCertifications(int reportPackageId)
        {
            throw new System.NotImplementedException();
        }

        public CopyOfRecordPdfFileDto GetReportPackageCopyOfRecordPdfFile(int reportPackageId)
        {
            throw new System.NotImplementedException();
        }

        public CopyOfRecordDataXmlFileDto GetReportPackageCopyOfRecordDataXmlFile(int reportPackageId)
        {
            throw new System.NotImplementedException();
        }

        public ReportPackageDto GetReportPackage(int reportPackageId)
        {
            var rptDto = new ReportPackageDto
            {
                Name = " 1st Quarter PCR",
                OrganizationRegulatoryProgramId = 3,
                SubMissionDateTime = DateTime.UtcNow
            };

            rptDto.OrganizationRegulatoryProgramDto =
                    _programService.GetOrganizationRegulatoryProgram(rptDto.OrganizationRegulatoryProgramId);

            return rptDto;
        }
    }
}