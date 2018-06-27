using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.ImportSampleFromFile;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.Mvc;
using Linko.LinkoExchange.Web.Shared;
using Linko.LinkoExchange.Web.ViewModels.Industry;
using Linko.LinkoExchange.Web.ViewModels.Shared;

namespace Linko.LinkoExchange.Web.Controllers
{
    public partial class IndustryController
    {
        [Route(template:"Sample/Import")]
        public ActionResult SampleImport()
        {
            SampleImportViewModel model = null;
            var importJobId = GetQueryParameterValue(parameterName:QueryParameters.ImportJobId);
            if (!string.IsNullOrEmpty(value:importJobId))
            {
                try
                {
                    model = SampleImportHelpers.FromImportJobId(importJobId:importJobId);
                }
                catch
                {
                    ModelState.AddModelError(key:string.Empty,
                                             errorMessage:string.Format(format:ErrorConstants.SampleImport.QueryParameterIsInvalid, arg0:QueryParameters.ImportJobId));
                }
            }

            if (model == null)
            {
                model = new SampleImportViewModel();
            }

            return RenderSampleImportStepView(model:model);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"Sample/Import")]
        public ActionResult SampleImport(SampleImportViewModel model)
        {
#if DEBUG
            _logger.Debug(message:"POST SampleImport: {0}", argument:SampleImportHelpers.ToJsonString(value:model));
#endif
            if (model.SampleImportDto == null)
            {
                model.SampleImportDto = new SampleImportDto
                                        {
                                            ImportId = model.ImportId
                                        };
            }

            if (!ModelState.IsValid)
            {
                return RenderSampleImportStepView(model:model);
            }

            try
            {
                PopulateSelectedDataProvider(model:model);

                if (model.SelectedDataSourceId == 0 || model.SampleImportDto.DataSource == null || !ModelState.IsValid)
                {
                    return RedirectSampleImportStepView(model:model, step:SampleImportViewModel.SampleImportStep.SelectDataSource);
                }

                PopulateSampleImportTempFile(model:model);
                if (model.ImportTempFileId == 0 || model.SampleImportDto.TempFile == null)
                {
                    return RedirectSampleImportStepView(model:model, step:SampleImportViewModel.SampleImportStep.SelectFile);
                }

                PopulateRowObjectsOrFileValidationErrors(model:model);
                if (model.StepFileValidation != null)
                {
                    return RedirectSampleImportStepView(model:model, step:SampleImportViewModel.SampleImportStep.SelectFile);
                }

                PopulateRequiredDefaultValuesDropdownsForEmptyRecommendedCells(model:model);
                var shouldRenderSelectDataDefaultView = model.CurrentSampleImportStep == SampleImportViewModel.SampleImportStep.SelectFile || 
                                                      (model.SampleImportDto.RequiredDefaultValues.Any() && 
                                                       model.SampleImportDto.RequiredDefaultValues.Count != GetNumberOfAssignedRequiredDefaultValues(model: model));
                if (shouldRenderSelectDataDefaultView)
                {
                    return RedirectSampleImportStepView(model:model, step:SampleImportViewModel.SampleImportStep.SelectDataDefault);
                }

                PopulateEmptyRecommendedCellsWithDataDefaults(model:model);
                PopulateDataTranslationsAndMissingTranslationIfExists(model:model);
                var shouldRenderTranslateDataView = model.CurrentSampleImportStep == SampleImportViewModel.SampleImportStep.SelectDataDefault ||
                                                      (model.DataTranslations != null && model.DataTranslations.Any(x => x.NumberOfMissingTranslations > 0));
                if (shouldRenderTranslateDataView)
                {
                    if (model.CurrentSampleImportStep == SampleImportViewModel.SampleImportStep.DataTranslations)
                    {
                        throw new BadRequest(message:ErrorConstants.SampleImport.DataTranslationsAreRequired);
                    }
                    return RedirectSampleImportStepView(model:model, step:SampleImportViewModel.SampleImportStep.DataTranslations);
                }

                PopulateSamplesOrDataValidationErrors(model:model);
                if (model.StepDataValidation != null)
                {
                    return RedirectSampleImportStepView(model:model, step:SampleImportViewModel.SampleImportStep.DataValidation);
                }

                if (model.CurrentSampleImportStep < SampleImportViewModel.SampleImportStep.ShowPreImportOutput)
                {
                    return RedirectSampleImportStepView(model:model, step:SampleImportViewModel.SampleImportStep.ShowPreImportOutput);
                }

                DoImportFinalSaveAndUpdateImportSummaryToViewModel(model:model);
                return RedirectSampleImportStepView(model:model, step:SampleImportViewModel.SampleImportStep.ShowImportOutput);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                return RenderSampleImportStepView(model:model);
            }
        }
		
