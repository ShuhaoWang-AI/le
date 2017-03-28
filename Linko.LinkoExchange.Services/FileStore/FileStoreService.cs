using System;
using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;

namespace Linko.LinkoExchange.Services
{
    public class FileStoreService : IFileStoreService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;
        private readonly IHttpContextService _httpContextService;
        private readonly ITimeZoneService _timeZoneService;

        private readonly string[] _validExtensions =
        {
            "docx", "doc", "xls", "xlsx", "pdf", "tif",
            "jpg", "jpeg", "bmp", "png", "txt", "csv"
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
            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var currentUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
                    var currentRegulatoryProgramId =
                        int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

                    fileStoreDto.OrganizationRegulatoryProgramId = currentRegulatoryProgramId;
                    fileStoreDto.UploaderUserId = currentUserId;
                    fileStoreDto.UploadDateTimeUtc = DateTimeOffset.UtcNow;
                    fileStoreDto.SizeByte = fileStoreDto.Data.Length;

                    var fileStore = _mapHelper.GetFileStoreFromFileStoreDto(fileStoreDto);
                    _dbContext.FileStores.Add(fileStore);

                    var fileStoreData = new FileStoreData
                    {

                        Data = fileStoreDto.Data,
                        FileStoreId = fileStore.FileStoreId
                    };

                    _dbContext.FileStoreDatas.Add(fileStoreData);
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

            //TODO: To determine if the attachment has been used in a Report Package (status ="Reported")
            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var currentUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
                    var currentRegulatoryProgramId =
                        int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

                    fileStoreDto.OrganizationRegulatoryProgramId = currentRegulatoryProgramId;
                    fileStoreDto.UploaderUserId = currentUserId;
                    fileStoreDto.UploadDateTimeUtc = DateTimeOffset.UtcNow;
                    fileStoreDto.SizeByte = fileStoreDto.Data.Length;

                    var fileStore = _mapHelper.GetFileStoreFromFileStoreDto(fileStoreDto);
                    _dbContext.FileStores.Add(fileStore);

                    var fileStoreData = new FileStoreData
                    {
                        Data = fileStoreDto.Data,
                        FileStoreId = fileStore.FileStoreId
                    };

                    _dbContext.FileStoreDatas.Add(fileStoreData);
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
            throw new NotImplementedException();
        }

        private FileStoreDto LocalizeFileStoreDtoUploadDateTime(FileStoreDto fileStoreDto, int currentOrgRegProgramId)
        {
            fileStoreDto.LastModificationDateTimeLocal = _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(fileStoreDto.UploadDateTimeUtc.DateTime, currentOrgRegProgramId);
            return fileStoreDto;
        }
    }
}