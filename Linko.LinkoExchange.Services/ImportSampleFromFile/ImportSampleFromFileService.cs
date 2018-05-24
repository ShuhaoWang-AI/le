﻿using System;
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
        #region fields

        private readonly IDataSourceService _dataSourceService;

        private readonly LinkoExchangeContext _dbContext;
        private readonly IFileStoreService _fileStoreService;
        private readonly IHttpContextService _httpContextService;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;
        private readonly IOrganizationService _organizationService;
        private readonly IReportElementService _reportElementService;
        private readonly IReportTemplateService _reportTemplateService;
        private readonly IParameterService _parameterService;
        private readonly ISampleService _sampleService;
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
                throw new ArgumentNullException(paramName: nameof(parameterService));
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
            ImportTempFileDto importTempFileDto;

            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(), descripition:$"importTempFileId={importTempFileId}"))
            {
                if (!CanUserExecuteApi(id:importTempFileId))
                {
                    throw new UnauthorizedAccessException();
                }

                var currentRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

                var importTempFile = _dbContext.ImportTempFiles.SingleOrDefault(i => i.ImportTempFileId == importTempFileId);

                importTempFileDto = _mapHelper.ToDto(fromDomainObject:importTempFile);

                if (importTempFileDto != null)
                {
                    importTempFileDto.UploadDateTimeLocal =
                        _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:importTempFileDto.UploadDateTimeLocal, orgRegProgramId:currentRegulatoryProgramId);
                }
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
                    throw CreateRuleViolationExceptionForValidationError(errorMessage:"No file was selected.");
                }

                if (importTempFileDto.RawFile.Length > maxFileSize)
                {
                    throw CreateRuleViolationExceptionForValidationError(errorMessage:$"The file size exceeds that {maxFileSize / 1024 / 1024} MB limit.");
                }

                var extension = Path.GetExtension(path:importTempFileDto.OriginalFileName)?.ToLower();

                // ReSharper disable once ArgumentsStyleStringLiteral
                var validFileTypes = _dbContext.FileTypes.Where(x => x.Extension.ToLower().Equals(".xlsx")).ToList();
                var validFileExtensions = validFileTypes.Select(i => i.Extension).Select(i => i.ToLower());

                if (!validFileExtensions.Contains(value:extension))
                {
                    throw CreateRuleViolationExceptionForValidationError(errorMessage:"The file type selected is not supported.");
                }

                var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                var currentUserProfileId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId));

                int importTempFileIdToReturn;
                using (_dbContext.CreateAutoCommitScope())
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

        public Workbook GetWorkbook(ImportTempFileDto importTempFileDto, bool isAuthorizationRequired = false)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(),
                                    descripition:$"importTempFileId={importTempFileDto?.ImportTempFileId?.ToString() ?? "null"}"))
            {
                if (importTempFileDto == null)
                {
                    throw CreateRuleViolationExceptionForValidationError(errorMessage:"The file is empty.");
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
                                _logger.Error(value:ex);

                                throw CreateRuleViolationExceptionForValidationError(errorMessage:"The file format is not recognized.");
                            }
                        }
                    }
                    else
                    {
                        throw CreateRuleViolationExceptionForValidationError(errorMessage:"The file format is not recognized.");
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

                try
                {
                    if (importTempFileDto?.ImportTempFileId != null && !CanUserExecuteApi(id:importTempFileDto.ImportTempFileId.Value))
                    {
                        throw new UnauthorizedAccessException();
                    }

                    //populate FileVersion in sampleImportDto
                    sampleImportDto.FileVersion = GetFileVersion();

                    if (sampleImportDto.FileVersion?.FileVersionId == null)
                    {
                        throw CreateRuleViolationExceptionForValidationError(errorMessage:"Authority does not have any sample import file template.");
                    }

                    //populate DataSource in sampleImportDto
                    sampleImportDto.DataSource = _dataSourceService.GetDataSourceById(dataSourceId:dataSourceId);

                    if (sampleImportDto.DataSource?.DataSourceId == null)
                    {
                        throw CreateRuleViolationExceptionForValidationError(errorMessage:"Industry did not selected data source.");
                    }

                    //populate Rows in sampleImportDto
                    var importFileWorkbook = GetWorkbook(importTempFileDto:importTempFileDto);

                    sampleImportDto.Rows = GetImportRowObjects(importFileWorkbook:importFileWorkbook, sampleImportDto:sampleImportDto, result:ref result);

                    if (sampleImportDto.Rows.Count == 0)
                    {
                        result.Errors.Add(item:new ErrorWithRowNumberDto {ErrorMessage = "The file has no row to import"});
                    }
                }
                catch (RuleViolationException ruleViolationException)
                {
                    if (importTempFileDto?.ImportTempFileId != null)
                    {
                        RemoveImportTempFile(importTempFileId:importTempFileDto.ImportTempFileId.Value);
                    }

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
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public SampleImportDto PopulateExistingTranslationData(SampleImportDto sampleImportDto)
        {
            var validationIssues = new List<RuleViolation>();
            foreach (var translationType in Enum.GetValues(enumType:typeof(DataSourceTranslationType)).Cast<DataSourceTranslationType>())
            {
                PopulateExistingTranslationData(sampleImportDto:sampleImportDto, translationType:translationType, validationIssues:validationIssues);
            }

            if (validationIssues.Count > 0)
            {
                throw new RuleViolationException(message:"Population Data Source Translation failed", validationIssues:validationIssues);
            }

            return sampleImportDto;
        }

        /// <inheritdoc />
        public List<MissingTranslationDto> GetMissingTranslationSet(SampleImportDto sampleImportDto)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public FileVersionDto GetFileVersion()
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod()))
            {
                var currentRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                var authority = _organizationService.GetAuthority(orgRegProgramId:currentRegulatoryProgramId);

                var fileVersion = _dbContext.FileVersions.FirstOrDefault(i => i.OrganizationRegulatoryProgramId == authority.OrganizationRegulatoryProgramId);

                var fileVersionDto = _mapHelper.ToDto(fromDomainObject:fileVersion);

                if (fileVersion?.LastModificationDateTimeUtc != null)
                {
                    var lastModificationDateTimeUtc = (DateTime) fileVersion?.LastModificationDateTimeUtc?.UtcDateTime;
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
                //using (_dbContext.CreateAutoCommitScope()) //TODO: add proper transaction
                {
                    var currentRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

                    //TODO: Add and update samples

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
                        var reportElementTypeId = Convert.ToInt32(reportElementTypeIdForIndustryFileUpload);
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

                        _fileStoreService.CreateFileStore(fileStoreDto:attachment);
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

        private List<ImportRowObject> GetImportRowObjects(Workbook importFileWorkbook, SampleImportDto sampleImportDto, ref ImportSampleFromFileValidationResultDto result)
        {
            var validationIssues = new List<ErrorWithRowNumberDto>();
            var rows = new List<ImportRowObject>();
            var worksheet = importFileWorkbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                validationIssues.Add(item:new ErrorWithRowNumberDto {ErrorMessage = "The file is empty"});
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
                var requiredColumns = sampleImportDto.FileVersion.FileVersionFields.Where(x => x.DataOptionalityName == DataOptionalityName.Required)
                                                     .Select(x => x.FileVersionFieldName.ToLower())
                                                     .ToList();

                var fileColumnDictionary = new Dictionary<int, FileVersionFieldDto>();
                var fileColumns = new List<string>();
                var isResultQualifierColumnIncluded = false;

                var usedCellRangeWithValues = worksheet.GetUsedCellRange(propertyDefinitions:new IPropertyDefinition[] {CellPropertyDefinitions.ValueProperty});

                if (usedCellRangeWithValues.RowCount == 0)
                {
                    validationIssues.Add(item:new ErrorWithRowNumberDto {ErrorMessage = "The file is empty"});
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
                                _logger.Error(message:$"Column header '{resultAsString}' is not belongs the authority's file template."
                                                      + $" Current OrgRegProgramId:{currentOrgRegProgramId}. DataScource: {sampleImportDto.DataSource.Name} ");
                            }
                        }
                        else
                        {
                            FileVersionFieldDto templateColumn;
                            if (!fileColumnDictionary.TryGetValue(columnIndex, out templateColumn))
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
                                                                      ErrorMessage = $"The length of {templateColumn.FileVersionFieldName} exceeds the maximum"
                                                                                     + $" of {templateColumn.Size}",
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
                                                                      ErrorMessage = $"{templateColumn.FileVersionFieldName} is not numeric",
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
                                                                      ErrorMessage = $"{templateColumn.FileVersionFieldName} is not valid Date",
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
                                                                      ErrorMessage = $"{templateColumn.FileVersionFieldName} is not boolean",
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

                            switch (templateColumn.DataOptionalityName)
                            {
                                //Check each row that any required field is missing or not
                                case DataOptionalityName.Required:

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
                                                                          ErrorMessage = $"{templateColumn.FileVersionFieldName} is required",
                                                                          RowNumbers = importRowObject.RowNumber.ToString()
                                                                      });
                                        }
                                    }

                                    break;

                                case DataOptionalityName.Optional: break;
                                case DataOptionalityName.Recommended:
                                    //TODO: User Story 8199, could reserve cell optinality state to speed up missing default values look up
                                    break;
                                default: throw new NotImplementedException();
                            }

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
                                                                  ErrorMessage = @"Result is required",
                                                                  RowNumbers = importRowObject.RowNumber.ToString()
                                                              });
                                }
                                else if (new List<string> {"ND", "NF"}.Contains(item:resultQualifierColumnValue) && !string.IsNullOrWhiteSpace(value:resultColumnValue))
                                {
                                    validationIssues.Add(item:new ErrorWithRowNumberDto
                                                              {
                                                                  ErrorMessage = $"Result Qualifier {resultQualifierColumnValue} cannot be followed by a value",
                                                                  RowNumbers = importRowObject.RowNumber.ToString()
                                                              });
                                }
                            }
                            else
                            {
                                validationIssues.Add(item:new ErrorWithRowNumberDto
                                                          {
                                                              ErrorMessage = $"Result Qualifier {resultQualifierColumnValue} is not valid",
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

        private static void DoColumnHeaderValidation(IEnumerable<string> requiredColumns, IEnumerable<string> fileColumns, ICollection<ErrorWithRowNumberDto> validationIssues)
        {
            //Check all required columns are exists or not
            var missingRequiredColumns = requiredColumns.Except(second:fileColumns, comparer:StringComparer.OrdinalIgnoreCase).ToList();

            if (missingRequiredColumns.Any())
            {
                validationIssues.Add(item:new ErrorWithRowNumberDto
                                          {
                                              ErrorMessage =
                                                  $"The file does not contain the required column(s) <{string.Join(separator:",", values:missingRequiredColumns.Select(name => name))}>"
                                          });
            }
        }

        private static DateTime? ToDateTime(ICellValue cellValue)
        {
            var rawValueAsNumber = string.IsNullOrWhiteSpace(value:cellValue.RawValue) ? default(double?) : Convert.ToDouble(value:cellValue.RawValue);
            var resultAsDateTime = rawValueAsNumber.HasValue ? FormatHelper.ConvertDoubleToDateTime(doubleValue:rawValueAsNumber.Value)?.Date : default(DateTime?);
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

        private SampleImportDto PopulateExistingTranslationData(SampleImportDto sampleImportDto, DataSourceTranslationType translationType,
                                                                ICollection<RuleViolation> validationIssues)
        {
            if (sampleImportDto.DataSource.DataSourceId == null)
            {
                validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null,
                                                            errorMessage:"Data Source is required to proceed Sample Import"));
                return sampleImportDto;
            }

            var translationDict = _dataSourceService.GetDataSourceTranslationDict(dataSourceId:sampleImportDto.DataSource.DataSourceId.Value,
                                                                                  translationType:translationType);
            var sampleImportColumnName = ToSampleImportColumnName(fromTranslationType:translationType);
            foreach (var row in sampleImportDto.Rows)
            {
                foreach (var cell in row.Cells)
                {
                    if (cell.SampleImportColumnName != sampleImportColumnName)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(value:cell.OriginalValue))
                    {
                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null,
                                                                    errorMessage:$"Value at row {row.RowNumber} and column {sampleImportColumnName} is required"));
                        continue;
                    }

                    DataSourceTranslationItemDto translationItem;
                    if (translationDict.TryGetValue(key:cell.OriginalValue, value:out translationItem))
                    {
                        cell.TranslatedValueId = translationItem.TranslationId;
                        cell.TranslatedValue = translationItem.TranslationName;
                    }
                    else
                    {
                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null,
                                                                    errorMessage:$"Cannot translate '{cell.OriginalValue}' to an existing {translationType}"));
                    }
                }
            }

            return sampleImportDto;
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

        private bool IsImportTempFileWithThisOwnerExist(int importTempFileId, int orgRegProgramId)
        {
            //Also handles scenarios where ImportTempFileId does not exist
            return _dbContext.ImportTempFiles.Any(fs => fs.ImportTempFileId == importTempFileId && fs.OrganizationRegulatoryProgramId == orgRegProgramId);
        }
    }
}