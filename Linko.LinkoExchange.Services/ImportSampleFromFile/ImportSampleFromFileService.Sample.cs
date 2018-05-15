using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.ImportSampleFromFile
{
	public partial class ImportSampleFromFileService
	{
		/// <inheritdoc />
		public ImportSampleFromFileValidationResultDto DoDataValidation(SampleImportDto sampleImportDto, out List<SampleDto> samplesDtos)
		{
			// Create a list of samples 
			var importSampleResultRows = GetImportSampleResultRows(sampleImportDto);

			if (!importSampleResultRows[0].ColumnMap.ContainsKey(SampleImportColumnName.LabSampleId))
			{
				foreach (var row in importSampleResultRows)
				{
					var fakeCell = new ImportCellObject
					               {
						               OriginalValue = "",
						               TranslatedValue = ""
					               };

					row.ColumnMap.Add(SampleImportColumnName.LabSampleId, fakeCell);
				}
			}

			samplesDtos = GetSampleDtosFromRows(importSampleResultRows);

			//TODO:  merge samples
			// 1. to determine sample result duplication 
			// 2. parameter result convert
			// 2. to determine if the sample already reported?  readyToReport?  or only Draft
			//        1. if in draft,  to add or update?  --> update
			//        2. if the sample is new --> just save 

			var validationResult = ValidateSamples(importSampleResultRows);

			return validationResult;
		}

		private ImportSampleFromFileValidationResultDto ValidateSamples(List<ImportSampleResultRow> importSampleResultRows)
		{
			ImportSampleFromFileValidationResultDto validationResult = new ImportSampleFromFileValidationResultDto();

			//To check each sample has duplicated rows 
			var sampleGroups = importSampleResultRows.GroupBy(i => i.Sample, (key, group) =>
			                                                                 new {
				                                                                 Sample = key, 
																				 SampleResults = group.ToList()
			                                                                 });

			// Check if sample has duplicate parameters 
			foreach (var sampleGroup in sampleGroups)
			{	
				var parameterGroup = sampleGroup.SampleResults.GroupBy(i => i.ParameterName);

				foreach (var pg in parameterGroup)
				{
					if (pg.Count() > 1)
					{
						if (validationResult.Errors == null)
						{
							validationResult.Errors = new List<ErrorWithRowNumberDto>();
							validationResult.Success = false;
						}

						validationResult.Errors.Add(new ErrorWithRowNumberDto
						                            {
							                            ErrorMessage = "Duplicated parameter",
							                            RowNumbers = string.Join(",", pg.ToList().Select(i => i.RowNumber.ToString()))
						                            });
					}
				}
			}

			// Check if sample is already reported,  or readyToReport 

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
			var currentOrganizationRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
			var programSettings = _settingService.GetProgramSettingsById(orgRegProgramId:_settingService
				                                                             .GetAuthority(orgRegProgramId:currentOrganizationRegulatoryProgramId).OrganizationRegulatoryProgramId);

			var resultQualifierValidValues = programSettings
				.Settings.Where(s => s.TemplateName.Equals(obj:SettingType.ResultQualifierValidValues)).Select(s => s.Value).First();

			var flowUnitValidValues = _unitService.GetFlowUnitValidValues().ToList();
			var massLoadingConversionFactorPoundsValue = programSettings.Settings.Where(s => s.TemplateName.Equals(obj:SettingType.MassLoadingConversionFactorPounds))
			                                                            .Select(s => s.Value)
			                                                            .FirstOrDefault();

			var massLoadingConversionFactorPounds = string.IsNullOrWhiteSpace(massLoadingConversionFactorPoundsValue)
				                                        ? 0.0f
				                                        : double.Parse(massLoadingConversionFactorPoundsValue);

			var isMassLoadingResultToUseLessThanSignStr =
				programSettings.Settings.Where(s => s.TemplateName.Equals(obj:SettingType.MassLoadingResultToUseLessThanSign))
				               .Select(s => s.Value)
				               .First();

			var isMassLoadingResultToUseLessThanSign = string.IsNullOrWhiteSpace(isMassLoadingResultToUseLessThanSignStr) || bool.Parse(isMassLoadingResultToUseLessThanSignStr);


			var massLoadingCalculationDecimalPlacesStr =
				programSettings.Settings.Where(s => s.TemplateName.Equals(obj:SettingType.MassLoadingCalculationDecimalPlaces))
				               .Select(s => s.Value)
				               .First();

			var massLoadingCalculationDecimalPlaces = string.IsNullOrWhiteSpace(massLoadingCalculationDecimalPlacesStr) ? 0 : int.Parse(massLoadingCalculationDecimalPlacesStr);

			var ctsEventTypeDto = _reportTemplateService.GetCtsEventTypes(isForSample:true).FirstOrDefault();
			var ctsEventCategoryName = ctsEventTypeDto != null ? ctsEventTypeDto.CtsEventCategoryName : "sample";

			var flowCell = dataTable.SelectMany(i => i.ColumnMap.Values).ToList()
			                        .SingleOrDefault(i => i.OriginalValue.Equals("Flow", StringComparison.OrdinalIgnoreCase));

			ImportSampleResultRow importFlowRow = null;
			int flowUnitId = 0;
			string flowUnitName = string.Empty;
			string flowValueEntered = string.Empty;
			double flowValue = 0.0;

			if (flowCell != null)
			{
				importFlowRow = dataTable.Single(i => i.RowNumber == flowCell.RowNumber);
				var flowUnitIdTranslated = importFlowRow.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValueId;
				if (flowUnitIdTranslated != null)
				{
					flowUnitId = flowUnitIdTranslated.Value;
				}

				flowUnitName = importFlowRow.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValue;
				flowValue = importFlowRow.ColumnMap[SampleImportColumnName.Result].TranslatedValue;
				flowValueEntered = importFlowRow.ColumnMap[SampleImportColumnName.Result].TranslatedValue.toString();
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
				var sampleDto = new SampleDto
				                {
					                MonitoringPointName = sampeGroup.MonitoringPont,
					                IsReadyToReport = false,
					                SampleStatusName = SampleStatusName.Draft,
					                FlowUnitId = flowUnitId,
					                FlowUnitName = flowUnitName,
					                FlowEnteredValue = flowValueEntered,
					                FlowValue = flowValue,
					                CtsEventTypeName = sampeGroup.SampleType,
					                CtsEventCategoryName = ctsEventCategoryName,
					                StartDateTimeLocal = sampeGroup.SampleStartDateTime,
					                EndDateTimeLocal = sampeGroup.SampleEndDateTime,
					                FlowUnitValidValues = flowUnitValidValues,
					                ResultQualifierValidValues = resultQualifierValidValues,
					                IsMassLoadingResultToUseLessThanSign = isMassLoadingResultToUseLessThanSign,
					                MassLoadingCalculationDecimalPlaces = massLoadingCalculationDecimalPlaces,
					                MassLoadingConversionFactorPounds = massLoadingConversionFactorPounds,
					                LabSampleIdentifier = sampeGroup.LabSampleId
				                };


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

				var sampleResults = new List<SampleResultDto>();
				sampleDto.SampleResults = sampleResults;

				// populate sampleResults 
				foreach (var importSampleResultRow in sampeGroup.SampleResults)
				{
					var sampleResultDto = CreateSampleResult(importSampleResultRow);
					sampleResults.Add(sampleResultDto);

					// calculate mass loading for the sample result 
					if (hasFlow)
					{
						CreateMassLoadingSampleResult(importFlowRow, sampleResultDto, sampleResults);
					}

					// Set up mapping between sample and sample result ImportSampleResultRow object. 
					importSampleResultRow.Sample = sampleDto;
					importSampleResultRow.SampleResult = sampleResultDto;
					importSampleResultRow.ParameterName = sampleResultDto.ParameterName;
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
		public SampleDto Sample { get; set; }
		public SampleResultDto SampleResult { get; set; }
		public string ParameterName { get; set; }
	}
}