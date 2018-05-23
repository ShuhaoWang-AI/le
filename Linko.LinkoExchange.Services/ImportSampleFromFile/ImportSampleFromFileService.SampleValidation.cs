using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.ImportSampleFromFile
{
	public partial class ImportSampleFromFileService
	{
		#region fields

		private IDictionary<int, UnitDto> _authorityUnitDict;
		private List<MonitoringPointParameter> _monitoringPointParameters;

		#endregion

		#region interface implementations

		/// <inheritdoc />
		public ImportSampleFromFileValidationResultDto DoDataValidation(SampleImportDto sampleImportDto, out List<SampleDto> samplesDtos)
		{
			samplesDtos = null;
			// Create a list of validate importingSamples, and return the validation results
			ImportSampleFromFileValidationResultDto validationResult;  
			
			using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(), descripition:$"DoDataValidation"))
			{
				List<ImportingSample> importingSamples;
				validationResult = GetValidImportingSamples(sampleImportDto:sampleImportDto, importingSamples:out importingSamples);

				if (validationResult.Errors.Any())
				{
					validationResult.Success = false;
					return validationResult;
				}

				CreateSamples(importingSamples:importingSamples);

				// Merge into existing draft samples or create new samples
				samplesDtos = MergeSamples(importingSamples:importingSamples);
			}

			return validationResult;
		}

		#endregion

		private void ResolveEffectiveUnits(ImportingSample importingSample, ImportSampleFromFileValidationResultDto validationResult)
		{
			// for each monitoring point parameter, find out the effective unit. 
			// 1. If a monitoring point limits exits and in effect in that period,  use it 
			// 2. Use monitoring point parameter default 
			// 3. Use parameter unit  

			if (_authorityUnitDict == null)
			{
				var authorityUnits = _unitService.GetUnits();
				_authorityUnitDict = authorityUnits.ToDictionary(unit => unit.UnitId);
			}

			var monitoringPointId = importingSample.MonitoringPoint.TranslatedValueId;

			DateTime start = importingSample.SampleEndDateTime.TranslatedValue;
			DateTime end = importingSample.SampleEndDateTime.TranslatedValue;

			foreach (var importingSampleResult in importingSample.SampleResults)
			{
				var parameterId = importingSampleResult.ColumnMap[key:SampleImportColumnName.ParameterName].TranslatedValueId;
				var unitId = importingSampleResult.ColumnMap[key:SampleImportColumnName.ResultUnit].TranslatedValueId;

				if (monitoringPointId <= 0 || parameterId <= 0 || unitId <= 0)
				{
					var errorMessage = "Invalid System unit translation defined by the Authority. Contact your Authority.";
					AddValidationError(validationResult:validationResult, errorMessage:errorMessage, rowNumber:importingSampleResult.RowNumber);
				}

				double result = importingSampleResult.ColumnMap[key:SampleImportColumnName.Result].TranslatedValue;

				importingSampleResult.EffectiveUnit = GetEffectiveUnit(monitoringPointId:monitoringPointId, parameterId:parameterId, start:start, end:end);

				if (importingSampleResult.EffectiveUnit == null)
				{
					var errorMessage = "Invalid System unit translation defined by the Authority. Contact your Authority.";
					AddValidationError(validationResult:validationResult, errorMessage:errorMessage, rowNumber:importingSampleResult.RowNumber);
				}

				var fromUnit = _authorityUnitDict[key:unitId];
				importingSampleResult.EffectiveUnitResult = _unitService.ConvertResultToTargetUnit(result:result, currentAuthorityUnit:fromUnit,
				                                                                                   targetAuthorityUnit:importingSampleResult.EffectiveUnit);
			}
		}

		private UnitDto GetEffectiveUnit(int monitoringPointId, int parameterId, DateTime start, DateTime end)
		{
			if (_monitoringPointParameters == null)
			{
				var orgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
				_monitoringPointParameters = _dbContext.MonitoringPointParameters
				                                       .Include(p => p.MonitoringPointParameterLimits)
				                                       .Include(p => p.Parameter)
				                                       .Where(i => i.MonitoringPoint.OrganizationRegulatoryProgramId == orgRegProgramId &&
				                                                   i.MonitoringPoint.IsEnabled && !i.MonitoringPoint.IsRemoved).ToList();
			}

			var monitoringParameters = _monitoringPointParameters.Where(i => i.ParameterId == parameterId && i.MonitoringPointId == monitoringPointId).ToList();

			foreach (var mp in monitoringParameters)
			{
				if (mp.EffectiveDateTime > start || mp.RetirementDateTime < end)
				{
					continue;
				}

				var mppl = mp.MonitoringPointParameterLimits.FirstOrDefault();

				if (mppl?.BaseUnit != null)
				{
					return _mapHelper.ToDto(fromDomainObject:mppl.BaseUnit);
				}
				else if (mp.DefaultUnit != null)
				{
					return _mapHelper.ToDto(fromDomainObject:mp.DefaultUnit);
				}
				else
				{
					return _mapHelper.ToDto(fromDomainObject:mp.Parameter.DefaultUnit);
				}
			}

			return _mapHelper.ToDto(fromDomainObject:monitoringParameters.First().Parameter.DefaultUnit);
		}

		private void ExtractFlowRow(ImportingSample importingSample, ImportSampleFromFileValidationResultDto validationResult)
		{
			var flow = importingSample.SampleResults.SingleOrDefault(i => i.ParameterName.Equals(value:"Flow", comparisonType:StringComparison.OrdinalIgnoreCase));
			if (flow == null)
			{
				return;
			}

			// If there is a Flow, but FlowValue or FlowUnit does not exist, throw exception. 
			double flowValue = flow.ColumnMap[key:SampleImportColumnName.Result].TranslatedValue;
			string flowUnitName = flow.ColumnMap[key:SampleImportColumnName.ResultUnit].TranslatedValue;
			var flowUnitId = flow.ColumnMap[key:SampleImportColumnName.ResultUnit].TranslatedValueId;

			if (flowValue <= 0.0)
			{
				var errorMessage = "Missing flow value.";
				AddValidationError(validationResult:validationResult, errorMessage:errorMessage, rowNumber:flow.RowNumber);
			}

			if (flowUnitId <= 0)
			{
				var errorMessage = "Missing flow unit.";
				AddValidationError(validationResult:validationResult, errorMessage:errorMessage, rowNumber:flow.RowNumber);
			}

			if (!flowUnitName.Equals(value:UnitName.pgd.ToString(), comparisonType:StringComparison.OrdinalIgnoreCase) &&
			    !flowUnitName.Equals(value:UnitName.mgd.ToString(), comparisonType:StringComparison.OrdinalIgnoreCase))
			{
				var errorMessage = "Invalid Flow unit for Mass Loadings calculations. Chosen unit must be gpd or mgd.";
				AddValidationError(validationResult:validationResult, errorMessage:errorMessage, rowNumber:flow.RowNumber);
			}

			importingSample.SampleResults.Remove(item:flow);
			importingSample.FlowRow = new FlowRow
			                          {
				                          FlowValue = flowValue,
				                          FlowUnitName = flowUnitName,
				                          FlowUnitId = flowUnitId
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
				var drftSamplesToUpdate = SearchSamplesInCategorySamples(searchIn:draftSamples, searchFor:importingSample.Sample);
				if (drftSamplesToUpdate.Any())
				{
					var importingSampleResultParameterIds = importingSample.Sample.SampleResults.Select(k => k.ParameterId);

					//Update all the drafts
					foreach (var draftSample in drftSamplesToUpdate)
					{
						// update sample results that are in draft
						var draftSampleResultsToUpdate = draftSample.SampleResults.Where(i => importingSampleResultParameterIds.Contains(value:i.ParameterId)).ToList();
						foreach (var sampleResultToUpdate in draftSampleResultsToUpdate)
						{
							var importingSampleResult = importingSample.Sample.SampleResults.Single(i => i.ParameterId == sampleResultToUpdate.ParameterId);
							UpdateDraftSampleResult(sampleResultToUpdate:sampleResultToUpdate, importingSampleResult:importingSampleResult);
						}

						// add the rest of sample results in importing sample result to this draft
						var draftSampleResultsToAdd = draftSample.SampleResults.Except(second:draftSampleResultsToUpdate);
						draftSample.SampleResults.ToList().AddRange(collection:draftSampleResultsToAdd);

						// re-calculate all mass loadings 
						if (importingSample.FlowRow != null || draftSample.FlowValue.HasValue)
						{
							// validationResult
							var flowRow = importingSample.FlowRow ?? new FlowRow
							                                         {
								                                         FlowUnitId = draftSample.FlowUnitId ?? 0,
								                                         FlowUnitName = draftSample.FlowUnitName,
								                                         FlowValue = draftSample.FlowValue ?? 0.0
							                                         };
							if (flowRow.FlowUnitId < 1)
							{
								continue;
							}

							// re-calculate mass loadings for all sample results
							foreach (var sampleResult in draftSample.SampleResults)
							{
								CreateMassLoadingSampleResult(importFlowRow:flowRow, sampleResultDto:sampleResult);
							}
						}
					}

					sampleDtos.AddRange(collection:drftSamplesToUpdate);
				}
				else // this sample is a new sample
				{
					sampleDtos.Add(item:importingSample.Sample);
				}
			}

			return sampleDtos;
		}

		private List<SampleDto> SearchSamplesInCategorySamples(List<SampleDto> searchIn, SampleDto searchFor)
		{
			return searchIn.Where(i => i.MonitoringPointId == searchFor.MonitoringPointId &&
			                           i.CtsEventTypeId == searchFor.CtsEventTypeId &&
			                           i.CollectionMethodId == searchFor.CollectionMethodId &&
			                           i.StartDateTimeLocal == searchFor.StartDateTimeLocal &&
			                           i.EndDateTimeLocal == searchFor.EndDateTimeLocal &&
			                           (string.IsNullOrWhiteSpace(value:i.LabSampleIdentifier) && string.IsNullOrWhiteSpace(value:searchFor.LabSampleIdentifier) ||
			                            !string.IsNullOrWhiteSpace(value:i.LabSampleIdentifier) && !string.IsNullOrWhiteSpace(value:searchFor.LabSampleIdentifier) &&
			                            i.LabSampleIdentifier.Equals(value:searchFor.LabSampleIdentifier, comparisonType:StringComparison.OrdinalIgnoreCase)
			                           )
			                     ).ToList();
		}

		private SampleResultDto CreateSampleResult(ImportSampleResultRow resultRow, bool isCalculatingMassLoading)
		{
			var sampleResultDto = new SampleResultDto
			                      {
				                      ParameterId = resultRow.ColumnMap[key:SampleImportColumnName.ParameterName].TranslatedValueId,
				                      ParameterName = resultRow.ColumnMap[key:SampleImportColumnName.ParameterName].TranslatedValue,
				                      UnitName = resultRow.EffectiveUnit.Name,
				                      UnitId = resultRow.EffectiveUnit.UnitId,
				                      Value = resultRow.EffectiveUnitResult,
				                      EnteredValue = resultRow.EffectiveUnitResult.ToString()
			                      };

			if (resultRow.ColumnMap.ContainsKey(key:SampleImportColumnName.ResultQualifier))
			{
				sampleResultDto.Qualifier = resultRow.ColumnMap[key:SampleImportColumnName.ResultQualifier].TranslatedValue;
			}

			if (resultRow.ColumnMap.ContainsKey(key:SampleImportColumnName.MethodDetectionLimit))
			{
				sampleResultDto.EnteredMethodDetectionLimit = resultRow.ColumnMap[key:SampleImportColumnName.MethodDetectionLimit].TranslatedValue.ToString();
				sampleResultDto.MethodDetectionLimit = resultRow.ColumnMap[key:SampleImportColumnName.MethodDetectionLimit].TranslatedValue;
			}

			if (resultRow.ColumnMap.ContainsKey(key:SampleImportColumnName.AnalysisMethod))
			{
				sampleResultDto.AnalysisMethod = resultRow.ColumnMap[key:SampleImportColumnName.AnalysisMethod].TranslatedValue.ToString();
			}

			if (resultRow.ColumnMap.ContainsKey(key:SampleImportColumnName.AnalysisDateTime))
			{
				sampleResultDto.AnalysisDateTimeLocal = resultRow.ColumnMap[key:SampleImportColumnName.AnalysisDateTime].TranslatedValue;
			}

			sampleResultDto.IsApprovedEPAMethod = true;
			sampleResultDto.IsCalcMassLoading = isCalculatingMassLoading;

			return sampleResultDto;
		}

		private void CreateMassLoadingSampleResult(FlowRow importFlowRow, SampleResultDto sampleResultDto)
		{
			if (importFlowRow == null)
			{
				return;
			}

			var mgPerLiter = "mg/l";

			var flowUnitName = importFlowRow.FlowUnitName;
			var flowResult = importFlowRow.FlowValue;
			var massloadingUnitId = importFlowRow.FlowUnitId;

			var massLoadingMultiplier = flowUnitName.Equals(value:UnitName.pgd.ToString(), comparisonType:StringComparison.OrdinalIgnoreCase) ? 1 : 0.000001;
			var resultUnitConversionFactor = sampleResultDto.UnitName.Equals(value:mgPerLiter, comparisonType:StringComparison.OrdinalIgnoreCase) ? 1 : 0.001;
			var massLoadingValue = flowResult * sampleResultDto.Value * massLoadingMultiplier * resultUnitConversionFactor;

			sampleResultDto.MassLoadingValue = massLoadingValue.ToString();
			sampleResultDto.MassLoadingQualifier = sampleResultDto.Qualifier;

			sampleResultDto.MassLoadingUnitId = massloadingUnitId;
			sampleResultDto.MassLoadingUnitName = flowUnitName;
		}

		private ImportSampleFromFileValidationResultDto GetValidImportingSamples(SampleImportDto sampleImportDto, out List<ImportingSample> importingSamples)
		{
			var validationResult = new ImportSampleFromFileValidationResultDto();

			importingSamples = CreateImportingSamples(sampleImportDto:sampleImportDto);

			foreach (var importingSample in importingSamples)
			{
				// check row duplication for each importingSample
				ValidateDataDuplicatedParameters(importingSample:importingSample, validationResult:validationResult);

				// Update the effective Unit for each row
				ResolveEffectiveUnits(importingSample:importingSample, validationResult:validationResult);

				// Handle flow row if there is a flow row
				ExtractFlowRow(importingSample:importingSample, validationResult:validationResult);
			}

			return validationResult;
		}

		private static void ValidateDataDuplicatedParameters(ImportingSample importingSample, ImportSampleFromFileValidationResultDto validationResult)
		{
			var parameterGroups = importingSample.SampleResults.GroupBy(i => i.ParameterName);

			foreach (var parameterGroup in parameterGroups)
			{
				// Check if sample has duplicate parameters 
				if (parameterGroup.Count() > 1)
				{
					var errorMessage = "Duplicate parameters exist";
					var isFlow = parameterGroup.Key.Equals(value:"Flow", comparisonType:StringComparison.OrdinalIgnoreCase);
					if (isFlow)
					{
						errorMessage = "Duplicate flows exist";
					}

					validationResult.Success = false;
					if (validationResult.Errors.Any(i => i.ErrorMessage.Equals(value:errorMessage)))
					{
						var error = validationResult.Errors.Single(i => i.ErrorMessage == errorMessage);
						error.RowNumbers = string.Join(separator:",", values:parameterGroup.ToList().Select(i => i.RowNumber.ToString()));
					}
					else
					{
						validationResult.Errors.Add(item:new ErrorWithRowNumberDto
						                                 {
							                                 ErrorMessage = errorMessage,
							                                 RowNumbers = string.Join(separator:",", values:parameterGroup.ToList().Select(i => i.RowNumber.ToString()))
						                                 });
					}
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

				dataTable.Add(item:sampleRow);

				foreach (var cell in row.Cells)
				{
					sampleRow.ColumnMap.Add(key:cell.SampleImportColumnName, value:cell);
				}
			}

			if (dataTable.Any() && !dataTable[index:0].ColumnMap.ContainsKey(key:SampleImportColumnName.LabSampleId))
			{
				foreach (var row in dataTable)
				{
					var fakeCell = new ImportCellObject
					               {
						               TranslatedValue = ""
					               };

					row.ColumnMap.Add(key:SampleImportColumnName.LabSampleId, value:fakeCell);
				}
			}

			//Group into samples 
			return dataTable.GroupBy(i => new
			                              {
				                              MonitoringPont = i.ColumnMap[key:SampleImportColumnName.MonitoringPoint],
				                              CollectionMethod = i.ColumnMap[key:SampleImportColumnName.CollectionMethod],
				                              SampleType = i.ColumnMap[key:SampleImportColumnName.SampleType],
				                              SampleStartDateTime = i.ColumnMap[key:SampleImportColumnName.SampleEndDateTime],
				                              SampleEndDateTime = i.ColumnMap[key:SampleImportColumnName.SampleEndDateTime],
				                              LabSampleId = i.ColumnMap[key:SampleImportColumnName.LabSampleId]
			                              }, (key, group) => new ImportingSample
			                                                 {
				                                                 MonitoringPoint = key.MonitoringPont,
				                                                 CollectionMethod = key.CollectionMethod,
				                                                 SampleType = key.SampleType,
				                                                 SampleStartDateTime = key.SampleStartDateTime,
				                                                 SampleEndDateTime = key.SampleEndDateTime,
				                                                 LabSampleId = key.LabSampleId,
				                                                 SampleResults = group.ToList()
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

			var massLoadingConversionFactorPounds = string.IsNullOrWhiteSpace(value:massLoadingConversionFactorPoundsValue)
				                                        ? 0.0f
				                                        : double.Parse(s:massLoadingConversionFactorPoundsValue);

			var isMassLoadingResultToUseLessThanSignStr =
				programSettings.Settings.Where(s => s.TemplateName.Equals(obj:SettingType.MassLoadingResultToUseLessThanSign))
				               .Select(s => s.Value)
				               .First();

			var isMassLoadingResultToUseLessThanSign = string.IsNullOrWhiteSpace(value:isMassLoadingResultToUseLessThanSignStr) || bool.Parse(value:isMassLoadingResultToUseLessThanSignStr);

			var massLoadingCalculationDecimalPlacesStr =
				programSettings.Settings.Where(s => s.TemplateName.Equals(obj:SettingType.MassLoadingCalculationDecimalPlaces))
				               .Select(s => s.Value)
				               .First();

			var massLoadingCalculationDecimalPlaces = string.IsNullOrWhiteSpace(value:massLoadingCalculationDecimalPlacesStr) ? 0 : int.Parse(s:massLoadingCalculationDecimalPlacesStr);

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
					                FlowEnteredValue = importingSample.FlowRow?.FlowValue.ToString(provider:CultureInfo.InvariantCulture),
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

				var isCalculationMassloading = importingSample.FlowRow != null;

				// populate sampleResults 
				foreach (var importSampleResultRow in importingSample.SampleResults)
				{
					var sampleResultDto = CreateSampleResult(resultRow:importSampleResultRow, isCalculatingMassLoading:isCalculationMassloading);
					sampleResults.Add(item:sampleResultDto);
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

		private static void AddValidationError(ImportSampleFromFileValidationResultDto validationResult, string errorMessage, int rowNumber)
		{
			var error = validationResult.Errors.SingleOrDefault(i => i.ErrorMessage == errorMessage);
			if (error == null)
			{
				error = new ErrorWithRowNumberDto
				        {
					        ErrorMessage = errorMessage,
					        RowNumbers = $"{rowNumber}"
				        };

				validationResult.Errors.Add(item:error);
			}
			else
			{
				error.RowNumbers = $"{error.RowNumbers}, {rowNumber}";
			}
		}
	}

	internal class ImportSampleResultRow
	{
		#region fields

		public IDictionary<SampleImportColumnName, ImportCellObject> ColumnMap;

		#endregion

		#region public properties

		public int RowNumber { get; set; }
		public string ParameterName { get; set; }
		public UnitDto EffectiveUnit { get; set; }
		public double? EffectiveUnitResult { get; set; }

		#endregion
	}

	internal class ImportingSample
	{
		#region public properties

		public ImportCellObject MonitoringPoint { get; set; }
		public ImportCellObject CollectionMethod { get; set; }
		public ImportCellObject SampleType { get; set; }
		public ImportCellObject SampleStartDateTime { get; set; }
		public ImportCellObject SampleEndDateTime { get; set; }
		public ImportCellObject LabSampleId { get; set; }
		public FlowRow FlowRow { get; set; }
		public List<ImportSampleResultRow> SampleResults { get; set; }
		public SampleDto Sample { get; set; }

		#endregion
	}

	internal class FlowRow
	{
		#region public properties

		public int FlowUnitId { get; set; }
		public string FlowUnitName { get; set; }
		public double FlowValue { get; set; }

		#endregion
	}
}