	    public JsonResult ImportSampleFile(SampleImportViewModel model, HttpPostedFileBase upload)
	    {
		    try
		    {
			    ImportTempFileDto importTempFileDto;
			    using (var reader = new BinaryReader(input:upload.InputStream))
			    {
				    var content = reader.ReadBytes(count:upload.ContentLength);

				    importTempFileDto = new ImportTempFileDto
				                        {
					                        OriginalFileName = upload.FileName,
					                        RawFile = content,
					                        MediaType = upload.ContentType
				                        };

				    var importTempFileId = _importSampleFromFileService.CreateImportTempFile(importTempFileDto:importTempFileDto);
				    importTempFileDto.ImportTempFileId = importTempFileId;
				    model.ImportTempFileId = importTempFileId;
				    model.SelectedFileName = upload.FileName;
			    }

			    PopulateSelectedDataProvider(model:model);
			    model.SampleImportDto.TempFile = importTempFileDto;
			    //PopulateSampleImportTempFile(model:model);
			    PopulateRowObjectsOrFileValidationErrors(model:model);
			    model.SampleImportDto = null;

			    return Json(data:model, behavior:JsonRequestBehavior.AllowGet);
		    }
		    catch (RuleViolationException rve)
		    {
			    Response.StatusCode = (int) HttpStatusCode.BadRequest;
			    return Json(data:MvcValidationExtensions.GetViolationErrors(ruleViolationException:rve), behavior:JsonRequestBehavior.AllowGet);
		    }
	    }

