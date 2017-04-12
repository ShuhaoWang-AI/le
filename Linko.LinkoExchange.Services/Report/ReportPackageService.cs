using System;
using System.Collections.Generic;
using Linko.LinkoExchange.Services.CopyOfRecord;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Program;

namespace Linko.LinkoExchange.Services.Report
{
    // TODO: To implement more 

    public class ReportPackageService : IReportPackageService
    {
        private readonly IProgramService _programService;
        private readonly ICopyOfRecordService _copyOfRecordService;
        public ReportPackageService(
            IProgramService programService,
            ICopyOfRecordService copyOfRecordService
            )
        {
            _programService = programService;
            _copyOfRecordService = copyOfRecordService;
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

        //TODO: to implement this!
        public ReportPackageDto GetReportPackage(int reportPackageId)
        {
            var rptDto = new ReportPackageDto
            {
                ReportPackageId = reportPackageId,
                Name = " 1st Quarter PCR",
                OrganizationRegulatoryProgramId = 3,
                SubMissionDateTime = DateTime.UtcNow,
            };

            rptDto.OrganizationRegulatoryProgramDto =
                    _programService.GetOrganizationRegulatoryProgram(rptDto.OrganizationRegulatoryProgramId);

            return rptDto;
        }

        public CopyOfRecordDto GetCopyOfRecordByReportPackageId(int reportPackageId)
        {
            var reportPackageDto = GetReportPackage(reportPackageId);
            return _copyOfRecordService.GetCopyOfRecordByReportPackage(reportPackageDto);
        }

        public CopyOfRecordDto CreateCopyOfRecordForReportPackage(int reportPackageId)
        {
            var reportPackageDto = GetReportPackage(reportPackageId);
            var attachments = GetReportPackageAttachments(reportPackageId);
            var copyOfRecordPdfFile = GetReportPackageCopyOfRecordPdfFile(reportPackageId);
            var copyOfRecordDataXmlFile = GetReportPackageCopyOfRecordDataXmlFile(reportPackageId);
            _copyOfRecordService.CreateCopyOfRecordForReportPackage(reportPackageId, attachments, copyOfRecordPdfFile, copyOfRecordDataXmlFile);

            return _copyOfRecordService.GetCopyOfRecordByReportPackage(reportPackageDto);
        }
    }
}