using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using NLog;

namespace Linko.LinkoExchange.Services.CopyOfRecord
{
    public class CopyOfRecordService : ICopyOfRecordService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly IDigitalSignatureManager _digitalSignatureManager;
        private readonly ILogger _logger;

        #endregion

        #region constructors and destructor

        public CopyOfRecordService(
            LinkoExchangeContext dbContext,
            ILogger logger,
            IDigitalSignatureManager digitalSignatureManager)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(paramName:nameof(dbContext));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(paramName:nameof(logger));
            }

            if (digitalSignatureManager == null)
            {
                throw new ArgumentNullException(paramName:nameof(digitalSignatureManager));
            }

            _dbContext = dbContext;
            _logger = logger;
            _digitalSignatureManager = digitalSignatureManager;
        }

        #endregion

        #region interface implementations

        public CopyOfRecordDto CreateCopyOfRecordForReportPackage(int reportPackageId, IEnumerable<FileStoreDto> attachments, CopyOfRecordPdfFileDto copyOfRecordPdfFileDto,
                                                                  CopyOfRecordDataXmlFileDto copyOfRecordDataXmlFileDto)
        {
            _logger.Info(message:$"Enter CopyOfRecordService.CreateCopyOfRecordForReportPackage. ReportPackageId:{reportPackageId}");

            var coreBytes = CreateZipFileData(attachments:attachments, copyOfRecordPdfFileDto:copyOfRecordPdfFileDto, copyOfRecordDataXmlFileDto:copyOfRecordDataXmlFileDto);

            var copyOfRecord = new Core.Domain.CopyOfRecord
                               {
                                   Hash = HashHelper.ComputeSha256Hash(data:coreBytes),
                                   HashAlgorithm = HashAlgorithm.Sha256.ToString(),
                                   SignatureAlgorithm = DigitalSignatureAlgorithm.Sha1.ToString(),
                                   Data = new byte[coreBytes.Length]
                               };

            Array.Copy(sourceArray:coreBytes, destinationArray:copyOfRecord.Data, length:coreBytes.Length);
            copyOfRecord.Signature = SignaData(hash:copyOfRecord.Hash);
            copyOfRecord.CopyOfRecordCertificateId = _digitalSignatureManager.LatestCertificateId();
            copyOfRecord.ReportPackageId = reportPackageId;
            _dbContext.CopyOfRecords.Add(entity:copyOfRecord);
            _dbContext.SaveChanges();

            var copyOfRecordDto = new CopyOfRecordDto
                                  {
                                      ReportPackageId = copyOfRecord.ReportPackageId,
                                      Signature = copyOfRecord.Signature,
                                      SignatureAlgorithm = copyOfRecord.SignatureAlgorithm,
                                      Hash = copyOfRecord.Hash,
                                      HashAlgorithm = copyOfRecord.HashAlgorithm,
                                      CopyOfRecordCertificateId = copyOfRecord.CopyOfRecordCertificateId
                                  };

            var message = $"Leave CopyOfRecordService.CreateCopyOfRecordForReportPackage. ReportPackageId:{reportPackageId}.";
            _logger.Info(message:message);

            return copyOfRecordDto;
        }

        public CopyOfRecordValidationResultDto ValidCopyOfRecordData(int reportPackageId)
        {
            _logger.Info(message:$"Enter CopyOfRecordService.ValidCopyOfRecordData. ReportPackageId:{reportPackageId}");

            var copyOfRecord = _dbContext.CopyOfRecords.Single(i => i.ReportPackageId == reportPackageId);
            var isValid = _digitalSignatureManager.VerifySignature(currentSignatureStr:copyOfRecord.Signature, dataToVerify:copyOfRecord.Data);
            var validationResult = new CopyOfRecordValidationResultDto
                                   {
                                       Valid = isValid,
                                       DigitalSignature = copyOfRecord.Signature
                                   };

            _logger.Info(message:$"Leave CopyOfRecordService.ValidCopyOfRecordData. ReportPackageId:{reportPackageId}");

            return validationResult;
        }

        public CopyOfRecordDto GetCopyOfRecordByReportPackage(ReportPackageDto reportPackage)
        {
            if (reportPackage == null)
            {
                throw new ArgumentNullException(paramName:nameof(reportPackage));
            }

            _logger.Info(message:$"Enter CopyOfRecordService.GetCopyOfRecordByReportPackage. ReportPackageId:{reportPackage.ReportPackageId}");

            var copyOfRecord = _dbContext.CopyOfRecords.Single(i => i.ReportPackageId == reportPackage.ReportPackageId);
            var copyOfRecordDto = new CopyOfRecordDto
                                  {
                                      ReportPackageId = reportPackage.ReportPackageId,
                                      Signature = copyOfRecord.Signature,
                                      SignatureAlgorithm = copyOfRecord.SignatureAlgorithm,
                                      Hash = copyOfRecord.Hash,
                                      HashAlgorithm = copyOfRecord.HashAlgorithm,
                                      Data = copyOfRecord.Data,
                                      CopyOfRecordCertificateId = copyOfRecord.CopyOfRecordCertificateId
                                  };

            var datetimePart = reportPackage.SubmissionDateTimeLocal.Value.ToString(format:"yyyyMMdd");
            var referenceNumber = string.IsNullOrWhiteSpace(value:reportPackage.OrganizationReferenceNumber) ? "" : reportPackage.OrganizationReferenceNumber;
            copyOfRecordDto.DownloadFileName = $"{referenceNumber} {reportPackage.Name} {datetimePart}.zip";
            copyOfRecordDto.DownloadFileName = StripReservedCharacters(fileName:copyOfRecordDto.DownloadFileName);

            _logger.Info(message:$"Leave CopyOfRecordService.GetCopyOfRecordByReportPackage. ReportPackageId:{reportPackage.ReportPackageId}");
            return copyOfRecordDto;
        }

        #endregion

        #region Private functions

        private string StripReservedCharacters(string fileName)
        {
            var reservedChars = new[] {"^", "<", ">", ":", "\"", "\\", "/", "|", "?", "*"};
            foreach (var reservedChar in reservedChars)
            {
                fileName = fileName.Replace(oldValue:reservedChar, newValue:"");
            }

            return fileName;
        }

        private string SignaData(string hash)
        {
            var hashBytes = Encoding.UTF8.GetBytes(s:hash);
            var hashBase64 = Convert.ToBase64String(inArray:hashBytes);
            return _digitalSignatureManager.SignData(base64Data:hashBase64);
        }

        private static void AddFileIntoZipArchive(ZipArchive archive, string fileName, byte[] fileData)
        {
            var archiveEntry = archive.CreateEntry(entryName:Path.GetFileName(path:fileName), compressionLevel:CompressionLevel.Optimal);
            using (var archiveEntryStream = archiveEntry.Open())
            {
                using (var streamWriter = new MemoryStream(buffer:fileData))
                {
                    streamWriter.CopyTo(destination:archiveEntryStream);
                }
            }
        }

        //1. get attachment files
        //2. get Copy Of Record.pdf
        //3. get Copy Of Record Data.xml
        private byte[] CreateZipFileData(IEnumerable<FileStoreDto> attachments, CopyOfRecordPdfFileDto copyOfRecordPdfFileDto,
                                         CopyOfRecordDataXmlFileDto copyOfRecordDataXmlFileDto)
        {
            byte[] coreBytes;
            using (var stream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(stream:stream, mode:ZipArchiveMode.Create))
                {
                    // Attachment files
                    foreach (var attachment in attachments)
                    {
                        AddFileIntoZipArchive(archive:zipArchive, fileName:attachment.Name, fileData:attachment.Data);
                    }

                    // Copy Of Record.pdf  
                    AddFileIntoZipArchive(archive:zipArchive, fileName:copyOfRecordPdfFileDto.FileName, fileData:copyOfRecordPdfFileDto.FileData);

                    // Copy Of Record Data.xml
                    AddFileIntoZipArchive(archive:zipArchive, fileName:copyOfRecordDataXmlFileDto.FileName, fileData:copyOfRecordDataXmlFileDto.FileData);
                }

                coreBytes = stream.ToArray();
            }

            return coreBytes;
        }

        #endregion
    }
}