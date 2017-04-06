using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;

namespace Linko.LinkoExchange.Services.CopyOrRecord
{
    public class CopyOfRecordService : ICopyOrRecordService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;
        private readonly IHttpContextService _httpContextService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IReportPackageService _reportPackageService;
        private readonly IDigitalSignManager _digitalSignManager;

        public CopyOfRecordService(
            LinkoExchangeContext dbContext,
            IMapHelper mapHelper,
            ILogger logger,
            IHttpContextService httpContextService,
            ITimeZoneService timeZoneService,
            IReportPackageService reportPackageService,
            IDigitalSignManager digitalSignManager)
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

            if (reportPackageService == null)
            {
                throw new ArgumentNullException(nameof(reportPackageService));
            }

            if (digitalSignManager == null)
            {
                throw new ArgumentNullException(nameof(digitalSignManager));
            }

            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _logger = logger;
            _httpContextService = httpContextService;
            _timeZoneService = timeZoneService;
            _reportPackageService = reportPackageService;
            _digitalSignManager = digitalSignManager;
        }

        public int CreateCopyOfRecordForReportPackage(int reportPackageId)
        {
            _logger.Info($"Enter CopyOfRecordService.CreateCopyOfRecordForReportPackage. ReportPackageId:{reportPackageId}");
            //TODO:
            //1. get attachment files
            //2. get pdf generated from form data
            //3. get cor proview pdf file
            //4. get anifestDto xml file
            using (var tansaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var coreBytes = CreateZipFileData(reportPackageId);

                    var copyOfRecord = new CopyOfRecord
                    {
                        Hash = HashHelper.ComputeSha256Hash(data: coreBytes),
                        HashAlgorithm = HashAlgorithm.Sha256.ToString(),
                        SignatureAlgorithm = DigitalSignAlgorithm.Sha1.ToString(),
                        Data = new byte[coreBytes.Length]
                    };

                    Array.Copy(sourceArray: coreBytes, destinationArray: copyOfRecord.Data, length: coreBytes.Length);
                    copyOfRecord.Signature = GenerateCorSignature(hash: copyOfRecord.Hash);

                    copyOfRecord = _dbContext.CopyOfRecords.Add(entity: copyOfRecord);
                    _dbContext.SaveChanges();
                    _dbContext.Commit(transaction: tansaction);

                    var message = $"Enter CopyOfRecordService.CreateCopyOfRecordForReportPackage. ReportPackageId:{reportPackageId}. CopyOfRecord Id:{copyOfRecord.CopyOfRecordId}";
                    _logger.Info(message: message);

                    return copyOfRecord.CopyOfRecordId;
                }
                catch (Exception ex)
                {
                    tansaction.Rollback();
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
        }

        public bool ValidCoreData(int copyOfRecordId)
        {
            throw new System.NotImplementedException();
        }

        public CopyOfRecordDto GetCopyOfRecordByReportPackageId(int reportPackageId)
        {
            throw new System.NotImplementedException();
        }

        public CopyOfRecordDto GetCopyOfRecordById(int copyOfRecordId)
        {
            throw new System.NotImplementedException();
        }

        #region Private functions

        private string GenerateCorSignature(string hash)
        {
            var hashBytes = Encoding.UTF8.GetBytes(s: hash);
            var hashBase64 = Convert.ToBase64String(inArray: hashBytes);

            return _digitalSignManager.GetDataSignature(base64Data: hashBase64);
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

        private byte[] CreateZipFileData(int reportPackageId)
        {
            var attachmentFiles = _reportPackageService.GetReportPackageAttachments(reportPackageId: reportPackageId);
            var certifications = _reportPackageService.GetReportPackageCertifications(reportPackageId: reportPackageId);
            var corPreviewFileDto = _reportPackageService.GetReportPackageSampleFormData(reportPackageId: reportPackageId);
            var manifestDto = _reportPackageService.GetReportPackageManefestData(reportPackageId: reportPackageId);

            byte[] coreBytes;
            using (var stream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(stream: stream, mode: ZipArchiveMode.Create))
                {
                    foreach (var attachment in attachmentFiles)
                    {
                        AddFileIntoZipArchive(archive: zipArchive, fileName: attachment.Name, fileData: attachment.Data);
                    }

                    // for certifications 
                    foreach (var certification in certifications)
                    {
                        var certificationData = Encoding.UTF8.GetBytes(certification.ReportElementTypeContent);
                        //TODO:  need a certification file name here 
                        var fileName = certification.ReportElementTypeName;

                        AddFileIntoZipArchive(archive: zipArchive, fileName: fileName, fileData: certificationData);
                    }

                    // for cor preview file data  
                    AddFileIntoZipArchive(archive: zipArchive, fileName: corPreviewFileDto.FileName, fileData: corPreviewFileDto.FileData);

                    // for manifest file  
                    AddFileIntoZipArchive(archive: zipArchive, fileName: manifestDto.FileName, fileData: manifestDto.FileData);
                }

                coreBytes = stream.ToArray();
            }
            return coreBytes;
        }

        #endregion
    }
}