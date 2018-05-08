using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.FileStore;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;

namespace Linko.LinkoExchange.Services.ImportTempFile
{
    public class ImportTempFileService : BaseService, IImportTempFileService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;

        private readonly FileStoreService _fileStoreService;
        private readonly IHttpContextService _httpContextService;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;
        private readonly ITimeZoneService _timeZoneService;

        #endregion

        #region constructors and destructor

        public ImportTempFileService(
            FileStoreService fileStoreService,
            LinkoExchangeContext dbContext,
            IMapHelper mapHelper,
            ILogger logger,
            IHttpContextService httpContextService,
            ITimeZoneService timeZoneService)
        {
            if (fileStoreService == null)
            {
                throw new ArgumentNullException(paramName:nameof(fileStoreService));
            }

            if (dbContext == null)
            {
                throw new ArgumentNullException(paramName:nameof(dbContext));
            }

            if (mapHelper == null)
            {
                throw new ArgumentNullException(paramName:nameof(mapHelper));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(paramName:nameof(logger));
            }

            if (httpContextService == null)
            {
                throw new ArgumentNullException(paramName:nameof(httpContextService));
            }

            if (timeZoneService == null)
            {
                throw new ArgumentNullException(paramName:nameof(timeZoneService));
            }

            _fileStoreService = fileStoreService;
            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _logger = logger;
            _httpContextService = httpContextService;
            _timeZoneService = timeZoneService;
        }

        #endregion

        #region interface implementations

        #region Overrides of BaseService

        /// <inheritdoc />
        public override bool CanUserExecuteApi(string apiName = "", params int[] id)
        {
            var retVal = false;

            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var currentPortalName = _httpContextService.GetClaimValue(claimType:CacheKey.PortalName);
            currentPortalName = string.IsNullOrWhiteSpace(value:currentPortalName) ? "" : currentPortalName.Trim().ToLower();

            switch (apiName)
            {
                case "GetImportTempFileById":
                    var importTempFileId = id[0];
                    retVal = IsImportTempFileWithThisOwnerExist(importTempFileId:importTempFileId, orgRegProgramId:currentOrgRegProgramId);
                    break;
                default: throw new Exception(message:$"ERROR: Unhandled API authorization attempt using name = '{apiName}'");
            }

            return retVal;
        }

        #endregion

        #endregion

        private bool IsImportTempFileWithThisOwnerExist(int importTempFileId, int orgRegProgramId)
        {
            //Also handles scenarios where ImportTempFileId does not exist
            return _dbContext.ImportTempFiles.Any(fs => fs.ImportTempFileId == importTempFileId && fs.OrganizationRegulatoryProgramId == orgRegProgramId);
        }

        #region Implementation of IImportTempFileService

        /// <inheritdoc />
        public ImportTempFileDto GetImportTempFileById(int importTempFileId)
        {
            ImportTempFileDto importTempFileDto;

            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(), descripition:$"importTempFileId={importTempFileId}"))
            {
                if (!CanUserExecuteApi(id:importTempFileId))
                {
                    throw new UnauthorizedAccessException();
                }

                var currentRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

                var importTempFile = _dbContext.ImportTempFiles.Single(i => i.ImportTempFileId == importTempFileId);

                importTempFileDto = _mapHelper.GetImportTempFileDtoFromImportTempFile(importTempFile:importTempFile);

                var fileExtension = _dbContext.FileTypes.SingleOrDefault(x => x.FileTypeId == importTempFileDto.FileTypeId)?.Extension;
                if (fileExtension != null)
                {
                    importTempFileDto.FileExtension = fileExtension;
                }

                importTempFileDto.UploadDateTimeLocal =
                    _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:importTempFileDto.UploadDateTimeLocal, orgRegProgramId:currentRegulatoryProgramId);
            }

            return importTempFileDto;
        }

        /// <inheritdoc />
        public int CreateImportTempFile(ImportTempFileDto importTempFileDto)
        {
            var importTempFileIdString = importTempFileDto.ImportTempFileId?.ToString() ?? "null";
            var maxFileSize = _fileStoreService.GetMaxFileSize();
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(), descripition:$"importTempFileId={importTempFileIdString}"))
            {
                if (importTempFileDto.RawFile == null || importTempFileDto.RawFile.Length < 1)
                {
                    var validationIssues = new List<RuleViolation>();
                    var message = "No file was selected.";
                    validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                    throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                }

                if (importTempFileDto.RawFile.Length > maxFileSize)
                {
                    var validationIssues = new List<RuleViolation>();

                    var message = $"The file size exceeds that {maxFileSize / 1024 / 1024} MB limit.";
                    validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                    throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                }

                var extension = Path.GetExtension(path:importTempFileDto.OriginalFileName)?.ToLower();
                // ReSharper disable once ArgumentsStyleStringLiteral
                var validFileTypes = _dbContext.FileTypes.Where(x => x.Extension.Equals(".xlsx")).ToList();
                var validFileExtensions = validFileTypes.Select(i => i.Extension).Select(i => i.ToLower());

                if (!validFileExtensions.Contains(value:extension))
                {
                    var validationIssues = new List<RuleViolation>();

                    var message = "The file type selected is not supported.";
                    validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                    throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                }

                var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                var currentUserProfileId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId));

                int importTempFileIdToReturn;
                using (_dbContext.CreateAutoCommitScope())
                {
                    var importTempFile = _mapHelper
                        .GetImportTempFileFromImportTempFileDto(dto:importTempFileDto,
                                                                existingDataSource:new Core.Domain.ImportTempFile
                                                                                   {
                                                                                       OrganizationRegulatoryProgramId = currentOrgRegProgramId,
                                                                                       UploadDateTimeUtc = DateTimeOffset.Now,
                                                                                       UploaderUserId = currentUserProfileId,
                                                                                       FileTypeId = validFileTypes
                                                                                           .Single(i => i.Extension.ToLower().Equals(value:extension)).FileTypeId
                                                                                   });

                    _dbContext.ImportTempFiles.Add(entity:importTempFile);

                    _dbContext.SaveChanges();

                    importTempFileIdToReturn = importTempFile.ImportTempFileId;
                }

                return importTempFileIdToReturn;
            }
        }

        #endregion
    }
}