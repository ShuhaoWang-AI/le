using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;

namespace Linko.LinkoExchange.Services.Attachment
{
    public class AttachmentService : IAttachmentService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper;
        private readonly IHttpContextService _httpContextService;

        private readonly string[] _validExtensions =
        {
            "docx", "doc", "xls", "xlsx", "pdf", "tif",
            "jpg", "jpeg", "bmp", "png", "txt", "csv"
        };

        public AttachmentService(
            LinkoExchangeContext dbContext,
            IMapHelper mapHelper,
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

            if (httpContextService == null)
            {
                throw new ArgumentNullException(nameof(httpContextService));
            }

            _dbContext = dbContext;
            _mapHelper = mapHelper;
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

        public List<AttachmentFileDto> GetUserAttachmentFiles()
        {
            var currentUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
            var currentRegulatoryProgramId =
                int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            var fileStore = _dbContext.FileStores.Where(i => i.OrganizationRegulatoryProgramId == currentRegulatoryProgramId);

            return new List<AttachmentFileDto>();

        }

        public void SaveAttachmentFile(AttachmentFileDto fileDto)
        {
            var currentUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
            var currentRegulatoryProgramId =
                int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));


            throw new NotImplementedException();
        }

        public List<AttachmentFileDto> GetAttachmentFile(int attachenmentFileId)
        {
            throw new NotImplementedException();
        }
    }
}