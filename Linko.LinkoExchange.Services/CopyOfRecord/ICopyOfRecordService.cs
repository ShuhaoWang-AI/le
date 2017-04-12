﻿
using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.CopyOfRecord
{
    public interface ICopyOfRecordService
    {
        void CreateCopyOfRecordForReportPackage(int reportPackageId, IEnumerable<FileStoreDto> attachments,
            CopyOfRecordPdfFileDto copyOfRecordPdfFileDto, CopyOfRecordDataXmlFileDto copyOfRecordDataXmlFileDto);

        bool ValidCoreData(int reportPackageId);
        CopyOfRecordDto GetCopyOfRecordByReportPackage(ReportPackageDto reportPackage);
    }
}
