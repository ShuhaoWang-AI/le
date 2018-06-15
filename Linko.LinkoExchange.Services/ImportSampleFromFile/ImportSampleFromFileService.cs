using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web.Hosting;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.DataSource;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.FileStore;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Parameter;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Sample;
using Linko.LinkoExchange.Services.SelectList;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.Unit;
using NLog;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Windows.Documents.Spreadsheet.Formatting;
using Telerik.Windows.Documents.Spreadsheet.Model;
using Telerik.Windows.Documents.Spreadsheet.PropertySystem;

namespace Linko.LinkoExchange.Services.ImportSampleFromFile
{
    public partial class ImportSampleFromFileService : BaseService, IImportSampleFromFileService
    {
        #region static fields and constants

        private static readonly IEnumerable<SampleImportColumnName> ColumnsNeedDataTranslation = new List<SampleImportColumnName>
                                                                                                  {
                                                                                                      SampleImportColumnName.MonitoringPoint,
                                                                                                      SampleImportColumnName.CollectionMethod,
                                                                                                      SampleImportColumnName.SampleType,
                                                                                                      SampleImportColumnName.ParameterName,
                                                                                                      SampleImportColumnName.ResultUnit
                                                                                                  };

        #endregion

        #region fields

        private readonly IDataSourceService _dataSourceService;
        private readonly LinkoExchangeContext _dbContext;
        private readonly IFileStoreService _fileStoreService;
        private readonly IHttpContextService _httpContextService;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;
        private readonly IOrganizationService _organizationService;
        private readonly IParameterService _parameterService;
        private readonly IReportElementService _reportElementService;
        private readonly IReportTemplateService _reportTemplateService;
        private readonly ISampleService _sampleService;
        private readonly ISelectListService _selectListService;
        private readonly ISettingService _settingService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IUnitService _unitService;

        #endregion

        #region constructors and destructor

        public ImportSampleFromFileService(
            IFileStoreService fileStoreService,
            IOrganizationService organizationService,
            LinkoExchangeContext dbContext,
            IMapHelper mapHelper,
            ILogger logger,
            IHttpContextService httpContextService,
            ITimeZoneService timeZoneService,
            ISampleService sampleService,
            IProgramService programService,
            ISelectListService selectListService,
            ISettingService settingService,
            IUnitService unitService,
            IReportElementService reportElementService,
            IParameterService parameterService,
            IReportTemplateService reportTemplateService,
            IDataSourceService dataSourceService)
        {
            if (fileStoreService == null)
            {
                throw new ArgumentNullException(paramName:nameof(fileStoreService));
            }

            if (organizationService == null)
            {
                throw new ArgumentNullException(paramName:nameof(organizationService));
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

            if (sampleService == null)
            {
                throw new ArgumentNullException(paramName:nameof(sampleService));
            }

            if (programService == null)
            {
                throw new ArgumentNullException(paramName:nameof(programService));
            }

            if (selectListService == null)
            {
                throw new ArgumentNullException(paramName:nameof(selectListService));
            }

            if (settingService == null)
            {
                throw new ArgumentNullException(paramName:nameof(settingService));
            }

            if (unitService == null)
            {
                throw new ArgumentNullException(paramName:nameof(unitService));
            }

            if (parameterService == null)
            {
                throw new ArgumentNullException(paramName:nameof(parameterService));
            }

            if (reportTemplateService == null)
            {
                throw new ArgumentNullException(paramName:nameof(reportElementService));
            }

            if (reportTemplateService == null)
            {
                throw new ArgumentNullException(paramName:nameof(reportTemplateService));
            }

            if (dataSourceService == null)
            {
                throw new ArgumentNullException(paramName:nameof(dataSourceService));
            }

            _fileStoreService = fileStoreService;
            _organizationService = organizationService;
            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _logger = logger;
            _httpContextService = httpContextService;
            _timeZoneService = timeZoneService;
            _unitService = unitService;
            _parameterService = parameterService;
            _reportElementService = reportElementService;
            _reportTemplateService = reportTemplateService;
            _selectListService = selectListService;
            _settingService = settingService;
            _sampleService = sampleService;
            _dataSourceService = dataSourceService;
        }

        #endregion

        #region interface implementations

        /// <inheritdoc />
        public override bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id)
        {
            bool retVal;

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
                default: throw new NotSupportedException(message:$"ERROR: Unhandled API authorization attempt using name = '{apiName}'");
            }

            return retVal;
        }

