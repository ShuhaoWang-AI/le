using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;

namespace Linko.LinkoExchange.Services.CopyOfRecord
{
    public class CopyOfRecordService : ICopyOfRecordService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;
        private readonly IHttpContextService _httpContextService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IDigitalSignatureManager _digitalSignatureManager;

        public CopyOfRecordService(
            LinkoExchangeContext dbContext,
            IMapHelper mapHelper,
            ILogger logger,
            IHttpContextService httpContextService,
            ITimeZoneService timeZoneService,
            IDigitalSignatureManager digitalSignatureManager)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (mapHelper == null)
            {
                throw new ArgumentNullException(nameof(mapHelper));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (httpContextService == null)
            {
                throw new ArgumentNullException(nameof(httpContextService));
            }

            if (timeZoneService == null)
            {
                throw new ArgumentNullException(nameof(timeZoneService));
            }

            if (digitalSignatureManager == null)
            {
                throw new ArgumentNullException(nameof(digitalSignatureManager));
            }

            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _logger = logger;
            _httpContextService = httpContextService;
            _timeZoneService = timeZoneService;
            _digitalSignatureManager = digitalSignatureManager;
        }

        public void CreateCopyOfRecordForReportPackage(int reportPackageId, IEnumerable<FileStoreDto> attachments, CopyOfRecordPdfFileDto copyOfRecordPdfFileDto, CopyOfRecordDataXmlFileDto copyOfRecordDataXmlFileDto)
        {
            _logger.Info($"Enter CopyOfRecordService.CreateCopyOfRecordForReportPackage. ReportPackageId:{reportPackageId}");

            try
            {
                var coreBytes = CreateZipFileData(attachments, copyOfRecordPdfFileDto, copyOfRecordDataXmlFileDto);

                var copyOfRecord = new Core.Domain.CopyOfRecord
                {
                    Hash = HashHelper.ComputeSha256Hash(data: coreBytes),
                    HashAlgorithm = HashAlgorithm.Sha256.ToString(),
                    SignatureAlgorithm = DigitalSignatureAlgorithm.Sha1.ToString(),
                    Data = new byte[coreBytes.Length]
                };

                Array.Copy(sourceArray: coreBytes, destinationArray: copyOfRecord.Data, length: coreBytes.Length);
                copyOfRecord.Signature = SignaData(hash: copyOfRecord.Hash);
                copyOfRecord.CopyOfRecordCertificateId = _digitalSignatureManager.LatestCertificateId();
                copyOfRecord.ReportPackageId = reportPackageId;
                _dbContext.CopyOfRecords.Add(entity: copyOfRecord);
                _dbContext.SaveChanges();

                var message = $"Leave CopyOfRecordService.CreateCopyOfRecordForReportPackage. ReportPackageId:{reportPackageId}.";
                _logger.Info(message: message);
            }
            catch (Exception ex)
            {
                var errors = new List<string>() { ex.Message };
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    errors.Add(ex.Message);
                }
                _logger.Error("Error happens {0} ", string.Join("," + Environment.NewLine, errors));
                throw;
            }
        }

        public bool ValidCoreData(int reportPackageId)
        {
            var copyOfRecord = _dbContext.CopyOfRecords.Single(i => i.ReportPackageId == reportPackageId);
            return _digitalSignatureManager.VerifySignature(copyOfRecord.Signature, copyOfRecord.Data);
        }

        public CopyOfRecordDto GetCopyOfRecordByReportPackage(ReportPackageDto reportPackage)
        {
            //TODO to verify user permissions?   
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

            var datetimePart = reportPackage.SubMissionDateTime.ToString(format: "yyyyMMdd");
            copyOfRecordDto.DownloadFileName = $"{reportPackage.OrganizationRegulatoryProgramId} {reportPackage.Name} {datetimePart}.zip";

            return copyOfRecordDto;
        }

        #region Private functions

        private string SignaData(string hash)
        {
            var hashBytes = Encoding.UTF8.GetBytes(s: hash);
            var hashBase64 = Convert.ToBase64String(inArray: hashBytes);

            return _digitalSignatureManager.SignData(base64Data: hashBase64);
        }
        private static void AddFileIntoZipArchive(ZipArchive archive, string fileName, byte[] fileData)
        {
            var archiveEntry = archive.CreateEntry(entryName: Path.GetFileName(path: fileName), compressionLevel: CompressionLevel.Optimal);
            using (var archiveEntryStream = archiveEntry.Open())
            using (var streamWriter = new MemoryStream(buffer: fileData))
            {
                streamWriter.CopyTo(destination: archiveEntryStream);
            }
        }

        //TODO:
        //1. get attachment files
        //2. get Copy Of Record.pdf
        //3. get Copy Of Record Data.xml 
        private byte[] CreateZipFileData(IEnumerable<FileStoreDto> attachments, CopyOfRecordPdfFileDto copyOfRecordPdfFileDto, CopyOfRecordDataXmlFileDto copyOfRecordDataXmlFileDto)
        {
            byte[] coreBytes;
            using (var stream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(stream: stream, mode: ZipArchiveMode.Create))
                {
                    // Attachment files
                    foreach (var attachment in attachments)
                    {
                        AddFileIntoZipArchive(archive: zipArchive, fileName: attachment.Name, fileData: attachment.Data);
                    }

                    // Copy Of Record.pdf  
                    AddFileIntoZipArchive(archive: zipArchive, fileName: copyOfRecordPdfFileDto.FileName, fileData: copyOfRecordPdfFileDto.FileData);

                    // Copy Of Record Data.xml
                    AddFileIntoZipArchive(archive: zipArchive, fileName: copyOfRecordDataXmlFileDto.FileName, fileData: copyOfRecordDataXmlFileDto.FileData);
                }

                coreBytes = stream.ToArray();
            }
            return coreBytes;
        }

        #endregion
    }
}