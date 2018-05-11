using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
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
using Telerik.Windows.Documents.Spreadsheet.FormatProviders;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Windows.Documents.Spreadsheet.Model;

namespace Linko.LinkoExchange.Services.ImportSampleFromFile
{
    public class ImportSampleFromFileService : BaseService, IImportSampleFromFileService
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

        public ImportSampleFromFileService(
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

        /// <inheritdoc />
        public override bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id)
        {
            var retVal = false;

            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var currentPortalName = _httpContextService.GetClaimValue(claimType:CacheKey.PortalName);
            currentPortalName = string.IsNullOrWhiteSpace(value:currentPortalName) ? "" : currentPortalName.Trim().ToLower();

            switch (apiName)
            {
                case "GetImportTempFileById":
                case "RemoveImportTempFile":
                case "GetWorkbook":
                case "DoFileValidation":
                    var importTempFileId = id[0];
                    retVal = IsImportTempFileWithThisOwnerExist(importTempFileId:importTempFileId, orgRegProgramId:currentOrgRegProgramId);
                    break;
                default: throw new Exception(message:$"ERROR: Unhandled API authorization attempt using name = '{apiName}'");
            }

            return retVal;
        }

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

                importTempFileDto = _mapHelper.GetDtoFromDomainObject(domainObject:importTempFile);

                importTempFileDto.UploadDateTimeLocal =
                    _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:importTempFileDto.UploadDateTimeLocal, orgRegProgramId:currentRegulatoryProgramId);
            }

            return importTempFileDto;
        }

        /// <inheritdoc />
        public int CreateImportTempFile(ImportTempFileDto importTempFileDto)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(),
                                    descripition:$"importTempFileId={importTempFileDto.ImportTempFileId?.ToString() ?? "null"}"))
            {
                var maxFileSize = _fileStoreService.GetMaxFileSize();
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
                var validFileTypes = _dbContext.FileTypes.Where(x => x.Extension.ToLower().Equals(".xlsx")).ToList();
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
                        .GetDomainObjectFromDto(dto:importTempFileDto,
                                                existingDomainObject:new ImportTempFile
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

        public Workbook GetWorkbook(ImportTempFileDto importTempFileDto, bool isAuthorizationRequired = false)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(),
                                    descripition:$"importTempFileId={importTempFileDto?.ImportTempFileId?.ToString() ?? "null"}"))
            {
                if (importTempFileDto == null)
                {
                    var validationIssues = new List<RuleViolation>();

                    var message = "The file is empty.";
                    validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                    throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                }

                if (isAuthorizationRequired && importTempFileDto?.ImportTempFileId != null && !CanUserExecuteApi(id:importTempFileDto.ImportTempFileId.Value))
                {
                    throw new UnauthorizedAccessException();
                }

                Workbook workbook = null;

                var providers = new List<IWorkbookFormatProvider>
                                {
                                    new XlsxFormatProvider()
                                };

                if (importTempFileDto.RawFile != null)
                {
                    var extension = importTempFileDto.FileExtension;
                    var provider = providers.FirstOrDefault(p => p.SupportedExtensions
                                                                  .Any(e => string.Compare(strA:extension, strB:e, comparisonType:StringComparison.InvariantCultureIgnoreCase)
                                                                            == 0));
                    if (provider != null)
                    {
                        using (var stream = new MemoryStream(buffer:importTempFileDto.RawFile))
                        {
                            try
                            {
                                workbook = provider.Import(input:stream);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(value:ex);

                                var validationIssues = new List<RuleViolation>();

                                var message = "The file format is not recognized.";
                                validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                            }
                        }
                    }
                    else
                    {
                        var validationIssues = new List<RuleViolation>();

                        var message = "The file format is not recognized.";
                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                        throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                    }
                }

                return workbook;
            }
        }

        /// <inheritdoc />
        public ImportSampleFromFileValidationResultDto DoFileValidation(int dataSourceId, ImportTempFileDto importTempFileDto, out SampleImportDto sampleImportDto)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(),
                                    descripition:$"importTempFileId={importTempFileDto?.ImportTempFileId?.ToString() ?? "null"}"))
            {
                var result = new ImportSampleFromFileValidationResultDto();
                sampleImportDto = new SampleImportDto
                                  {
                                      TempFile = importTempFileDto
                                  };

                if (importTempFileDto?.ImportTempFileId != null && !CanUserExecuteApi(id:importTempFileDto.ImportTempFileId.Value))
                {
                    throw new UnauthorizedAccessException();
                }

                try
                {
                    //TODO: Implement proper validation
                    result.Success = true;
                    result.Errors = null;
                    var importFileWorkbook = GetWorkbook(importTempFileDto:importTempFileDto);

                    //result.ImportFileWorkbook = importFileWorkbook;
                }
                catch (RuleViolationException ruleViolationException)
                {
                    if (importTempFileDto?.ImportTempFileId != null)
                    {
                        RemoveImportTempFile(importTempFileId:importTempFileDto.ImportTempFileId.Value);
                    }

                    result.Success = false;
                    result.Errors = ruleViolationException.ValidationIssues?.Select(x => new ErrorWithRowNumberDto {ErrorMessage = x.ErrorMessage, RowNumbers = ""});
                }

                return result;
            }
        }

        /// <inheritdoc />
        public Dictionary<SystemFieldName, List<CustomSelectListItemDto>> GetRequiredDataDefaults(SampleImportDto sampleImportDto)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public SampleImportDto PopulateExistingTranslationData(SampleImportDto sampleImportDto)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public List<MissingTranslationDto> GetMissingTranslationSet(SampleImportDto sampleImportDto)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ImportSampleFromFileValidationResultDto DoDataValidation(SampleImportDto sampleImportDto, out List<SampleImportDto> samplesDtos)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void RemoveImportTempFile(int importTempFileId)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(), descripition:$"importTempFileId={importTempFileId}"))
            {
                if (!CanUserExecuteApi(id:importTempFileId))
                {
                    throw new UnauthorizedAccessException();
                }

                using (_dbContext.CreateAutoCommitScope())
                {
                    var importTempFile = _dbContext.ImportTempFiles.Single(i => i.ImportTempFileId == importTempFileId);
                    if (importTempFile != null)
                    {
                        _dbContext.ImportTempFiles.Remove(entity:importTempFile);
                        _dbContext.SaveChanges();
                    }
                }
            }
        }

        #endregion

        private bool IsImportTempFileWithThisOwnerExist(int importTempFileId, int orgRegProgramId)
        {
            //Also handles scenarios where ImportTempFileId does not exist
            return _dbContext.ImportTempFiles.Any(fs => fs.ImportTempFileId == importTempFileId && fs.OrganizationRegulatoryProgramId == orgRegProgramId);
        }
    }
}