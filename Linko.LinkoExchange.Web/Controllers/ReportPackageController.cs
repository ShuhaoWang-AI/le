using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.ViewModels.ReportPackage;
using Linko.LinkoExchange.Web.ViewModels.Shared;

namespace Linko.LinkoExchange.Web.Controllers
{
    [RoutePrefix(prefix:"ReportPackage")]
    public class ReportPackageController:Controller
    {
        #region default action

        // GET: ReprotPackage
        public ActionResult Index()
        {
            return RedirectToAction(actionName:"ReportPackages", controllerName:"ReportPackage", routeValues:new {reportStatus = ReportStatusName.Draft});
        }

        #endregion

        #region constructor

        private readonly IReportPackageService _reportPackageService;
        private readonly IReportTemplateService _reportTemplateService;

        public ReportPackageController(IReportPackageService reportPackageService, IReportTemplateService reportTemplateService)
        {
            _reportPackageService = reportPackageService;
            _reportTemplateService = reportTemplateService;
        }

        #endregion

        #region Show Report Package List

        // GET: /ReportPackages
        [Route(template:"ReportPackages/{reportStatus}")]
        public ActionResult ReportPackages(ReportStatusName reportStatus)
        {
            ViewBag.ReportStatusName = reportStatus;
            return View();
        }

