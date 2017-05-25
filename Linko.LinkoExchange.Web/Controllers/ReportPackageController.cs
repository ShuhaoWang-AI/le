﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Resources;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.ViewModels.ReportPackage;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using Microsoft.Ajax.Utilities;

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

        private readonly IAuthenticationService _authenticationService;
        private readonly IReportPackageService _reportPackageService;
        private readonly IReportTemplateService _reportTemplateService;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContextService;
        private readonly IUserService _userService;

        public ReportPackageController(IAuthenticationService authenticationService, IReportPackageService reportPackageService, IReportTemplateService reportTemplateService,
                                       LinkoExchangeContext linkoExchangeContext,
                                       IHttpContextService httpContextService, IQuestionAnswerService questionAnswerService, IUserService userService)
        {
            _authenticationService = authenticationService;
            _reportPackageService = reportPackageService;
            _reportTemplateService = reportTemplateService;
            _questionAnswerService = questionAnswerService;
            _dbContext = linkoExchangeContext;
            _httpContextService = httpContextService;
            _userService = userService;
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
            var dtos = _reportPackageService.GetReportPackagesByStatusName(reportStatusName:reportStatus);

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

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"{id:int}/Details")]
        public ActionResult ReportPackageDetails(int id, ReportPackageViewModel model)
        {
            try
            {
                var vm = _reportPackageService.GetReportPackage(reportPackageId:id, isIncludeAssociatedElementData:true);

                // TODO: need to update the ReportPackageDto using ReportPackageViewModel
                vm.Comments = model.Comments;

                _reportPackageService.SaveReportPackage(reportPackageDto:vm, isUseTransaction:false);

                TempData[key:"ShowSuccessMessage"] = true;
                TempData[key:"SuccessMessage"] = "Report Package updated successfully!";

                ModelState.Clear();
                return RedirectToAction(actionName:"ReportPackageDetails", controllerName:"ReportPackage", routeValues:new {id});
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(viewName:"ReportPackageDetails", model:model);
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

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"{id:int}/ReadyToSubmit")]
        public ActionResult ReadyToSubmit(int id, ReportPackageViewModel model)
        {
            try
            {
                using (var transaction = _dbContext.BeginTransaction())
                {
                    try
                    {
                        var vm = _reportPackageService.GetReportPackage(reportPackageId:id, isIncludeAssociatedElementData:true);

                        // TODO: need to update the ReportPackageDto using ReportPackageViewModel
                        vm.Comments = model.Comments;

                        _reportPackageService.SaveReportPackage(reportPackageDto:vm, isUseTransaction:false);
                        _reportPackageService.UpdateStatus(reportPackageId:id, reportStatus:ReportStatusName.ReadyToSubmit, isUseTransaction:false);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                TempData[key:"ShowSuccessMessage"] = true;
                TempData[key:"SuccessMessage"] = "Report Package updated successfully!";

                ModelState.Clear();
                return RedirectToAction(actionName:"ReportPackageDetails", controllerName:"ReportPackage", routeValues:new {id});
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(viewName:"ReportPackageDetails", model:model);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"{id:int}/EnableReportPackage")]
        public ActionResult EnableReportPackage(int id)
        {
            try
            {
                _reportPackageService.UpdateStatus(reportPackageId:id, reportStatus:ReportStatusName.Draft, isUseTransaction:true);

                TempData[key:"ShowSuccessMessage"] = true;
                TempData[key:"SuccessMessage"] = "Report Package updated successfully!";

                ModelState.Clear();
                return RedirectToAction(actionName:"ReportPackageDetails", controllerName:"ReportPackage", routeValues:new {id});
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(viewName:"ReportPackageDetails", model:PrepareReportPackageDetails(id:id));
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"{id:int}/SignAndSubmit")]
        public ActionResult SignAndSubmit(int id, ReportPackageViewModel model)
        {
            if (TempData[key:"FailedCount"] == null)
            {
                // first attempt
                TempData[key:"FailedCount"] = 0;
            }
            else
            {
                TempData[key:"FailedCount"] = TempData[key:"FailedCount"];
            }

            try
            {
                var isValid = true;

                if (model.IsSubmissionBySignatoryRequired)
                {
                    // TODO: need to verify user credential before submission and also check current user has permission to submit or not
                    if (model.Password == null || model.Password.Trim().Length == 0)
                    {
                        ModelState.AddModelError(key:"Password", errorMessage:@"Password is required.");
                        isValid = false;
                    }

                    if (model.Answer == null || model.Answer.Trim().Length == 0)
                    {
                        ModelState.AddModelError(key:"Answer", errorMessage:@"KBQ answer is required.");
                        isValid = false;
                    }

                    if (isValid)
                    {
                        var currentUserName = _httpContextService.GetClaimValue(claimType:CacheKey.UserName);
                        var failedCount = int.Parse(s:TempData[key:"FailedCount"].ToString().IfNullOrWhiteSpace(defaultValue:"0"));
                        var result = _authenticationService.SignInByUserName(userName:currentUserName, password:model.Password, isPersistent:false).Result;

                        switch (result.AutehticationResult)
                        {
                            case AuthenticationResult.Success:
                                isValid = _questionAnswerService.ConfirmCorrectAnswer(userQuestionAnswerId:model.QuestionAnswerId, answer:model.Answer);
                                if (!isValid)
                                {
                                    TempData[key:"FailedCount"] = failedCount + 1;
                                    ViewBag.ShowSubmissionValidationErrorMessage = true;
                                    ViewBag.SubmissionValidationErrorMessage = "The answer is incorrect. Please try again.";
                                }
                                break;
                            default:
                                isValid = false;
                                TempData[key:"FailedCount"] = failedCount + 1;
                                ViewBag.ShowSubmissionValidationErrorMessage = true;
                                ViewBag.SubmissionValidationErrorMessage = Message.InvalidLoginAttempt;
                                break;
                        }
                    }
                }

                if (isValid)
                {
                    _reportPackageService.SignAndSubmitReportPackage(reportPackageId:id);

                    return View(viewName:"Confirmation", model:new ConfirmationViewModel
                                                               {
                                                                   Title = "Report Package Submission Confirmation ",
                                                                   Message = "Report package submitted successfully."
                                                               });
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            TempData.Keep(key:"FailedCount");
            return View(viewName:"ReportPackageDetails", model:PrepareReportPackageDetails(id:id));
        }

        [Route(template:"{id:int}/Details/Preview")]
        public ActionResult ReportPackagePreview(int id)
        {
            try
            {
                var fileStore = _reportPackageService.GetReportPackageCopyOfRecordPdfFile(reportPackageId:id);
                var fileDownloadName = fileStore.FileName;
                const string contentType = "application/pdf";
                var fileStream = new MemoryStream(buffer:fileStore.FileData) {Position = 0};

                return File(fileStream:fileStream, contentType:contentType, fileDownloadName:fileDownloadName);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(viewName:"ReportPackageDetails", model:PrepareReportPackageDetails(id:id));
        }

        [Route(template:"{id:int}/COR")]
        public ActionResult DownloadCor(int id)
        {
            try
            {
                var copyOfRecordDto = _reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId:id);
                const string contentType = "application/zip";
                var fileDownloadName = copyOfRecordDto.DownloadFileName;
                var dataStream = new MemoryStream(buffer:copyOfRecordDto.Data) {Position = 0};
                return File(fileStream:dataStream, contentType:contentType, fileDownloadName:fileDownloadName);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(viewName:"ReportPackageDetails", model:PrepareReportPackageDetails(id:id));
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"{id:int}/COR/Validate")]
        public ActionResult ValidateCor(int id)
        {
            try
            {
                var verifyResult = _reportPackageService.VerififyCopyOfRecord(reportPackageId:id);
                ViewBag.ShowValidateCorMessage = true;
                ViewBag.VerifyResult = verifyResult;
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
                                SamplesAndResultsTypes = vm.SamplesAndResultsTypes?.Select(t => new ReportElementTypeViewModel
                                                                                                {
                                                                                                    Id = t.ReportElementTypeId,
                                                                                                    Name = t.ReportElementTypeName
                                                                                                }).ToList(),
                                AttachmentTypes = vm.AttachmentTypes?.Select(t => new ReportElementTypeViewModel
                                                                                  {
                                                                                      Id = t.ReportElementTypeId,
                                                                                      Name = t.ReportElementTypeName
                                                                                  }).ToList(),
                                CertificationTypes = vm.CertificationTypes?.Select(t => new ReportElementTypeViewModel
                                                                                        {
                                                                                            Id = t.ReportElementTypeId,
                                                                                            Name = t.ReportElementTypeName,
                                                                                            Content = t.ReportElementTypeContent
                                                                                        }).ToList(),
                                IsSubmissionBySignatoryRequired = vm.IsSubmissionBySignatoryRequired
                            };

                if (viewModel.Status == ReportStatusName.ReadyToSubmit)
                {
                    var currentOrganizationRegulatoryProgramUserId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramUserId));
                    viewModel.IsCurrentUserSignatory = _userService.GetOrganizationRegulatoryProgramUser(orgRegProgUserId:currentOrganizationRegulatoryProgramUserId).IsSignatory;

                    var currentUserProfileId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId));
                    var userQuestion = _questionAnswerService.GetRandomQuestionAnswerFromUserProfileId(userProfileId:currentUserProfileId, questionType:QuestionTypeName.KBQ);

                    viewModel.QuestionAnswerId = userQuestion.Answer.UserQuestionAnswerId ?? 0;
                    viewModel.Question = userQuestion.Question.Content;
                    viewModel.Answer = "";
                    viewModel.Password = "";
                }
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