using System;
using System.Collections.Generic;
using System.Data;
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
	public class ImportSampleFromFileService : BaseService, IImportSampleFromFileService
	{
		#region fields

		private readonly LinkoExchangeContext _dbContext;

		private readonly FileStoreService _fileStoreService;
		private readonly IHttpContextService _httpContextService;
		private readonly ILogger _logger;
		private readonly IMapHelper _mapHelper;
		private readonly ITimeZoneService _timeZoneService; 
		private readonly ISettingService _settingService;
		private readonly IUnitService _unitService;
		private readonly IReportTemplateService _reportTemplateService;

		#endregion

		#region constructors and destructor

		public ImportSampleFromFileService(
			FileStoreService fileStoreService,
			LinkoExchangeContext dbContext,
			IMapHelper mapHelper,
			ILogger logger,
			IHttpContextService httpContextService,
			ITimeZoneService timeZoneService,
			ISampleService sampleService,
			IProgramService programService,
			ISettingService settingService,
			IUnitService unitService,
			IReportTemplateService reportTemplateService)
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

			_fileStoreService = fileStoreService;
			_dbContext = dbContext;
			_mapHelper = mapHelper;
			_logger = logger;
			_httpContextService = httpContextService;
			_timeZoneService = timeZoneService;
			_unitService = unitService;
			_reportTemplateService = reportTemplateService;
			_settingService = settingService;
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
					throw new Exception(message:$"ERROR: Unhandled API authorization attempt using name = '{apiName}'");
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

                importTempFileDto = _mapHelper.ToDto(fromDomainObject:importTempFile);

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
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public List<MissingTranslationDto> GetMissingTranslationSet(SampleImportDto sampleImportDto)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public ImportSampleFromFileValidationResultDto DoDataValidation(SampleImportDto sampleImportDto, out List<SampleDto> samplesDtos)
		{
			// Create a list of samples 
			var dataRows = GetImportSampleResultRows(sampleImportDto);

			if (!dataRows[0].ColumnMap.ContainsKey(SampleImportColumnName.LabSampleId))
			{
				foreach (var row in dataRows)
				{
					var fakeCell = new ImportCellObject
					               {
						               OriginalValue = "",
						               TranslatedValue = ""
					               };

					row.ColumnMap.Add(SampleImportColumnName.LabSampleId, fakeCell);
				}
			}
			
			samplesDtos = GetSampleDtosFromRows(dataRows);

			//TODO:  merge samples
			// 1. to determine sample result duplication 
			// 2. parameter result convert
			// 2. to determine if the sample already reported?  readyToReport?  or only Draft
			//        1. if in draft,  to add or update?  --> update
			//        2. if the sample is new --> just save 

			var validationResult = ValidateSamples(samplesDtos);

			return validationResult;
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
		
		private ImportSampleFromFileValidationResultDto ValidateSamples(List<SampleDto> sampleDtos)
		{
			//TODO do validation 
			return null;
		}

		private static SampleResultDto CreateSampleResult(ImportSampleResultRow resultRow)
		{
			if (!resultRow.ColumnMap.ContainsKey(SampleImportColumnName.ParameterName))
			{
				throw new NoNullAllowedException("SampleImportColumnName.TranslatedValueId");
			}

			var sampleResultDto = new SampleResultDto();
			var parameterid = resultRow.ColumnMap[SampleImportColumnName.ParameterName].TranslatedValueId;
			if (parameterid != null)
			{
				sampleResultDto.ParameterId = parameterid.Value;
			}

			sampleResultDto.ParameterName = resultRow.ColumnMap[SampleImportColumnName.ParameterName].TranslatedValue;

			if (resultRow.ColumnMap.ContainsKey(SampleImportColumnName.ResultQualifier))
			{
				sampleResultDto.Qualifier = resultRow.ColumnMap[SampleImportColumnName.ResultQualifier].TranslatedValue;
			}

			if (resultRow.ColumnMap.ContainsKey(SampleImportColumnName.Result))
			{
				sampleResultDto.EnteredValue = resultRow.ColumnMap[SampleImportColumnName.Result].OriginalValue;
				sampleResultDto.Value = resultRow.ColumnMap[SampleImportColumnName.Result].TranslatedValue;
			}

			if (resultRow.ColumnMap.ContainsKey(SampleImportColumnName.ResultUnit))
			{
				sampleResultDto.UnitName = resultRow.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValue;
				var resultUnitId = resultRow.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValueId;
				if (resultUnitId != null)
				{
					sampleResultDto.UnitId = resultUnitId.Value;
				}
			}

			if (resultRow.ColumnMap.ContainsKey(SampleImportColumnName.MethodDetectionLimit))
			{
				sampleResultDto.EnteredMethodDetectionLimit = resultRow.ColumnMap[SampleImportColumnName.MethodDetectionLimit].TranslatedValue.ToString();
				sampleResultDto.MethodDetectionLimit = resultRow.ColumnMap[SampleImportColumnName.MethodDetectionLimit].TranslatedValue;
			}

			if (resultRow.ColumnMap.ContainsKey(SampleImportColumnName.AnalysisMethod))
			{
				sampleResultDto.AnalysisMethod = resultRow.ColumnMap[SampleImportColumnName.AnalysisMethod].TranslatedValue.ToString();
			}

			if (resultRow.ColumnMap.ContainsKey(SampleImportColumnName.AnalysisDateTime))
			{
				sampleResultDto.AnalysisDateTimeLocal = resultRow.ColumnMap[SampleImportColumnName.AnalysisDateTime].TranslatedValue;
			}

			sampleResultDto.IsApprovedEPAMethod = true;
			sampleResultDto.IsCalcMassLoading = false;

			return sampleResultDto;
		}

		private static void CreateMassLoadingSampleResult(ImportSampleResultRow importFlowRow, SampleResultDto sampleResultDto, List<SampleResultDto> sampleResults)
		{
			if (importFlowRow.ColumnMap.ContainsKey(SampleImportColumnName.ResultUnit))
			{
				throw new NoNullAllowedException("Flow ResultUnit is missing");
			}

			if (importFlowRow.ColumnMap.ContainsKey(SampleImportColumnName.Result))
			{
				throw new NoNullAllowedException("Flow Result is missing");
			}

			var mgPerLiter = "mg/l";

			string flowUnitName = importFlowRow.ColumnMap[SampleImportColumnName.ResultUnit].OriginalValue;
			double flowResult = importFlowRow.ColumnMap[SampleImportColumnName.Result].TranslatedValue;

			double massLoadingMultiplier = flowUnitName.Equals(UnitName.pgd.ToString(), StringComparison.OrdinalIgnoreCase) ? 1 : 0.000001;

			var resultUnitConversionFactor = sampleResultDto.UnitName.Equals(mgPerLiter, StringComparison.OrdinalIgnoreCase) ? 1 : 0.001;

			var massLoadingValue = flowResult * sampleResultDto.Value * massLoadingMultiplier * resultUnitConversionFactor;

			var massloadingResult = new SampleResultDto();

			sampleResults.Add(massloadingResult);

			massloadingResult.ParameterId = sampleResultDto.ParameterId;
			massloadingResult.ParameterName = sampleResultDto.ParameterName;

			massloadingResult.Qualifier = sampleResultDto.Qualifier;
			massloadingResult.EnteredValue = sampleResultDto.EnteredValue;

			sampleResultDto.EnteredValue = massLoadingValue.ToString();
			sampleResultDto.Value = massLoadingValue;

			var translatedValueId = importFlowRow.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValueId;
			if (translatedValueId != null)
			{
				int flowUnitId = translatedValueId.Value;
				sampleResultDto.UnitId = flowUnitId;
			}

			sampleResultDto.UnitName = flowUnitName;

			sampleResultDto.EnteredMethodDetectionLimit = sampleResultDto.EnteredMethodDetectionLimit;
			sampleResultDto.MethodDetectionLimit = sampleResultDto.MethodDetectionLimit;

			sampleResultDto.AnalysisMethod = sampleResultDto.AnalysisMethod;

			sampleResultDto.AnalysisDateTimeLocal = sampleResultDto.AnalysisDateTimeLocal;
		}

		private static List<ImportSampleResultRow> GetImportSampleResultRows(SampleImportDto sampleImportDto)
		{
			var dataTable = new List<ImportSampleResultRow>();
			foreach (var row in sampleImportDto.Rows)
			{
				var sampleRow = new ImportSampleResultRow
				{
					ColumnMap = new Dictionary<SampleImportColumnName, ImportCellObject>(),
					RowNumber = row.RowNumber
				};

				foreach (var cell in row.Cells)
				{
					sampleRow.ColumnMap.Add(cell.SampleImportColumnName, cell);
				}
			}

			return dataTable;
		}

		private List<SampleDto> GetSampleDtosFromRows(List<ImportSampleResultRow> dataTable)
		{
			var currentOrganizationRegulatoryProgramId = int.Parse(s: _httpContextService.GetClaimValue(claimType: CacheKey.OrganizationRegulatoryProgramId));
			var programSettings = _settingService.GetProgramSettingsById(orgRegProgramId: _settingService
																			 .GetAuthority(orgRegProgramId: currentOrganizationRegulatoryProgramId).OrganizationRegulatoryProgramId);

			var resultQualifierValidValues = programSettings
				.Settings.Where(s => s.TemplateName.Equals(obj: SettingType.ResultQualifierValidValues)).Select(s => s.Value).First();

			var flowUnitValidValues = _unitService.GetFlowUnitValidValues().ToList();
			var massLoadingConversionFactorPoundsValue = programSettings.Settings.Where(s => s.TemplateName.Equals(obj: SettingType.MassLoadingConversionFactorPounds))
																		.Select(s => s.Value)
																		.FirstOrDefault();

			var massLoadingConversionFactorPounds = string.IsNullOrWhiteSpace(massLoadingConversionFactorPoundsValue)
														? 0.0f
														: double.Parse(massLoadingConversionFactorPoundsValue);

			var isMassLoadingResultToUseLessThanSignStr =
				programSettings.Settings.Where(s => s.TemplateName.Equals(obj: SettingType.MassLoadingResultToUseLessThanSign))
							   .Select(s => s.Value)
							   .First();

			var isMassLoadingResultToUseLessThanSign = string.IsNullOrWhiteSpace(isMassLoadingResultToUseLessThanSignStr) || bool.Parse(isMassLoadingResultToUseLessThanSignStr);


			var massLoadingCalculationDecimalPlacesStr =
				programSettings.Settings.Where(s => s.TemplateName.Equals(obj: SettingType.MassLoadingCalculationDecimalPlaces))
							   .Select(s => s.Value)
							   .First();

			var massLoadingCalculationDecimalPlaces = string.IsNullOrWhiteSpace(massLoadingCalculationDecimalPlacesStr) ? 0 : int.Parse(massLoadingCalculationDecimalPlacesStr);


			var ctsEventTypeDto = _reportTemplateService.GetCtsEventTypes(isForSample: true).FirstOrDefault();
			var ctsEventCategoryName = ctsEventTypeDto != null ? ctsEventTypeDto.CtsEventCategoryName : "sample";

			var flowCell = dataTable.SelectMany(i => i.ColumnMap.Values).ToList()
									.SingleOrDefault(i => i.OriginalValue.Equals("Flow", StringComparison.OrdinalIgnoreCase));

			ImportSampleResultRow importFlowRow = null;
			if (flowCell != null)
			{
				importFlowRow = dataTable.Single(i => i.RowNumber == flowCell.RowNumber);
			}

			var hasFlow = flowCell != null;

			var sampleGroups = dataTable.GroupBy(i => new
			{
				MonitoringPont = i.ColumnMap[SampleImportColumnName.MonitoringPoint].TranslatedValue,
				CollectionMethod = i.ColumnMap[SampleImportColumnName.CollectionMethod].TranslatedValue,
				SampleType = i.ColumnMap[SampleImportColumnName.SampleType].TranslatedValue,
				SampleStartDateTime = i.ColumnMap[SampleImportColumnName.SampleEndDateTime].TranslatedValue,
				SampleEndDateTime = i.ColumnMap[SampleImportColumnName.SampleEndDateTime].TranslatedValue,
				LabSampleId = i.ColumnMap[SampleImportColumnName.LabSampleId].TranslatedValue,
			}, (key, group) => new
			{
				key.MonitoringPont,
				key.CollectionMethod,
				key.SampleType,
				key.SampleStartDateTime,
				key.SampleEndDateTime,
				key.LabSampleId,
				SampleResults = @group.ToList()
			});

			var sampleDtos = new List<SampleDto>();
			foreach (var sampeGroup in sampleGroups)
			{
				var sampleDto = new SampleDto { MonitoringPointName = sampeGroup.MonitoringPont };


				sampleDto.IsReadyToReport = false;
				sampleDto.SampleStatusName = SampleStatusName.Draft;

				sampleDto.FlowUnitId = 1;
				sampleDto.FlowUnitName = "";
				sampleDto.FlowEnteredValue = "";

				sampleDto.MonitoringPointName = sampeGroup.MonitoringPont;
				var monitoringPointId = sampeGroup.SampleResults[0].ColumnMap[SampleImportColumnName.MonitoringPoint].TranslatedValueId;
				if (!monitoringPointId.HasValue)
				{
					throw new NoNullAllowedException("MonitoringPoint.TranslatedValueId");
				}

				sampleDto.MonitoringPointId = monitoringPointId.Value;

				sampleDto.CollectionMethodName = sampeGroup.CollectionMethod;
				var collectionMethodId = sampeGroup.SampleResults[0].ColumnMap[SampleImportColumnName.CollectionMethod].TranslatedValueId;
				if (collectionMethodId == null)
				{
					throw new NoNullAllowedException("CollectionMethod.TranslatedValueId");
				}

				sampleDto.CollectionMethodId = collectionMethodId.Value;
				sampleDto.CtsEventTypeName = sampeGroup.SampleType;
				sampleDto.CtsEventCategoryName = ctsEventCategoryName;

				sampleDto.StartDateTimeLocal = sampeGroup.SampleStartDateTime;
				sampleDto.EndDateTimeLocal = sampeGroup.SampleEndDateTime;

				sampleDto.FlowUnitValidValues = flowUnitValidValues;
				sampleDto.ResultQualifierValidValues = resultQualifierValidValues;
				sampleDto.IsMassLoadingResultToUseLessThanSign = isMassLoadingResultToUseLessThanSign;
				sampleDto.MassLoadingCalculationDecimalPlaces = massLoadingCalculationDecimalPlaces;
				sampleDto.MassLoadingConversionFactorPounds = massLoadingConversionFactorPounds;
				sampleDto.LabSampleIdentifier = sampeGroup.LabSampleId;

				var sampleResults = new List<SampleResultDto>();
				sampleDto.SampleResults = sampleResults;

				// populate sampleResults 
				foreach (var resultRow in sampeGroup.SampleResults)
				{
					var sampleResultDto = CreateSampleResult(resultRow);
					sampleResults.Add(sampleResultDto);

					// calculate mass loading for the sample result 
					if (hasFlow)
					{
						CreateMassLoadingSampleResult(importFlowRow, sampleResultDto, sampleResults);
					}
				}

				sampleDtos.Add(sampleDto);
			}

			return sampleDtos;
		}

	}

	class ImportSampleResultRow
	{
		public int RowNumber { get; set; }  
		public IDictionary<SampleImportColumnName, ImportCellObject> ColumnMap; 
	}
}