﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.Mvc;
using Linko.LinkoExchange.Web.ViewModels.Industry;
using Linko.LinkoExchange.Web.ViewModels.Shared;

namespace Linko.LinkoExchange.Web.Controllers
{
    public partial class IndustryController : BaseController
    {
        #region dataSources list view

        // GET: /Industry/DataSources
        [Route(template:"DataSources")]
        public ActionResult DataSources()
        {
            var id = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var industry = _organizationService.GetOrganizationRegulatoryProgram(orgRegProgId:id);
            ViewBag.Title = string.Format(format:"{0} Data Sources", arg0:industry.OrganizationDto.OrganizationName);

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
                                                                                           vm.LastModificationDateTimeLocal,
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
                                     message = "Please select an Data Source."
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
                ViewBag.SuccessMessage = "Data Source created successfully!";
                ModelState.Clear();
                return RedirectToAction(actionName:"DataSourceDetails", controllerName:"Industry", routeValues:new {id});
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
                ViewBag.SuccessMessage = "Data Source updated successfully!";
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
                                                     new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:"Remove Data Source failed."));
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                model = PrepareDataSourcesDetails(id:id);
                return View(viewName:"DataSourceDetails", model:model);
            }
        }

        private DataSourceViewModel PrepareDataSourcesDetails(int id)
        {
            var dataSource = _dataSourceService.GetDataSourceById(dataSourceId:id);

            var viewModel = new DataSourceViewModel
                            {
                                Id = dataSource.DataSourceId,
                                Name = dataSource.Name,
                                Description = dataSource.Description
                            };
            return viewModel;
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult DataSourceDetails_ImportSamples(DataSourceViewModel model)
        {
            return RedirectToAction(actionName:"SampleImport", controllerName:"Industry",
                                    routeValues:new
                                                {
                                                    id = model.Id
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
            var viewModel = PrepareDataSourcesDetails(id: id);

            PrepareDataSourceTranslationsDropdownLists();

            return View(model: viewModel);
        }

        private void PrepareDataSourceTranslationsDropdownLists()
        {
            var monitoryPointsDropdown = _selectListService.GetIndustryMonitoringPointSelectList()
                                                           .Select(x => _mapHelper.ToDropdownOptionViewModel(fromDto:x))
                                                           .ToList();
            SaveViewDataDataSourceTranslationDropdownOptions(dropdownOptions:monitoryPointsDropdown,
                                                             translationType:DataSourceTranslationType.MonitoringPoint,
                                                             dropdownOptionsKey:"availableAuthorityMonitoringPoints",
                                                             defaultDropdownOptionKey:"defaultAuthorityMonitoringPoint");

            var sampleTypesDropdown = _selectListService.GetAuthoritySampleTypeSelectList()
                                                        .Select(x => _mapHelper.ToDropdownOptionViewModel(fromDto:x))
                                                        .ToList();
            SaveViewDataDataSourceTranslationDropdownOptions(dropdownOptions:sampleTypesDropdown,
                                                             translationType:DataSourceTranslationType.SampleType,
                                                             dropdownOptionsKey:"availableAuthoritySampleTypes",
                                                             defaultDropdownOptionKey:"defaultAuthoritySampleType");

            var collectionMethodsDropdown = _selectListService.GetAuthorityCollectionMethodSelectList()
                                                              .Select(x => _mapHelper.ToDropdownOptionViewModel(fromDto:x))
                                                              .ToList();
            SaveViewDataDataSourceTranslationDropdownOptions(dropdownOptions:collectionMethodsDropdown,
                                                             translationType:DataSourceTranslationType.CollectionMethod,
                                                             dropdownOptionsKey:"availableAuthorityCollectionMethods",
                                                             defaultDropdownOptionKey:"defaultAuthorityCollectionMethod");

            var parameterDropdownOptionViewModels = _selectListService.GetAuthorityParameterSelectList()
                                                                      .Select(x => _mapHelper.ToDropdownOptionViewModel(fromDto:x))
                                                                      .ToList();
            SaveViewDataDataSourceTranslationDropdownOptions(dropdownOptions:parameterDropdownOptionViewModels,
                                                             translationType:DataSourceTranslationType.Parameter,
                                                             dropdownOptionsKey:"availableAuthorityParameters",
                                                             defaultDropdownOptionKey:"defaultAuthorityParameter");

            var unitDropdownOptionViewModels = _selectListService.GetAuthorityUnitSelectList()
                                                                 .Select(x => _mapHelper.ToDropdownOptionViewModel(fromDto:x))
                                                                 .ToList();
            SaveViewDataDataSourceTranslationDropdownOptions(dropdownOptions:unitDropdownOptionViewModels,
                                                             translationType:DataSourceTranslationType.Unit,
                                                             dropdownOptionsKey:"availableAuthorityUnits",
                                                             defaultDropdownOptionKey:"defaultAuthorityUnit");
        }

        private void SaveViewDataDataSourceTranslationDropdownOptions(List<DropdownOptionViewModel> dropdownOptions,
                                                                      DataSourceTranslationType translationType,
                                                                      string dropdownOptionsKey,
                                                                      string defaultDropdownOptionKey)
        {
            dropdownOptions.Insert(index:0, item:new DropdownOptionViewModel
                                                 {
                                                     Id = 0,
                                                     DisplayName = "Please select an existing " + GetTranslatedTypeDomainName(translationType:translationType)
            });
            ViewData[key:dropdownOptionsKey] = dropdownOptions;
            ViewData[key:defaultDropdownOptionKey] = dropdownOptions.First();
        }

        private string GetTranslatedTypeDomainName(DataSourceTranslationType translationType)
        {
            switch (translationType)
            {
                case DataSourceTranslationType.MonitoringPoint: return "Monitoring Point";
                case DataSourceTranslationType.SampleType: return "Sample Type";
                case DataSourceTranslationType.CollectionMethod: return "Collection Method";
                case DataSourceTranslationType.Parameter: return "Parameter";
                case DataSourceTranslationType.Unit: return "Unit";
                default: throw new NotImplementedException(message: $"DataSourceTranslationType {translationType} is unsupported");
            }
        }

        public ActionResult DataSourceDetailsTranslations_Read([CustomDataSourceRequest] DataSourceRequest request,
                                                               int dataSourceId,
                                                               DataSourceTranslationType translationType)
        {
            var dataSourceTranslations = _dataSourceService.GetDataSourceTranslations(dataSourceId:dataSourceId, translationType:translationType);
            var translationNameDict = GetTranslationSelectListDictionay(translationType:translationType);

            var viewModels = dataSourceTranslations.Select(x => CreateDataSourceTranslationViewModel(dataSourceTranslation:x, translationType:translationType,
                                                                                                     translationNameDict:translationNameDict));
            var result = viewModels.ToDataSourceResult(request:request);
            return Json(data:result);
        }

        private Dictionary<int, string> GetTranslationSelectListDictionay(DataSourceTranslationType translationType)
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

        private DataSourceTranslationViewModel CreateDataSourceTranslationViewModel(DataSourceTranslationDto dataSourceTranslation,
                                                                                    DataSourceTranslationType translationType,
                                                                                    IReadOnlyDictionary<int, string> translationNameDict)
        {
            var viewModel = DataSourceTranslationViewModel.Create(translationType:translationType);

            viewModel.Id = dataSourceTranslation.Id;
            viewModel.DataSourceId = dataSourceTranslation.DataSourceId;
            viewModel.DataSourceTerm = dataSourceTranslation.DataSourceTerm;
            viewModel.TranslationType = translationType;
            viewModel.TranslatedItem = new DropdownOptionViewModel
                                       {
                                           Id = dataSourceTranslation.TranslationItem.TranslationId,
                                           DisplayName = translationNameDict[key:dataSourceTranslation.TranslationItem.TranslationId]
                                       };

            return viewModel;
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult DataSourceDetailsTranslations_Update([CustomDataSourceRequest] DataSourceRequest request, DataSourceTranslationViewModel viewModel)
        {
            var translationType = viewModel.TranslationType;
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
                                                                             TranslationId = viewModel.TranslatedItem.Id.Value
                                                                         }
                                                   };
                    _dataSourceService.SaveDataSourceTranslation(dataSourceTranslation:dataSourceTranslationDto, translationType:translationType);
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
            if (viewModel.TranslatedItem?.Id != null && viewModel.TranslatedItem.Id != 0)
            {
                return;
            }

            var errorMessage = $"{GetTranslatedTypeDomainName(translationType:translationType)} is required";
            ModelState.AddModelError(key:"TranslatedItem", errorMessage: errorMessage);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult DataSourceDetailsTranslations_Destroy([CustomDataSourceRequest] DataSourceRequest request, DataSourceTranslationViewModel viewModel)
        {
            try
            {
                if (viewModel != null && viewModel.Id.HasValue)
                {
                    var translationType = viewModel.TranslationType;
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
        public ActionResult DataSourceDetailsTranslations_Create([CustomDataSourceRequest] DataSourceRequest request,
                                                                 DataSourceTranslationViewModel viewModel)
        {
            var translationType = viewModel.TranslationType;
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
                    _dataSourceService.SaveDataSourceTranslation(dataSourceTranslation:dataSourceTranslationDto, translationType:translationType);
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