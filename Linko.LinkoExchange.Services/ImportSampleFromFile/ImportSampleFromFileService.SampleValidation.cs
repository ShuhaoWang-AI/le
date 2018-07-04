using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.ImportSampleFromFile
{
	public partial class ImportSampleFromFileService
	{
		#region fields

		private List<Core.Domain.Parameter> _authorityParameters;
		private IDictionary<int, UnitDto> _authorityUnitDict;
		private Core.Domain.Parameter _massLoadingFlowParameter;
		private List<MonitoringPointParameter> _monitoringPointParameters;

		#endregion

		#region interface implementations

		/// <inheritdoc />
		public ImportSampleFromFileValidationResultDto DoDataValidation(SampleImportDto sampleImportDto)
		{
			// Create a list of validate importSampleWrappers, and return the validation results
			ImportSampleFromFileValidationResultDto validationResult;

			using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(), description:$"DoDataValidation"))
			{
				List<ImportSampleWrapper> importingSamples;
				validationResult = GetValidImportingSamples(sampleImportDto:sampleImportDto, groupedSampleWrappers:out importingSamples);

				if (validationResult.Errors.Any())
				{
					return validationResult;
				}

				PopulateSamples(importSampleWrappers:importingSamples);

				// Merge into existing draft samples or create new samples
				sampleImportDto.SampleDtos = MergeSamples(importingSamples:importingSamples);
			}

			return validationResult;
		}

		#endregion

		private void ResolveEffectiveUnits(ImportSampleWrapper importSampleWrapper, ImportSampleFromFileValidationResultDto validationResult)
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

			var monitoringPointId = importSampleWrapper.MonitoringPoint.TranslatedValueId;

			DateTime start = importSampleWrapper.SampleEndDateTime.TranslatedValue;
			DateTime end = importSampleWrapper.SampleEndDateTime.TranslatedValue;

			foreach (var importingSampleResult in importSampleWrapper.SampleResults)
			{
				var parameterId = importingSampleResult.ColumnMap[key:SampleImportColumnName.ParameterName].TranslatedValueId;
				var unitId = importingSampleResult.ColumnMap[key:SampleImportColumnName.ResultUnit].TranslatedValueId;

				if (monitoringPointId <= 0)
				{
					AddValidationError(validationResult:validationResult, errorMessage:ErrorConstants.SampleImport.DefaultMonitoringPointIsRequired, rowNumber:importingSampleResult.RowNumber);
				}

				if (parameterId <= 0)
				{
					AddValidationError(validationResult:validationResult, errorMessage:ErrorConstants.SampleImport.DefaultCollectionMethodIsRequired, rowNumber:importingSampleResult.RowNumber);
				}

				if (unitId <= 0)
				{
					AddValidationError(validationResult:validationResult, errorMessage:ErrorConstants.SampleImport.DefaultSampleTypeIsRequired, rowNumber:importingSampleResult.RowNumber);
				}

				double? result = importingSampleResult.ColumnMap[key:SampleImportColumnName.Result].TranslatedValue;

				if (_massLoadingFlowParameter == null)
				{
					_massLoadingFlowParameter = _parameterService.GetFlowParameter();
				}

				if (importingSampleResult.ParameterName.Equals(_massLoadingFlowParameter.Name))
				{
					var validMassFlowUnits = _unitService.GetFlowUnitValidValues().ToList();
					var validMassFlowUnitIds = validMassFlowUnits.Select(i => i.UnitId).ToList(); 

					if (importingSampleResult.ColumnMap.ContainsKey(SampleImportColumnName.ResultUnit))
					{
						if (validMassFlowUnitIds.Contains(importingSampleResult.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValueId))
						{
							importingSampleResult.EffectiveUnitResult = result;
							importingSampleResult.EffectiveUnit = new UnitDto
							                                      {
								                                      UnitId = importingSampleResult.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValueId,
								                                      Name = importingSampleResult.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValue 
							                                      };
							continue;
						}

						if (validMassFlowUnits.Count > 1 && validMassFlowUnitIds.Contains(importingSampleResult.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValueId))
						{
							importingSampleResult.EffectiveUnitResult = result;
							var index = validMassFlowUnitIds.IndexOf(importingSampleResult.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValueId); 
							importingSampleResult.EffectiveUnit = validMassFlowUnits[index];

							continue;
						}
					}
				}

				importingSampleResult.EffectiveUnit = GetEffectiveUnit(monitoringPointId:monitoringPointId, parameterId:parameterId, start:start, end:end);

				var fromUnit = _authorityUnitDict[key:unitId];
				var targetUnit = importingSampleResult.EffectiveUnit;

				if (targetUnit == null)
				{
					AddValidationError(validationResult: validationResult, errorMessage: ErrorConstants.SampleImport.DataValidation.TranslatedUnitDoesNotSupportUnitConversion, rowNumber: importingSampleResult.RowNumber);
				}
				else if (fromUnit.UnitId != targetUnit.UnitId)
				{
					try
					{
						importingSampleResult.EffectiveUnitResult = _unitService.ConvertResultToTargetUnit(result:result, currentAuthorityUnit:fromUnit,
						                                                                                   targetAuthorityUnit:targetUnit);
					}
					catch (RuleViolationException ex)
					{
						_logger.Warn(message:ex.GetFirstErrorMessage());
						AddValidationError(validationResult:validationResult,
						                   errorMessage:string.Format(format:ErrorConstants.SampleImport.DataValidation.TranslatedUnitDoesNotSupportUnitConversion,
						                                              arg0:fromUnit.Name, arg1:targetUnit.Name),
						                   rowNumber:importingSampleResult.RowNumber);
					} 
				}
				else
				{
					importingSampleResult.EffectiveUnitResult = result;
				}
			}
		}

		private UnitDto GetEffectiveUnit(int monitoringPointId, int parameterId, DateTime start, DateTime end)
		{
			var orgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
			if (_monitoringPointParameters == null)
			{
				_monitoringPointParameters = _dbContext.MonitoringPointParameters
				                                       .Include(p => p.MonitoringPointParameterLimits)
				                                       .Include(p => p.Parameter)
				                                       .Where(i => i.MonitoringPoint.OrganizationRegulatoryProgramId == orgRegProgramId
				                                                   && i.MonitoringPoint.IsEnabled
				                                                   && !i.MonitoringPoint.IsRemoved).ToList();
			}

			var monitoringParameters = _monitoringPointParameters.Where(i => i.ParameterId == parameterId && i.MonitoringPointId == monitoringPointId).ToList();

			if (monitoringParameters.Count > 0)
			{
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
			else
			{
				if (_authorityParameters == null)
				{
					var authOrgRegProgramId = _organizationService.GetAuthority(orgRegProgramId:orgRegProgramId).OrganizationRegulatoryProgramId;
					_authorityParameters = _dbContext.Parameters
					                                 .Include(p => p.DefaultUnit)
					                                 .Where(p => p.OrganizationRegulatoryProgramId == authOrgRegProgramId && !p.IsRemoved).ToList();
				}

				var parameter = _authorityParameters.FirstOrDefault(param => param.ParameterId == parameterId);
				if (parameter != null)
				{
					return _mapHelper.ToDto(fromDomainObject:parameter.DefaultUnit);
				}
				else
				{
					throw CreateRuleViolationExceptionForValidationError(errorMessage:ErrorConstants.SampleImport.DataValidation.ParameterUnitIsUnspecified);
				}
			}
		}

		private void ExtractFlowRow(ImportSampleWrapper importSampleWrapper, ImportSampleFromFileValidationResultDto validationResult)
		{
			if (_massLoadingFlowParameter == null)
			{
				_massLoadingFlowParameter = _parameterService.GetFlowParameter();
			}

			var flow = importSampleWrapper.SampleResults.FirstOrDefault(i => i.ParameterName.Equals(value:_massLoadingFlowParameter.Name, comparisonType:StringComparison.OrdinalIgnoreCase));
			if (flow == null)
			{
				return;
			}

			string qualifier = flow.ColumnMap[key:SampleImportColumnName.ResultQualifier].TranslatedValue;
			if (!string.IsNullOrWhiteSpace(value:qualifier))
			{
				if (qualifier.Equals(value:"ND", comparisonType:StringComparison.OrdinalIgnoreCase) || qualifier.Equals(value:"NF", comparisonType:StringComparison.OrdinalIgnoreCase))
				{
					if (!string.IsNullOrWhiteSpace(flow.ColumnMap[key:SampleImportColumnName.ResultQualifier].OriginalValueString))
					{
						AddValidationError(validationResult:validationResult, errorMessage:ErrorConstants.SampleImport.DataValidation.FlowResultQualifierMustBeEmpty, rowNumber:flow.RowNumber);
					}

					if (string.IsNullOrWhiteSpace(flow.ColumnMap[key:SampleImportColumnName.Result].OriginalValueString))
					{
						AddValidationError(validationResult:validationResult, errorMessage:ErrorConstants.SampleImport.DataValidation.FlowValueIsInvalid, rowNumber:flow.RowNumber);
					}

					return;
				}
			}

			// If there is a Flow, but FlowValue or FlowUnit does not exist, throw exception. 
			if (!flow.EffectiveUnitResult.HasValue  || flow.EffectiveUnitResult <= 0.0)
			{
				AddValidationError(validationResult:validationResult, errorMessage:ErrorConstants.SampleImport.DataValidation.FlowValueIsInvalid, rowNumber:flow.RowNumber);
			}

			if (flow.EffectiveUnit.UnitId <= 0)
			{
				AddValidationError(validationResult:validationResult, errorMessage:ErrorConstants.SampleImport.DataValidation.FlowUnitIsUnSpecified, rowNumber:flow.RowNumber);
			}

			if (!string.IsNullOrWhiteSpace(value:flow.ColumnMap[key:SampleImportColumnName.ResultQualifier].OriginalValueString))
			{
				AddValidationError(validationResult:validationResult, errorMessage:ErrorConstants.SampleImport.DataValidation.FlowResultQualifierMustBeEmpty, rowNumber:flow.RowNumber);
			}

			var validFlowUnits = _unitService.GetFlowUnitValidValues().ToList();
			var validFlowUnitIds = validFlowUnits.Select(s => s.UnitId).ToList();
			var validFlowUnitNames = validFlowUnits.Select(i => i.Name).ToList();
			if (validFlowUnits.Count >1 && !validFlowUnitIds.Contains(item: flow.EffectiveUnit.UnitId))
			{
				var unitsStr = "";
				if (validFlowUnitIds.Count > 2)
				{
					unitsStr = string.Join(separator:", ", values: validFlowUnitNames);
					var lastPosition = unitsStr.LastIndexOf(value:",", comparisonType:StringComparison.OrdinalIgnoreCase);
					unitsStr = unitsStr.Substring(startIndex:0, length:lastPosition) + " or" + unitsStr.Substring(startIndex:lastPosition + 1);
				}
				else
				{
					unitsStr = string.Join(separator:" or ", values: validFlowUnitNames);
				}

				AddValidationError(validationResult:validationResult,
				                   errorMessage:string.Format(format:ErrorConstants.SampleImport.DataValidation.FlowUnitIsInvalidOnMassLoadingCalculation, arg0:unitsStr),
				                   rowNumber:flow.RowNumber);
			}
			else if (validFlowUnits.Count == 1 && validFlowUnits[0].UnitId != flow.EffectiveUnit.UnitId)
			{
				// need to do unit conversion again
				var newEffectiveUnit = validFlowUnits[0]; 
				flow.EffectiveUnitResult = _unitService.ConvertResultToTargetUnit(result:flow.EffectiveUnitResult, currentAuthorityUnit:flow.EffectiveUnit,targetAuthorityUnit: newEffectiveUnit);
				flow.EffectiveUnit = newEffectiveUnit;
			}

			importSampleWrapper.SampleResults.Remove(item:flow);
			importSampleWrapper.FlowRow = new FlowRow
			                              {
				                              FlowValue = flow.EffectiveUnitResult ?? 0.0f,
				                              FlowUnitName = flow.EffectiveUnit.Name,
				                              FlowUnitId = flow.EffectiveUnit.UnitId
			                              };
		}

		private List<SampleDto> MergeSamples(List<ImportSampleWrapper> importingSamples)
		{
			var sampleDtos = new List<SampleDto>();

			var min = importingSamples.Min(i => i.Sample.StartDateTimeLocal);
			var max = importingSamples.Max(i => i.Sample.EndDateTimeLocal);

			var massLoadingUnit = _unitService.GetUnitForMassLoadingCalculations();

			// Get existing draft samples for that user;
			var existingDraftSamples = _sampleService.GetSamples(status:SampleStatusName.Draft, startDate:min, endDate:max, isIncludeChildObjects:true).ToList();

			foreach (var importingSample in importingSamples)
			{
				var importingSampleResults = importingSample.Sample.SampleResults.ToList(); 
				// Sample results are in an existing draft sample, then update that draft
				var draftSamplesToUpdate = SearchSamplesInCategorySamples(searchIn:existingDraftSamples, searchFor:importingSample.Sample);
				if (draftSamplesToUpdate.Any())
				{
					var importingSampleResultParameterIds = importingSampleResults.Select(k => k.ParameterId);

					//Update all the drafts
					foreach (var draftSample in draftSamplesToUpdate)
					{
						// Parameters only exist in existing draft. 
						var existingResultsOtherFromImport = draftSample.SampleResults.Where(i => !importingSampleResultParameterIds.Contains(value:i.ParameterId)).ToList();
						var draftSampleParameterIds = draftSample.SampleResults.Select(i => i.ParameterId).ToList();

                        // Mark sample results that only exist in existing draft as "Existing and Unchanged"
                        // If there is no flow in importing file,
						foreach (var existingResultOtherFromImport in existingResultsOtherFromImport)
						{
                            if (importingSample.FlowRow == null)
							{
								existingResultOtherFromImport.ExistingUnchanged = true;
							}
							else if (!existingResultOtherFromImport.Value.HasValue)
							{
							 	existingResultOtherFromImport.ExistingUnchanged = true;
                            }
						}

						var copyOfImportingSampleResults = DeepCopy<List<SampleResultDto>>(item: importingSampleResults);
						var commonParameters = copyOfImportingSampleResults.Where(i => draftSampleParameterIds.Contains(item: i.ParameterId)).ToList();
						var draftSampleResultDict = draftSample.SampleResults.ToDictionary(i => i.ParameterId, i => i.ConcentrationSampleResultId);

						foreach (var parameter in commonParameters)
						{
							parameter.ConcentrationSampleResultId = draftSampleResultDict[key:parameter.ParameterId];
						}

						var resultSamples = copyOfImportingSampleResults;
						resultSamples.AddRange(collection:existingResultsOtherFromImport);

						draftSample.SampleResults = resultSamples;

						// re-calculate all mass loadings 
						if (importingSample.FlowRow != null || draftSample.FlowValue.HasValue)
						{
							// validationResult
							var flowRow = importingSample.FlowRow
							              ?? new FlowRow
							                 {
								                 FlowUnitId = draftSample.FlowUnitId ?? 0,
								                 FlowUnitName = draftSample.FlowUnitName,
								                 FlowValue = draftSample.FlowValue ?? 0.0
										  };
							if (flowRow.FlowUnitId < 1)
							{
								continue;
							}

							draftSample.FlowUnitId = flowRow.FlowUnitId;
							draftSample.FlowValue = flowRow.FlowValue;
							draftSample.FlowUnitName = flowRow.FlowUnitName;
							draftSample.FlowEnteredValue = flowRow.FlowValue.ToString(provider:CultureInfo.InvariantCulture);

							// re-calculate mass loadings for all sample results
							foreach (var sampleResult in draftSample.SampleResults)
							{
								//TODO
								//if (sampleResult.IsCalcMassLoading)
								{
									CreateMassLoadingSampleResult(importFlowRow:flowRow,
									                              sampleResultDto:sampleResult,
									                              sampleDto:draftSample,
									                              massLoadingUnitDto:massLoadingUnit
									                             );
								}
							}

							importingSample.Sample = draftSample;
						}
					}

					sampleDtos.AddRange(collection:draftSamplesToUpdate);
				}
				else // this sample is a new sample
				{
					sampleDtos.Add(item:importingSample.Sample);

					// re-calculate all mass loadings 
					if (importingSample.FlowRow != null)
					{
						// validationResult
						var flowRow = importingSample.FlowRow;
						if (flowRow.FlowUnitId < 1)
						{
							continue;
						}

						// re-calculate mass loadings for all sample results
						foreach (var sampleResult in importingSample.Sample.SampleResults)
						{
							CreateMassLoadingSampleResult(importFlowRow:flowRow,
							                              sampleResultDto:sampleResult,
							                              sampleDto:importingSample.Sample,
							                              massLoadingUnitDto:massLoadingUnit);
						}
					}
				}
			}

			return sampleDtos;
		}

		private List<SampleDto> SearchSamplesInCategorySamples(List<SampleDto> searchIn, SampleDto searchFor)
		{
			return searchIn.Where(i => i.MonitoringPointId == searchFor.MonitoringPointId
			                           && i.CtsEventTypeId == searchFor.CtsEventTypeId
			                           && i.CollectionMethodId == searchFor.CollectionMethodId
			                           && i.StartDateTimeLocal == searchFor.StartDateTimeLocal
			                           && i.EndDateTimeLocal == searchFor.EndDateTimeLocal
			                           && (string.IsNullOrWhiteSpace(value:i.LabSampleIdentifier) && string.IsNullOrWhiteSpace(value:searchFor.LabSampleIdentifier)
			                               || !string.IsNullOrWhiteSpace(value:i.LabSampleIdentifier)
			                               && !string.IsNullOrWhiteSpace(value:searchFor.LabSampleIdentifier)
			                               && i.LabSampleIdentifier.Equals(value:searchFor.LabSampleIdentifier, comparisonType:StringComparison.OrdinalIgnoreCase)
			                              )
			                     ).ToList();
		}

		private SampleResultDto ToSampleResult(ImportRowObjectForGroupBySample resultRow, bool isCalculatingMassLoading)
		{
			var sampleResultDto = new SampleResultDto
			                      {
				                      ParameterId = resultRow.ColumnMap[key:SampleImportColumnName.ParameterName].TranslatedValueId,
				                      ParameterName = resultRow.ColumnMap[key:SampleImportColumnName.ParameterName].TranslatedValue,
				                      UnitName = resultRow.EffectiveUnit.Name,
				                      UnitId = resultRow.EffectiveUnit.UnitId,
				                      Value = resultRow.EffectiveUnitResult,
				                      EnteredValue = resultRow.EffectiveUnitResult.HasValue? resultRow.EffectiveUnitResult.ToString() : ""
			                      };

			if (resultRow.ColumnMap.ContainsKey(key:SampleImportColumnName.ResultQualifier))
			{
				sampleResultDto.Qualifier = resultRow.ColumnMap[key:SampleImportColumnName.ResultQualifier].TranslatedValue;
			}

			if (resultRow.ColumnMap.ContainsKey(key:SampleImportColumnName.MethodDetectionLimit))
			{
				sampleResultDto.EnteredMethodDetectionLimit = resultRow.ColumnMap[key:SampleImportColumnName.MethodDetectionLimit].TranslatedValue?.ToString();
				sampleResultDto.MethodDetectionLimit = resultRow.ColumnMap[key:SampleImportColumnName.MethodDetectionLimit].TranslatedValue;
			}

			if (resultRow.ColumnMap.ContainsKey(key:SampleImportColumnName.AnalysisMethod))
			{
				sampleResultDto.AnalysisMethod = resultRow.ColumnMap[key:SampleImportColumnName.AnalysisMethod].TranslatedValue?.ToString();
			}

			if (resultRow.ColumnMap.ContainsKey(key:SampleImportColumnName.AnalysisDateTime))
			{
				sampleResultDto.AnalysisDateTimeLocal = resultRow.ColumnMap[key:SampleImportColumnName.AnalysisDateTime].TranslatedValue;
			}

			sampleResultDto.IsApprovedEPAMethod = true;
			sampleResultDto.IsCalcMassLoading = isCalculatingMassLoading;

			return sampleResultDto;
		}

		private void CreateMassLoadingSampleResult(FlowRow importFlowRow, SampleResultDto sampleResultDto, SampleDto sampleDto, UnitDto massLoadingUnitDto)
		{
			if (importFlowRow == null || !sampleResultDto.Value.HasValue)
			{
				sampleResultDto.IsCalcMassLoading = true;
				sampleResultDto.MassLoadingValue = null;
				sampleResultDto.MassLoadingQualifier = null;
				return;
			}

			var massLoadingBaseUnit = "mgd";
			var resultBaseUnit = "mg/l";

			var flowResult = importFlowRow.FlowValue;

			var massLoadingQualifier = "";
			if (sampleResultDto.Qualifier == "<" && sampleDto.IsMassLoadingResultToUseLessThanSign)
			{
				massLoadingQualifier = "<";
			}

			var massLoadingMultiplier = sampleDto.MassLoadingConversionFactorPounds ?? 0.0f;
			var flowUnitConversionFactor = importFlowRow.FlowUnitName.Equals(value:massLoadingBaseUnit, comparisonType:StringComparison.OrdinalIgnoreCase) ? 1 : 0.000001;
			var resultUnitConversionFactor = sampleResultDto.UnitName.Equals(value:resultBaseUnit, comparisonType:StringComparison.OrdinalIgnoreCase) ? 1 : 0.001;

			var numbers = new[] {flowResult, sampleResultDto.Value ?? 0, massLoadingMultiplier, flowUnitConversionFactor, resultUnitConversionFactor};
			var massloading = _sampleService.CalculateFlowNumbersProduct(numbers:numbers, decimals:sampleDto.MassLoadingCalculationDecimalPlaces ?? 0);
			sampleResultDto.MassLoadingValue = massloading.ProductStr;
			sampleResultDto.MassLoadingQualifier = massLoadingQualifier;

			sampleResultDto.MassLoadingUnitId = massLoadingUnitDto.UnitId;
			sampleResultDto.MassLoadingUnitName = massLoadingUnitDto.Name;
			sampleResultDto.IsCalcMassLoading = true; 
		}

		private ImportSampleFromFileValidationResultDto GetValidImportingSamples(SampleImportDto sampleImportDto, out List<ImportSampleWrapper> groupedSampleWrappers)
		{
			var validationResult = new ImportSampleFromFileValidationResultDto();
			ValidateAuthorityRequiredFieldsForNonSystemRequired(sampleImportDto:sampleImportDto, validationResult:validationResult);
			groupedSampleWrappers = GroupSampleWrappers(sampleImportDto:sampleImportDto);

			foreach (var importingSample in groupedSampleWrappers)
			{
				// check if start date, end date 
				ValidateParametersDateTime(importSampleWrapper: importingSample, validationResult: validationResult); 

				// check row duplication for each importSampleWrapper
				ValidateDataDuplicatedParameters(importSampleWrapper:importingSample, validationResult:validationResult);

				// Update the effective Unit for each row
				ResolveEffectiveUnits(importSampleWrapper:importingSample, validationResult:validationResult);

				// Handle flow row if there is a flow row
				ExtractFlowRow(importSampleWrapper:importingSample, validationResult:validationResult);
			}

			return validationResult;
		}

		private void ValidateParametersDateTime(ImportSampleWrapper importSampleWrapper, ImportSampleFromFileValidationResultDto validationResult)
		{
			var startDateTime = DateTime.Parse(importSampleWrapper.SampleStartDateTime.OriginalValueString); 
			var endDateTime = DateTime.Parse(importSampleWrapper.SampleEndDateTime.OriginalValueString);
			if (startDateTime > endDateTime)
			{
				foreach (var importRowObjectForGroupBySample in importSampleWrapper.SampleResults)
				{
					AddValidationError(validationResult: validationResult, errorMessage: ErrorConstants.SampleImport.DataValidation.EndDateMustBeAfterStartDate, rowNumber: importRowObjectForGroupBySample.RowNumber);
				} 
			}

			var orgRegProgramId = int.Parse(s: _httpContextService.GetClaimValue(claimType: CacheKey.OrganizationRegulatoryProgramId)); 
			// Determine if start date time or end date time are future based on authority's settings
			var authorityNow = _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(DateTime.UtcNow, orgRegProgramId);
			if (startDateTime > authorityNow || endDateTime > authorityNow)
			{
				foreach (var importRowObjectForGroupBySample in importSampleWrapper.SampleResults)
				{
					AddValidationError(validationResult: validationResult, errorMessage: ErrorConstants.SampleImport.DataValidation.SampleDatesCannotBeFutureDates, rowNumber: importRowObjectForGroupBySample.RowNumber);
				}
			}
		}

		private void ValidateDataDuplicatedParameters(ImportSampleWrapper importSampleWrapper, ImportSampleFromFileValidationResultDto validationResult)
		{
			if (_massLoadingFlowParameter == null)
			{
				_massLoadingFlowParameter = _parameterService.GetFlowParameter();
			}
			var parameterGroups = importSampleWrapper.SampleResults.GroupBy(i => i.ParameterName);

			foreach (var parameterGroup in parameterGroups)
			{
				// Check if sample has duplicate parameters 
				if (parameterGroup.Count() > 1)
				{
					var errorMessage = ErrorConstants.SampleImport.DataValidation.DuplicateParametersInSameSample;
					if (validationResult.Errors.Any(i => i.ErrorMessage.Equals(value:errorMessage)))
					{
						var error = validationResult.Errors.Single(i => i.ErrorMessage == errorMessage);
						error.RowNumbers = $"{error.RowNumbers},{string.Join(separator:",", values:parameterGroup.ToList().Select(i => i.RowNumber.ToString()))}";
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

		private void ValidateAuthorityRequiredFieldsForNonSystemRequired(SampleImportDto sampleImportDto, ImportSampleFromFileValidationResultDto validationResult)
		{
			if (_massLoadingFlowParameter == null)
			{
				_massLoadingFlowParameter = _parameterService.GetFlowParameter();
			}

			// Validate Authority Required fields for non-system required, Except row is not for mass loading flow parameter
			var columnsToValidate = sampleImportDto.FileVersion.FileVersionFields
			                                       .Where(x => x.IsSystemRequired == false && x.DataOptionalityName == DataOptionalityName.Required)
			                                       .ToDictionary(k => k.SystemFieldName, v => v.FileVersionFieldName);

			var rowsToValidate =
				sampleImportDto.Rows.Where(row => !((string) row.Cells.First(x => x.SampleImportColumnName == SampleImportColumnName.ParameterName).TranslatedValue)
					                                  .Equals(value:_massLoadingFlowParameter.Name, comparisonType:StringComparison.OrdinalIgnoreCase));

			foreach (var row in rowsToValidate)
			{
				foreach (var cell in row.Cells.Where(x => columnsToValidate.Keys.Contains(value:x.SampleImportColumnName)))
				{
					if (string.IsNullOrWhiteSpace(value:cell.OriginalValueString))
					{
						AddValidationError(validationResult:validationResult,
						                   errorMessage:string.Format(format:ErrorConstants.SampleImport.DataValidation.FieldValueIsRequired,
						                                              arg0:columnsToValidate[key:cell.SampleImportColumnName]),
						                   rowNumber:row.RowNumber);
					}
				}
			}
		}

		private List<ImportSampleWrapper> GroupSampleWrappers(SampleImportDto sampleImportDto)
		{
			var rowsToGroupBySample = new List<ImportRowObjectForGroupBySample>();

			foreach (var row in sampleImportDto.Rows)
			{
				var rowToGroupBy = new ImportRowObjectForGroupBySample
				                   {
					                   ColumnMap = new Dictionary<SampleImportColumnName, ImportCellObject>(),
					                   RowNumber = row.RowNumber
				                   };

				rowsToGroupBySample.Add(item:rowToGroupBy);

				foreach (var cell in row.Cells)
				{
					rowToGroupBy.ColumnMap.Add(key:cell.SampleImportColumnName, value:cell);
					if (cell.SampleImportColumnName == SampleImportColumnName.ParameterName)
					{
						rowToGroupBy.ParameterName = cell.TranslatedValue;
					}
				}
			}

			if (rowsToGroupBySample.Any() && !rowsToGroupBySample[index:0].ColumnMap.ContainsKey(key:SampleImportColumnName.LabSampleId))
			{
				foreach (var rowToGroupBy in rowsToGroupBySample)
				{
					var fakeCell = new ImportCellObject
					               {
						               TranslatedValue = ""
					               };

					rowToGroupBy.ColumnMap.Add(key:SampleImportColumnName.LabSampleId, value:fakeCell);
				}
			}

			//Group into samples  
			var groupedSampleWrappers = rowsToGroupBySample.GroupBy(i => new
			                                                             {
				                                                             MonitoringPont = i.ColumnMap[key:SampleImportColumnName.MonitoringPoint].TranslatedValueId,
				                                                             CollectionMethod = i.ColumnMap[key:SampleImportColumnName.CollectionMethod].TranslatedValueId,
				                                                             SampleType = i.ColumnMap[key:SampleImportColumnName.SampleType].TranslatedValueId,
				                                                             SampleStartDateTime = i.ColumnMap[key:SampleImportColumnName.SampleStartDateTime].TranslatedValue,
				                                                             SampleEndDateTime = i.ColumnMap[key:SampleImportColumnName.SampleEndDateTime].TranslatedValue,
				                                                             LabSampleId = i.ColumnMap[key:SampleImportColumnName.LabSampleId].TranslatedValue
			                                                             })
			                                               .Select(group =>
				                                                       new ImportSampleWrapper
				                                                       {
					                                                       MonitoringPoint = group.First().ColumnMap[key:SampleImportColumnName.MonitoringPoint],
					                                                       CollectionMethod = group.First().ColumnMap[key:SampleImportColumnName.CollectionMethod],
					                                                       SampleType = group.First().ColumnMap[key:SampleImportColumnName.SampleType],
					                                                       SampleStartDateTime = group.First().ColumnMap[key:SampleImportColumnName.SampleStartDateTime],
					                                                       SampleEndDateTime = group.First().ColumnMap[key:SampleImportColumnName.SampleEndDateTime],
					                                                       LabSampleId = group.First().ColumnMap[key:SampleImportColumnName.LabSampleId],
					                                                       SampleResults = group.ToList()
				                                                       }).ToList();
			return groupedSampleWrappers;
		}

		private void PopulateSamples(List<ImportSampleWrapper> importSampleWrappers)
		{
			var currentOrganizationRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
			var programSettings = _settingService.GetProgramSettingsById(orgRegProgramId:_settingService
				                                                             .GetAuthority(orgRegProgramId:currentOrganizationRegulatoryProgramId)
				                                                             .OrganizationRegulatoryProgramId);

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

			foreach (var importSampleWrapper in importSampleWrappers)
			{
				var sampleDto = new SampleDto
				                {
					                MonitoringPointName = importSampleWrapper.MonitoringPoint.TranslatedValue,
					                MonitoringPointId = importSampleWrapper.MonitoringPoint.TranslatedValueId,
					                IsReadyToReport = false,
					                SampleStatusName = SampleStatusName.Draft,
					                FlowUnitId = importSampleWrapper.FlowRow?.FlowUnitId,
					                FlowUnitName = importSampleWrapper.FlowRow != null ? importSampleWrapper.FlowRow.FlowUnitName : "",
					                FlowEnteredValue = importSampleWrapper.FlowRow?.FlowValue.ToString(provider:CultureInfo.InvariantCulture),
					                FlowValue = importSampleWrapper.FlowRow?.FlowValue,
					                CtsEventTypeId = importSampleWrapper.SampleType.TranslatedValueId,
					                CtsEventTypeName = importSampleWrapper.SampleType.TranslatedValue,
					                CtsEventCategoryName = ctsEventCategoryName,
					                CollectionMethodId = importSampleWrapper.CollectionMethod.TranslatedValueId,
					                CollectionMethodName = importSampleWrapper.CollectionMethod.TranslatedValue,
					                LabSampleIdentifier = importSampleWrapper.LabSampleId.TranslatedValue,
					                StartDateTimeLocal = importSampleWrapper.SampleStartDateTime.TranslatedValue,
					                EndDateTimeLocal = importSampleWrapper.SampleEndDateTime.TranslatedValue,
					                FlowUnitValidValues = flowUnitValidValues,
					                ResultQualifierValidValues = resultQualifierValidValues,
					                IsMassLoadingResultToUseLessThanSign = isMassLoadingResultToUseLessThanSign,
					                MassLoadingCalculationDecimalPlaces = massLoadingCalculationDecimalPlaces,
					                MassLoadingConversionFactorPounds = massLoadingConversionFactorPounds
				                };

				var sampleResults = new List<SampleResultDto>();
				sampleDto.SampleResults = sampleResults;

				importSampleWrapper.Sample = sampleDto;

				var isCalculationMassloading = importSampleWrapper.FlowRow != null;

				// populate sampleResults 
				foreach (var importSampleResultRow in importSampleWrapper.SampleResults)
				{
					var sampleResultDto = ToSampleResult(resultRow:importSampleResultRow, isCalculatingMassLoading:isCalculationMassloading);
					sampleResults.Add(item:sampleResultDto);
					importSampleResultRow.ParameterName = sampleResultDto.ParameterName;
				}
			}
		}

		private static T DeepCopy<T>(T item)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream();
			formatter.Serialize(stream, item);
			stream.Seek(0, SeekOrigin.Begin);
			T result = (T)formatter.Deserialize(stream);
			stream.Close();
			return result;
		}

		private static void AddValidationError(ImportSampleFromFileValidationResultDto validationResult, string errorMessage, int? rowNumber = default(int?))
		{
			var error = validationResult.Errors.SingleOrDefault(i => i.ErrorMessage == errorMessage);
			if (error == null)
			{
				error = new ErrorWithRowNumberDto
				        {
					        ErrorMessage = errorMessage,
					        RowNumbers = rowNumber.HasValue ? $"{rowNumber}" : ""
				        };

				validationResult.Errors.Add(item:error);
			}
			else
			{
				error.RowNumbers = rowNumber.HasValue ? $"{error.RowNumbers}, {rowNumber}" : $"{error.RowNumbers}";
			}
		}

		private class ImportRowObjectForGroupBySample
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

		private class ImportSampleWrapper
		{
			#region public properties

			public ImportCellObject MonitoringPoint { get; set; }
			public ImportCellObject CollectionMethod { get; set; }
			public ImportCellObject SampleType { get; set; }
			public ImportCellObject SampleStartDateTime { get; set; }
			public ImportCellObject SampleEndDateTime { get; set; }
			public ImportCellObject LabSampleId { get; set; }
			public FlowRow FlowRow { get; set; }
			public List<ImportRowObjectForGroupBySample> SampleResults { get; set; }
			public SampleDto Sample { get; set; }

			#endregion
		}

		private class FlowRow
		{
			#region public properties

			public int FlowUnitId { get; set; }
			public string FlowUnitName { get; set; }
			public double FlowValue { get; set; }

			#endregion
		}
	}
}