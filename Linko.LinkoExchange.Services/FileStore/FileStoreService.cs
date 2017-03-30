using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;

namespace Linko.LinkoExchange.Services.FileStore
{
    public class FileStoreService : IFileStoreService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;
        private readonly IHttpContextService _httpContextService;
        private readonly ITimeZoneService _timeZoneService;

        // Max file size 10 M Byte 
        private const int MaxFileSize = 1024 * 1024 * 10;
        private const int SizeToReduce = 1024 * 1024 * 2;

        private readonly string[] _validExtensions =
        {
            ".docx", ".doc", ".xls", ".xlsx", ".pdf", ".tif",
            ".jpg", ".jpeg", ".bmp", ".png", ".txt", ".csv"
        };

        public FileStoreService(
            LinkoExchangeContext dbContext,
            IMapHelper mapHelper,
            ILogger logger,
            IHttpContextService httpContextService,
            ITimeZoneService timeZoneService)
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

            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _logger = logger;
            _httpContextService = httpContextService;
            _timeZoneService = timeZoneService;
        }

        public List<string> GetValidAttachmentFileExtensions()
        {
            return new List<string>(_validExtensions);
        }

        public bool IsValidFileExtension(string ext)
        {
            if (string.IsNullOrWhiteSpace(ext))
            {
                return false;
            }

            if (ext.StartsWith("."))
            {
                ext = ext.Substring(1);
            }

            return _validExtensions.Contains(ext);
        }

        public List<FileStoreDto> GetFileStores()
        {
            _logger.Info("Enter FileStoreService.GetFileStores.");

            var currentRegulatoryProgramId =
                int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            var fileStores = _dbContext.FileStores.Where(i => i.OrganizationRegulatoryProgramId == currentRegulatoryProgramId);
            var fileStoreDtos = fileStores.Select(i => _mapHelper.GetFileStoreDtoFromFileStore(i)).ToList();

            fileStoreDtos = fileStoreDtos.Select(i => LocalizeFileStoreDtoUploadDateTime(i, currentRegulatoryProgramId)).ToList();

            _logger.Info("Leave FileStoreService.GetFileStores.");
            return fileStoreDtos;
        }

        public int CreateFileStore(FileStoreDto fileStoreDto)
        {
            _logger.Info("Enter FileStoreService.CreateFileStore.");

            if (fileStoreDto.Data.Length > MaxFileSize)
            {
                List<RuleViolation> validationIssues = new List<RuleViolation>();

                string message = "The file size exceeds that 10 MB limit.";
                validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
            }

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var currentUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
                    var currentRegulatoryProgramId =
                        int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

                    // Try to reduce the image file size
                    var extension = Path.GetExtension(fileStoreDto.OriginalFileName);
                    if (IsImageFile(extension) && fileStoreDto.Data.Length > SizeToReduce)
                    {
                        TryToReduceFileDataSize(fileStoreDto);
                    }

                    // Set [ReportElementTypeId] fied, search db by ReprotElementTypeName   
                    fileStoreDto.ReportElementTypeId =
                        _dbContext.ReportElementTypes.Single(
                            i => i.Name == fileStoreDto.ReportElementTypeName).ReportElementTypeId;

                    fileStoreDto.OrganizationRegulatoryProgramId = currentRegulatoryProgramId;
                    fileStoreDto.UploaderUserId = currentUserId;
                    fileStoreDto.UploadDateTimeUtc = DateTimeOffset.UtcNow;
                    fileStoreDto.SizeByte = fileStoreDto.Data.Length;

                    var fileName = Path.GetFileNameWithoutExtension(fileStoreDto.OriginalFileName);
                    fileStoreDto.Name = $"{fileName}_0{extension}";

                    var fileStore = _mapHelper.GetFileStoreFromFileStoreDto(fileStoreDto);
                    fileStore = _dbContext.FileStores.Add(fileStore);
                    _dbContext.SaveChanges();

                    // File name is the file name plus id plus extension
                    fileStore.Name = $"{fileName}_{fileStore.FileStoreId}{extension}";
                    var fileStoreData = new FileStoreData
                    {

                        Data = fileStoreDto.Data,
                        FileStoreId = fileStore.FileStoreId
                    };

                    _dbContext.FileStoreDatas.Add(fileStoreData);
                    _dbContext.SaveChanges();
                    _dbContext.Commit(transaction);
                    _logger.Info("Leave FileStoreService.CreateFileStore.");

                    return fileStore.FileStoreId;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
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

        public FileStoreDto GetFileStoreById(int fileStoreId)
        {
            _logger.Info("Enter FileStoreService.GetFileStoreById, attachmentFileId={0}.", fileStoreId);

            var currentRegulatoryProgramId =
                int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            var fileStore =
                _dbContext.FileStores.Single(i => i.OrganizationRegulatoryProgramId == currentRegulatoryProgramId);

            var fileStoreDto = _mapHelper.GetFileStoreDtoFromFileStore(fileStore);
            fileStoreDto.Data = _dbContext.FileStoreDatas.Single(i => i.FileStoreId == fileStoreDto.FileStoreId.Value).Data;

            LocalizeFileStoreDtoUploadDateTime(fileStoreDto, currentRegulatoryProgramId);
            _logger.Info("Leave FileStoreService.GetFileStoreById, attachmentFileId={0}.", fileStoreId);

            return fileStoreDto;
        }

        public void UpdateFileStore(FileStoreDto fileStoreDto)
        {
            _logger.Info("Enter FileStoreService.UpdateFileStore.");
            //Check if the file is already set to be 'reproted' or not 
            var testFileStore = _dbContext.FileStores.Single(i => i.FileStoreId == fileStoreDto.FileStoreId);
            // Check the fileStoreId is in tReportFile table or not, if it, means that file is included in a reprot  
            if (_dbContext.ReportFiles.Any(t => t.FileStoreId == fileStoreDto.FileStoreId))
            {
                string message = "The attachment is used in a Report Package, and cannot be changed.";
                List<RuleViolation> validationIssues = new List<RuleViolation>();
                validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
            }

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    testFileStore.Description = fileStoreDto.Description;
                    testFileStore.ReportElementTypeName = fileStoreDto.ReportElementTypeName;

                    _dbContext.SaveChanges();
                    _dbContext.Commit(transaction);
                    _logger.Info("Leave FileStoreService.UpdateFileStore.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
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

        public void DeleteFileStore(int fileStoreId)
        {
            _logger.Info("Enter FileStoreService.DeleteFileStore.");

            //Check if the file is already set to be 'reproted' or not 
            if (_dbContext.ReportFiles.Any(t => t.FileStoreId == fileStoreId))
            {
                List<RuleViolation> validationIssues = new List<RuleViolation>();
                string message = "The attachment is used in a Report Package, and cannot be changed.";
                validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
            }

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var testFileStore = _dbContext.FileStores.Single(i => i.FileStoreId == fileStoreId);
                    var fileStoreDate = _dbContext.FileStoreDatas.Single(i => i.FileStoreId == fileStoreId);
                    _dbContext.FileStoreDatas.Remove(fileStoreDate);
                    _dbContext.FileStores.Remove(testFileStore);
                    _dbContext.SaveChanges();
                    _dbContext.Commit(transaction);
                    _logger.Info("Leave FileStoreService.DeleteFileStore.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
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
        private FileStoreDto LocalizeFileStoreDtoUploadDateTime(FileStoreDto fileStoreDto, int currentOrgRegProgramId)
        {
            fileStoreDto.LastModificationDateTimeLocal = _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(fileStoreDto.UploadDateTimeUtc.DateTime, currentOrgRegProgramId);
            return fileStoreDto;
        }

        private void TryToReduceFileDataSize(FileStoreDto fileStoreDto)
        {
            using (var stream = new MemoryStream(fileStoreDto.Data))
            {
                var oldImage = new Bitmap(stream);
                var newImage = new Bitmap(oldImage.Width, oldImage.Height);

                var graphic = Graphics.FromImage(newImage);
                graphic.InterpolationMode = InterpolationMode.Low;
                graphic.DrawImage(oldImage, 0, 0, oldImage.Width, oldImage.Height);
                graphic.Dispose();

                // transfer newImage into byte;  
                var extension = Path.GetExtension(fileStoreDto.OriginalFileName);
                var format = GetNormalizedFormat(extension);
                var newImageData = ImageToByte(newImage, format);
                if (newImageData.Length < fileStoreDto.Data.Length)
                {
                    Array.Copy(newImageData, 0, fileStoreDto.Data, 0, newImageData.Length);
                }
            }
        }

        private ImageFormat GetNormalizedFormat(string extension)
        {
            if (!extension.StartsWith("."))
            {
                extension = $".{extension}";
            }

            extension = extension.ToUpper();

            var format = ImageFormat.Jpeg;
            if (extension == ".BMP")
            {
                format = ImageFormat.Bmp;
            }
            if (extension == ".PMG")
            {
                format = ImageFormat.Png;
            }
            if (extension == ".TIF" || extension == ".TIFF")
            {
                format = ImageFormat.Tiff;
            }

            return format;
        }

        private bool IsImageFile(string extension)
        {
            string[] validImageFormats = { ".tif", ".jpg", ".jpeg", ".bmp", ".png" };

            if (!extension.StartsWith("."))
            {
                extension = $".{extension}";
            }

            return validImageFormats.Contains(extension);
        }

        private byte[] ImageToByte(Image img, ImageFormat format)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, format);
                return stream.ToArray();
            }
        }
    }
}