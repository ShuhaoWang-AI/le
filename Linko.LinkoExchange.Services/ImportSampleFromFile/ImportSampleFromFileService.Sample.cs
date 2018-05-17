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

			
			// figure out the date range of these file import.  
			//TODO: is local time or converted to utc?
			var min = importSampleResultRows.Min(i => i.Sample.StartDateTimeLocal);
			var max = importSampleResultRows.Min(i => i.Sample.EndDateTimeLocal); 

			var samples = _sampleService.GetSamples(status: SampleStatusName.All, startDate: min, endDate: max, isIncludeChildObjects: true).ToList();
			var reportedSamples = samples.Where(i => i.SampleStatusName == SampleStatusName.Reported).ToList();
			var readyToReportSamples = samples.Where(i => i.SampleStatusName == SampleStatusName.ReadyToReport).ToList();
			var draftSamples = samples.Where(i => i.SampleStatusName == SampleStatusName.Draft).ToList();

			var validationResult = ValidateSamples(importSampleResultRows, reportedSamples, readyToReportSamples, draftSamples);
			if (validationResult.Success == false)
			{
				return validationResult;
			}
			 
			// TODO handle new samples, and samples that have same result in draft mode  
			var validSampleRows = importSampleResultRows.Where(i => i.Valid);

			// 

			// 1. unit conversion 

			// 2. 

			return null;
		}

		private ImportSampleFromFileValidationResultDto ValidateSamples(List<ImportSampleResultRow> importSampleResultRows, 
																		List<SampleDto> reportedSamples,
		                                                                List<SampleDto> readyToReportSamples,
																		List<SampleDto> draftSamples )
		{
			ImportSampleFromFileValidationResultDto validationResult = new ImportSampleFromFileValidationResultDto();

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

				// Check if this sample is reported
				var importingSample = importingSampleGroup.Sample;
				var isNewSample = true;
				if (IsCategorySamplesContains(reportedSamples, importingSample))
				{
					validationResult.Success = false;
					var importingSampleResult = importingSample.SampleResults.ToList();

					var errors = ValidateSampleGroups(importingSampleGroup.SampleResultRows, importingSampleResult, reportedSamples, "Sample has been reported");
					validationResult.Errors.AddRange(errors);
					isNewSample = false;
				}

				// Check if this sample is readyToReport 
				if (IsCategorySamplesContains(readyToReportSamples, importingSample))
				{
					validationResult.Success = false;
					var importingSampleResult = importingSample.SampleResults.ToList();

					var errors = ValidateSampleGroups(importingSampleGroup.SampleResultRows, importingSampleResult, readyToReportSamples, "Sample is ready to report");
					validationResult.Errors.AddRange(errors);
					isNewSample = false;
				}

				// Sample results are in a draft sample
				var samplesInDatabase = SearchSamplesInCategorySamples(draftSamples, importingSample);
				if (samplesInDatabase.Any())
				{
					isNewSample = false;
					var importingSampleResultParameterIds = importingSample.SampleResults.Select(i => i.ParameterId);  

					//Update all the drafts
					foreach (var draftSample in samplesInDatabase)
					{ 
						var draftParametersToUpdate = draftSample.SampleResults.Where(i => importingSampleResultParameterIds.Contains(i.ParameterId)).ToList();
						foreach (var draftParameter in draftParametersToUpdate)
						{
							var newSampleResult = importingSample.SampleResults.Single(i => i.ParameterId == draftParameter.ParameterId);

							draftParameter.Qualifier = newSampleResult.Qualifier;
							draftParameter.EnteredValue = newSampleResult.EnteredValue;
							draftParameter.Value = newSampleResult.Value;
							draftParameter.UnitId = newSampleResult.UnitId;
							draftParameter.UnitName = newSampleResult.UnitName;
							draftParameter.EnteredMethodDetectionLimit = newSampleResult.EnteredMethodDetectionLimit;
							draftParameter.MethodDetectionLimit = newSampleResult.MethodDetectionLimit;
							draftParameter.AnalysisMethod = newSampleResult.AnalysisMethod;
							draftParameter.AnalysisDateTimeLocal = newSampleResult.AnalysisDateTimeLocal;
							draftParameter.IsApprovedEPAMethod = newSampleResult.IsApprovedEPAMethod;
							draftParameter.IsCalcMassLoading = newSampleResult.IsCalcMassLoading;
							draftParameter.LastModificationDateTimeLocal = newSampleResult.LastModificationDateTimeLocal; 
						}
					}
				}

				// This sample is a new sample
				if (isNewSample)
				{
					// ? 
				}
			}

			return validationResult;
		}

		List<ErrorWithRowNumberDto> ValidateSampleGroups(List<ImportSampleResultRow> sampleGroupResultRows, 
														 List<SampleResultDto> importingSampleResultDtos,
		                                                 List<SampleDto> sampleCategories, string errorMessage)
		{
			var errors = new List<ErrorWithRowNumberDto>();
			var reportedSampleResults = importingSampleResultDtos.Intersect(sampleCategories.SelectMany(i => i.SampleResults)).ToList();

			if (reportedSampleResults.Any())
			{
				var reportedSampleResultRows = sampleGroupResultRows.Where(i => reportedSampleResults.Contains(i.SampleResult)).ToList();
				var rowNumbers = reportedSampleResultRows.Select(i => i.RowNumber.ToString()).ToList();

				foreach (var sampleResultRow in reportedSampleResultRows)
				{
					sampleResultRow.Valid = false;
				}

				errors = GetSampleResultErrors(rowNumbers, errorMessage);
			}

			return errors;
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

		bool IsCategorySamplesContains(List<SampleDto> categorySamples, SampleDto sample)
		{
			return categorySamples.Any(i => i.MonitoringPointId == sample.MonitoringPointId &&
									i.CtsEventTypeId == sample.CtsEventTypeId &&
									i.CollectionMethodId == sample.CollectionMethodId &&
									i.StartDateTimeLocal == sample.StartDateTimeLocal &&
									i.EndDateTimeLocal == sample.EndDateTimeLocal &&
									(string.IsNullOrWhiteSpace(i.LabSampleIdentifier) && string.IsNullOrWhiteSpace(sample.LabSampleIdentifier) ||
									 !string.IsNullOrWhiteSpace(i.LabSampleIdentifier) && !string.IsNullOrWhiteSpace(sample.LabSampleIdentifier) &&
									 i.LabSampleIdentifier.Equals(sample.LabSampleIdentifier, StringComparison.OrdinalIgnoreCase)
									)
							  );
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

		private void CreateMassLoadingSampleResult(ImportSampleResultRow importFlowRow, SampleResultDto sampleResultDto)
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

			sampleResultDto.MassLoadingValue = massLoadingValue.ToString();
			sampleResultDto.MassLoadingQualifier = sampleResultDto.Qualifier;
			sampleResultDto.MassLoadingUnitId = importFlowRow.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValueId.Value;
			sampleResultDto.MassLoadingUnitName = importFlowRow.ColumnMap[SampleImportColumnName.ResultUnit].TranslatedValue;
		}

		private static List<ImportSampleResultRow> GetImportSampleResultRows(SampleImportDto sampleImportDto)
		{
			var dataTable = new List<ImportSampleResultRow>();
			foreach (var row in sampleImportDto.Rows)
			{
				var sampleRow = new ImportSampleResultRow
				                {
					                ColumnMap = new Dictionary<SampleImportColumnName, ImportCellObject>(),
					                RowNumber = row.RowNumber,
									Valid = true
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
						CreateMassLoadingSampleResult(importFlowRow, sampleResultDto);
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
		public bool Valid { get; set; }
	}
}