	    [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult SampleImportSaveMonitoringPointDataTranslation([CustomDataSourceRequest] DataSourceRequest request,
                                                            DataSourceMonitoringPointTranslationViewModel viewModel)
        {
            viewModel.TranslatedItem = viewModel.MonitoringPoint;
            return SampleImportSaveDataTranslation(request, viewModel);
        }

        [AcceptVerbs(verbs: HttpVerbs.Post)]
        public ActionResult SampleImportSaveSampleTypeDataTranslation([CustomDataSourceRequest] DataSourceRequest request,
                                                                           DataSourceSampleTypeTranslationViewModel viewModel)
        {
            viewModel.TranslatedItem = viewModel.SampleType;
            return SampleImportSaveDataTranslation(request:request, viewModel:viewModel);
        }

        [AcceptVerbs(verbs: HttpVerbs.Post)]
        public ActionResult SampleImportSaveCollectionMethodDataTranslation([CustomDataSourceRequest] DataSourceRequest request,
                                                                           DataSourceCollectionMethodTranslationViewModel viewModel)
        {
            viewModel.TranslatedItem = viewModel.CollectionMethod;
            return SampleImportSaveDataTranslation(request:request, viewModel:viewModel);
        }

        [AcceptVerbs(verbs: HttpVerbs.Post)]
        public ActionResult SampleImportSaveParameterDataTranslation([CustomDataSourceRequest] DataSourceRequest request,
                                                                           DataSourceParameterTranslationViewModel viewModel)
        {
            viewModel.TranslatedItem = viewModel.Parameter;
            return SampleImportSaveDataTranslation(request:request, viewModel:viewModel);
        }

        [AcceptVerbs(verbs: HttpVerbs.Post)]
        public ActionResult SampleImportSaveUnitDataTranslation([CustomDataSourceRequest] DataSourceRequest request,
                                                                           DataSourceUnitTranslationViewModel viewModel)
        {
            viewModel.TranslatedItem = viewModel.Unit;
            return SampleImportSaveDataTranslation(request:request, viewModel:viewModel);
        }

        private ActionResult SampleImportSaveDataTranslation([CustomDataSourceRequest] DataSourceRequest request,
                                                            DataSourceTranslationViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dataSourceTranslationDto = new DataSourceTranslationDto
                                                   {
                                                       Id = viewModel.Id,
                                                       DataSourceId = viewModel.DataSourceId,
                                                       DataSourceTerm = viewModel.DataSourceTerm,
                                                       TranslationItem = new DataSourceTranslationItemDto
                                                                         {
                                                                             // ReSharper disable once PossibleInvalidOperationException
                                                                             TranslationId = viewModel.TranslatedItem.Id.Value,
                                                                             TranslationName = viewModel.TranslatedItem.DisplayName
                                                                         }
                                                   };
                    viewModel.Id = _dataSourceService.SaveDataSourceTranslation(dataSourceTranslation:dataSourceTranslationDto, translationType:viewModel.TranslationType);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return Json(data:new[] {viewModel}.ToDataSourceResult(request:request, modelState:ModelState));
        }

        private static ListItemDto GetUserSelectedDefaultValue(int columnId, string columnText)
        {
            if (columnId == 0 || string.IsNullOrEmpty(value:columnText))
            {
                return null;
            }

            return new ListItemDto
                   {
                       Id = columnId,
                       DisplayValue = columnText
                   };
        }

        private static string GetDataTranslationSubTitleFromColumnName(SampleImportColumnName columnName)
        {
            switch (columnName)
            {
                case SampleImportColumnName.MonitoringPoint: return "Monitoring Points";
                case SampleImportColumnName.SampleType: return "Sample Types";
                case SampleImportColumnName.CollectionMethod: return "Collection Methods";
                case SampleImportColumnName.ParameterName: return "Parameters";
                case SampleImportColumnName.ResultUnit: return "Units";
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void PopulateAvailableDataProviders(SampleImportViewModel model)
        {
            var currentOrganizationRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var dataSources = _dataSourceService.GetDataSources(organizationRegulatoryProgramId:currentOrganizationRegulatoryProgramId);
            model.StepSelectDataSource = new StepSelectDataSourceViewModel
                                         {
                                             AvailableDataSources =
                                                 dataSources.Select(x => new SelectListItem
                                                                         {
                                                                             Text = x.Name,
                                                                             Value = x.DataSourceId.ToString(),
                                                                             Selected =
                                                                                 x.DataSourceId.ToString()
                                                                                  .Equals(value:model.SelectedDataSourceId
                                                                                                     .ToString())
                                                                         }).ToList()
                                         };
            model.StepSelectDataSource.AvailableDataSources.Insert(index:0, item:new SelectListItem {Text = @"Select Data Provider", Value = "0", Disabled = true});

            if (model.SelectedDataSourceId > 0 || model.StepSelectDataSource.AvailableDataSources.Count != 2)
            {
                return;
            }

            var selectedDataSource = model.StepSelectDataSource.AvailableDataSources[index:1];
            model.SelectedDataSourceId = int.Parse(s:selectedDataSource.Value);
            model.SelectedDataSourceName = selectedDataSource.Text;
        }

        private void PopulateSelectedDataProvider(SampleImportViewModel model)
        {
            if (model.SelectedDataSourceId == 0)
            {
                return;
            }

            var selectedDataSource = _dataSourceService.GetDataSourceById(dataSourceId:model.SelectedDataSourceId, withDataTranslations:true);
            if (selectedDataSource == null)
            {
                if (model.StepSelectDataSource == null)
                {
                    PopulateAvailableDataProviders(model:model);
                }

                model.SelectedDataSourceId = 0;
                model.CurrentSampleImportStep = SampleImportViewModel.SampleImportStep.SelectDataSource;
                throw new BadRequest(message:ErrorConstants.SampleImport.DataProviderDoesNotExist);
            }

            //reserve user selected DataProvider into SampleImportDto for render or  to render the step view
            model.SampleImportDto.DataSource = selectedDataSource;
        }

        private void PopulateSampleImportTempFile(SampleImportViewModel model)
        {
            if (model.ImportTempFileId == 0)
            {
                return;
            }

            var importTempFileDto = _importSampleFromFileService.GetImportTempFileById(importTempFileId:model.ImportTempFileId);
            if (importTempFileDto == null)
            {
                model.ImportTempFileId = 0;
                model.CurrentSampleImportStep = SampleImportViewModel.SampleImportStep.SelectFile;
                throw new BadRequest(message:ErrorConstants.SampleImport.CannotFindImportFile);
            }

            model.SampleImportDto.TempFile = importTempFileDto;
            model.SelectedFileName = importTempFileDto.OriginalFileName;
        }

        private void PopulateRowObjectsOrFileValidationErrors(SampleImportViewModel model)
        {
            var fileValidationResultDto = _importSampleFromFileService.DoFileValidation(sampleImportDto:model.SampleImportDto);

            if (!fileValidationResultDto.Success)
            {
                model.StepFileValidation = new StepFileValidationViewModel
                                           {
                                               Errors = fileValidationResultDto.Errors.Select(x => new ErrorWithRowNumberViewModel
                                                                                                   {
                                                                                                       ErrorMessage = x.ErrorMessage,
                                                                                                       RowNumbers = x.RowNumbers
                                                                                                   }).ToList()
                                           };

                _logger.Debug(message:"Sample/Import has {0} errors at step File Validation", argument:model.StepFileValidation.Errors.Count);
            }
        }

        private void PopulateRequiredDefaultValuesDropdownsForEmptyRecommendedCells(SampleImportViewModel model)
        {
            var requiredDefaultValues = _importSampleFromFileService.GetRequiredDataDefaults(sampleImportDto:model.SampleImportDto);

            if (requiredDefaultValues.Count == 0)
            {
                return;
            }

            model.SampleImportDto.RequiredDefaultValues = requiredDefaultValues;
            model.SelectedDefaultMonitoringPointName = GetSelectedRequiredDefaultName(requiredDefaultValues:requiredDefaultValues,
                                                                                      columnName:SampleImportColumnName.MonitoringPoint,
                                                                                      defaultDataId:model.SelectedDefaultMonitoringPointId);
            model.SelectedDefaultCollectionMethodName = GetSelectedRequiredDefaultName(requiredDefaultValues:requiredDefaultValues,
                                                                                       columnName:SampleImportColumnName.CollectionMethod,
                                                                                       defaultDataId:model.SelectedDefaultCollectionMethodId);
            model.SelectedDefaultSampleTypeName = GetSelectedRequiredDefaultName(requiredDefaultValues:requiredDefaultValues,
                                                                                 columnName:SampleImportColumnName.SampleType,
                                                                                 defaultDataId:model.SelectedDefaultSampleTypeId);
        }

        private string GetSelectedRequiredDefaultName(List<RequiredDataDefaultsDto> requiredDefaultValues, SampleImportColumnName columnName, int defaultDataId)
        {
            if (defaultDataId == 0)
            {
                return null;
            }

            return requiredDefaultValues.Find(x => x.SampleImportColumnName == columnName)?
                .Options.Find(x => x.Id == defaultDataId)?.DisplayValue;
        }

        private int GetNumberOfAssignedRequiredDefaultValues(SampleImportViewModel model)
        {
            var number = 0;
            if (model.SelectedDefaultMonitoringPointId > 0)
            {
                number++;
            }

            if (model.SelectedDefaultCollectionMethodId > 0)
            {
                number++;
            }

            if (model.SelectedDefaultSampleTypeId > 0)
            {
                number++;
            }

            return number;
        }

        private void PopulateEmptyRecommendedCellsWithDataDefaults(SampleImportViewModel model)
        {
            var defaultMonitoringPoint = GetUserSelectedDefaultValue(columnId:model.SelectedDefaultMonitoringPointId,
                                                                     columnText:model.SelectedDefaultMonitoringPointName);
            var defaultCollectionMethod = GetUserSelectedDefaultValue(columnId:model.SelectedDefaultCollectionMethodId,
                                                                      columnText:model.SelectedDefaultCollectionMethodName);
            var defaultSampleType = GetUserSelectedDefaultValue(columnId:model.SelectedDefaultSampleTypeId,
                                                                columnText:model.SelectedDefaultSampleTypeName);

            _importSampleFromFileService.PopulateDataDefaults(sampleImportDto:model.SampleImportDto,
                                                              defaultMonitoringPoint:defaultMonitoringPoint,
                                                              defaultCollectionMethod:defaultCollectionMethod,
                                                              defaultSampleType:defaultSampleType);
        }

        private void PopulateDataTranslationsAndMissingTranslationIfExists(SampleImportViewModel model)
        {
            model.FlowParameterName = _parameterService.GetFlowParameter().Name;

            var missingTranslations = _importSampleFromFileService
                .PopulateExistingTranslationDataAndReturnMissingTranslationSet(sampleImportDto:model.SampleImportDto, dataSourceId:model.SelectedDataSourceId);

            model.DataTranslations = ExtractBothMissingTranslationsAndExistingTranslations(model:model, missingTranslations:missingTranslations);
        }

        private List<ImportDataTranslationViewModel> ExtractBothMissingTranslationsAndExistingTranslations(SampleImportViewModel model,
                                                                                                        List<ImportDataTranslationDto> missingTranslations)
        {
            var involvedTranslations = new List<ImportDataTranslationViewModel>();

            ExtractBothMissingTranslationsAndExistingTranslations(model:model, targetList:involvedTranslations, columnName:SampleImportColumnName.MonitoringPoint,
                                                                  missingTranslations:missingTranslations);
            ExtractBothMissingTranslationsAndExistingTranslations(model:model, targetList:involvedTranslations, columnName:SampleImportColumnName.CollectionMethod,
                                                                  missingTranslations:missingTranslations);
            ExtractBothMissingTranslationsAndExistingTranslations(model:model, targetList:involvedTranslations, columnName:SampleImportColumnName.SampleType,
                                                                  missingTranslations:missingTranslations);
            ExtractBothMissingTranslationsAndExistingTranslations(model:model, targetList:involvedTranslations, columnName:SampleImportColumnName.ParameterName,
                                                                  missingTranslations:missingTranslations);
            ExtractBothMissingTranslationsAndExistingTranslations(model:model, targetList:involvedTranslations, columnName:SampleImportColumnName.ResultUnit,
                                                                  missingTranslations:missingTranslations);

            return involvedTranslations;
        }

        private void ExtractBothMissingTranslationsAndExistingTranslations(SampleImportViewModel model, List<ImportDataTranslationViewModel> targetList,
                                                                           SampleImportColumnName columnName, List<ImportDataTranslationDto> missingTranslations)
        {
            var translationType = ImportSampleFromFileService.ColumnNameTranslationTypeDict[key:columnName];
            var missingTranslation = missingTranslations.FirstOrDefault(x => x.SampleImportColumnName == columnName);

            var selectListType = ImportSampleFromFileService.TranslationTypeSelectListTypeDict[key:translationType];

            var availableLinkoExchangeTerms = _selectListService.GetSelectList(selectListType:selectListType, withEmptyItem:true)
                                                                .Select(x => new DropdownOptionViewModel
                                                                             {
                                                                                 Id = x.Id,
                                                                                 DisplayName = x.DisplayValue,
                                                                                 Description = x.Description
                                                                             })
                                                                .ToList();
            var defaultLinkoExchangeTerm = availableLinkoExchangeTerms.First();

            var dropdownOptionsKey = SampleImportHelpers.GetDropdownOptionsKey(translationType: translationType);
            var defaultDropdownOptionKey = SampleImportHelpers.GetDefaultDropdownOptionKey(translationType: translationType);
            ViewData[key:dropdownOptionsKey] = availableLinkoExchangeTerms;
            ViewData[key:defaultDropdownOptionKey] = defaultLinkoExchangeTerm;

            _logger.Debug("ViewData added {0}: {1}", defaultDropdownOptionKey, defaultLinkoExchangeTerm);
            _logger.Debug("ViewData added {0}: {1}", dropdownOptionsKey, SampleImportHelpers.ToJsonString(availableLinkoExchangeTerms));

            var translationsViewModel = new ImportDataTranslationViewModel
                                        {
                                            Title = GetDataTranslationSubTitleFromColumnName(columnName:columnName),
                                            TranslationType = translationType,
                                            DataTranslations = new List<IDataSourceTranslationViewModel>()
                                        };
            targetList.Add(item:translationsViewModel);

            if (missingTranslation != null)
            {
                var missingDataTranslations = missingTranslation.MissingTranslations
                                                                .Select(term =>
                                                                        {
                                                                            var translationModel = DataSourceTranslationViewModelHelper.Create(translationType);
                                                                            translationModel.DataSourceTerm = term;
                                                                            translationModel.DataSourceId = model.SelectedDataSourceId;
                                                                            translationModel.TranslationType = translationType;
                                                                            translationModel.TranslatedItem = defaultLinkoExchangeTerm;
                                                                            return translationModel;
                                                                        }).ToList();

                translationsViewModel.NumberOfMissingTranslations = missingDataTranslations.Count;
                translationsViewModel.DataTranslations.AddRange(collection:missingDataTranslations);
            }

            var translatedTerms = model.SampleImportDto.Rows.Select(row => row.Cells.First(cell => cell.SampleImportColumnName == columnName))
                                       .Where(cell => !string.IsNullOrEmpty(value:cell.OriginalValueString) && cell.TranslatedValueId > 0)
                                       .Select(cell => cell.OriginalValueString)
                                       .Distinct(comparer:StringComparer.OrdinalIgnoreCase)
                                       .ToList();
            List<IDataSourceTranslationViewModel> existingDataTranslations;
            switch (translationType)
            {
                case DataSourceTranslationType.MonitoringPoint:
                    existingDataTranslations = GetExistingDataTranslations(translationType:translationType, involvedTerms:translatedTerms,
                                                                           dataTranslations:model.SampleImportDto.DataSource.DataSourceMonitoringPoints);
                    break;
                case DataSourceTranslationType.SampleType:
                    existingDataTranslations = GetExistingDataTranslations(translationType:translationType, involvedTerms:translatedTerms,
                                                                           dataTranslations:model.SampleImportDto.DataSource.DataSourceSampleTypes);
                    break;
                case DataSourceTranslationType.CollectionMethod:
                    existingDataTranslations = GetExistingDataTranslations(translationType:translationType, involvedTerms:translatedTerms,
                                                                           dataTranslations:model.SampleImportDto.DataSource.DataSourceCollectionMethods);
                    break;
                case DataSourceTranslationType.Parameter:
                    existingDataTranslations = GetExistingDataTranslations(translationType:translationType, involvedTerms:translatedTerms,
                                                                           dataTranslations:model.SampleImportDto.DataSource.DataSourceParameters);
                    break;
                case DataSourceTranslationType.Unit:
                    existingDataTranslations = GetExistingDataTranslations(translationType:translationType, involvedTerms:translatedTerms,
                                                                           dataTranslations:model.SampleImportDto.DataSource.DataSourceUnits);
                    break;
                default: throw new NotSupportedException(message:$"DataSourceTranslationType {translationType} is unsupported");
            }

            translationsViewModel.DataTranslations.AddRange(collection:existingDataTranslations);
            translationsViewModel.NumberOfExistingTranslations = existingDataTranslations.Count;
        }

        private List<IDataSourceTranslationViewModel> GetExistingDataTranslations(DataSourceTranslationType translationType,
                                                                                 ICollection<string> involvedTerms,
                                                                                 IEnumerable<DataSourceTranslationDto> dataTranslations)
        {
            return dataTranslations.Where(x => involvedTerms.CaseInsensitiveContains(value:x.DataSourceTerm))
                                   .Select(x => {
                                               var translationModel = DataSourceTranslationViewModelHelper.Create(translationType);
                                               translationModel.Id = x.Id;
                                               translationModel.DataSourceTerm = x.DataSourceTerm;
                                               translationModel.DataSourceId = x.DataSourceId;
                                               translationModel.TranslationType = translationType;
                                               translationModel.TranslatedItem = new DropdownOptionViewModel
                                                                                 {
                                                                                     Id = x.TranslationItem.TranslationId,
                                                                                     DisplayName = x.TranslationItem.TranslationName
                                                                                 };
                                               return translationModel;
                                           })
                                   .ToList();
        }

        private void PopulateSamplesOrDataValidationErrors(SampleImportViewModel model)
        {
            ImportSampleFromFileValidationResultDto dataValidationResultDto = null;
            if (model.SampleImportDto.SampleDtos == null)
            {
                dataValidationResultDto = _importSampleFromFileService.DoDataValidation(sampleImportDto:model.SampleImportDto);
            }

            if (dataValidationResultDto != null && !dataValidationResultDto.Success)
            {
                model.StepDataValidation = new StepDataValidationViewModel
                                           {
                                               Errors = dataValidationResultDto.Errors.Select(x => new ErrorWithRowNumberViewModel
                                                                                                   {
                                                                                                       ErrorMessage = x.ErrorMessage,
                                                                                                       RowNumbers = x.RowNumbers
                                                                                                   }).ToList()
                                           };

                _logger.Debug(message:"Sample/Import has {0} errors at step Data Validation", argument:model.StepDataValidation.Errors.Count);
            }
        }

        private void PopulatePreviewData(SampleImportViewModel model)
        {
            _sampleService.SampleComplianceCheck(sampleDtos:model.SampleImportDto.SampleDtos);
            model.Samples = model.SampleImportDto.SampleDtos.Select(selector:_mapHelper.ToViewModel).ToList();

            // to attach a identifier for each sample 
            foreach (var sample in model.Samples)
            {
                sample.Identifier = Guid.NewGuid().ToString();
            }

            model.ImportSummary = SampleImportHelpers.CreateImportSummary(samples:model.Samples);
        }

        private ActionResult RenderSampleImportStepView(SampleImportViewModel model)
        {
#if DEBUG
            _logger.Debug(message:"RenderSampleImportStepView: {0}",
                          argument:SampleImportHelpers.ToJsonString(value:SampleImportHelpers.ToSampleImportQueryParameters(model:model)));
#endif

            try
            {
                switch (model.CurrentSampleImportStep)
                {
                    case SampleImportViewModel.SampleImportStep.SelectDataSource: return RenderImportSelectDataProviderView(model:model);
                    case SampleImportViewModel.SampleImportStep.SelectFile: return RenderImportSelectFileView(model:model);
                    //case SampleImportViewModel.SampleImportStep.FileValidation: return RenderImportFileValidationView(model:model);
                    case SampleImportViewModel.SampleImportStep.SelectDataDefault: return RenderImportSelectDataDefaultView(model:model);
                    case SampleImportViewModel.SampleImportStep.DataTranslations: return RenderImportDataTranslationsView(model:model);
                    case SampleImportViewModel.SampleImportStep.DataValidation: return RenderImportDataValidationView(model:model);
                    case SampleImportViewModel.SampleImportStep.ShowPreImportOutput: return RenderImportPreviewView(model:model);
                    case SampleImportViewModel.SampleImportStep.ShowImportOutput: return RenderImportSummaryView(model:model);
                    default: throw new InternalServerError(message:$"{model.CurrentSampleImportStep} is unsupported");
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(model:model);
        }

        private ActionResult RenderImportSelectDataProviderView(SampleImportViewModel model)
        {
            PopulateSelectedDataProvider(model:model);
            PopulateAvailableDataProviders(model:model);
            return View(model:model);
        }

        private ActionResult RenderImportSelectFileView(SampleImportViewModel model)
        {
            PopulateSelectedDataProvider(model:model);

            ViewBag.MaxFileSize = _fileStoreService.GetMaxFileSize();
            return View(model:model);
        }

        private ActionResult RenderImportFileValidationView(SampleImportViewModel model)
        {
            PopulateSelectedDataProvider(model:model);
            PopulateSampleImportTempFile(model:model);
            PopulateRowObjectsOrFileValidationErrors(model:model);
            
            ViewBag.MaxFileSize = _fileStoreService.GetMaxFileSize();
            return View(model:model);
        }

        private ActionResult RenderImportSelectDataDefaultView(SampleImportViewModel model)
        {
            PopulateSelectedDataProvider(model:model);
            PopulateSampleImportTempFile(model:model);
            PopulateRowObjectsOrFileValidationErrors(model:model);
            PopulateRequiredDefaultValuesDropdownsForEmptyRecommendedCells(model:model);

            return View(model:model);
        }

        private ActionResult RenderImportDataTranslationsView(SampleImportViewModel model)
        {
            PopulateSelectedDataProvider(model:model);
            PopulateSampleImportTempFile(model:model);
            PopulateRowObjectsOrFileValidationErrors(model:model);
            PopulateRequiredDefaultValuesDropdownsForEmptyRecommendedCells(model:model);
            PopulateEmptyRecommendedCellsWithDataDefaults(model:model);
            PopulateDataTranslationsAndMissingTranslationIfExists(model:model);

            return View(model:model);
        }

        private ActionResult RenderImportDataValidationView(SampleImportViewModel model)
        {
            PopulateSelectedDataProvider(model:model);
            PopulateSampleImportTempFile(model:model);
            PopulateRowObjectsOrFileValidationErrors(model:model);
            PopulateRequiredDefaultValuesDropdownsForEmptyRecommendedCells(model:model);
            PopulateEmptyRecommendedCellsWithDataDefaults(model:model);
            PopulateDataTranslationsAndMissingTranslationIfExists(model:model);
            PopulateSamplesOrDataValidationErrors(model:model);

            return View(model:model);
        }

        private ActionResult RenderImportPreviewView(SampleImportViewModel model)
        {
            PopulateSelectedDataProvider(model:model);
            PopulateSampleImportTempFile(model:model);
            PopulateRowObjectsOrFileValidationErrors(model:model);
            PopulateRequiredDefaultValuesDropdownsForEmptyRecommendedCells(model:model);
            PopulateEmptyRecommendedCellsWithDataDefaults(model:model);
            PopulateDataTranslationsAndMissingTranslationIfExists(model:model);
            PopulateSamplesOrDataValidationErrors(model:model);
            PopulatePreviewData(model:model);

            return View(model:model);
        }

        private ActionResult RedirectSampleImportStepView(SampleImportViewModel model, SampleImportViewModel.SampleImportStep step)
        {
            model.CurrentSampleImportStep = step;
            var importJobId = SampleImportHelpers.ToImportJobId(model:model);

#if DEBUG
            _logger.Debug(message:"Redirect SampleImport: {0}",
                          argument:SampleImportHelpers.ToJsonString(value:SampleImportHelpers.ToSampleImportQueryParameters(model:model)));
            _logger.Debug(message:"Redirect importJobId: {0}", argument:importJobId);
#endif

            var url = Url.Action(actionName: "SampleImport", controllerName: "Industry",
                                 routeValues: new RouteValueDictionary { { QueryParameters.ImportJobId, importJobId } })
                      + "#"
                      + step;
            return Redirect(url: url);
        }

        private void DoImportFinalSaveAndUpdateImportSummaryToViewModel(SampleImportViewModel model)
        {
            PopulatePreviewData(model:model);

            _importSampleFromFileService.ImportSampleAndCreateAttachment(sampleImportDto:model.SampleImportDto);

            // ReSharper disable once PossibleInvalidOperationException
            model.ImportFileId = (int) model.SampleImportDto.ImportedFile.FileStoreId;
        }

        private ActionResult RenderImportSummaryView(SampleImportViewModel model)
        {
            if (model.ImportSummary == null)
            {
                ModelState.AddModelError(key:string.Empty, errorMessage:ErrorConstants.SampleImport.ImportSummaryIsOutDate);
            }

            return View(model:model);
        }
    }
}