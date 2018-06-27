using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.DataSource;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.ImportSampleFromFile;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.Mvc;
using Linko.LinkoExchange.Web.Shared;
using Linko.LinkoExchange.Web.ViewModels.Industry;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using DataSource = Kendo.Mvc.UI.DataSource;

namespace Linko.LinkoExchange.Web.Controllers
{
    public partial class IndustryController
    {
        #region dataSources list view

        // GET: /Industry/DataSources
        [Route(template:"DataSources")]
        public ActionResult DataSources()
        {
            var id = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var industry = _organizationService.GetOrganizationRegulatoryProgram(orgRegProgId:id);
            ViewBag.Title = string.Format(format:"{0} Data Providers", arg0:industry.OrganizationDto.OrganizationName);

            return View();
        }

        public ActionResult DataSources_Read([CustomDataSourceRequest] DataSourceRequest request)
        {
            var organizationRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var dataSources = _dataSourceService.GetDataSources(organizationRegulatoryProgramId:organizationRegulatoryProgramId);

            var viewModels = dataSources.Select(vm => new DataSourceViewModel
                                                      {
                                                          Id = vm.DataSourceId,
                                                          Name = vm.Name,
                                                          Description = vm.Description,
                                                          LastModificationDateTimeLocal = vm.LastModificationDateTimeLocal,
                                                          LastModifierUserName = vm.LastModifierFullName
                                                      });

            var result = viewModels.ToDataSourceResult(request:request, selector:vm => new
                                                                                       {
                                                                                           vm.Id,
                                                                                           vm.Name,
                                                                                           vm.Description,
                                                                                           LastModificationDateTimeLocal = vm.LastModificationDateTimeLocal.ToString(provider: CultureInfo.CurrentCulture),
                                                                                           vm.LastModifierUserName
                                                                                       });

            return Json(data:result);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult DataSources_Select(IEnumerable<DataSourceViewModel> items)
        {
            try
            {
                if (items != null && ModelState.IsValid)
                {
                    var item = items.First();
                    return Json(data:new
                                     {
                                         redirect = true,
                                         newurl = Url.Action(actionName:"DataSourceDetails", controllerName:"Industry", routeValues:new

                                                                                                                                    {
                                                                                                                                        id = item.Id
                                                                                                                                    })
                                     });
                }

                return Json(data:new
                                 {
                                     redirect = false,
                                     message = "Please select an Data Provider."
                });
            }
            catch (RuleViolationException rve)
            {
                return Json(data:new
                                 {
                                     redirect = false,
                                     message = MvcValidationExtensions.GetViolationMessages(ruleViolationException:rve)
                                 });
            }
        }
        
        public ActionResult DownloadSampleImportTemplate()
        {
            var fileDto = _importSampleFromFileService.DownloadSampleImportTemplate(fileVersionName:FileVersionTemplateName.SampleImport.ToString());
            var fileStream = new MemoryStream(buffer:fileDto.Data) {Position = 0};

            return File(fileStream:fileStream, contentType:fileDto.ContentType, fileDownloadName:fileDto.Name);
        }

        public ActionResult DownloadSampleImportTemplateInstruction()
        {
            var fileDto = _importSampleFromFileService.DownloadSampleImportTemplateInstruction(fileVersionName:FileVersionTemplateName.SampleImport.ToString());
            var fileStream = new MemoryStream(buffer:fileDto.Data) {Position = 0};

            return File(fileStream:fileStream, contentType:fileDto.ContentType, fileDownloadName:fileDto.Name);
        }
        #endregion

        #region dataSource details view

        [Route(template:"DataSource/New")]
        public ActionResult NewDataSourceDetails()
        {
            var viewModel = new DataSourceViewModel();

            return View(viewName:"DataSourceDetails", model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"DataSource/New")]
        public ActionResult NewDataSourceDetails(DataSourceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(viewName:"DataSourceDetails", model:model);
            }

            try
            {
                var id = _dataSourceService.SaveDataSource(dataSourceDto:new DataSourceDto
                                                                         {
                                                                             Name = model.Name,
                                                                             Description = model.Description
                                                                         });
                ViewBag.ShowSuccessMessage = true;
                ViewBag.SuccessMessage = "Data Provider created successfully!";
                ModelState.Clear();

                model = PrepareDataSourcesDetails(id: id);
                return View(viewName: "DataSourceDetails", model: model);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                return View(viewName:"DataSourceDetails", model:model);
            }
        }