        /// <inheritdoc />
        public ImportTempFileDto GetImportTempFileById(int importTempFileId)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(), descripition:$"importTempFileId={importTempFileId}"))
            {
                if (!CanUserExecuteApi(id:importTempFileId))
                {
                    throw new UnauthorizedAccessException();
                }

                var currentRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

                var importTempFile = _dbContext.ImportTempFiles.SingleOrDefault(i => i.ImportTempFileId == importTempFileId);
                if (importTempFile == null)
                {
                    throw new BadRequest(message:ErrorConstants.SampleImport.CannotFindImportFile);
                }

                var importTempFileDto = _mapHelper.ToDto(fromDomainObject:importTempFile);

                importTempFileDto.UploadDateTimeLocal =
                    _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:importTempFileDto.UploadDateTimeLocal, orgRegProgramId:currentRegulatoryProgramId);

                return importTempFileDto;
            }
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
                    throw new BadRequest(message:ErrorConstants.SampleImport.CannotFindImportFile);
                }

                if (importTempFileDto.RawFile.Length > maxFileSize)
                {
                    throw new BadRequest(message:string.Format(format:ErrorConstants.SampleImport.FileValidation.ImportFileExceedSizeLimitation, arg0:maxFileSize / 1024 / 1024));
                }

                var extension = Path.GetExtension(path:importTempFileDto.OriginalFileName)?.ToLower();

                // ReSharper disable once ArgumentsStyleStringLiteral
                var validFileTypes = _dbContext.FileTypes.Where(x => x.Extension.ToLower().Equals(".xlsx")).ToList();
                var validFileExtensions = validFileTypes.Select(i => i.Extension).Select(i => i.ToLower());

                if (!validFileExtensions.Contains(value:extension))
                {
                    throw new BadRequest(message:ErrorConstants.SampleImport.FileValidation.FileTypeIsUnsupported);
                }

                var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                var currentUserProfileId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId));

                int importTempFileIdToReturn;
                using (_dbContext.BeginTransactionScope(from:MethodBase.GetCurrentMethod()))
                {
                    var importTempFile = _mapHelper
                        .ToDomainObject(fromDto:importTempFileDto,
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

        /// <inheritdoc />
        public ImportSampleFromFileValidationResultDto DoFileValidation(SampleImportDto sampleImportDto)
        {
            if (sampleImportDto == null)
            {
                throw new ArgumentNullException(paramName:nameof(SampleImportDto));
            }
            if (sampleImportDto.DataSource == null)
            {
                throw new ArgumentNullException(paramName:nameof(SampleImportDto), message:ErrorConstants.SampleImport.DataProviderDoesNotExist);
            }
            if (sampleImportDto.TempFile == null)
            {
                throw new ArgumentNullException(paramName: nameof(SampleImportDto), message:ErrorConstants.SampleImport.CannotFindImportFile);
            }
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(),
                                    descripition:$"importTempFileId={sampleImportDto.TempFile.ImportTempFileId.ToString()}"))
            {
                var result = new ImportSampleFromFileValidationResultDto();

                try
                {
                    if (sampleImportDto.TempFile.ImportTempFileId != null && !CanUserExecuteApi(id: sampleImportDto.TempFile.ImportTempFileId.Value))
                    {
                        throw new UnauthorizedAccessException();
                    }

                    //populate FileVersion in sampleImportDto
                    sampleImportDto.FileVersion = GetFileVersion(fileVersionName:FileVersionTemplateName.SampleImport.ToString());

                    if (sampleImportDto.FileVersion?.FileVersionId == null)
                    {
                        throw new BadRequest(message:ErrorConstants.SampleImport.ImportTemplateDoesNotExist);
                    }

                    //populate Rows in sampleImportDto
                    var importFileWorkbook = GetWorkbook(importTempFileDto:sampleImportDto.TempFile);

                    sampleImportDto.Rows = GetImportRowObjects(importFileWorkbook:importFileWorkbook, sampleImportDto:sampleImportDto, result:ref result);

                    if (sampleImportDto.Rows.Count == 0)
                    {
                        result.Errors.Add(item:new ErrorWithRowNumberDto {ErrorMessage = ErrorConstants.SampleImport.FileValidation.ImportFileIsEmpty});
                    }
                }
                catch (RuleViolationException ruleViolationException)
                {
                    if (ruleViolationException.ValidationIssues.Count > 0)
                    {
                        result.Errors.AddRange(collection:ruleViolationException
                                                   .ValidationIssues?.Select(x => new ErrorWithRowNumberDto {ErrorMessage = x.ErrorMessage, RowNumbers = ""})
                                                   .ToList());
                    }
                }

                return result;
            }
        }

        /// <inheritdoc />
        public List<RequiredDataDefaultsDto> GetRequiredDataDefaults(SampleImportDto sampleImportDto)
        {
            var requiredDataDefaults = new List<RequiredDataDefaultsDto>();

            AddRequiredDataDefaultDtoOrPopulateDefaultValue(sampleImportDto: sampleImportDto, requiredDataDefaults: requiredDataDefaults, 
                                                            columnName: SampleImportColumnName.MonitoringPoint);
            AddRequiredDataDefaultDtoOrPopulateDefaultValue(sampleImportDto: sampleImportDto, requiredDataDefaults: requiredDataDefaults, 
                                                            columnName: SampleImportColumnName.CollectionMethod);
            AddRequiredDataDefaultDtoOrPopulateDefaultValue(sampleImportDto: sampleImportDto, requiredDataDefaults: requiredDataDefaults, 
                                                            columnName: SampleImportColumnName.SampleType);

            return requiredDataDefaults;
        }

        public void PopulateDataDefaults(SampleImportDto sampleImportDto, ListItemDto defaultMonitoringPoint, ListItemDto defaultCollectionMethod, ListItemDto defaultSampleType)
        {
            PopulateEmptyColumnValueAsDefaultValue(sampleImportDto: sampleImportDto, columnName: SampleImportColumnName.MonitoringPoint, defaultValue: defaultMonitoringPoint);
            PopulateEmptyColumnValueAsDefaultValue(sampleImportDto: sampleImportDto, columnName: SampleImportColumnName.CollectionMethod, defaultValue: defaultCollectionMethod);
            PopulateEmptyColumnValueAsDefaultValue(sampleImportDto: sampleImportDto, columnName: SampleImportColumnName.SampleType, defaultValue: defaultSampleType);
        }

        /// <inheritdoc />
        public List<MissingTranslationDto> PopulateExistingTranslationDataAndReturnMissingTranslationSet(SampleImportDto sampleImportDto, int dataSourceId)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(),
                                    descripition:sampleImportDto.ImportId.ToString()))
            {
                //TODO: Should add audit log events to indicate system deleted they are no longer available data translations
                _dataSourceService.DeleteInvalidDataSourceTranslations(dataSourceId:dataSourceId);

                sampleImportDto.DataSource = _dataSourceService.GetDataSourceById(dataSourceId:dataSourceId, withDataTranslations:true);
                PopulateExistingTranslationData(sampleImportDto:sampleImportDto);

                List<DataSourceTranslationItemDto> translationItemsShouldAutoGenerate;
                var missingTranslations = GetMissingTranslationSet(sampleImportDto:sampleImportDto, translationItemsShouldAutoGenerate:out translationItemsShouldAutoGenerate);

                if (translationItemsShouldAutoGenerate.Any())
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    _dataSourceService.SaveDataSourceTranslations(dataSourceId:dataSourceId, dataSourceTranslations:translationItemsShouldAutoGenerate);

                    var shouldRepopulateTranslation = !missingTranslations.Any();
                    if (shouldRepopulateTranslation)
                    {
                        sampleImportDto.DataSource = _dataSourceService.GetDataSourceById(dataSourceId:dataSourceId, withDataTranslations:true);
                        PopulateExistingTranslationData(sampleImportDto:sampleImportDto);
                    }
                }

                return missingTranslations;
            }
        }

        /// <inheritdoc />
        public FileVersionDto GetFileVersion(string fileVersionName)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod()))
            {
                var currentRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                var authority = _organizationService.GetAuthority(orgRegProgramId:currentRegulatoryProgramId);

                var fileVersion = _dbContext.FileVersions.FirstOrDefault(i => i.OrganizationRegulatoryProgramId == authority.OrganizationRegulatoryProgramId

                                                                              // ReSharper disable once ArgumentsStyleOther
                                                                              && i.Name.ToLower().Equals(fileVersionName.ToLower()));

                var fileVersionDto = _mapHelper.ToDto(fromDomainObject:fileVersion);

                if (fileVersion != null)
                {
                    var lastModificationDateTimeUtc = fileVersion.LastModificationDateTimeUtc?.UtcDateTime ?? fileVersion.CreationDateTimeUtc.UtcDateTime;
                    fileVersionDto.LastModificationDateTimeLocal =
                        _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:lastModificationDateTimeUtc, orgRegProgramId:currentRegulatoryProgramId);
                }

                if (fileVersionDto.LastModifierUserId.HasValue)
                {
                    var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == fileVersionDto.LastModifierUserId.Value);
                    fileVersionDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                }
                else
                {
                    fileVersionDto.LastModifierFullName = "N/A";
                }

                return fileVersionDto;
            }
        }

        /// <inheritdoc />
        public void ImportSampleAndCreateAttachment(SampleImportDto sampleImportDto)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod()))
            {
                using (_dbContext.BeginTransactionScope(from:MethodBase.GetCurrentMethod()))
                {
                    var currentRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

                    // Add or update samples
                    foreach (var sampleDto in sampleImportDto.SampleDtos)
                    {
                        _sampleService.SaveSample(sample:sampleDto);
                    }

                    // Create new attachment
                    var tempFile = sampleImportDto.TempFile;
                    var reportElementTypeIdForIndustryFileUpload =
                        _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:currentRegulatoryProgramId, settingType:SettingType.ReportElementTypeIdForIndustryFileUpload);

                    if (string.IsNullOrWhiteSpace(value:reportElementTypeIdForIndustryFileUpload))
                    {
                        throw CreateRuleViolationExceptionForValidationError(errorMessage:"Attachment Type for Industry File Upload is missing !");
                    }
                    else
                    {
                        var reportElementTypeId = Convert.ToInt32(value:reportElementTypeIdForIndustryFileUpload);
                        var reportElementType = _reportElementService.GetReportElementTypes(categoryName:ReportElementCategoryName.Attachments)
                                                                     .First(c => c.ReportElementTypeId == reportElementTypeId);

                        var attachment = new FileStoreDto
                                         {
                                             OriginalFileName = tempFile.OriginalFileName,
                                             ReportElementTypeId = reportElementTypeId,
                                             ReportElementTypeName = reportElementType.Name,
                                             Description = "Imported data",
                                             Data = tempFile.RawFile,
                                             SizeByte = tempFile.SizeByte,
                                             MediaType = tempFile.MediaType,
                                             FileTypeId = tempFile.FileTypeId,
                                             OrganizationRegulatoryProgramId = currentRegulatoryProgramId
                                         };

                        attachment.FileStoreId = _fileStoreService.CreateFileStore(fileStoreDto:attachment);

                        sampleImportDto.ImportedFile = attachment;
                    }

                    // remove temp import file
                    if (sampleImportDto.TempFile.ImportTempFileId.HasValue)
                    {
                        RemoveImportTempFile(importTempFileId:sampleImportDto.TempFile.ImportTempFileId.Value);
                    }
                }
            }
        }

        /// <inheritdoc />
        public FileVersionDto GetFileVersionForAuthorityConfiguration(FileVersionTemplateName fileVersionTemplateName)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod()))
            {
                var currentRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                var authority = _organizationService.GetAuthority(orgRegProgramId:currentRegulatoryProgramId);

                var fileVersionTemplate = _dbContext.FileVersionTemplates.FirstOrDefault(i => i.Name == fileVersionTemplateName.ToString());

                if (fileVersionTemplate == null)
                {
                    throw new ArgumentException(message:$"File version template {fileVersionTemplateName} not found!");
                }

                //TODO: when there will be multiple FileVersions for same authority need to update following condition
                var fileVersion = _dbContext.FileVersions.FirstOrDefault(i => i.OrganizationRegulatoryProgramId == authority.OrganizationRegulatoryProgramId
                                                                              && i.Name == fileVersionTemplateName.ToString());

                if (fileVersion == null)
                {
                    //TODO: when there will be multiple FileVersions then following code might not be needed
                    using (_dbContext.BeginTransactionScope(from:MethodBase.GetCurrentMethod()))
                    {
                        fileVersion = new FileVersion
                                      {
                                          Name = fileVersionTemplate.Name,
                                          Description = fileVersionTemplate.Description,
                                          OrganizationRegulatoryProgramId = authority.OrganizationRegulatoryProgramId,
                                          CreationDateTimeUtc = DateTimeOffset.Now
                                      };

                        _dbContext.FileVersions.Add(entity:fileVersion);
                        _dbContext.SaveChanges();

                        foreach (var fileVersionTemplateField in fileVersionTemplate.FileVersionTemplateFields)
                        {
                            var fileVersionField = new FileVersionField
                                                   {
                                                       Name = fileVersionTemplateField.SystemField.Name,
                                                       Description = fileVersionTemplateField.SystemField.Description,
                                                       FileVersionId = fileVersion.FileVersionId,
                                                       SystemFieldId = fileVersionTemplateField.SystemFieldId,
                                                       DataOptionalityId = fileVersionTemplateField.SystemField.IsRequired
                                                                               ? (int) DataOptionalityName.Required
                                                                               : (int) DataOptionalityName.Optional,
                                                       Size = fileVersionTemplateField.SystemField.Size,
                                                       ExampleData = fileVersionTemplateField.SystemField.ExampleData,
                                                       AdditionalComments = fileVersionTemplateField.SystemField.AdditionalComments
                                                   };
                            _dbContext.FileVersionFields.Add(entity:fileVersionField);
                            _dbContext.SaveChanges();
                            fileVersion.FileVersionFields.Add(item:fileVersionField);
                        }
                    }
                }

                var fileVersionDto = _mapHelper.ToDto(fromDomainObject:fileVersion);

                fileVersionDto = _mapHelper.MargeToFileVersionDto(fileVersionDto:fileVersionDto, fileVersionTemplate:fileVersionTemplate);

                var lastModificationDateTimeUtc = fileVersion.LastModificationDateTimeUtc?.UtcDateTime ?? fileVersion.CreationDateTimeUtc.UtcDateTime;
                fileVersionDto.LastModificationDateTimeLocal =
                    _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:lastModificationDateTimeUtc, orgRegProgramId:currentRegulatoryProgramId);

                if (fileVersionDto.LastModifierUserId.HasValue)
                {
                    var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == fileVersionDto.LastModifierUserId.Value);
                    fileVersionDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                }
                else
                {
                    fileVersionDto.LastModifierFullName = "N/A";
                }

                return fileVersionDto;
            }
        }

        /// <inheritdoc />
        public int? AddOrUpdateFileVersionFieldForAuthorityConfiguration(int fileVersionId, FileVersionFieldDto dto)
        {
            using (_dbContext.BeginTransactionScope(from:MethodBase.GetCurrentMethod()))
            {
                FileVersionField domainObject = null;

                if (dto.FileVersionFieldId.HasValue)
                {
                    domainObject = _dbContext.FileVersionFields.FirstOrDefault(i => i.FileVersionFieldId == dto.FileVersionFieldId);
                }

                if (domainObject == null)
                {
                    domainObject = new FileVersionField();
                }

                if (!dto.IsIncluded)
                {
                    _dbContext.FileVersionFields.Remove(entity:domainObject);
                    _dbContext.SaveChanges();
                    return null;
                }

                domainObject.Name = dto.FileVersionFieldName;
                domainObject.Description = dto.Description;
                domainObject.FileVersionId = fileVersionId;
                domainObject.SystemFieldId = (int) dto.SystemFieldName;
                domainObject.DataOptionalityId = (int) dto.DataOptionalityName;
                domainObject.Size = dto.Size;
                domainObject.ExampleData = dto.ExampleData;
                domainObject.AdditionalComments = dto.AdditionalComments;

                if (!dto.FileVersionFieldId.HasValue)
                {
                    _dbContext.FileVersionFields.Add(entity:domainObject);
                }

                _dbContext.SaveChanges();

                var fileVersion = _dbContext.FileVersions.FirstOrDefault(i => i.FileVersionId == fileVersionId);

                if (fileVersion != null)
                {
                    fileVersion.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;
                    fileVersion.LastModifierUserId = _httpContextService.CurrentUserProfileId();
                    _dbContext.SaveChanges();
                }

                return domainObject.FileVersionFieldId;
            }
        }

        /// <inheritdoc />
        public ExportFileDto DownloadSampleImportTemplate(string fileVersionName)
        {
            var workbook = new Workbook();
            var fileVersion = GetFileVersion(fileVersionName:fileVersionName);

            if (fileVersion == null)
            {
                throw CreateRuleViolationExceptionForValidationError(errorMessage:"There is no file");
            }

            workbook.SuspendLayoutUpdate();

            var worksheet = workbook.Worksheets.Add();
            worksheet.Name = "Data";
            const int headerRowIndex = 0;
            var colIndex = 0;

            worksheet.Rows[fromIndex:headerRowIndex, toIndex:headerRowIndex].SetIsBold(value:true);

            foreach (var fileVersionFileVersionField in fileVersion.FileVersionFields.OrderBy(x => (int) x.SystemFieldName))
            {
                worksheet.Cells[rowIndex:headerRowIndex, columnIndex:colIndex].SetValue(value:fileVersionFileVersionField.FileVersionFieldName);

                var columnSelection = worksheet.Columns[index:colIndex];

                AutoFitExcelColumnWidth(columnSelection:columnSelection, worksheet:worksheet, colIndex:colIndex);
                colIndex++;
            }

            workbook.ResumeLayoutUpdate();
            return GetExcelOutputFileDto(fileNameWithoutExtension:fileVersionName + "Template_" + fileVersion.LastModificationDateTimeLocal.ToString(format:"yyyyMMdd"),
                                         workbook:workbook);
        }

        /// <inheritdoc />
        public ExportFileDto DownloadSampleImportTemplateInstruction(string fileVersionName)
        {
            var workbook = new Workbook();
            var fileVersion = GetFileVersion(fileVersionName:fileVersionName);

            if (fileVersion == null)
            {
                throw CreateRuleViolationExceptionForValidationError(errorMessage:"There is no file instruction");
            }

            var instructionFilePath = HostingEnvironment.MapPath(virtualPath:"~/Resources/SampleImportInstruction.xlsx"); //TODO: need to make dynamic later

            Workbook instructionWorkbook = null;

            if (instructionFilePath != null)
            {
                using (Stream stream = File.OpenRead(path:instructionFilePath))
                {
                    instructionWorkbook = new XlsxFormatProvider().Import(input:stream);
                }
            }

            workbook.SuspendLayoutUpdate();

            AddDataDescriptionSheetForSampleImportInstruction(workbook:workbook, fileVersion:fileVersion);

            if (instructionWorkbook != null)
            {
                AddDataExampleSheetForSampleImportInstruction(workbook:workbook, fileVersion:fileVersion, instructionWorkbook:instructionWorkbook);
                AddDataRulesSheetForSampleImportInstruction(workbook:workbook, fileVersion:fileVersion, instructionWorkbook:instructionWorkbook);
                AddHowTheImportWorksSheetForSampleImportInstruction(workbook:workbook, fileVersion:fileVersion, instructionWorkbook:instructionWorkbook);
                AddDataExampleWithMassLoadingsSheetForSampleImportInstruction(workbook:workbook, fileVersion:fileVersion, instructionWorkbook:instructionWorkbook);
            }

            workbook.ResumeLayoutUpdate();

            return GetExcelOutputFileDto(fileNameWithoutExtension:fileVersionName + "Instruction_" + fileVersion.LastModificationDateTimeLocal.ToString(format:"yyyyMMdd"),
                                         workbook:workbook);
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

                using (_dbContext.BeginTransactionScope(from:MethodBase.GetCurrentMethod()))
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

        private Workbook GetWorkbook(ImportTempFileDto importTempFileDto)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(),
                                    descripition:$"importTempFileId={importTempFileDto?.ImportTempFileId?.ToString() ?? "null"}"))
            {
                Workbook workbook = null;

                var providers = new List<IWorkbookFormatProvider>
                                {
                                    new XlsxFormatProvider()
                                };

                if (importTempFileDto?.RawFile != null)
                {
                    var extension = importTempFileDto.FileExtension;
                    var provider = providers.FirstOrDefault(p => p.SupportedExtensions.Any(e => string.Compare(strA:extension,
                                                                                                               strB:e,
                                                                                                               comparisonType:StringComparison.InvariantCultureIgnoreCase)
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
                                // ReSharper disable once ArgumentsStyleStringLiteral
                                // ReSharper disable once ArgumentsStyleNamedExpression
                                _logger.Error(ex, "Open Import file failed:");
                                throw new BadRequest(message:ErrorConstants.SampleImport.FileValidation.ImportFileIsCorrupted);
                            }
                        }
                    }
                    else
                    {
                        throw new BadRequest(message:ErrorConstants.SampleImport.FileValidation.ImportFileIsCorrupted);
                    }
                }

                return workbook;
            }
        }

        private void AddRequiredDataDefaultDtoOrPopulateDefaultValue(SampleImportDto sampleImportDto,
                                                                     List<RequiredDataDefaultsDto> requiredDataDefaults,
                                                                     SampleImportColumnName columnName)
        {
            var emptyValueCells = GetEmptyValueCells(sampleImportDto:sampleImportDto, columnName:columnName);
            if (!emptyValueCells.Any())
            {
                return;
            }

            _logger.Debug(message: "{0} found empty {1} cells which requires default value", argument1: sampleImportDto.ImportId.ToString(), argument2: columnName);
            requiredDataDefaults.Add(item: new RequiredDataDefaultsDto
                                           {
                                               SampleImportColumnName = columnName,
                                               Options = GetSelectListBySampleImportColumn(columnName: columnName)
                                           });
        }

        private void PopulateEmptyColumnValueAsDefaultValue(SampleImportDto sampleImportDto, SampleImportColumnName columnName, ListItemDto defaultValue)
        {
            var doesDefaultValueExist = defaultValue != null && defaultValue.Id > 0;
            if (!doesDefaultValueExist)
            {
                return;
            }

            var emptyValueCells = GetEmptyValueCells(sampleImportDto: sampleImportDto, columnName: columnName);
            if (!emptyValueCells.Any())
            {
                return;
            }

            _logger.Debug(message: "{0} populated empty {1} cells with requires default value", argument1: sampleImportDto.ImportId.ToString(), argument2: columnName);
            foreach (var cell in emptyValueCells)
            {
                cell.TranslatedValueId = defaultValue.Id;
                cell.TranslatedValue = defaultValue.DisplayValue;
            }
        }

        private static List<ImportCellObject> GetEmptyValueCells(SampleImportDto sampleImportDto, SampleImportColumnName columnName)
        {
            return sampleImportDto.Rows.Select(row => row.Cells.First(cell => cell.SampleImportColumnName == columnName))
                                  .Where(cell => string.IsNullOrEmpty(value:cell.OriginalValueString))
                                  .ToList();
        }

        private List<MissingTranslationDto> GetMissingTranslationSet(SampleImportDto sampleImportDto, out List<DataSourceTranslationItemDto> translationItemsShouldAutoGenerate)
        {
            translationItemsShouldAutoGenerate = new List<DataSourceTranslationItemDto>();

            var missingTranslations = new List<MissingTranslationDto>();
            foreach (var columnName in ColumnsNeedDataTranslation)
            {
                List<DataSourceTranslationItemDto> translationItemsNeedToAutoGenerate;
                var missingTranslation = GetMissingTranslationOrNull(sampleImportDto:sampleImportDto, columnName:columnName,
                                                                     translationItemsShouldAutoGenerate:out translationItemsNeedToAutoGenerate);
                if (missingTranslation != null)
                {
                    missingTranslations.Add(item:missingTranslation);
                }

                if (translationItemsNeedToAutoGenerate.Any())
                {
                    translationItemsShouldAutoGenerate.AddRange(collection:translationItemsNeedToAutoGenerate);
                }
            }

            return missingTranslations;
        }

        private MissingTranslationDto GetMissingTranslationOrNull(SampleImportDto sampleImportDto, SampleImportColumnName columnName,
                                                                  out List<DataSourceTranslationItemDto> translationItemsShouldAutoGenerate)
        {
            translationItemsShouldAutoGenerate = new List<DataSourceTranslationItemDto>();
            var missingTranslationTerms = sampleImportDto.Rows.Select(row => row.Cells.First(cell => cell.SampleImportColumnName == columnName))
                                                         .Where(cell => !string.IsNullOrEmpty(value:cell.OriginalValueString) && cell.TranslatedValueId == 0)
                                                         .Select(cell => cell.OriginalValueString)
                                                         .Distinct(comparer:StringComparer.OrdinalIgnoreCase)
                                                         .ToList();
            if (!missingTranslationTerms.Any())
            {
                return null;
            }

            translationItemsShouldAutoGenerate = DetectAutoGenerateTranslationItems(missingTranslationTerms:missingTranslationTerms,
                                                                                    translationType:ToTranslationType(columnName:columnName));
            if (translationItemsShouldAutoGenerate.Any())
            {
                var generatedTranslationNames = translationItemsShouldAutoGenerate.Select(x => x.TranslationName);
                _logger.Debug(message:"There are {0} {1} translation(s) should auto gererate: {2}", argument1:translationItemsShouldAutoGenerate.Count,
                              argument2:columnName, argument3:string.Join(separator:",", values:translationItemsShouldAutoGenerate.Select(x => x.TranslationName)));

                missingTranslationTerms = missingTranslationTerms.ExceptStringsCaseInsensitive(value:generatedTranslationNames).ToList();
                if (!missingTranslationTerms.Any())
                {
                    return null;
                }
            }

            _logger.Debug(message:"Column {0} missing translation(s) at {1}: {2}", argument1:columnName, argument2:sampleImportDto.ImportId.ToString(),
                          argument3:string.Join(separator:",", values:missingTranslationTerms));
            return new MissingTranslationDto
                   {
                       SampleImportColumnName = columnName,
                       MissingTranslations = missingTranslationTerms,
                       Options = GetSelectListBySampleImportColumn(columnName:columnName)
                   };
        }

        private List<DataSourceTranslationItemDto> DetectAutoGenerateTranslationItems(IEnumerable<string> missingTranslationTerms, DataSourceTranslationType translationType)
        {
            Dictionary<string, DataSourceTranslationItemDto> translationItemsDict;
            switch (translationType)
            {
                case DataSourceTranslationType.MonitoringPoint:
                    translationItemsDict = _selectListService.GetIndustryMonitoringPointSelectList()
                                                             .ToCaseInsensitiveDictionary(x => x.DisplayValue,
                                                                                          x => new DataSourceTranslationItemDto
                                                                                               {
                                                                                                   TranslationId = x.Id,
                                                                                                   TranslationName = x.DisplayValue,
                                                                                                   TranslationType = translationType
                                                                                               });
                    break;
                case DataSourceTranslationType.SampleType:
                    translationItemsDict = _selectListService.GetAuthoritySampleTypeSelectList()
                                                             .ToCaseInsensitiveDictionary(x => x.DisplayValue,
                                                                                          x => new DataSourceTranslationItemDto
                                                                                               {
                                                                                                   TranslationId = x.Id,
                                                                                                   TranslationName = x.DisplayValue,
                                                                                                   TranslationType = translationType
                                                                                               });
                    break;
                case DataSourceTranslationType.CollectionMethod:
                    translationItemsDict = _selectListService.GetAuthorityCollectionMethodSelectList()
                                                             .ToCaseInsensitiveDictionary(x => x.DisplayValue,
                                                                                          x => new DataSourceTranslationItemDto
                                                                                               {
                                                                                                   TranslationId = x.Id,
                                                                                                   TranslationName = x.DisplayValue,
                                                                                                   TranslationType = translationType
                                                                                               });
                    break;
                case DataSourceTranslationType.Parameter:
                    translationItemsDict = _selectListService.GetAuthorityParameterSelectList()
                                                             .ToCaseInsensitiveDictionary(x => x.DisplayValue,
                                                                                          x => new DataSourceTranslationItemDto
                                                                                               {
                                                                                                   TranslationId = x.Id,
                                                                                                   TranslationName = x.DisplayValue,
                                                                                                   TranslationType = translationType
                                                                                               });
                    break;
                case DataSourceTranslationType.Unit:
                    translationItemsDict = _selectListService.GetAuthorityUnitSelectList()
                                                             .ToCaseInsensitiveDictionary(x => x.DisplayValue,
                                                                                          x => new DataSourceTranslationItemDto
                                                                                               {
                                                                                                   TranslationId = x.Id,
                                                                                                   TranslationName = x.DisplayValue,
                                                                                                   TranslationType = translationType
                                                                                               });
                    break;
                default: throw CreateRuleViolationExceptionForValidationError(errorMessage:$"DataSourceTranslationType {translationType} is unsupported");
            }

            var matchedTermsForAutoGeneration = missingTranslationTerms.IntersectStringsCaseInsensitive(value:translationItemsDict.Keys).ToList();
            return translationItemsDict.Where(x => matchedTermsForAutoGeneration.CaseInsensitiveContains(value:x.Key)).Select(x => x.Value).ToList();
        }

        private List<ListItemDto> GetSelectListBySampleImportColumn(SampleImportColumnName columnName)
        {
            switch (columnName)
            {
                case SampleImportColumnName.MonitoringPoint: return _selectListService.GetIndustryMonitoringPointSelectList(withEmptyItem:true);
                case SampleImportColumnName.SampleType: return _selectListService.GetAuthoritySampleTypeSelectList(withEmptyItem:true);
                case SampleImportColumnName.CollectionMethod: return _selectListService.GetAuthorityCollectionMethodSelectList(withEmptyItem:true);
                case SampleImportColumnName.ParameterName: return _selectListService.GetAuthorityParameterSelectList(withEmptyItem:true);
                case SampleImportColumnName.ResultUnit: return _selectListService.GetAuthorityUnitSelectList(withEmptyItem:true);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private List<ImportRowObject> GetImportRowObjects(Workbook importFileWorkbook, SampleImportDto sampleImportDto, ref ImportSampleFromFileValidationResultDto result)
        {
            var validationIssues = new List<ErrorWithRowNumberDto>();
            var rows = new List<ImportRowObject>();
            var worksheet = importFileWorkbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                validationIssues.Add(item:new ErrorWithRowNumberDto {ErrorMessage = ErrorConstants.SampleImport.FileValidation.ImportFileIsEmpty});
            }
            else
            {
                // first populate the resultQualifierValidValues from Authority's settings
                var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                var programSettings = _settingService.GetProgramSettingsById(orgRegProgramId:_settingService
                                                                                 .GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationRegulatoryProgramId);

                var resultQualifierValidValues = programSettings.Settings
                                                                .Where(s => s.TemplateName.Equals(obj:SettingType.ResultQualifierValidValues))
                                                                .Select(s => s.Value).First()?.Split(',').ToList()
                                                 ?? new List<string> {""};
                resultQualifierValidValues.Add(item:""); // for "NUMERIC" values

                var templateColumnDictionary = sampleImportDto.FileVersion.FileVersionFields.ToDictionary(x => x.FileVersionFieldName.ToLower(), x => x);
                var requiredColumns = sampleImportDto.FileVersion.FileVersionFields
                                                     .Where(x => x.DataOptionalityName == DataOptionalityName.Required || x.IsSystemRequired)
                                                     .Select(x => x.FileVersionFieldName.ToLower())
                                                     .ToList();

                var fileColumnDictionary = new Dictionary<int, FileVersionFieldDto>();
                var fileColumns = new List<string>();
                var isResultQualifierColumnIncluded = false;

                var usedCellRangeWithValues = worksheet.GetUsedCellRange(propertyDefinitions:new IPropertyDefinition[] {CellPropertyDefinitions.ValueProperty});

                if (usedCellRangeWithValues.RowCount == 0)
                {
                    validationIssues.Add(item:new ErrorWithRowNumberDto {ErrorMessage = ErrorConstants.SampleImport.FileValidation.ImportFileIsEmpty});
                }

                //loop through the rows
                for (var rowIndex = usedCellRangeWithValues.FromIndex.RowIndex; rowIndex <= usedCellRangeWithValues.ToIndex.RowIndex; rowIndex++)
                {
                    var importRowObject = new ImportRowObject
                                          {
                                              RowNumber = rowIndex + 1,
                                              Cells = new List<ImportCellObject>()
                                          };

                    for (var columnIndex = usedCellRangeWithValues.FromIndex.ColumnIndex; columnIndex <= usedCellRangeWithValues.ToIndex.ColumnIndex; columnIndex++)
                    {
                        var cellValue = worksheet.Cells[rowIndex:rowIndex, columnIndex:columnIndex].GetValue().Value;
                        var format = worksheet.Cells[rowIndex:rowIndex, columnIndex:columnIndex].GetFormat().Value;

                        var resultAsString = cellValue.GetValueAsString(format:format);

                        //Find column header from first row and map with FileVersionFieldDto
                        if (rowIndex == 0)
                        {
                            if (templateColumnDictionary.ContainsKey(key:resultAsString.ToLower()))
                            {
                                var templateColumn = templateColumnDictionary[key:resultAsString.ToLower()];
                                fileColumnDictionary.Add(key:columnIndex, value:templateColumn);
                                fileColumns.Add(item:resultAsString);

                                if (templateColumn.SystemFieldName == SampleImportColumnName.ResultQualifier)
                                {
                                    isResultQualifierColumnIncluded = true;
                                }
                            }
                            else
                            {
                                _logger.Error(message:$"#NewSampleImportFileColumn - Column header '{resultAsString}' is not belongs the authority's file template."
                                                      + $" Current OrgRegProgramId:{currentOrgRegProgramId}. DataScource: {sampleImportDto.DataSource.Name} ");
                            }
                        }
                        else
                        {
                            FileVersionFieldDto templateColumn;
                            if (!fileColumnDictionary.TryGetValue(key:columnIndex, value:out templateColumn))
                            {
                                // column is not in the Authority template. So no need to process
                                continue;
                            }

                            var importCellObject = new ImportCellObject
                                                   {
                                                       SampleImportColumnName = templateColumn.SystemFieldName,
                                                       OriginalValueString = cellValue.GetResultValueAsString(format:format).Trim()
                                                   };

                            //Validate each field has correct data format or not
                            switch (templateColumn.DataFormatName)
                            {
                                case DataFormatName.Text:

                                    //Validate text field length
                                    if (resultAsString.Length > templateColumn.Size)
                                    {
                                        validationIssues.Add(item:new ErrorWithRowNumberDto
                                                                  {
                                                                      ErrorMessage = string.Format(format:ErrorConstants.SampleImport.FileValidation.FieldValueExceedMaximumSize,
                                                                                                   arg0:templateColumn.FileVersionFieldName, arg1:templateColumn.Size),
                                                                      RowNumbers = importRowObject.RowNumber.ToString()
                                                                  });
                                    }

                                    importCellObject.OriginalValue = importCellObject.OriginalValueString;
                                    break;

                                case DataFormatName.Float:
                                    try
                                    {
                                        importCellObject.OriginalValue = ToDouble(cellValue:cellValue);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(value:ex);

                                        validationIssues.Add(item:new ErrorWithRowNumberDto
                                                                  {
                                                                      ErrorMessage = string.Format(format:ErrorConstants.SampleImport.FileValidation.FieldValueIsNotNumeric,
                                                                                                   arg0:templateColumn.FileVersionFieldName),
                                                                      RowNumbers = importRowObject.RowNumber.ToString()
                                                                  });
                                        importCellObject.OriginalValue = default(double?);
                                    }

                                    break;

                                case DataFormatName.DateTime:

                                    //Validate date format
                                    try
                                    {
                                        importCellObject.OriginalValue = ToDateTime(cellValue:cellValue);
                                        importCellObject.OriginalValueString = importCellObject.OriginalValue?.ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(value:ex);
                                        validationIssues.Add(item:new ErrorWithRowNumberDto
                                                                  {
                                                                      ErrorMessage = string.Format(format:ErrorConstants.SampleImport.FileValidation.FieldValueIsNotDate,
                                                                                                   arg0:templateColumn.FileVersionFieldName),
                                                                      RowNumbers = importRowObject.RowNumber.ToString()
                                                                  });
                                        importCellObject.OriginalValue = default(DateTime?);
                                    }

                                    break;

                                case DataFormatName.Bit:
                                    try
                                    {
                                        importCellObject.OriginalValue = ToBoolean(cellValue:cellValue);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(value:ex);
                                        validationIssues.Add(item:new ErrorWithRowNumberDto
                                                                  {
                                                                      ErrorMessage = string.Format(format:ErrorConstants.SampleImport.FileValidation.FieldValueIsNotBoolean,
                                                                                                   arg0:templateColumn.FileVersionFieldName),
                                                                      RowNumbers = importRowObject.RowNumber.ToString()
                                                                  });
                                        importCellObject.OriginalValue = default(bool?);
                                    }

                                    break;
                                default: throw new NotImplementedException();
                            }

                            // populate TranslatedValue where TranslatedValue is same as OriginalValue
                            switch (templateColumn.SystemFieldName)
                            {
                                case SampleImportColumnName.SampleStartDateTime:
                                case SampleImportColumnName.SampleEndDateTime:
                                case SampleImportColumnName.ResultQualifier:
                                case SampleImportColumnName.Result:
                                case SampleImportColumnName.LabSampleId:
                                case SampleImportColumnName.MethodDetectionLimit:
                                case SampleImportColumnName.AnalysisDateTime:
                                case SampleImportColumnName.AnalysisMethod:
                                    importCellObject.TranslatedValue = importCellObject.OriginalValue;
                                    break;
                                case SampleImportColumnName.MonitoringPoint:
                                case SampleImportColumnName.SampleType:
                                case SampleImportColumnName.CollectionMethod:
                                case SampleImportColumnName.ParameterName:
                                case SampleImportColumnName.ResultUnit:

                                    // will be populate later 
                                    break;
                                default: throw new NotImplementedException();
                            }

                            // Validate Authority Required fields for system required
                            ValidateAuthorityRequiredFieldsForSystemRequired(templateColumn:templateColumn,
                                                                             importCellObject:importCellObject,
                                                                             validationIssues:validationIssues,
                                                                             importRowObject:importRowObject);

                            importRowObject.Cells.Add(item:importCellObject);
                        }
                    }

                    if (rowIndex == 0)
                    {
                        DoColumnHeaderValidation(requiredColumns:requiredColumns, fileColumns:fileColumns, validationIssues:validationIssues);
                    }
                    else
                    {
                        //validate result field with qualifier
                        if (isResultQualifierColumnIncluded)
                        {
                            var resultColumnValue = importRowObject.Cells.First(x => x.SampleImportColumnName == SampleImportColumnName.Result).OriginalValueString;
                            var resultQualifierColumnValue =
                                importRowObject.Cells.First(x => x.SampleImportColumnName == SampleImportColumnName.ResultQualifier).OriginalValueString;

                            if (resultQualifierValidValues.Contains(item:resultQualifierColumnValue))
                            {
                                if (new List<string> {"", "<", ">"}.Contains(item:resultQualifierColumnValue) && string.IsNullOrWhiteSpace(value:resultColumnValue))
                                {
                                    validationIssues.Add(item:new ErrorWithRowNumberDto
                                                              {
                                                                  ErrorMessage = ErrorConstants.SampleImport.FileValidation.ResultIsRequired,
                                                                  RowNumbers = importRowObject.RowNumber.ToString()
                                                              });
                                }
                                else if (new List<string> {"ND", "NF"}.CaseInsensitiveContains(value:resultQualifierColumnValue)
                                         && !string.IsNullOrWhiteSpace(value:resultColumnValue))
                                {
                                    validationIssues.Add(item:new ErrorWithRowNumberDto
                                                              {
                                                                  ErrorMessage = ErrorConstants.SampleImport.FileValidation.ResultQualifierNdNfShouldNotHaveAValue,
                                                                  RowNumbers = importRowObject.RowNumber.ToString()
                                                              });
                                }
                            }
                            else
                            {
                                validationIssues.Add(item:new ErrorWithRowNumberDto
                                                          {
                                                              ErrorMessage = string.Format(format:ErrorConstants.SampleImport.FileValidation.ResultQualifierIsInvalid,
                                                                                           arg0:resultQualifierColumnValue),
                                                              RowNumbers = importRowObject.RowNumber.ToString()
                                                          });
                            }
                        }

                        //Convert workbook.cell to ImportCellObject
                        rows.Add(item:importRowObject);
                    }
                }
            }

            // check there is any validation issue or not
            if (validationIssues.Count > 0)
            {
                result.Errors = validationIssues;
            }

            return rows;
        }

        private static void ValidateAuthorityRequiredFieldsForSystemRequired(FileVersionFieldDto templateColumn, ImportCellObject importCellObject, List<ErrorWithRowNumberDto> validationIssues,
                                                                             ImportRowObject importRowObject)
        {
            if (templateColumn.IsSystemRequired && templateColumn.DataOptionalityName == DataOptionalityName.Required)
            {
                if (string.IsNullOrWhiteSpace(value:importCellObject.OriginalValueString))
                {
                    if (importCellObject.SampleImportColumnName == SampleImportColumnName.ResultQualifier)
                    {
                        importCellObject.OriginalValue = ""; // this is valid for NUMERIC results
                    }
                    else
                    {
                        validationIssues.Add(item:new ErrorWithRowNumberDto
                                                  {
                                                      ErrorMessage = string.Format(format:ErrorConstants.SampleImport.FileValidation.FieldValueIsRequired,
                                                                                   arg0:templateColumn.FileVersionFieldName),
                                                      RowNumbers = importRowObject.RowNumber.ToString()
                                                  });
                    }
                }
            }
        }

        private static void DoColumnHeaderValidation(IEnumerable<string> requiredColumns, IEnumerable<string> fileColumns, ICollection<ErrorWithRowNumberDto> validationIssues)
        {
            //Check all required columns are exists or not
            var missingRequiredColumns = requiredColumns.Except(second:fileColumns, comparer:StringComparer.OrdinalIgnoreCase).ToList();

            if (missingRequiredColumns.Any())
            {
                validationIssues.Add(item:new ErrorWithRowNumberDto
                                          {
                                              ErrorMessage = string.Format(format:ErrorConstants.SampleImport.FileValidation.ImportFileMissingRequiredFields,
                                                                           arg0:string.Join(separator:",", values:missingRequiredColumns.Select(name => name)))
                                          });
            }
        }

        private static DateTime? ToDateTime(ICellValue cellValue)
        {
            var rawValueAsNumber = string.IsNullOrWhiteSpace(value:cellValue.RawValue) ? default(double?) : Convert.ToDouble(value:cellValue.RawValue);
            var resultAsDateTime = rawValueAsNumber.HasValue ? FormatHelper.ConvertDoubleToDateTime(doubleValue:rawValueAsNumber.Value) : default(DateTime?);
            return resultAsDateTime;
        }

        private static bool ToBoolean(ICellValue cellValue)
        {
            return Convert.ToBoolean(value:cellValue.RawValue);
        }

        private static double? ToDouble(ICellValue cellValue)
        {
            var rawValueAsNumber = string.IsNullOrWhiteSpace(value:cellValue.RawValue) ? default(double?) : Convert.ToDouble(value:cellValue.RawValue);
            return rawValueAsNumber;
        }

        private SampleImportDto PopulateExistingTranslationData(SampleImportDto sampleImportDto)
        {
            foreach (var translationType in Enum.GetValues(enumType:typeof(DataSourceTranslationType)).Cast<DataSourceTranslationType>())
            {
                PopulateExistingTranslationData(sampleImportDto:sampleImportDto, translationType:translationType);
            }

            return sampleImportDto;
        }

        private SampleImportDto PopulateExistingTranslationData(SampleImportDto sampleImportDto, DataSourceTranslationType translationType)
        {
            if (sampleImportDto.DataSource.DataSourceId == null)
            {
                throw new BadRequest(message:ErrorConstants.SampleImport.DataProviderDoesNotExist);
            }

            var translationDict = GetDataSourceTranslationDict(dataSourceWithDataTranslations:sampleImportDto.DataSource, translationType:translationType);
            var sampleImportColumnName = ToSampleImportColumnName(fromTranslationType:translationType);

            var cellsToPopulate = sampleImportDto.Rows.Select(row => row.Cells.Find(cell => cell.SampleImportColumnName == sampleImportColumnName))
                                                 .Where(cell => !string.IsNullOrEmpty(value:cell.OriginalValue))
                                                 .ToList();

            foreach (var cell in cellsToPopulate)
            {
                DataSourceTranslationItemDto translationItem;
                if (!translationDict.TryGetValue(key:cell.OriginalValueString, value:out translationItem))
                {
                    _logger.Info(message:"{0} translation should set missing translation to term '{1}'", argument1:translationType, argument2:cell.OriginalValueString);
                    continue;
                }

                cell.TranslatedValueId = translationItem.TranslationId;
                cell.TranslatedValue = translationItem.TranslationName;
            }

            return sampleImportDto;
        }

        private Dictionary<string, DataSourceTranslationItemDto> GetDataSourceTranslationDict(DataSourceDto dataSourceWithDataTranslations,
                                                                                              DataSourceTranslationType translationType)
        {
            ICollection<DataSourceTranslationDto> dataSourceTranslations;
            switch (translationType)
            {
                case DataSourceTranslationType.MonitoringPoint:
                    dataSourceTranslations = dataSourceWithDataTranslations.DataSourceMonitoringPoints;
                    break;
                case DataSourceTranslationType.SampleType:
                    dataSourceTranslations = dataSourceWithDataTranslations.DataSourceSampleTypes;
                    break;
                case DataSourceTranslationType.CollectionMethod:
                    dataSourceTranslations = dataSourceWithDataTranslations.DataSourceCollectionMethods;
                    break;
                case DataSourceTranslationType.Parameter:
                    dataSourceTranslations = dataSourceWithDataTranslations.DataSourceParameters;
                    break;
                case DataSourceTranslationType.Unit:
                    dataSourceTranslations = dataSourceWithDataTranslations.DataSourceUnits;
                    break;
                default: throw new InternalServerError(message:$"DataSourceTranslationType {translationType} is unsupported");
            }

            var caseInsensitiveDictionary = new Dictionary<string, DataSourceTranslationItemDto>(comparer:StringComparer.OrdinalIgnoreCase);
            foreach (var translation in dataSourceTranslations)
            {
                caseInsensitiveDictionary.Add(key:translation.DataSourceTerm, value:translation.TranslationItem);
            }

            return caseInsensitiveDictionary;
        }

        private static SampleImportColumnName ToSampleImportColumnName(DataSourceTranslationType fromTranslationType)
        {
            switch (fromTranslationType)
            {
                case DataSourceTranslationType.MonitoringPoint: return SampleImportColumnName.MonitoringPoint;
                case DataSourceTranslationType.SampleType: return SampleImportColumnName.SampleType;
                case DataSourceTranslationType.CollectionMethod: return SampleImportColumnName.CollectionMethod;
                case DataSourceTranslationType.Parameter: return SampleImportColumnName.ParameterName;
                case DataSourceTranslationType.Unit: return SampleImportColumnName.ResultUnit;
                default: throw new NotSupportedException(message:$"DataSourceTranslationType {fromTranslationType} is unsupported");
            }
        }

        private static DataSourceTranslationType ToTranslationType(SampleImportColumnName columnName)
        {
            switch (columnName)
            {
                case SampleImportColumnName.MonitoringPoint: return DataSourceTranslationType.MonitoringPoint;
                case SampleImportColumnName.SampleType: return DataSourceTranslationType.SampleType;
                case SampleImportColumnName.CollectionMethod: return DataSourceTranslationType.CollectionMethod;
                case SampleImportColumnName.ParameterName: return DataSourceTranslationType.Parameter;
                case SampleImportColumnName.ResultUnit: return DataSourceTranslationType.Unit;
                default: throw new BadRequest(message:$"Cannot convert SampleImportColumnName {columnName} to DataSourceTranslationType.");
            }
        }

        private bool IsImportTempFileWithThisOwnerExist(int importTempFileId, int orgRegProgramId)
        {
            //Also handles scenarios where ImportTempFileId does not exist
            return _dbContext.ImportTempFiles.Any(fs => fs.ImportTempFileId == importTempFileId && fs.OrganizationRegulatoryProgramId == orgRegProgramId);
        }

        private static void AutoFitExcelColumnWidth(ColumnSelection columnSelection, Worksheet worksheet, int colIndex)
        {
            columnSelection.AutoFitWidth();

            //NOTE: workaround for incorrect auto fit.
            var newWidth = worksheet.Columns[index:colIndex].GetWidth().Value.Value + 15;
            columnSelection.SetWidth(value:new ColumnWidth(value:newWidth, isCustom:false));
        }

        private static ExportFileDto GetExcelOutputFileDto(string fileNameWithoutExtension, Workbook workbook)
        {
            var formatProvider = new XlsxFormatProvider();
            byte[] renderedBytes;

            using (var ms = new MemoryStream())
            {
                formatProvider.Export(workbook:workbook, output:ms);
                renderedBytes = ms.ToArray();
            }

            var fileDto = new ExportFileDto
                          {
                              Name = fileNameWithoutExtension + ".xlsx",
                              ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                              Data = renderedBytes
                          };
            return fileDto;
        }

        private static void AddDataDescriptionSheetForSampleImportInstruction(Workbook workbook, FileVersionDto fileVersion)
        {
            var worksheet = workbook.Worksheets.Add();
            worksheet.Name = "Data Descriptions";
            var rowIndex = 1;
            var colIndex = 0;

            foreach (var fileVersionFileVersionField in fileVersion.FileVersionFields.OrderBy(x => (int) x.SystemFieldName))
            {
                foreach (var columnName in Enum.GetValues(enumType:typeof(DataDescriptionColumnName)).Cast<DataDescriptionColumnName>())
                {
                    colIndex = (int) columnName;

                    switch (columnName)
                    {
                        case DataDescriptionColumnName.FileVersionFieldName:
                            worksheet.Cells[rowIndex:rowIndex, columnIndex:colIndex].SetValue(value:fileVersionFileVersionField.FileVersionFieldName);
                            break;
                        case DataDescriptionColumnName.DataOptionalityName:
                            worksheet.Cells[rowIndex:rowIndex, columnIndex:colIndex].SetValue(value:fileVersionFileVersionField.DataOptionalityName.ToString());
                            break;
                        case DataDescriptionColumnName.DataFormatName:
                            worksheet.Cells[rowIndex:rowIndex, columnIndex:colIndex].SetValue(value:fileVersionFileVersionField.DataFormatDescription);
                            break;
                        case DataDescriptionColumnName.FieldSize:
                            worksheet.Cells[rowIndex:rowIndex, columnIndex:colIndex].SetValue(value:fileVersionFileVersionField.Size.ToString());
                            break;
                        case DataDescriptionColumnName.Description:
                            worksheet.Cells[rowIndex:rowIndex, columnIndex:colIndex].SetValue(value:fileVersionFileVersionField.Description);
                            break;
                        case DataDescriptionColumnName.ExampleData:
                            worksheet.Cells[rowIndex:rowIndex, columnIndex:colIndex].SetValue(value:fileVersionFileVersionField.ExampleData);
                            break;
                        case DataDescriptionColumnName.AdditionalComments:
                            worksheet.Cells[rowIndex:rowIndex, columnIndex:colIndex].SetValue(value:fileVersionFileVersionField.AdditionalComments);
                            break;
                        default: throw new ArgumentOutOfRangeException();
                    }
                }

                rowIndex++;
            }

            const int headerRowIndex = 0;
            const double maxColumnWidth = 400;
            const double minColumnWidth = 85;

            worksheet.Rows[fromIndex:0, toIndex:0].SetIsBold(value:true);
            worksheet.Rows[fromIndex:0, toIndex:0].SetHorizontalAlignment(value:RadHorizontalAlignment.Center);
            worksheet.Columns[fromIndex:0, toIndex:0].SetIsBold(value:true);

            foreach (var columnName in Enum.GetValues(enumType:typeof(DataDescriptionColumnName)).Cast<DataDescriptionColumnName>())
            {
                colIndex = (int) columnName;
                var columnSelection = worksheet.Columns[index:colIndex];
                AutoFitExcelColumnWidth(columnSelection:columnSelection, worksheet:worksheet, colIndex:colIndex);

                if (worksheet.Columns[index:colIndex].GetWidth().Value.Value > maxColumnWidth)
                {
                    columnSelection.SetWidth(value:new ColumnWidth(value:maxColumnWidth, isCustom:false));
                }

                if (worksheet.Columns[index:colIndex].GetWidth().Value.Value < minColumnWidth)
                {
                    columnSelection.SetWidth(value:new ColumnWidth(value:minColumnWidth, isCustom:false));
                }

                switch (columnName)
                {
                    case DataDescriptionColumnName.FileVersionFieldName:
                        worksheet.Cells[rowIndex:headerRowIndex, columnIndex:colIndex].SetValue(value:"Column Header");
                        break;
                    case DataDescriptionColumnName.DataOptionalityName:
                        worksheet.Cells[rowIndex:headerRowIndex, columnIndex:colIndex].SetValue(value:"Required?");
                        break;
                    case DataDescriptionColumnName.DataFormatName:
                        worksheet.Cells[rowIndex:headerRowIndex, columnIndex:colIndex].SetValue(value:"Data Format");
                        break;
                    case DataDescriptionColumnName.FieldSize:
                        worksheet.Cells[rowIndex:headerRowIndex, columnIndex:colIndex].SetValue(value:"Field Size");
                        columnSelection.SetHorizontalAlignment(value:RadHorizontalAlignment.CenterContinuous);
                        break;
                    case DataDescriptionColumnName.Description:
                        worksheet.Cells[rowIndex:headerRowIndex, columnIndex:colIndex].SetValue(value:"Field Description");
                        columnSelection.SetIsWrapped(value:true);
                        break;
                    case DataDescriptionColumnName.ExampleData:
                        worksheet.Cells[rowIndex:headerRowIndex, columnIndex:colIndex].SetValue(value:"Example Data");
                        columnSelection.SetIsWrapped(value:true);
                        columnSelection.SetIsBold(value:true);
                        break;
                    case DataDescriptionColumnName.AdditionalComments:
                        worksheet.Cells[rowIndex:headerRowIndex, columnIndex:colIndex].SetValue(value:"Additional Comments");
                        columnSelection.SetIsWrapped(value:true);
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void AddDataExampleSheetForSampleImportInstruction(Workbook workbook, FileVersionDto fileVersion, Workbook instructionWorkbook)
        {
            var instructionSheet = instructionWorkbook.Worksheets.GetByName(sheetName:"Data Example");

            if (instructionSheet != null)
            {
                var worksheet = workbook.Worksheets.Add();
                worksheet.CopyFrom(sourceWorksheet:instructionSheet);
                worksheet.Name = "Data Example";

                UpdateHeaderRowAndRemoveExtraColumns(fileVersion:fileVersion, worksheet:worksheet);
            }
        }

        private static void AddDataRulesSheetForSampleImportInstruction(Workbook workbook, FileVersionDto fileVersion, Workbook instructionWorkbook)
        {
            var instructionSheet = instructionWorkbook.Worksheets.GetByName(sheetName:"Data Rules");
            if (instructionSheet != null)
            {
                var worksheet = workbook.Worksheets.Add();
                worksheet.CopyFrom(sourceWorksheet:instructionSheet);
                worksheet.Name = "Data Rules";
            }
        }

        private static void AddHowTheImportWorksSheetForSampleImportInstruction(Workbook workbook, FileVersionDto fileVersion, Workbook instructionWorkbook)
        {
            var instructionSheet = instructionWorkbook.Worksheets.GetByName(sheetName:"How the Import works");
            if (instructionSheet != null)
            {
                var worksheet = workbook.Worksheets.Add();
                worksheet.CopyFrom(sourceWorksheet:instructionSheet);
                worksheet.Name = "How the Import works";
            }
        }

        private static void AddDataExampleWithMassLoadingsSheetForSampleImportInstruction(Workbook workbook, FileVersionDto fileVersion, Workbook instructionWorkbook)
        {
            var instructionSheet = instructionWorkbook.Worksheets.GetByName(sheetName:"Data Example with Mass Loadings");

            if (instructionSheet != null)
            {
                var worksheet = workbook.Worksheets.Add();
                worksheet.CopyFrom(sourceWorksheet:instructionSheet);
                worksheet.Name = "Data Example with Mass Loadings";

                UpdateHeaderRowAndRemoveExtraColumns(fileVersion:fileVersion, worksheet:worksheet);
            }
        }

        private static void UpdateHeaderRowAndRemoveExtraColumns(FileVersionDto fileVersion, Worksheet worksheet)
        {
            const int headerRowIndex = 0;

            worksheet.Rows[fromIndex:headerRowIndex, toIndex:headerRowIndex].SetIsBold(value:true);

            var usedCellRangeWithValues = worksheet.GetUsedCellRange(propertyDefinitions:new IPropertyDefinition[] {CellPropertyDefinitions.ValueProperty});
            var templateColumnDictionary = fileVersion.FileVersionFields.ToDictionary(x => x.SystemFieldName.ToString().ToLower(), x => x);
            var needToRemoveColumns = new List<int>();

            for (var columnIndex = usedCellRangeWithValues.FromIndex.ColumnIndex; columnIndex <= usedCellRangeWithValues.ToIndex.ColumnIndex; columnIndex++)
            {
                var cellValue = worksheet.Cells[rowIndex:headerRowIndex, columnIndex:columnIndex].GetValue().Value;
                var format = worksheet.Cells[rowIndex:headerRowIndex, columnIndex:columnIndex].GetFormat().Value;
                var resultAsString = cellValue.GetValueAsString(format:format).Trim();

                //Find column header from first row and map with FileVersionFieldDto
                if (templateColumnDictionary.ContainsKey(key:resultAsString.ToLower()))
                {
                    var fileVersionFileVersionField = templateColumnDictionary[key:resultAsString.ToLower()];
                    worksheet.Cells[rowIndex:headerRowIndex, columnIndex:columnIndex].SetValue(value:fileVersionFileVersionField.FileVersionFieldName);

                    var columnSelection = worksheet.Columns[index:columnIndex];
                    AutoFitExcelColumnWidth(columnSelection:columnSelection, worksheet:worksheet, colIndex:columnIndex);
                }
                else
                {
                    needToRemoveColumns.Add(item:columnIndex);
                }
            }

            foreach (var columnIndex in needToRemoveColumns.OrderByDescending(x => x)) // need to be descending otherwise when remove changes the column index 
            {
                worksheet.Columns[index:columnIndex].Remove();
            }
        }

        private enum DataDescriptionColumnName
        {
            FileVersionFieldName,
            DataOptionalityName,
            DataFormatName,
            FieldSize,
            Description,
            ExampleData,
            AdditionalComments
        }
    }
}