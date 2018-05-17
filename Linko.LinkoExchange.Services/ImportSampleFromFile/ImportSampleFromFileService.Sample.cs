using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

			CategorizeImportSampleResultRows(importSampleResultRows);

			var validationResult = ValidSampleResultRows(importSampleResultRows, out samplesDtos);
			if (validationResult.Success == false)
			{
				return validationResult;
			}

			// TODO. unit conversion

			// 

			return validationResult;
		}

		private ImportSampleFromFileValidationResultDto ValidSampleResultRows(List<ImportSampleResultRow> importSampleResultRows, out List<SampleDto> sampleDtos)
		{
			ImportSampleFromFileValidationResultDto validationResult = new ImportSampleFromFileValidationResultDto();
			sampleDtos = new List<SampleDto>(); 

			var min = importSampleResultRows.Min(i => i.Sample.StartDateTimeLocal);
			var max = importSampleResultRows.Min(i => i.Sample.EndDateTimeLocal);

			// Get existing draft samples for that user;
			var draftSamples = _sampleService.GetSamples(status:SampleStatusName.Draft, startDate:min, endDate:max, isIncludeChildObjects:true).ToList();
			
			//To check each sample has duplicated rows 
			var importingSampleGroups = importSampleResultRows.GroupBy(i => i.Sample, (key, group) =>
				                                                                          new
				                                                                          {
					                                                                          Sample = key,
					                                                                          SampleResultRows = group.ToList()
				                                                                          });

			foreach (var importingSampleGroup in importingSampleGroups)
			{
				var parameterGroup = importingSampleGroup.SampleResultRows.GroupBy(i => i.ParameterName);

				foreach (var pg in parameterGroup)
				{
					// Check if sample has duplicate parameters 
					if (pg.Count() > 1)
					{
						validationResult.Success = false;
						validationResult.Errors.Add(item:new ErrorWithRowNumberDto
						                                 {
							                                 ErrorMessage = "Duplicated parameter",
							                                 RowNumbers = string.Join(separator:",", values:pg.ToList().Select(i => i.RowNumber.ToString()))
						                                 });
					}
				}

				var importingSample = importingSampleGroup.Sample;
			 
				// Sample results are in a draft sample, then update that draft
				var drftSamplesToUpdate = SearchSamplesInCategorySamples(draftSamples, importingSample);
				if (drftSamplesToUpdate.Any())
				{
					var importingSampleResultParameterIds = importingSampleGroup.SampleResultRows.Select(j => j.SampleResult).Select(k => k.ParameterId);

					//Update all the drafts
					foreach (var draftSample in drftSamplesToUpdate)
					{
						var draftSampleResultsToUpdate = draftSample.SampleResults.Where(i => importingSampleResultParameterIds.Contains(i.ParameterId)).ToList();
						foreach (var sampleResultToUpdate in draftSampleResultsToUpdate)
						{
							var importingSampleResult = importingSample.SampleResults.Single(i => i.ParameterId == sampleResultToUpdate.ParameterId);
							UpdateDraftSampleResult(sampleResultToUpdate, importingSampleResult);
						}

						// add the rest of sample results in importing sample result to this draft
						var draftSampleResultsToAdd = draftSample.SampleResults.Except(draftSampleResultsToUpdate);
						draftSample.SampleResults.ToList().AddRange(draftSampleResultsToAdd);
					}

					sampleDtos.AddRange(drftSamplesToUpdate);
				}
				else
				{
					sampleDtos.Add(importingSample);
				}
			}

			return validationResult;
		}
 
		List<SampleDto> SearchSamplesInCategorySamples(List<SampleDto> searchIn, SampleDto searchFor)
		{
			return searchIn.Where(i => i.MonitoringPointId == searchFor.MonitoringPointId &&
			                           i.CtsEventTypeId == searchFor.CtsEventTypeId &&
			                           i.CollectionMethodId == searchFor.CollectionMethodId &&
			                           i.StartDateTimeLocal == searchFor.StartDateTimeLocal &&
			                           i.EndDateTimeLocal == searchFor.EndDateTimeLocal &&
			                           (string.IsNullOrWhiteSpace(i.LabSampleIdentifier) && string.IsNullOrWhiteSpace(searchFor.LabSampleIdentifier) ||
			                            !string.IsNullOrWhiteSpace(i.LabSampleIdentifier) && !string.IsNullOrWhiteSpace(searchFor.LabSampleIdentifier) &&
			                            i.LabSampleIdentifier.Equals(searchFor.LabSampleIdentifier, StringComparison.OrdinalIgnoreCase)
			                           )
			                     ).ToList();
		}

		List<ErrorWithRowNumberDto> GetSampleResultErrors(List<string> rowNumbers, string message)
		{
			return rowNumbers.Select(i => new ErrorWithRowNumberDto
			                              {
				                              ErrorMessage = message,
				                              RowNumbers = string.Join(separator:",", values:rowNumbers)
			                              }).ToList();
		}

		private SampleResultDto CreateSampleResult(ImportSampleResultRow resultRow)
		{
			var sampleResultDto = new SampleResultDto();
			if (resultRow.ColumnMap.ContainsKey(SampleImportColumnName.ParameterName))
			{
				var parameterId = resultRow.ColumnMap[SampleImportColumnName.ParameterName].TranslatedValueId;
				if (parameterId != null)
				{
					sampleResultDto.ParameterId = parameterId.Value;
				}

				sampleResultDto.ParameterName = resultRow.ColumnMap[SampleImportColumnName.ParameterName].TranslatedValue;
			}
			
			if (resultRow.ColumnMap.ContainsKey(SampleImportColumnName.ResultQualifier))
			{
				sampleResultDto.Qualifier = resultRow.ColumnMap[SampleImportColumnName.ResultQualifier].TranslatedValue;
			}

			if (resultRow.ColumnMap.ContainsKey(SampleImportColumnName.Result))
			{
				sampleResultDto.Value = resultRow.ColumnMap[SampleImportColumnName.Result].TranslatedValue;
				sampleResultDto.EnteredValue = sampleResultDto.Value.ToString(); 
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

		private void CreateMassLoadingSampleResult(ImportSampleResultRow importFlowRow, SampleResultDto sampleResultDto)
		{
			if (!importFlowRow.ColumnMap.ContainsKey(SampleImportColumnName.ResultUnit))
			{
				throw new NoNullAllowedException("Flow ResultUnit is missing");
			}

			if (!importFlowRow.ColumnMap.ContainsKey(SampleImportColumnName.Result))
			{
				throw new NoNullAllowedException("Flow Result is missing");
			}

			var mgPerLiter = "mg/l";

			string flowUnitName = importFlowRow.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValue;
			double flowResult = importFlowRow.ColumnMap[SampleImportColumnName.Result].TranslatedValue;

			double massLoadingMultiplier = flowUnitName.Equals(UnitName.pgd.ToString(), StringComparison.OrdinalIgnoreCase) ? 1 : 0.000001;

			var resultUnitConversionFactor = sampleResultDto.UnitName.Equals(mgPerLiter, StringComparison.OrdinalIgnoreCase) ? 1 : 0.001;

			var massLoadingValue = flowResult * sampleResultDto.Value * massLoadingMultiplier * resultUnitConversionFactor;
			 
			sampleResultDto.MassLoadingValue = massLoadingValue.ToString();
			sampleResultDto.MassLoadingQualifier = sampleResultDto.Qualifier; 

			var massloadingUnitId = importFlowRow.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValueId;
			if (massloadingUnitId != null)
			{
				sampleResultDto.MassLoadingUnitId = massloadingUnitId.Value; 
			}
				
			sampleResultDto.MassLoadingUnitName = importFlowRow.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValue;
		}

		private List<ImportSampleResultRow> GetImportSampleResultRows(SampleImportDto sampleImportDto)
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

			if (!dataTable.Any() || dataTable[0].ColumnMap.ContainsKey(SampleImportColumnName.LabSampleId))
			{
				return dataTable;
			}

			foreach (var row in dataTable)
			{
				var fakeCell = new ImportCellObject
				               {
					               OriginalValue = "",
					               TranslatedValue = ""
				               };

				row.ColumnMap.Add(SampleImportColumnName.LabSampleId, fakeCell);
			}

			return dataTable;
		}

		private void CategorizeImportSampleResultRows(List<ImportSampleResultRow> dataTable)
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
			var flowUnitId = 0;
			var flowUnitName = string.Empty;
			var flowValueEntered = string.Empty;
			var flowValue = 0.0;

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
						CreateMassLoadingSampleResult(importFlowRow, sampleResultDto);
					}

					// Set up mapping between sample and sample result ImportSampleResultRow object. 
					importSampleResultRow.Sample = sampleDto;
					importSampleResultRow.SampleResult = sampleResultDto;
					importSampleResultRow.ParameterName = sampleResultDto.ParameterName;
				} 
			}
		}

		private void UpdateDraftSampleResult(SampleResultDto sampleResultToUpdate, SampleResultDto importingSampleResult)
		{
			sampleResultToUpdate.Qualifier = importingSampleResult.Qualifier;
			sampleResultToUpdate.EnteredValue = importingSampleResult.EnteredValue;
			sampleResultToUpdate.Value = importingSampleResult.Value;
			sampleResultToUpdate.UnitId = importingSampleResult.UnitId;
			sampleResultToUpdate.UnitName = importingSampleResult.UnitName;
			sampleResultToUpdate.EnteredMethodDetectionLimit = importingSampleResult.EnteredMethodDetectionLimit;
			sampleResultToUpdate.MethodDetectionLimit = importingSampleResult.MethodDetectionLimit;
			sampleResultToUpdate.AnalysisMethod = importingSampleResult.AnalysisMethod;
			sampleResultToUpdate.AnalysisDateTimeLocal = importingSampleResult.AnalysisDateTimeLocal;
			sampleResultToUpdate.IsApprovedEPAMethod = importingSampleResult.IsApprovedEPAMethod;
			sampleResultToUpdate.IsCalcMassLoading = importingSampleResult.IsCalcMassLoading;
			sampleResultToUpdate.LastModificationDateTimeLocal = importingSampleResult.LastModificationDateTimeLocal;

			// Update mass loading properties
			sampleResultToUpdate.MassLoadingValue = importingSampleResult.MassLoadingValue;
			sampleResultToUpdate.MassLoadingQualifier = importingSampleResult.MassLoadingQualifier;
			sampleResultToUpdate.MassLoadingUnitId = importingSampleResult.MassLoadingUnitId;
			sampleResultToUpdate.MassLoadingUnitName = importingSampleResult.MassLoadingUnitName;
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