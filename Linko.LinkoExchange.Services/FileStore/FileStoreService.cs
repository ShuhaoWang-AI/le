using System;
using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using NLog;

namespace Linko.LinkoExchange.Services
{
    public class FileStoreService : IFileStoreService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;
        private readonly IHttpContextService _httpContextService;

        private readonly string[] _validExtensions =
        {
            "docx", "doc", "xls", "xlsx", "pdf", "tif",
            "jpg", "jpeg", "bmp", "png", "txt", "csv"
        };

        public FileStoreService(
            LinkoExchangeContext dbContext,
            IMapHelper mapHelper,
            ILogger logger,
            IHttpContextService httpContextService)
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


            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _logger = logger;
            _httpContextService = httpContextService;
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

        public List<FileStoreDto> GetUserAttachmentFiles()
        {
            _logger.Info("Enter FileStoreService.GetUserAttachmentFiles.");

            var currentRegulatoryProgramId =
                int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            var fileStores = _dbContext.FileStores.Where(i => i.OrganizationRegulatoryProgramId == currentRegulatoryProgramId);
            var fileStoreDtos = fileStores.Select(i => _mapHelper.GetFileStoreDtoFromFileStore(i)).ToList();

            _logger.Info("Leave FileStoreService.GetUserAttachmentFiles.");
            return fileStoreDtos;
        }

        public void SaveFileStores(FileStoreDto fileStoreDto)
        {
            _logger.Info("Enter FileStoreService.SaveFileStores.");
            var currentUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
            //var currentRegulatoryProgramId =
            //    int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));


            _logger.Info("Leave FileStoreService.SaveFileStores.");
            throw new NotImplementedException();
        }

        public FileStoreDto GetFileStoreById(int attachenmentFileId)
        {
            _logger.Info("Enter FileStoreService.GetFileStoreById, attachmentFileId={0}.", attachenmentFileId);

            var currentRegulatoryProgramId =
                int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            var fileStore = _dbContext.FileStores.Single(i => i.OrganizationRegulatoryProgramId == currentRegulatoryProgramId);
            var fileStoreDto = _mapHelper.GetFileStoreDtoFromFileStore(fileStore);
            fileStoreDto.Data = _dbContext.FileStoreDatas.Single(i => i.FileStoreId == fileStoreDto.FileStoreId.Value).Data;

            _logger.Info("Leave FileStoreService.GetFileStoreById, attachmentFileId={0}.", attachenmentFileId);

            return fileStoreDto;
        }
    }
}