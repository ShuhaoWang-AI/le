using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.Mvc;
using Linko.LinkoExchange.Web.ViewModels.Industry;
using Linko.LinkoExchange.Web.ViewModels.Shared;

namespace Linko.LinkoExchange.Web.Controllers
{
    public partial class IndustryController
    {
        #region import samples from file

        [Route(template:"Sample/Import")]
        public ActionResult SampleImport()
        {
            var viewModel = new SampleImportViewModel
                            {
                                CurrentSampleImportStep = SampleImportViewModel.SampleImportStep.SelectDataSource
                            };
            try
            {
                var dataSources = GetDataSourcesAndUpdateStepSelectDataSourceAvailableDataSources(viewModel:viewModel);
                var selectedDataSourceId = GetQueryParameterValueAsInt(parameterName:QueryParameters.DataProviderId);
                if (selectedDataSourceId.HasValue)
                {
                    var selectedDataSource = dataSources.Find(x => x.DataSourceId == selectedDataSourceId);
                    if (selectedDataSource == null)
                    {
                        throw new BadRequest(message:ErrorConstants.SampleImport.DataProviderDoesNotExist);
                    }

                    viewModel.SelectedDataSourceId = (int) selectedDataSourceId;
                    viewModel.SelectedDataSourceName = selectedDataSource.Name;
                    ViewBag.MaxFileSize = _fileStoreService.GetMaxFileSize();
                    viewModel.CurrentSampleImportStep = SampleImportViewModel.SampleImportStep.SelectFile;
                }
                else if (dataSources.Count == 1 && dataSources[index:0].DataSourceId.HasValue)
                {
                    viewModel.SelectedDataSourceId = (int) dataSources[index:0].DataSourceId;
                    viewModel.SelectedDataSourceName = dataSources[index:0].Name;
                    ViewBag.MaxFileSize = _fileStoreService.GetMaxFileSize();
                    viewModel.CurrentSampleImportStep = SampleImportViewModel.SampleImportStep.SelectFile;
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(model:viewModel);
        }

        private List<DataSourceDto> GetDataSourcesAndUpdateStepSelectDataSourceAvailableDataSources(SampleImportViewModel viewModel)
        {
            var currentOrganizationRegulatoryProgramId = int.Parse(s: _httpContextService.GetClaimValue(claimType: CacheKey.OrganizationRegulatoryProgramId));
            var dataSources = _dataSourceService.GetDataSources(organizationRegulatoryProgramId: currentOrganizationRegulatoryProgramId);
            if (viewModel.CurrentSampleImportStep == SampleImportViewModel.SampleImportStep.SelectDataSource)
            {
                viewModel.StepSelectDataSource = new StepSelectDataSourceViewModel
                                                 {
                                                     AvailableDataSources = dataSources.Select(x => new SelectListItem
                                                                                                    {
                                                                                                        Text = x.Name,
                                                                                                        Value = x.DataSourceId.ToString(),
                                                                                                        Selected =
                                                                                                            x.DataSourceId.ToString()
                                                                                                             .Equals(value: viewModel
                                                                                                                            .SelectedDataSourceId
                                                                                                                            .ToString())
                                                                                                    }).ToList(),
                                                     AvailableDataProviders = dataSources.Select(x => new ListItemDto
                                                                                                      {
                                                                                                          DisplayValue = x.Name,
                                                                                                          // ReSharper disable once PossibleInvalidOperationException
                                                                                                          Id = (int)x.DataSourceId
                                                                                                      }).ToList()
                                                 };
                viewModel.StepSelectDataSource.AvailableDataSources.Insert(index: 0, item: new SelectListItem { Text = @"Select Data Provider", Value = "0", Disabled = true });
            }
            return dataSources;
        }

        private static string ToJson(dynamic value)
        {
            return new JavaScriptSerializer().Serialize(obj:value);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"Sample/Import")]
        public ActionResult SampleImport(SampleImportViewModel model)
        {
#if DEBUG
            _logger.Debug(message: "SampleImport: {0}", argument:ToJson(value:model));
#endif
            try
            {
                switch (model.CurrentSampleImportStep)
                {
                    case SampleImportViewModel.SampleImportStep.SelectDataSource: return DoImportSelectDataProvider(model:model);
                    case SampleImportViewModel.SampleImportStep.SelectFile: return DoImportSelectFile(model:model);
                    case SampleImportViewModel.SampleImportStep.SelectDataDefault: return DoImportSelectDataDefault(model:model);
                    case SampleImportViewModel.SampleImportStep.DataTranslations: return DoImportDataTranslations(model:model);
                    case SampleImportViewModel.SampleImportStep.ShowPreImportOutput: return DoImportPreview(model:model);
                    case SampleImportViewModel.SampleImportStep.ShowImportOutput: return DoImportFinalSave(model:model);
                    default: throw new InternalServerError(message:$"{model.CurrentSampleImportStep} is unsupported");
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                return View(model:model);
            }
        }

        [AcceptVerbs(verbs: HttpVerbs.Post)]
        public ActionResult SampleImportAddMissingTranslation([CustomDataSourceRequest] DataSourceRequest request, DataSourceTranslationViewModel viewModel)
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
                    viewModel.Id = _dataSourceService.SaveDataSourceTranslation(dataSourceTranslation: dataSourceTranslationDto, translationType: viewModel.TranslationType);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException: rve, modelState: ViewData.ModelState);
            }