        public ActionResult ReportPackages_Read([DataSourceRequest] DataSourceRequest request, ReportStatusName reportStatus)
        {
            var dtos = _reportPackageService.GetReportPackagesByStatusName(reportStatusName:reportStatus, isAuthorityViewing:false);

            var viewModels = dtos.Select(vm => new ReportPackageViewModel
                                               {
                                                   Id = vm.ReportPackageId,
                                                   LastModificationDateTimeLocal = vm.LastModificationDateTimeLocal,
                                                   LastModifierUserName = vm.LastModifierFullName,
                                                   LastSentDateTimeLocal = vm.LastSentDateTimeLocal,
                                                   Name = vm.Name,
                                                   OrganizationName = vm.OrganizationName,
                                                   OrganizationReferenceNumber = vm.OrganizationReferenceNumber,
                                                   PeriodEndDateTimeLocal = vm.PeriodEndDateTimeLocal,
                                                   PeriodStartDateTimeLocal = vm.PeriodStartDateTimeLocal,
                                                   RepudiationDateTimeLocal = vm.RepudiationDateTimeLocal,
                                                   RepudiatorFirstName = vm.RepudiatorFirstName,
                                                   RepudiatorLastName = vm.RepudiatorLastName,
                                                   Status = vm.ReportStatusName,
                                                   SubmissionDateTimeLocal = vm.SubmissionDateTimeLocal,
                                                   SubmitterFirstName = vm.SubmitterFirstName,
                                                   SubmitterLastName = vm.SubmitterLastName
                                               });

            var result = viewModels.ToDataSourceResult(request:request, selector:vm => new
                                                                                       {
                                                                                           vm.Id,
                                                                                           vm.LastModificationDateTimeLocal,
                                                                                           vm.LastModifierUserName,
                                                                                           vm.LastSentDateTimeLocal,
                                                                                           vm.Name,
                                                                                           vm.OrganizationName,
                                                                                           vm.OrganizationReferenceNumber,
                                                                                           vm.PeriodEndDateTimeLocal,
                                                                                           vm.PeriodStartDateTimeLocal,
                                                                                           vm.RepudiationDateTimeLocal,
                                                                                           vm.Repudiator,
                                                                                           vm.Status,
                                                                                           vm.SubmissionDateTimeLocal,
                                                                                           vm.Submitter
                                                                                       });

            return Json(data:result);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult ReportPackages_Select(IEnumerable<ReportPackageViewModel> items)
        {
            try
            {
                if (items != null)
                {
                    var item = items.First();
                    return Json(data:new
                                     {
                                         redirect = true,
                                         newurl = Url.Action(actionName:"ReportPackageDetails", controllerName:"ReportPackage", routeValues:new {id = item.Id})
                                     });
                }
                return Json(data:new
                                 {
                                     redirect = false,
                                     message = "Please select a report package."
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

        #region Report Package Details

        [Route(template:"New")]
        public ActionResult NewReportPackage()
        {
            var reportPackageTemplates = _reportTemplateService.GetReportPackageTemplates(isForCreatingDraft:true, includeChildObjects:false).Select(vm => new ReportPackageTemplateViewModel
                                                                                                                                                           {
                                                                                                                                                               Id = vm.ReportPackageTemplateId,
                                                                                                                                                               Name = vm.Name,
                                                                                                                                                               Description = vm.Description
                                                                                                                                                           }).ToList();

            var viewModel = new NewReportPackageViewModel {AllReportPackageTemplates = reportPackageTemplates};

            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"New")]
        public ActionResult NewReportPackage(NewReportPackageViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.EndDateTimeLocal < model.StartDateTimeLocal)
                    {
                        var validationIssues = new List<RuleViolation>();
                        var message = "End Date must be greater than Start Date.";
                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                        throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                    }

                    var id = _reportPackageService.CreateDraft(reportPackageTemplateId:model.SelectedReportPackageTemplateId, startDateTimeLocal:model.StartDateTimeLocal,
                                                               endDateTimeLocal:model.EndDateTimeLocal);

                    TempData[key:"ShowSuccessMessage"] = true;
                    TempData[key:"SuccessMessage"] = "Report Package created successfully!";

                    ModelState.Clear();
                    return RedirectToAction(actionName:"ReportPackageDetails", controllerName:"ReportPackage", routeValues:new {id});
                }
                catch (RuleViolationException rve)
                {
                    MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                }
            }
            else
            {
                if (ModelState[key:"."] != null)
                {
                    foreach (var issue in ModelState[key:"."].Errors)
                    {
                        ModelState.AddModelError(key:string.Empty, errorMessage:issue.ErrorMessage);
                    }
                }
            }

            var reportPackageTemplates = _reportTemplateService.GetReportPackageTemplates(isForCreatingDraft:true, includeChildObjects:false).Select(vm => new ReportPackageTemplateViewModel
                                                                                                                                                           {
                                                                                                                                                               Id = vm.ReportPackageTemplateId,
                                                                                                                                                               Name = vm.Name,
                                                                                                                                                               Description = vm.Description
                                                                                                                                                           }).ToList();
            model.AllReportPackageTemplates = reportPackageTemplates;
            return View(model:model);
        }

        [Route(template:"{id:int}/Details")]
        public ActionResult ReportPackageDetails(int id)
        {
            var viewModel = PrepareReportPackageDetails(id:id);

            ViewBag.ShowSuccessMessage = TempData[key:"ShowSuccessMessage"] ?? false;
            ViewBag.SuccessMessage = TempData[key:"SuccessMessage"] ?? "";

            return View(model:viewModel);
        }
        
        [Route(template:"{id:int}/Delete")]
        public ActionResult DeleteReportPackage(int id)
        {
            try
            {
                _reportPackageService.DeleteReportPackage(reportPackageId:id);

                return View(viewName:"Confirmation", model:new ConfirmationViewModel
                                                           {
                                                               Title = "Delete Report Package",
                                                               Message = "Report package deleted successfully."
                                                           });
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(viewName:"ReportPackageDetails", model:PrepareReportPackageDetails(id:id));
        }

        private ReportPackageViewModel PrepareReportPackageDetails(int id)
        {
            var viewModel = new ReportPackageViewModel();

            try
            {
                var vm = _reportPackageService.GetReportPackage(reportPackageId:id, isIncludeAssociatedElementData:true);

                viewModel = new ReportPackageViewModel
                            {
                                Id = vm.ReportPackageId,
                                Name = vm.Name,
                                PeriodStartDateTimeLocal = vm.PeriodStartDateTimeLocal,
                                PeriodEndDateTimeLocal = vm.PeriodEndDateTimeLocal,
                                OrganizationRegulatoryProgramId = vm.OrganizationRegulatoryProgramId,
                                OrganizationName = vm.OrganizationName,
                                OrganizationReferenceNumber = vm.OrganizationReferenceNumber,
                                Status = vm.ReportStatusName,
                                CtsEventTypeName = vm.CtsEventTypeName,
                                CtsEventTypeId = vm.CtsEventTypeId,
                                ReportPackageTemplateElementCategories = vm.ReportPackageTemplateElementCategories,
                                Comments = vm.Comments,
                                SamplesAndResultsTypes = vm.SamplesAndResultsTypes.Select(t => new ReportElementTypeViewModel
                                                                                                          {
                                                                                                              Id = t.ReportElementTypeId,
                                                                                                              Name = t.ReportElementTypeName
                                                                                                          }).ToList(),
                                AttachmentTypes = vm.AttachmentTypes.Select(t => new ReportElementTypeViewModel
                                                                                                          {
                                                                                                              Id = t.ReportElementTypeId,
                                                                                                              Name = t.ReportElementTypeName
                                                                                                          }).ToList(),
                                CertificationTypes = vm.CertificationTypes.Select(t => new ReportElementTypeViewModel
                                                                                                          {
                                                                                                              Id = t.ReportElementTypeId,
                                                                                                              Name = t.ReportElementTypeName,
                                                                                                              Content = t.ReportElementTypeContent
                                                                                                          }).ToList()

                            };
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }
            return viewModel;
        }

        #endregion

    }
}