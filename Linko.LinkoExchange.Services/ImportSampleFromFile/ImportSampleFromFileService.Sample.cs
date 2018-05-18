using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
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
			samplesDtos = null;

			// Create a list of validate importingSamples, and return the validation result
			List<ImportingSample> importingSamples;

			var validationResult = GetValidImportingSamples(sampleImportDto, out importingSamples);

			if (validationResult.Errors.Any())
			{
				validationResult.Success = false;
				return validationResult;
			}

			validationResult = UpdateEffectiveUnits(importingSamples);

			if (validationResult.Errors.Any())
			{
				validationResult.Success = false;
				return validationResult;
			}

			CreateSamples(importingSamples);

			// Merge into existing draft samples or create new samples
			samplesDtos = MergeSamples(importingSamples);

			return validationResult;
		}

		private ImportSampleFromFileValidationResultDto UpdateEffectiveUnits(List<ImportingSample> importingSamples)
		{
			// for each monitoring point parameter, find out the effective unit. 
			// to determine the sample result parameter unit 
			// 1. If a monitoring point limits exits and in effect in that period,  use it 
			// 2. Use monitoring point parameter default 
			// 3. Use parameter unit 
			var validationResult = new ImportSampleFromFileValidationResultDto();

			var authorityUnits = _unitService.GetUnits();
			var authorityUnitDict = authorityUnits.ToDictionary(unit => unit.UnitId);

			var orgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

			var monitoringPointParameters = _dbContext.MonitoringPointParameters
			                                         .Include(p => p.MonitoringPointParameterLimits)
			                                         .Include(p => p.Parameter)
			                                         .Where(i => i.MonitoringPoint.OrganizationRegulatoryProgramId == orgRegProgramId &&
			                                                     i.MonitoringPoint.IsEnabled && !i.MonitoringPoint.IsRemoved).ToList();

			foreach (var importingSample in importingSamples)
			{
				//TODO : to do unit conversion check 

				int monitoringPointId = importingSample.MonitoringPoint.TranslatedValueId;

				DateTime start = importingSample.SampleEndDateTime.TranslatedValue;
				DateTime end = importingSample.SampleEndDateTime.TranslatedValue;

				foreach (var importingSampleResult in importingSample.SampleResults)
				{
					int parameterId = importingSampleResult.ColumnMap[SampleImportColumnName.ParameterName].TranslatedValueId;
					int unitId = importingSampleResult.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValueId;
					double result = importingSampleResult.ColumnMap[SampleImportColumnName.Result].TranslatedValue;

					importingSampleResult.EffectiveUnit = GetEffectiveUnit(monitoringPointParameters, monitoringPointId, parameterId, start, end);

					var fromUnit = authorityUnitDict[unitId];
					importingSampleResult.EffectiveUnitResult = _unitService.ConvertResultToTargetUnit(result, fromUnit, importingSampleResult.EffectiveUnit);
				}
			}

			return validationResult;
		}

		private UnitDto GetEffectiveUnit(List<MonitoringPointParameter> pool, int monitoringPointId, int parameterId, DateTime start, DateTime end)
		{
			var smallPool = pool.Where(i => i.ParameterId == parameterId && i.MonitoringPointId == monitoringPointId).ToList(); 

			foreach (var mp in smallPool)
			{
				if (mp.EffectiveDateTime > start || mp.RetirementDateTime < end)
				{
					continue;
				}

				var mppl = mp.MonitoringPointParameterLimits.FirstOrDefault();

				if (mppl?.BaseUnit != null)
				{
					return _mapHelper.ToDto(mppl.BaseUnit);
				}
				else if (mp.DefaultUnit != null)
				{
					return _mapHelper.ToDto(mp.DefaultUnit);
				}
				else
				{
					return _mapHelper.ToDto(mp.Parameter.DefaultUnit);
				}
			}

			return _mapHelper.ToDto(smallPool.First().Parameter.DefaultUnit) ;
		}

		private void ExtractFlowRow(ImportingSample importingSample)
		{
			var flow = importingSample.SampleResults.SingleOrDefault(i => i.ParameterName.Equals("Flow", StringComparison.OrdinalIgnoreCase));
			if (flow == null)
			{
				return;
			}

			importingSample.SampleResults.Remove(flow);
			importingSample.FlowRow = new FlowRow
			                          {
				                          FlowValue = flow.ColumnMap[SampleImportColumnName.Result].TranslatedValue,
				                          FlowUnitName = flow.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValue,
				                          FlowUnitId = flow.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValueId
			                          };

		}

		private List<SampleDto> MergeSamples(List<ImportingSample> importingSamples)
		{
			var sampleDtos = new List<SampleDto>();

			var min = importingSamples.Min(i => i.Sample.StartDateTimeLocal);
			var max = importingSamples.Min(i => i.Sample.EndDateTimeLocal);

			// Get existing draft samples for that user;
			var draftSamples = _sampleService.GetSamples(status:SampleStatusName.Draft, startDate:min, endDate:max, isIncludeChildObjects:true).ToList();


			foreach (var importingSample in importingSamples)
			{
				// Sample results are in a draft sample, then update that draft
				var drftSamplesToUpdate = SearchSamplesInCategorySamples(draftSamples, importingSample.Sample);
				if (drftSamplesToUpdate.Any())
				{
					var importingSampleResultParameterIds = importingSample.Sample.SampleResults.Select(k => k.ParameterId);

					//Update all the drafts
					foreach (var draftSample in drftSamplesToUpdate)
					{
						// update sample results that are in draft
						var draftSampleResultsToUpdate = draftSample.SampleResults.Where(i => importingSampleResultParameterIds.Contains(i.ParameterId)).ToList();
						foreach (var sampleResultToUpdate in draftSampleResultsToUpdate)
						{
							var importingSampleResult = importingSample.Sample.SampleResults.Single(i => i.ParameterId == sampleResultToUpdate.ParameterId);
							UpdateDraftSampleResult(sampleResultToUpdate, importingSampleResult);
						}

						// add the rest of sample results in importing sample result to this draft
						var draftSampleResultsToAdd = draftSample.SampleResults.Except(draftSampleResultsToUpdate);
						draftSample.SampleResults.ToList().AddRange(draftSampleResultsToAdd);

						// recalculate all mass loading in draftSample
						if (importingSample.FlowRow != null || draftSample.FlowValue.HasValue)
						{
							FlowRow flowRow = importingSample.FlowRow ?? new FlowRow
							                                             {
								                                             FlowUnitId = draftSample.FlowUnitId.Value,
								                                             FlowUnitName = draftSample.FlowUnitName,
								                                             FlowValue = draftSample.FlowValue ?? 0.0
							                                             };

							// samples exist in draft, but not exist in importing results, calculate mass loading 
							var draftSampleResultsToRecalculateMass = draftSample.SampleResults.Except(draftSampleResultsToUpdate);
							foreach (var sampleResultToUpdate in draftSampleResultsToRecalculateMass)
							{
								CreateMassLoadingSampleResult(flowRow, sampleResultToUpdate);
							}
						}
					}

					sampleDtos.AddRange(drftSamplesToUpdate);
				}
				else // this sample is a new sample
				{
					sampleDtos.Add(importingSample.Sample);
				} 
			}

			return sampleDtos;
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

		private SampleResultDto CreateSampleResult(ImportSampleResultRow resultRow)
		{
			var sampleResultDto = new SampleResultDto();
			if (resultRow.ColumnMap.ContainsKey(SampleImportColumnName.ParameterName))
			{
				sampleResultDto.ParameterId = resultRow.ColumnMap[SampleImportColumnName.ParameterName].TranslatedValueId;
				sampleResultDto.ParameterName = resultRow.ColumnMap[SampleImportColumnName.ParameterName].TranslatedValue;
			}
			
			if (resultRow.ColumnMap.ContainsKey(SampleImportColumnName.ResultQualifier))
			{
				sampleResultDto.Qualifier = resultRow.ColumnMap[SampleImportColumnName.ResultQualifier].TranslatedValue;
			}
			
			sampleResultDto.UnitName = resultRow.EffectiveUnit.Name;
			sampleResultDto.UnitId = resultRow.EffectiveUnit.UnitId; 
			sampleResultDto.Value = resultRow.EffectiveUnitResult;

			sampleResultDto.EnteredValue = resultRow.EffectiveUnitResult.ToString();
			
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

		private void CreateMassLoadingSampleResult(FlowRow importFlowRow, SampleResultDto sampleResultDto)
		{
			if (importFlowRow == null)
			{
				return;
			}
			
			var mgPerLiter = "mg/l";

			string flowUnitName = importFlowRow.FlowUnitName;
			double flowResult = importFlowRow.FlowValue;
			var massloadingUnitId = importFlowRow.FlowUnitId; 

			double massLoadingMultiplier = flowUnitName.Equals(UnitName.pgd.ToString(), StringComparison.OrdinalIgnoreCase) ? 1 : 0.000001;
			var resultUnitConversionFactor = sampleResultDto.UnitName.Equals(mgPerLiter, StringComparison.OrdinalIgnoreCase) ? 1 : 0.001;
			var massLoadingValue = flowResult * sampleResultDto.Value * massLoadingMultiplier * resultUnitConversionFactor;
			 
			sampleResultDto.MassLoadingValue = massLoadingValue.ToString();
			sampleResultDto.MassLoadingQualifier = sampleResultDto.Qualifier; 
			
			sampleResultDto.MassLoadingUnitId = massloadingUnitId;
			sampleResultDto.MassLoadingUnitName = flowUnitName;
		}

		private ImportSampleFromFileValidationResultDto GetValidImportingSamples(SampleImportDto sampleImportDto, out List<ImportingSample> importingSamples)
		{
			ImportSampleFromFileValidationResultDto validationResult = new ImportSampleFromFileValidationResultDto();

			importingSamples = CreateImportingSamples(sampleImportDto);

			foreach (var importingSample in importingSamples)
			{
				ValidateDataDuplication(importingSample, validationResult);

				ExtractFlowRow(importingSample);
			}

			return validationResult;
		}

		private static void ValidateDataDuplication(ImportingSample importingSample, ImportSampleFromFileValidationResultDto validationResult)
		{
			var parameterGroups = importingSample.SampleResults.GroupBy(i => i.ParameterName);

			foreach (var parameterGroup in parameterGroups)
			{
				// Check if sample has duplicate parameters 
				if (parameterGroup.Count() > 1)
				{
					var errorMessage = "Duplicated parameter";
					var isFlow = parameterGroup.Key.Equals("Flow", StringComparison.OrdinalIgnoreCase);
					if (isFlow)
					{
						errorMessage = "Duplicate flows exist, A sample has more than 1 parameter representing the Sample Flow used in calculating mass. One must be removed to do the import.";
					}

					validationResult.Success = false;
					validationResult.Errors.Add(item:new ErrorWithRowNumberDto
					                                 {
						                                 ErrorMessage = errorMessage,
						                                 RowNumbers = string.Join(separator:",", values:parameterGroup.ToList().Select(i => i.RowNumber.ToString()))
					                                 });
				}
			}
		}

		private List<ImportingSample> CreateImportingSamples(SampleImportDto sampleImportDto)
		{
			var dataTable = new List<ImportSampleResultRow>();

			foreach (var row in sampleImportDto.Rows)
			{
				var sampleRow = new ImportSampleResultRow
				                {
					                ColumnMap = new Dictionary<SampleImportColumnName, ImportCellObject>(),
					                RowNumber = row.RowNumber
				                };

				dataTable.Add(sampleRow);

				foreach (var cell in row.Cells)
				{
					sampleRow.ColumnMap.Add(cell.SampleImportColumnName, cell);
				}
			}

			if (dataTable.Any() && dataTable[0].ColumnMap.ContainsKey(SampleImportColumnName.LabSampleId))
			{
				foreach (var row in dataTable)
				{
					var fakeCell = new ImportCellObject
					               {
						               TranslatedValue = ""
					               };

					row.ColumnMap.Add(SampleImportColumnName.LabSampleId, fakeCell);
				}
			}

			//Group into samples 
			return dataTable.GroupBy(i => new
			                              {
				                              MonitoringPont = i.ColumnMap[SampleImportColumnName.MonitoringPoint],
				                              CollectionMethod = i.ColumnMap[SampleImportColumnName.CollectionMethod],
				                              SampleType = i.ColumnMap[SampleImportColumnName.SampleType],
				                              SampleStartDateTime = i.ColumnMap[SampleImportColumnName.SampleEndDateTime],
				                              SampleEndDateTime = i.ColumnMap[SampleImportColumnName.SampleEndDateTime],
				                              LabSampleId = i.ColumnMap[SampleImportColumnName.LabSampleId],
			                              }, (key, group) => new ImportingSample
			                                                 {
				                                                 MonitoringPoint = key.MonitoringPont,
				                                                 CollectionMethod = key.CollectionMethod,
				                                                 SampleType = key.SampleType,
				                                                 SampleStartDateTime = key.SampleStartDateTime,
				                                                 SampleEndDateTime = key.SampleEndDateTime,
				                                                 LabSampleId = key.LabSampleId,
				                                                 SampleResults = @group.ToList()
			                                                 }).ToList();
		}

		private void CreateSamples(List<ImportingSample> importingSamples)
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


			foreach (var importingSample in importingSamples)
			{
				var sampleDto = new SampleDto
				                {
					                MonitoringPointName = importingSample.MonitoringPoint.TranslatedValue,
					                MonitoringPointId = importingSample.MonitoringPoint.TranslatedValueId,
					                IsReadyToReport = false,
					                SampleStatusName = SampleStatusName.Draft,
					                FlowUnitId = importingSample.FlowRow?.FlowUnitId,
					                FlowUnitName = importingSample.FlowRow != null ? importingSample.FlowRow.FlowUnitName : "",
					                FlowEnteredValue = importingSample.FlowRow?.FlowValue.ToString(CultureInfo.InvariantCulture),
					                FlowValue = importingSample.FlowRow?.FlowValue,
					                CtsEventTypeId = importingSample.SampleType.TranslatedValueId,
					                CtsEventTypeName = importingSample.SampleType.TranslatedValue,
					                CtsEventCategoryName = ctsEventCategoryName,
					                CollectionMethodId = importingSample.CollectionMethod.TranslatedValueId,
					                CollectionMethodName = importingSample.CollectionMethod.TranslatedValue,
					                LabSampleIdentifier = importingSample.LabSampleId.TranslatedValue,
					                StartDateTimeLocal = importingSample.SampleStartDateTime.TranslatedValue,
					                EndDateTimeLocal = importingSample.SampleEndDateTime.TranslatedValue,
					                FlowUnitValidValues = flowUnitValidValues,
					                ResultQualifierValidValues = resultQualifierValidValues,
					                IsMassLoadingResultToUseLessThanSign = isMassLoadingResultToUseLessThanSign,
					                MassLoadingCalculationDecimalPlaces = massLoadingCalculationDecimalPlaces,
					                MassLoadingConversionFactorPounds = massLoadingConversionFactorPounds
				                };

				var sampleResults = new List<SampleResultDto>();
				sampleDto.SampleResults = sampleResults;

				importingSample.Sample = sampleDto;

				// populate sampleResults 
				foreach (var importSampleResultRow in importingSample.SampleResults)
				{
					var sampleResultDto = CreateSampleResult(importSampleResultRow);
					sampleResults.Add(sampleResultDto);

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
		public UnitDto EffectiveUnit { get; set; }
		public double? EffectiveUnitResult { get; set; }
	}

	class ImportingSample
	{
		public ImportCellObject MonitoringPoint { get; set; }
		public ImportCellObject CollectionMethod { get; set; }
		public ImportCellObject SampleType { get; set; }
		public ImportCellObject SampleStartDateTime { get; set; }
		public ImportCellObject SampleEndDateTime { get; set; }
		public ImportCellObject LabSampleId { get; set; }
		public FlowRow FlowRow { get; set; }
		public List<ImportSampleResultRow> SampleResults { get; set; }
		public SampleDto Sample { get; set; }
	}

	class FlowRow
	{
		public int FlowUnitId { get; set; }
		public string FlowUnitName { get; set; }
		public double FlowValue { get; set; } 
	}
}