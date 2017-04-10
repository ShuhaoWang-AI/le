﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            _logger.Info("Enter FileStoreService.GetValidAttachmentFileExtensions.");

            var fileTypes = _dbContext.FileTypes.Select(i => i.Extension);

            _logger.Info("Leave FileStoreService.GetValidAttachmentFileExtensions.");
            return new List<string>(fileTypes);
        }

        // TODO to get from table tFileType
        // Here should validate data from tFileType table
        public bool IsValidFileExtension(string ext)
        {
            if (string.IsNullOrWhiteSpace(ext))
            {
                return false;
            }

            if (!ext.StartsWith("."))
            {
                ext = $".{ext}";
            }
            var fileTypes = _dbContext.FileTypes.Select(i => i.Extension);
            return fileTypes.Contains(ext);
        }

        public List<FileStoreDto> GetFileStores()
        {
            _logger.Info("Enter FileStoreService.GetFileStores.");

            var currentRegulatoryProgramId =
                int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            var fileStores =
                _dbContext.FileStores.Where(i => i.OrganizationRegulatoryProgramId == currentRegulatoryProgramId)
                    .Include(t => t.OrganizationRegulatoryProgram).ToList();

            var fileStoreDtos = fileStores.Select(i => _mapHelper.GetFileStoreDtoFromFileStore(i)).ToList();

            fileStoreDtos = fileStoreDtos.Select(i => FileStoreDtoHelper(i, currentRegulatoryProgramId)).ToList();

            _logger.Info("Leave FileStoreService.GetFileStores.");
            return fileStoreDtos;
        }

        public int CreateFileStore(FileStoreDto fileStoreDto)
        {
            _logger.Info("Enter FileStoreService.CreateFileStore.");

            if (fileStoreDto.Data == null || fileStoreDto.Data.Length < 1)
            {
                List<RuleViolation> validationIssues = new List<RuleViolation>();
                string message = "Empty file.";
                validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
            }

            if (fileStoreDto.Data.Length > MaxFileSize)
            {
                List<RuleViolation> validationIssues = new List<RuleViolation>();

                string message = "The file size exceeds that 10 MB limit.";
                validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
            }

            var extension = Path.GetExtension(fileStoreDto.OriginalFileName);
            var validFileTypes = _dbContext.FileTypes.ToList();
            var validFileExtensions = validFileTypes.Select(i => i.Extension);

            if (!validFileExtensions.Contains(extension))
            {
                List<RuleViolation> validationIssues = new List<RuleViolation>();

                string message = "Not Supported File Format.";
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
                    if (IsImageFile(extension) && fileStoreDto.Data.Length > SizeToReduce)
                    {
                        TryToReduceFileDataSize(fileStoreDto);
                    }

                    fileStoreDto.OrganizationRegulatoryProgramId = currentRegulatoryProgramId;
                    fileStoreDto.UploaderUserId = currentUserId;
                    fileStoreDto.SizeByte = fileStoreDto.Data.Length;
                    fileStoreDto.FileTypeId = validFileTypes.Single(i => i.Extension.ToLower().Equals(extension)).FileTypeId;
                    var fileName = Path.GetFileNameWithoutExtension(fileStoreDto.OriginalFileName);
                    var timeTick = DateTimeOffset.UtcNow.Ticks.ToString();
                    fileStoreDto.Name = $"{fileName}_{timeTick}_{extension}";

                    var fileStore = _mapHelper.GetFileStoreFromFileStoreDto(fileStoreDto);

                    fileStore.UploadDateTimeUtc = DateTimeOffset.UtcNow;

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

        public FileStoreDto GetFileStoreById(int fileStoreId, bool includingFileData = false)
        {
            _logger.Info("Enter FileStoreService.GetFileStoreById, attachmentFileId={0}.", fileStoreId);

            var currentRegulatoryProgramId =
                int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            var fileStore =
                _dbContext.FileStores.Single(i => i.FileStoreId == fileStoreId);

            var fileStoreDto = _mapHelper.GetFileStoreDtoFromFileStore(fileStore);
            if (includingFileData)
            {
                fileStoreDto.Data = _dbContext.FileStoreDatas.Single(i => i.FileStoreId == fileStoreDto.FileStoreId.Value).Data;
            }


            FileStoreDtoHelper(fileStoreDto, currentRegulatoryProgramId);
            _logger.Info("Leave FileStoreService.GetFileStoreById, attachmentFileId={0}.", fileStoreId);

            return fileStoreDto;
        }

        public void UpdateFileStore(FileStoreDto fileStoreDto)
        {
            _logger.Info("Enter FileStoreService.UpdateFileStore.");
            //Check if the file is already set to be 'reproted' or not 
            var fileStoreToUpdate = _dbContext.FileStores.Single(i => i.FileStoreId == fileStoreDto.FileStoreId);
            // Check the fileStoreId is in tReportFile table or not, if it, means that file is included in a reprot  
            if (IsFileInReports(fileStoreDto.FileStoreId.Value))
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
                    var currentUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
                    fileStoreToUpdate.LastModifierUserId = currentUserId;
                    fileStoreToUpdate.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;

                    fileStoreToUpdate.Description = fileStoreDto.Description;
                    fileStoreToUpdate.ReportElementTypeName = fileStoreDto.ReportElementTypeName;
                    fileStoreToUpdate.ReportElementTypeId = fileStoreDto.ReportElementTypeId;

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
            if (IsFileInReports(fileStoreId))
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

        public bool IsFileInReports(int fileStoreId)
        {
            _logger.Info("Enter FileStoreService.IsFileInReports.");

            var isFileInReports = _dbContext.ReportFiles.Any(i => i.FileStoreId == fileStoreId);

            _logger.Info("Leave FileStoreService.IsFileInReports.");

            return isFileInReports;
        }

        private FileStoreDto FileStoreDtoHelper(FileStoreDto fileStoreDto, int currentOrgRegProgramId)
        {
            fileStoreDto.UploalDateTimeLocal = _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(fileStoreDto.UploalDateTimeLocal.DateTime, currentOrgRegProgramId);

            fileStoreDto.UploaderUserFullName = GetUserFullName(fileStoreDto.UploaderUserId);
            fileStoreDto.LastModifierUserFullName = GetUserFullName(fileStoreDto.LastModifierUserId);
            fileStoreDto.LastModificationDateTimeLocal = _timeZoneService.
                GetLocalizedDateTimeUsingSettingForThisOrg(fileStoreDto.LastModificationDateTimeLocal?.DateTime ?? fileStoreDto.UploalDateTimeLocal.DateTime, currentOrgRegProgramId);
            return fileStoreDto;
        }

        private string GetUserFullName(int? userId)
        {
            if (userId.HasValue)
            {
                var uploaderUser = _dbContext.Users.Single(user => user.UserProfileId == userId.Value);
                return $"{uploaderUser.FirstName} {uploaderUser.LastName}";
            }

            return "N/A";
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
                    fileStoreDto.Data = new byte[newImageData.Length];
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