            return Json(data: new[] { viewModel }.ToDataSourceResult(request: request, modelState: ModelState));
        }

        private ActionResult DoImportSelectDataProvider(SampleImportViewModel model)
        {
            if (ModelState.IsValid)
            {
                MoveToStep(model:model, nextStep:SampleImportViewModel.SampleImportStep.SelectFile);
                ViewBag.MaxFileSize = _fileStoreService.GetMaxFileSize();
                return DoImportSelectFile(model:model);
            }

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
            return View(model:model);
        }

        private void MoveToStep(SampleImportViewModel model, SampleImportViewModel.SampleImportStep nextStep)
        {
            ModelState.Remove(key:"CurrentSampleImportStep");
            model.CurrentSampleImportStep = nextStep;
            _logger.Debug(message:"SampleImport move to next step {0}", argument:nextStep);
        }

        private ActionResult DoImportSelectFile(SampleImportViewModel model)
        {
            if (!model.ImportTempFileId.HasValue)
            {
                return View(model: model);
            }

            MoveToStep(model: model, nextStep: SampleImportViewModel.SampleImportStep.FileValidation);
            return DoImportFileValidation(model:model);
        }

        private ActionResult DoImportFileValidation(SampleImportViewModel model)
        {
            var fileValidationErrorView = LoadSampleImportDtoAndGetFileValidationErrorViewOrNullToContinue(model:model);
            if (fileValidationErrorView != null)
            {
                return fileValidationErrorView;
            }

            MoveToStep(model: model, nextStep: SampleImportViewModel.SampleImportStep.SelectDataDefault);
            return DoImportSelectDataDefault(model:model);
        }

        private ActionResult LoadSampleImportDtoAndGetFileValidationErrorViewOrNullToContinue(SampleImportViewModel model)
        {
            if (!model.ImportTempFileId.HasValue)
            {
                throw new BadRequest(message:ErrorConstants.SampleImport.CannotFindImportFile);
            }
            if (model.SampleImportDto == null)
            {
                // ReSharper disable once PossibleInvalidOperationException
                var importTempFileDto = _importSampleFromFileService.GetImportTempFileById(importTempFileId: model.ImportTempFileId.Value);
                SampleImportDto sampleImportDto;
                var fileValidationResultDto =
                    _importSampleFromFileService.DoFileValidation(dataSourceId: model.SelectedDataSourceId, importTempFileDto: importTempFileDto,
                                                                  sampleImportDto: out sampleImportDto);

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

                    _logger.Debug(message: "Sample/Import has {0} errors at step File Validation", argument: model.StepFileValidation.Errors.Count);

                    MoveToStep(model: model, nextStep: SampleImportViewModel.SampleImportStep.FileValidation);
                    return View(model: model);
                }

                model.SampleImportDto = sampleImportDto;
            }
            
