using System;
using System.Collections.Generic;
using Linko.LinkoExchange.Services.CopyOfRecord;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;

namespace Linko.LinkoExchange.Services.Report
{
    // TODO: To implement more 

    public class ReportPackageService : IReportPackageService
    {
        private readonly IProgramService _programService;
        private readonly ICopyOfRecordService _copyOfRecordService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ILogger _logger;

        public ReportPackageService(
            IProgramService programService,
            ICopyOfRecordService copyOfRecordService,
            ITimeZoneService timeZoneService,
            ILogger logger
            )
        {
            _programService = programService;
            _copyOfRecordService = copyOfRecordService;
            _timeZoneService = timeZoneService;
            _logger = logger;
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

        public CopyOfRecordValidationResultDto VerififyCopyOfRecord(int reportPackageId)
        {
            _logger.Info("Enter ReportPackageService.VerififyCopyOfRecord. reportPackageId={0}", reportPackageId);

            var validationResult = _copyOfRecordService.ValidCopyOfRecordData(reportPackageId);

            _logger.Info("Enter ReportPackageService.VerififyCopyOfRecord. reportPackageId={0}", reportPackageId);

            return validationResult;
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

            rptDto.SubMissionDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingSettingForThisOrg(rptDto.SubMissionDateTime, rptDto.OrganizationRegulatoryProgramId);

            return rptDto;
        }

        public CopyOfRecordDto GetCopyOfRecordByReportPackageId(int reportPackageId)
        {
            _logger.Info("Enter ReportPackageService.GetCopyOfRecordByReportPackageId. reportPackageId={0}", reportPackageId);

            var reportPackageDto = GetReportPackage(reportPackageId);
            var copyOfRecordDto = _copyOfRecordService.GetCopyOfRecordByReportPackage(reportPackageDto);

            _logger.Info("Enter ReportPackageService.GetCopyOfRecordByReportPackageId. reportPackageId={0}", reportPackageId);

            return copyOfRecordDto;
        }

        public CopyOfRecordDto CreateCopyOfRecordForReportPackage(int reportPackageId)
        {
            _logger.Info("Enter ReportPackageService.CreateCopyOfRecordForReportPackage. reportPackageId={0}", reportPackageId);

            var attachments = GetReportPackageAttachments(reportPackageId);
            var copyOfRecordPdfFile = GetReportPackageCopyOfRecordPdfFile(reportPackageId);
            var copyOfRecordDataXmlFile = GetReportPackageCopyOfRecordDataXmlFile(reportPackageId);
            var copyOfRecordDto = _copyOfRecordService.CreateCopyOfRecordForReportPackage(reportPackageId, attachments, copyOfRecordPdfFile, copyOfRecordDataXmlFile);

            _logger.Info("Enter ReportPackageService.CreateCopyOfRecordForReportPackage. reportPackageId={0}", reportPackageId);

            return copyOfRecordDto;
        }
    }
}