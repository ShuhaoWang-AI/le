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
using Linko.LinkoExchange.Services.DataSource;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.FileStore;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Sample;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.Unit;
using NLog;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Windows.Documents.Spreadsheet.Model;

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
		private readonly IReportTemplateService _reportTemplateService;
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
				default:
					throw new NotSupportedException(message:$"ERROR: Unhandled API authorization attempt using name = '{apiName}'");
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

					sampleImportDto.Rows = GetImportRowObjects(importFileWorkbook:importFileWorkbook, fileVersion:sampleImportDto.FileVersion);

					if (sampleImportDto.Rows.Count == 0)
					{
						throw CreateRuleViolationExceptionForValidationError(errorMessage:"The file is empty.");
					}
				}
				catch (RuleViolationException ruleViolationException)
				{
					if (importTempFileDto?.ImportTempFileId != null)
					{
						RemoveImportTempFile(importTempFileId:importTempFileDto.ImportTempFileId.Value);
					}

					result.Success = false;
					result.Errors = ruleViolationException.ValidationIssues?.Select(x => new ErrorWithRowNumberDto {ErrorMessage = x.ErrorMessage, RowNumbers = ""}).ToList();
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

		private List<ImportRowObject> GetImportRowObjects(Workbook importFileWorkbook, FileVersionDto fileVersion)
		{
			var validationIssues = new List<RuleViolation>();
			var rows = new List<ImportRowObject>();

			//Find column header from first row and map with FileVersionFieldDto

			//Check all required and recommended columns are exists or not

			//loop through the rows
			//Check each row that any required field is missing or not
			//Validate each field has correct data format or not
			//Convert workbook.cell to ImportCellObject
			//Validate text field length
			//Validate date format
			//validate result field with qualifier

			// check there is any validation issue or not
			if (validationIssues.Count > 0)
			{
				throw new RuleViolationException(message:"File validation failed", validationIssues:validationIssues);
			}

			return rows;
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
				case DataSourceTranslationType.MonitoringPoint:
					return SampleImportColumnName.MonitoringPoint;
				case DataSourceTranslationType.SampleType:
					return SampleImportColumnName.SampleType;
				case DataSourceTranslationType.CollectionMethod:
					return SampleImportColumnName.CollectionMethod;
				case DataSourceTranslationType.Parameter:
					return SampleImportColumnName.ParameterName;
				case DataSourceTranslationType.Unit:
					return SampleImportColumnName.ResultUnit;
				default:
					throw new NotSupportedException(message:$"DataSourceTranslationType {fromTranslationType} is unsupported");
			}
		}

		private bool IsImportTempFileWithThisOwnerExist(int importTempFileId, int orgRegProgramId)
		{
			//Also handles scenarios where ImportTempFileId does not exist
			return _dbContext.ImportTempFiles.Any(fs => fs.ImportTempFileId == importTempFileId && fs.OrganizationRegulatoryProgramId == orgRegProgramId);
		}
	}
}