            return null;
        }

        private ActionResult DoImportSelectDataDefault(SampleImportViewModel model)
        {
            var fileValidationErrorView = LoadSampleImportDtoAndGetFileValidationErrorViewOrNullToContinue(model: model);
            if (fileValidationErrorView != null)
            {
                return fileValidationErrorView;
            }

            var requireUserSelectDefaultValueView = GetSelectDefaultValuesViewOrNullToContinue(model:model);
            if (requireUserSelectDefaultValueView != null)
            {
                return requireUserSelectDefaultValueView;
            }

            MoveToStep(model: model, nextStep: SampleImportViewModel.SampleImportStep.DataTranslations);
            return DoImportDataTranslations(model:model);
        }

        private ActionResult GetSelectDefaultValuesViewOrNullToContinue(SampleImportViewModel model)
        {
            var selectedDefaultMonitoringPoint = GetUserSelectedDefaultValue(columnId: model.SelectedDefaultMonitoringPointId,
                                                                             columnText: model.SelectedDefaultMonitoringPointName);
            var selectedDefaultCollectionMethod = GetUserSelectedDefaultValue(columnId: model.SelectedDefaultCollectionMethodId,
                                                                             columnText: model.SelectedDefaultCollectionMethodName);
            var selectedDefaultSampleType = GetUserSelectedDefaultValue(columnId: model.SelectedDefaultSampleTypeId,
                                                                             columnText: model.SelectedDefaultSampleTypeName);

            var requiredDefaultValues = _importSampleFromFileService.GetRequiredDataDefaults(sampleImportDto: model.SampleImportDto,
                                                                                             defaultMonitoringPoint: selectedDefaultMonitoringPoint,
                                                                                             defaultCollectionMethod: selectedDefaultCollectionMethod,
                                                                                             defaultSampleType: selectedDefaultSampleType);

            if (requiredDefaultValues.Count > 0)
            {
                model.SampleImportDto.RequiredDefaultValues = requiredDefaultValues;
                MoveToStep(model: model, nextStep: SampleImportViewModel.SampleImportStep.SelectDataDefault);
                return View(model: model);
            }

            return null;
        }

        private ActionResult DoImportDataTranslations(SampleImportViewModel model)
        {
            var fileValidationErrorView = LoadSampleImportDtoAndGetFileValidationErrorViewOrNullToContinue(model: model);
            if (fileValidationErrorView != null)
            {
                return fileValidationErrorView;
            }

            var requireUserSelectDefaultValueView = GetSelectDefaultValuesViewOrNullToContinue(model: model);
            if (requireUserSelectDefaultValueView != null)
            {
                return requireUserSelectDefaultValueView;
            }

            var dataTranslationView = GetDataTranslationViewOrNullToContinue(model);
            if (dataTranslationView != null)
            {
                return dataTranslationView;
            }

            return DoImportDataValidation(model:model);
        }

        private ActionResult GetDataTranslationViewOrNullToContinue(SampleImportViewModel model)
        {
            var missingTranslations = _importSampleFromFileService.PopulateExistingTranslationDataAndReturnMissingTranslationSet(sampleImportDto:model.SampleImportDto, 
                                                                                                                                 dataSourceId:model.SelectedDataSourceId);
            if (!missingTranslations.Any())
            {
                return null;
            }

            var missingTranslationDto = missingTranslations.First();

            var availableLinkoExchangeTerms = missingTranslationDto.Options.Select(x => new DropdownOptionViewModel
                                                                                        {
                                                                                            Id = x.Id,
                                                                                            DisplayName = x.DisplayValue,
                                                                                            Description = x.Description
                                                                                        })
                                                                   .ToList();
            var defaultLinkoExchangeTerm = availableLinkoExchangeTerms.First();

            ViewData[key: "availableLinkoExchangeTerms"] = availableLinkoExchangeTerms;
            ViewData[key: "defaultLinkoExchangeTerm"] = defaultLinkoExchangeTerm;

            var translationType = ToTranslationType(columnName: missingTranslationDto.SampleImportColumnName);
            var misstingTranslationViewModels = missingTranslationDto.MissingTranslations
                                                                     .Select(term => new DataSourceTranslationViewModel
                                                                     {
                                                                         DataSourceId = model.SelectedDataSourceId,
                                                                         DataSourceTerm = term,
                                                                         TranslationType = translationType,
                                                                         TranslatedItem = defaultLinkoExchangeTerm
                                                                     }).ToList();
            model.MissingTranslation = new MissingTranslationViewModel
            {
                Title = GetDataTranslationSubTitleFromColumnName(columnName: missingTranslationDto.SampleImportColumnName),
                TranslationType = translationType,
                MisstingTranslations = misstingTranslationViewModels
            };
            return View(model: model);
        }

        private DataSourceTranslationType ToTranslationType(SampleImportColumnName columnName)
        {
            switch (columnName)
            {
                case SampleImportColumnName.MonitoringPoint: return DataSourceTranslationType.MonitoringPoint;
                case SampleImportColumnName.SampleType: return DataSourceTranslationType.SampleType;
                case SampleImportColumnName.CollectionMethod: return DataSourceTranslationType.CollectionMethod;
                case SampleImportColumnName.ParameterName: return DataSourceTranslationType.Parameter;
                case SampleImportColumnName.ResultUnit: return DataSourceTranslationType.Unit;
                default: throw new BadRequest(message:$"Cannot convert SampleImportColumnName {columnName} to DataSourceTranslationType.");
            }
        }

        private string GetDataTranslationSubTitleFromColumnName(SampleImportColumnName columnName)
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

        private ActionResult DoImportDataValidation(SampleImportViewModel model)
        {
            var dataValidationErrorView = GetDataValidationErrorViewOrNullToContinue(model);
            if (dataValidationErrorView != null)
            {
                return dataValidationErrorView;
            }

            MoveToStep(model: model, nextStep: SampleImportViewModel.SampleImportStep.ShowPreImportOutput);
            return DoImportPreview(model: model);
        }

        private ActionResult GetDataValidationErrorViewOrNullToContinue(SampleImportViewModel model)
        {
            ImportSampleFromFileValidationResultDto dataValidationResultDto = null;
            if (model.SampleImportDto.SampleDtos == null)
            {
                dataValidationResultDto = _importSampleFromFileService.DoDataValidation(sampleImportDto: model.SampleImportDto);
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

                _logger.Debug(message: "Sample/Import has {0} errors at step Data Validation", argument: model.StepDataValidation.Errors.Count);

                MoveToStep(model: model, nextStep: SampleImportViewModel.SampleImportStep.DataValidation);
                return View(model: model);
            }

            return null;
        }

        private ActionResult DoImportPreview(SampleImportViewModel model)
        {
	        foreach (var sampleDto in model.SampleImportDto.SampleDtos)
	        {
		        _sampleService.SampleComplianceCheck(sampleDto);
	        } 

			model.Samples = model.SampleImportDto.SampleDtos.Select(_mapHelper.ToViewModel).ToList();
			 
			// to attach a identifier for each sample 
	        foreach (var sample in model.Samples)
			{
				sample.Identifier = Guid.NewGuid().ToString();
			}

			return View(model: model);
        }

	    private ActionResult DoImportFinalSave(SampleImportViewModel model)
	    {
		    var fileValidationErrorView = LoadSampleImportDtoAndGetFileValidationErrorViewOrNullToContinue(model:model);
		    if (fileValidationErrorView != null)
		    {
			    return fileValidationErrorView;
		    }

		    var requireUserSelectDefaultValueView = GetSelectDefaultValuesViewOrNullToContinue(model:model);
		    if (requireUserSelectDefaultValueView != null)
		    {
			    return requireUserSelectDefaultValueView;
		    }

            var dataTranslationView = GetDataTranslationViewOrNullToContinue(model);
            if (dataTranslationView != null)
            {

                return dataTranslationView;
            }

            var dataValidationErrorView = GetDataValidationErrorViewOrNullToContinue(model: model);
            if (dataValidationErrorView != null)
            {
                MoveToStep(model: model, nextStep: SampleImportViewModel.SampleImportStep.DataValidation);
                return dataValidationErrorView;
            }

			_importSampleFromFileService.ImportSampleAndCreateAttachment(sampleImportDto: model.SampleImportDto);
			return View(model:model);
	    }

	    public JsonResult ImportSampleFile(SampleImportViewModel model, HttpPostedFileBase upload)
        {
            try
            {
                int id;

                if (upload != null && upload.ContentLength > 0)
                {
                    using (var reader = new BinaryReader(input:upload.InputStream))
                    {
                        var content = reader.ReadBytes(count:upload.ContentLength);

                        var importTempFileDto = new ImportTempFileDto
                                                {
                                                    OriginalFileName = upload.FileName,
                                                    RawFile = content,
                                                    MediaType = upload.ContentType
                                                };

                        id = _importSampleFromFileService.CreateImportTempFile(importTempFileDto:importTempFileDto);
                    }
                }
                else
                {
                    var validationIssues = new List<RuleViolation>();
                    var message = "No file was selected.";
                    validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                    throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                }

                model.ImportTempFileId = id;
                model.SelectedFileName = upload.FileName;

                return Json(data:model, behavior:JsonRequestBehavior.AllowGet);
            }
            catch (RuleViolationException rve)
            {
                Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return Json(data:MvcValidationExtensions.GetViolationErrors(ruleViolationException:rve), behavior:JsonRequestBehavior.AllowGet);
            }
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

        #endregion
    }
}