        // GET: /Industry/DataSource/{id}/Details
        [Route(template:"DataSource/{id:int}/Details")]
        public ActionResult DataSourceDetails(int id)
        {
            var viewModel = PrepareDataSourcesDetails(id:id);

            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"DataSource/{id:int}/Details")]
        public ActionResult DataSourceDetails(int id, DataSourceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model:model);
            }

            try
            {
                _dataSourceService.SaveDataSource(dataSourceDto:new DataSourceDto
                                                                {
                                                                    DataSourceId = model.Id,
                                                                    Name = model.Name,
                                                                    Description = model.Description
                                                                });
                ViewBag.ShowSuccessMessage = true;
                ViewBag.SuccessMessage = "Data Provider updated successfully!";
                ModelState.Clear();
                model = PrepareDataSourcesDetails(id:id);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                model = PrepareDataSourcesDetails(id:id);
            }

            return View(viewName:"DataSourceDetails", model:model);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"DataSource/{id:int}/Details/Remove")]
        public ActionResult DataSourceRemove(int id, DataSourceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(viewName:"DataSourceDetails", model:model);
            }

            try
            {
                _dataSourceService.DeleteDataSource(dataSourceId:id);
                TempData[key:"DeleteDataSourceSucceed"] = true;
                return RedirectToAction(actionName:"DataSources");
            }
            catch
            {
                var rve = new RuleViolationException(
                                                     "Validation errors",
                                                     new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage: "Remove Data Provider failed."));
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                model = PrepareDataSourcesDetails(id:id);
                return View(viewName:"DataSourceDetails", model:model);
            }
        }

        private DataSourceViewModel PrepareDataSourcesDetails(int id)
        {
            _dataSourceService.DeleteInvalidDataSourceTranslations(dataSourceId:id);
            var dataSource = _dataSourceService.GetDataSourceById(dataSourceId:id);

            var viewModel = new DataSourceViewModel
                            {
                                Id = dataSource.DataSourceId,
                                Name = dataSource.Name,
                                Description = dataSource.Description,
                                FlowParameterName = _parameterService.GetFlowParameter().Name
                            };
            return viewModel;
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult DataSourceDetails_ImportSamples(DataSourceViewModel model)
        {
            var sampleImportId = SampleImportHelpers.ToImportJobId(model: new SampleImportViewModel
                                                                       {
                                                                           // ReSharper disable once PossibleInvalidOperationException
                                                                           SelectedDataSourceId = (int)model.Id
                                                                       });
            return RedirectToAction(actionName: "SampleImport", routeValues: new RouteValueDictionary {
                                                                                                          { QueryParameters.ImportJobId, sampleImportId}
                                                                                                      });
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult DataSourceDetails_ViewTranslations(DataSourceViewModel model)
        {
            return RedirectToAction(actionName:"DataSourceDetailsTranslations", controllerName:"Industry",
                                    routeValues:new
                                                {
                                                    id = model.Id
                                                });
        }

        #endregion

        #region DataSource Details View Translations

        [Route(template:"DataSource/{id:int}/Details/Translations")]
        public ActionResult DataSourceDetailsTranslations(int id)
        {
            var viewModel = PrepareDataSourcesDetails(id:id);

            PrepareDataSourceTranslationsDropdownLists();

            return View(model:viewModel);
        }

        private void PrepareDataSourceTranslationsDropdownLists()
        {
            var monitoryPointsDropdown = _selectListService.GetIndustryMonitoringPointSelectList()
                                                           .Select(x => _mapHelper.ToDropdownOptionViewModel(fromDto:x))
                                                           .ToList();
            SaveViewDataDataSourceTranslationDropdownOptions(dropdownOptions:monitoryPointsDropdown,
                                                             translationType:DataSourceTranslationType.MonitoringPoint);

            var sampleTypesDropdown = _selectListService.GetAuthoritySampleTypeSelectList()
                                                        .Select(x => _mapHelper.ToDropdownOptionViewModel(fromDto:x))
                                                        .ToList();
            SaveViewDataDataSourceTranslationDropdownOptions(dropdownOptions:sampleTypesDropdown,
                                                             translationType:DataSourceTranslationType.SampleType);

            var collectionMethodsDropdown = _selectListService.GetAuthorityCollectionMethodSelectList()
                                                              .Select(x => _mapHelper.ToDropdownOptionViewModel(fromDto:x))
                                                              .ToList();
            SaveViewDataDataSourceTranslationDropdownOptions(dropdownOptions:collectionMethodsDropdown,
                                                             translationType:DataSourceTranslationType.CollectionMethod);

            var parameterDropdownOptionViewModels = _selectListService.GetAuthorityParameterSelectList()
                                                                      .Select(x => _mapHelper.ToDropdownOptionViewModel(fromDto:x))
                                                                      .ToList();
            SaveViewDataDataSourceTranslationDropdownOptions(dropdownOptions:parameterDropdownOptionViewModels,
                                                             translationType:DataSourceTranslationType.Parameter);

            var unitDropdownOptionViewModels = _selectListService.GetAuthorityUnitSelectList()
                                                                 .Select(x => _mapHelper.ToDropdownOptionViewModel(fromDto:x))
                                                                 .ToList();
            SaveViewDataDataSourceTranslationDropdownOptions(dropdownOptions:unitDropdownOptionViewModels,
                                                             translationType:DataSourceTranslationType.Unit);
        }

        private void SaveViewDataDataSourceTranslationDropdownOptions(List<DropdownOptionViewModel> dropdownOptions,
                                                                      DataSourceTranslationType translationType)
        {
            var dropdownOptionsKey = SampleImportHelpers.GetDropdownOptionsKey(translationType:translationType);
            var defaultDropdownOptionKey = SampleImportHelpers.GetDefaultDropdownOptionKey(translationType:translationType);

            ViewData[key:dropdownOptionsKey] = dropdownOptions;
            ViewData[key:defaultDropdownOptionKey] = dropdownOptions.First();

        }

        public static string GetTranslatedTypeDomainName(DataSourceTranslationType translationType, bool plural = false)
        {
            return DataSourceHelper.GetTranslatedTypeDomainName(translationType:translationType, plural:plural);
        }

        public ActionResult DataSourceDetailsTranslations_Read([CustomDataSourceRequest] DataSourceRequest request,
                                                               int dataSourceId,
                                                               DataSourceTranslationType translationType)
        {
            var dataSourceTranslations = _dataSourceService.GetDataSourceTranslations(dataSourceId:dataSourceId, translationType:translationType);
            var translationNameDict = GetTranslationSelectListDictionary(translationType:translationType);

            var viewModels = dataSourceTranslations.Select(x => CreateDataSourceTranslationViewModel(dataSourceTranslation:x, translationType:translationType,
                                                                                                     translationNameDict:translationNameDict));
            var result = viewModels.ToDataSourceResult(request:request);
            return Json(data:result);
        }

        private Dictionary<int, string> GetTranslationSelectListDictionary(DataSourceTranslationType translationType)
        {
            switch (translationType)
            {
                case DataSourceTranslationType.MonitoringPoint: return _selectListService.GetIndustryMonitoringPointSelectList().ToDictionary(x => x.Id, x => x.DisplayValue);
                case DataSourceTranslationType.SampleType: return _selectListService.GetAuthoritySampleTypeSelectList().ToDictionary(x => x.Id, x => x.DisplayValue);
                case DataSourceTranslationType.CollectionMethod: return _selectListService.GetAuthorityCollectionMethodSelectList().ToDictionary(x => x.Id, x => x.DisplayValue);
                case DataSourceTranslationType.Parameter: return _selectListService.GetAuthorityParameterSelectList().ToDictionary(x => x.Id, x => x.DisplayValue);
                case DataSourceTranslationType.Unit: return _selectListService.GetAuthorityUnitSelectList().ToDictionary(x => x.Id, x => x.DisplayValue);
                default: throw new NotImplementedException(message:$"DataSourceTranslationType {translationType} is unsupported");
            }
        }

        private IDataSourceTranslationViewModel CreateDataSourceTranslationViewModel(DataSourceTranslationDto dataSourceTranslation,
                                                                                    DataSourceTranslationType translationType,
                                                                                    IReadOnlyDictionary<int, string> translationNameDict)
        {
            string displayName;
            if (!translationNameDict.TryGetValue(key:dataSourceTranslation.TranslationItem.TranslationId, value:out displayName))
            {
                throw new InternalServerError(message:$"{translationType} translation '{dataSourceTranslation.DataSourceTerm} is unavailable, "
                                                      + "it should be already automatically removed.");
            }
            var viewModel = DataSourceTranslationViewModelHelper.Create(translationType: translationType,
                                                                        defaultValue: new DropdownOptionViewModel
                                                                                      {
                                                                                          Id = dataSourceTranslation.TranslationItem.TranslationId,
                                                                                          DisplayName = displayName
                                                                                      });

            viewModel.Id = dataSourceTranslation.Id;
            viewModel.DataSourceId = dataSourceTranslation.DataSourceId;
            viewModel.DataSourceTerm = dataSourceTranslation.DataSourceTerm;
            viewModel.TranslationType = translationType;

            return viewModel;
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult UpdateDataSourceMonitoringPointTranslation([CustomDataSourceRequest] DataSourceRequest request, DataSourceMonitoringPointTranslationViewModel viewModel)
        {
            viewModel.TranslatedItem = viewModel.MonitoringPoint;
            return UpdateDataSourceDetailsTranslation(request:request, viewModel:viewModel, translationType:DataSourceTranslationType.MonitoringPoint);
        }

        [AcceptVerbs(verbs: HttpVerbs.Post)]
        public ActionResult UpdateDataSourceCollectionMethodTranslation([CustomDataSourceRequest] DataSourceRequest request, DataSourceCollectionMethodTranslationViewModel viewModel)
        {
            viewModel.TranslatedItem = viewModel.CollectionMethod;
            return UpdateDataSourceDetailsTranslation(request: request, viewModel: viewModel, translationType: DataSourceTranslationType.CollectionMethod);
        }

        [AcceptVerbs(verbs: HttpVerbs.Post)]
        public ActionResult UpdateDataSourceSampleTypeTranslation([CustomDataSourceRequest] DataSourceRequest request, DataSourceSampleTypeTranslationViewModel viewModel)
        {
            viewModel.TranslatedItem = viewModel.SampleType;
            return UpdateDataSourceDetailsTranslation(request: request, viewModel: viewModel, translationType: DataSourceTranslationType.SampleType);
        }

        [AcceptVerbs(verbs: HttpVerbs.Post)]
        public ActionResult UpdateDataSourceParameterTranslation([CustomDataSourceRequest] DataSourceRequest request, DataSourceParameterTranslationViewModel viewModel)
        {
            viewModel.TranslatedItem = viewModel.Parameter;
            return UpdateDataSourceDetailsTranslation(request: request, viewModel: viewModel, translationType: DataSourceTranslationType.Parameter);
        }

        [AcceptVerbs(verbs: HttpVerbs.Post)]
        public ActionResult UpdateDataSourceUnitTranslation([CustomDataSourceRequest] DataSourceRequest request, DataSourceUnitTranslationViewModel viewModel)
        {
            viewModel.TranslatedItem = viewModel.Unit;
            return UpdateDataSourceDetailsTranslation(request: request, viewModel: viewModel, translationType: DataSourceTranslationType.Unit);
        }

        private ActionResult UpdateDataSourceDetailsTranslation([CustomDataSourceRequest] DataSourceRequest request, 
                                                                DataSourceTranslationViewModel viewModel,
                                                                DataSourceTranslationType translationType)
        {
            ValidateDataSourceTranslationViewModel(viewModel:viewModel, translationType:translationType);
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
                                                                             TranslationId = viewModel.TranslatedItem.Id.Value,
                                                                             TranslationName = viewModel.TranslatedItem.DisplayName
                                                       }
                                                   };
                    viewModel.Id = _dataSourceService.SaveDataSourceTranslation(dataSourceTranslation:dataSourceTranslationDto, translationType:translationType);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return Json(data:new[] {viewModel}.ToDataSourceResult(request:request, modelState:ModelState));
        }

        private void ValidateDataSourceTranslationViewModel(DataSourceTranslationViewModel viewModel, DataSourceTranslationType translationType)
        {
            if (string.IsNullOrWhiteSpace(value:viewModel.DataSourceTerm))
            {
                ModelState.AddModelError(key:"DataSourceTerm", errorMessage: @"Import file term is required.");
            }

            if (viewModel.TranslatedItem?.Id != null && viewModel.TranslatedItem.Id != 0)
            {
                return;
            }

            var errorMessage = $"{GetTranslatedTypeDomainName(translationType:translationType)} is required";
            ModelState.AddModelError(key:"TranslatedItem", errorMessage:errorMessage);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult DestroyDataSourceMonitoringPointTranslation([CustomDataSourceRequest] DataSourceRequest request, DataSourceMonitoringPointTranslationViewModel viewModel)
        {
            return DestroyDataSourceTranslations(request:request, viewModel:viewModel, translationType:DataSourceTranslationType.MonitoringPoint);
        }

        [AcceptVerbs(verbs: HttpVerbs.Post)]
        public ActionResult DestroyDataSourceCollectionMethodTranslation([CustomDataSourceRequest] DataSourceRequest request, DataSourceCollectionMethodTranslationViewModel viewModel)
        {
            return DestroyDataSourceTranslations(request: request, viewModel: viewModel, translationType: DataSourceTranslationType.CollectionMethod);
        }

        [AcceptVerbs(verbs: HttpVerbs.Post)]
        public ActionResult DestroyDataSourceSampleTypeTranslation([CustomDataSourceRequest] DataSourceRequest request, DataSourceSampleTypeTranslationViewModel viewModel)
        {
            return DestroyDataSourceTranslations(request: request, viewModel: viewModel, translationType: DataSourceTranslationType.SampleType);
        }

        [AcceptVerbs(verbs: HttpVerbs.Post)]
        public ActionResult DestroyDataSourceParameterTranslation([CustomDataSourceRequest] DataSourceRequest request, DataSourceParameterTranslationViewModel viewModel)
        {
            return DestroyDataSourceTranslations(request: request, viewModel: viewModel, translationType: DataSourceTranslationType.Parameter);
        }

        [AcceptVerbs(verbs: HttpVerbs.Post)]
        public ActionResult DestroyDataSourcUnitTranslation([CustomDataSourceRequest] DataSourceRequest request, DataSourceUnitTranslationViewModel viewModel)
        {
            return DestroyDataSourceTranslations(request: request, viewModel: viewModel, translationType: DataSourceTranslationType.Unit);
        }


        public ActionResult DestroyDataSourceTranslations([CustomDataSourceRequest] DataSourceRequest request, 
                                                          DataSourceTranslationViewModel viewModel,
                                                          DataSourceTranslationType translationType)
        {
            try
            {
                if (viewModel != null && viewModel.Id.HasValue)
                {
                    var dataSourceTranslationDto = new DataSourceTranslationDto
                                                   {
                                                       Id = viewModel.Id,
                                                       DataSourceId = viewModel.DataSourceId,
                                                       DataSourceTerm = viewModel.DataSourceTerm,
                                                       TranslationItem = new DataSourceTranslationItemDto
                                                                         {
                                                                             TranslationId = viewModel.TranslatedItem.Id.Value
                                                                         }
                                                   };
                    _dataSourceService.DeleteDataSourceTranslation(dataSourceTranslation:dataSourceTranslationDto, translationType:translationType);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return Json(data:new[] {viewModel}.ToDataSourceResult(request:request, modelState:ModelState));
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult CreateDataSourceMonitoringPointTranslation([CustomDataSourceRequest] DataSourceRequest request, DataSourceMonitoringPointTranslationViewModel viewModel)
        {
            viewModel.TranslatedItem = viewModel.MonitoringPoint;
            return CreateDataSourceDetailsTranslation(request:request, viewModel:viewModel, translationType:DataSourceTranslationType.MonitoringPoint);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult CreateDataSourceCollectionMethodTranslation([CustomDataSourceRequest] DataSourceRequest request, DataSourceCollectionMethodTranslationViewModel viewModel)
        {
            viewModel.TranslatedItem = viewModel.CollectionMethod;
            return CreateDataSourceDetailsTranslation(request:request, viewModel:viewModel,
                                                      translationType:DataSourceTranslationType.CollectionMethod);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult CreateDataSourceSampleTypeTranslation([CustomDataSourceRequest] DataSourceRequest request, DataSourceSampleTypeTranslationViewModel viewModel)
        {
            viewModel.TranslatedItem = viewModel.SampleType;
            return CreateDataSourceDetailsTranslation(request:request, viewModel:viewModel,
                                                      translationType:DataSourceTranslationType.SampleType);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult CreateDataSourceParameterTranslation([CustomDataSourceRequest] DataSourceRequest request, DataSourceParameterTranslationViewModel viewModel)
        {
            viewModel.TranslatedItem = viewModel.Parameter;
            return CreateDataSourceDetailsTranslation(request:request, viewModel:viewModel,
                                                      translationType:DataSourceTranslationType.Parameter);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult CreateDataSourceUnitTranslation([CustomDataSourceRequest] DataSourceRequest request, DataSourceUnitTranslationViewModel viewModel)
        {
            viewModel.TranslatedItem = viewModel.Unit;
            return CreateDataSourceDetailsTranslation(request:request, viewModel:viewModel,
                                                      translationType:DataSourceTranslationType.Unit);
        }

        private ActionResult CreateDataSourceDetailsTranslation([CustomDataSourceRequest] DataSourceRequest request,
                                                                DataSourceTranslationViewModel viewModel,
                                                                DataSourceTranslationType translationType)
        {
            ValidateDataSourceTranslationViewModel(viewModel:viewModel, translationType:translationType);
            try
            {
                if (ModelState.IsValid)
                {
                    var dataSourceTranslationDto = new DataSourceTranslationDto
                                                   {
                                                       DataSourceId = viewModel.DataSourceId,
                                                       DataSourceTerm = viewModel.DataSourceTerm,
                                                       TranslationItem = new DataSourceTranslationItemDto
                                                                         {
                                                                             TranslationType = translationType,
                                                                             TranslationId = viewModel.TranslatedItem.Id.Value,
                                                                             TranslationName = viewModel.TranslatedItem.DisplayName
                                                                         }
                                                   };
                    viewModel.Id = _dataSourceService.SaveDataSourceTranslation(dataSourceTranslation:dataSourceTranslationDto, translationType:translationType);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return Json(data:new[] {viewModel}.ToDataSourceResult(request:request, modelState:ModelState));
        }

        #endregion
    }
}