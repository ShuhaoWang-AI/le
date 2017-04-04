using System;
using System.IO;
using System.IO.Compression;
using System.Text;
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

        public CopyOfRecordService(
            LinkoExchangeContext dbContext,
            IMapHelper mapHelper,
            ILogger logger,
            IHttpContextService httpContextService,
            ITimeZoneService timeZoneService,
            IReportPackageService reportPackageService)
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

            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _logger = logger;
            _httpContextService = httpContextService;
            _timeZoneService = timeZoneService;
            _reportPackageService = reportPackageService;
        }

        public CopyOfRecordDto CreateCopyOfRecordForReportPackage(int reportPackageId)
        {
            //TODO:
            //1. get attachment files
            //2. get pdf generated from form data
            //3. ?? 
            var attachmentFiles = _reportPackageService.GetReportPackageAttachments(reportPackageId);
            var certifications = _reportPackageService.GetReportPackageCertifications(reportPackageId);
            var corPreviewFileDto = _reportPackageService.GetReportPackageSampleFormData(reportPackageId);
            var manifestDto = _reportPackageService.GetReportPackageManefestData(reportPackageId);

            byte[] corData;
            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    foreach (var attachment in attachmentFiles)
                    {
                        var archiveEntry = archive.CreateEntry(Path.GetFileName(attachment.Name), CompressionLevel.Optimal);
                        using (var archiveEntryStream = archiveEntry.Open())
                        using (var streamWriter = new MemoryStream(attachment.Data))
                        {
                            streamWriter.CopyTo(archiveEntryStream);
                        }
                    }

                    // for certifications 
                    foreach (var certification in certifications)
                    {
                        var archiveEntry = archive.CreateEntry(Path.GetFileName(certification.ReportElementTypeName),
                            CompressionLevel.Optimal);

                        using (var archiveEntryStream = archiveEntry.Open())
                        {
                            var certificationData = Encoding.UTF8.GetBytes(certification.ReportElementTypeContent);
                            using (var streamWriter = new MemoryStream(certificationData))
                            {
                                streamWriter.CopyTo(archiveEntryStream);
                            }
                        }
                    }

                    // for cor preview file data  
                    var previewFileAarchiveEntry = archive.CreateEntry(Path.GetFileName(corPreviewFileDto.FileName),
                            CompressionLevel.Optimal);

                    using (var archiveEntryStream = previewFileAarchiveEntry.Open())
                    {
                        using (var streamWriter = new MemoryStream(corPreviewFileDto.FileData))
                        {
                            streamWriter.CopyTo(archiveEntryStream);
                        }
                    }

                    // for manifest file 
                    // for cor preview file data  
                    var manifestFileAarchiveEntry = archive.CreateEntry(Path.GetFileName(manifestDto.FileName),
                            CompressionLevel.Optimal);

                    using (var archiveEntryStream = manifestFileAarchiveEntry.Open())
                    {
                        using (var streamWriter = new MemoryStream(manifestDto.FileData))
                        {
                            streamWriter.CopyTo(archiveEntryStream);
                        }
                    }
                }
            }

            // Create a zip file for 

            throw new System.NotImplementedException();
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
    }
}