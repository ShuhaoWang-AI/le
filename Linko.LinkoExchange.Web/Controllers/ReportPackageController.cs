﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Resources;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.FileStore;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Sample;
using Linko.LinkoExchange.Services.Sync;
using Linko.LinkoExchange.Services.Unit;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.Mvc;
using Linko.LinkoExchange.Web.ViewModels.ReportPackage;
using Linko.LinkoExchange.Web.ViewModels.Shared;

namespace Linko.LinkoExchange.Web.Controllers
{
    [PortalAuthorize("authority", "industry")]
    [RoutePrefix(prefix:"ReportPackage")]
    public class ReportPackageController : BaseController
    {
        #region fields

        private readonly IAuthenticationService _authenticationService;
        private readonly LinkoExchangeContext _dbContext;
        private readonly IFileStoreService _fileStoreService;
        private readonly IHttpContextService _httpContextService;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IReportPackageService _reportPackageService;
        private readonly IReportTemplateService _reportTemplateService;
        private readonly ISampleService _sampleService;
        private readonly ISyncService _syncService;
        private readonly IUserService _userService;

        #endregion

        #region constructors and destructor

        public ReportPackageController(
            IAuthenticationService authenticationService,
            IReportPackageService reportPackageService,
            IReportTemplateService reportTemplateService,
            LinkoExchangeContext linkoExchangeContext,
            IFileStoreService fileStoreService,
            ISampleService sampleService,
            IHttpContextService httpContextService,
            IQuestionAnswerService questionAnswerService,
            IUserService userService,
            ISyncService syncService,
            IUnitService unitService)
            : base(httpContextService:httpContextService, userService:userService, reportPackageService:reportPackageService, sampleService:sampleService, unitService:unitService )
        {
            _authenticationService = authenticationService;
            _reportPackageService = reportPackageService;
            _reportTemplateService = reportTemplateService;
            _questionAnswerService = questionAnswerService;
            _dbContext = linkoExchangeContext;
            _fileStoreService = fileStoreService;
            _sampleService = sampleService;
            _httpContextService = httpContextService;
            _userService = userService;
            _syncService = syncService;
        }

        #endregion

        #region default action

        // GET: ReprotPackage
        public ActionResult Index()
        {
            return RedirectToAction(actionName:"ReportPackages", controllerName:"ReportPackage", routeValues:new {reportStatus = ReportStatusName.Draft});
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

        public ActionResult ReportPackages_Read([CustomDataSourceRequest] DataSourceRequest request, ReportStatusName reportStatus)
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
                                                                                           LastModificationDateTimeLocal =
                                                                                           vm.LastModificationDateTimeLocal.ToString(provider:CultureInfo.CurrentCulture),
                                                                                           vm.LastModifierUserName,
                                                                                           LastSentDateTimeLocal = vm.LastSentDateTimeLocal.ToString(),
                                                                                           vm.Name,
                                                                                           vm.OrganizationName,
                                                                                           vm.OrganizationReferenceNumber,
                                                                                           PeriodEndDateTimeLocal =
                                                                                           vm.PeriodEndDateTimeLocal.ToString(provider:CultureInfo.CurrentCulture),
                                                                                           PeriodStartDateTimeLocal =
                                                                                           vm.PeriodStartDateTimeLocal.ToString(provider:CultureInfo.CurrentCulture),
                                                                                           RepudiationDateTimeLocal = vm.RepudiationDateTimeLocal.ToString(),
                                                                                           vm.Repudiator,
                                                                                           vm.Status,
                                                                                           SubmissionDateTimeLocal = vm.SubmissionDateTimeLocal.ToString(),
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
                    var newUrl = Url.Action(actionName:"ReportPackageDetails", controllerName:"ReportPackage", routeValues:new {id = item.Id});
                    return Json(data:new
                                     {
                                         redirect = true,
                                         newurl = newUrl
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
        [PortalAuthorize("industry")]
        public ActionResult NewReportPackage()
        {
            var reportPackageTemplates = _reportTemplateService
                .GetReportPackageTemplates(isForCreatingDraft:true, includeChildObjects:false).Select(vm => new ReportPackageTemplateViewModel
                                                                                                            {
                                                                                                                Id = vm.ReportPackageTemplateId,
                                                                                                                Name = vm.Name,
                                                                                                                Description = vm.Description
                                                                                                            }).OrderBy(c => c.Name).ToList();

            var viewModel = new NewReportPackageViewModel {AllReportPackageTemplates = reportPackageTemplates};

            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"New")]
        [PortalAuthorize("industry")]
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

                    Debug.Assert(condition:model.StartDateTimeLocal != null, message:"model.StartDateTimeLocal != null");
                    Debug.Assert(condition:model.EndDateTimeLocal != null, message:"model.EndDateTimeLocal != null");

                    var id = _reportPackageService.CreateDraft(reportPackageTemplateId:model.SelectedReportPackageTemplateId, startDateTimeLocal:model.StartDateTimeLocal.Value,
                                                               endDateTimeLocal:model.EndDateTimeLocal.Value);

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

            var reportPackageTemplates = _reportTemplateService
                .GetReportPackageTemplates(isForCreatingDraft:true, includeChildObjects:false).Select(vm => new ReportPackageTemplateViewModel
                                                                                                            {
                                                                                                                Id = vm.ReportPackageTemplateId,
                                                                                                                Name = vm.Name,
                                                                                                                Description = vm.Description
                                                                                                            }).OrderBy(c => c.Name).ToList();
            model.AllReportPackageTemplates = reportPackageTemplates;
            return View(model:model);
        }

        /// <summary>
        /// Gets the data necessary to populate the Report Summary Panel in the
        /// Report Package Details page.
        /// </summary>
        /// <param name="reportPackageViewModel">The existing populated view model used to 
        /// display the other panels on the page.</param>
        /// <returns></returns>
        private ReportSummaryViewModel GetReportSummaryViewModel(ReportPackageViewModel reportPackageViewModel)
        {
            var reportSummaryViewModel = new ReportSummaryViewModel()
                                         {
                                             ReportContentReviewItems = new List<ReportContentReviewItem>(),
                                             SamplingRequirementsItems =  new List<SamplingRequirementsItem>()
                                         };

            //
            // Report Content Review (Progress bar and grid)
            //
            
            //Sample Element Types
            foreach (var sampleElement in reportPackageViewModel.SamplesAndResultsTypes)
            {
                var contentReviewItem = new ReportContentReviewItem()
                                        {
                                            ReportElementName = sampleElement.Name,
                                            IsRequired = sampleElement.IsRequired 
                                        };

                //Is "Present in Report" if "Id" can be found in list of "Selected Samples" (Sample Types with at least 1 sample included)
                contentReviewItem.IsPresentInReport =
                    reportPackageViewModel.SelectedSamples.Any(selectedSample => selectedSample.Id == sampleElement.Id 
                                                                && selectedSample.ChildElements.Count > 0);
                reportSummaryViewModel.ReportContentReviewItems.Add(contentReviewItem);

            }

            //Attachment Element Types
            foreach (var attachmentElement in reportPackageViewModel.AttachmentTypes)
            {
                var contentReviewItem = new ReportContentReviewItem()
                                        {
                                            ReportElementName = attachmentElement.Name,
                                            IsRequired = attachmentElement.IsRequired
                                        };


                //Is "Present in Report" if "Id" can be found in list of "Selected Attachments" (Attachment Types with at least 1 file included)
                contentReviewItem.IsPresentInReport =
                    reportPackageViewModel.SelectedAttachments.Any(selectedAttachment => selectedAttachment.Id == attachmentElement.Id
                                                                                 && selectedAttachment.ChildElements.Count > 0);
                reportSummaryViewModel.ReportContentReviewItems.Add(contentReviewItem);

            }

            //Certification Element Types
            foreach (var certificationElement in reportPackageViewModel.CertificationTypes)
            {
                var contentReviewItem = new ReportContentReviewItem()
                                        {
                                            ReportElementName = certificationElement.Name,
                                            IsRequired = certificationElement.IsRequired
                                        };


                //Is "Present in Report" if "Id" can be found in list of "Included Certifications" (and have "Value" as true)
                contentReviewItem.IsPresentInReport =
                    reportPackageViewModel.IncludedCertifications.Any(certification => certification.Key == certificationElement.Id.ToString()
                                                                                         && certification.Value);
                reportSummaryViewModel.ReportContentReviewItems.Add(contentReviewItem);

            }

            //
            // Sample Result Compliance Summary (Progress bar)
            //
            reportSummaryViewModel.SampleResultsInComplianceCount = 0;
            reportSummaryViewModel.SampleResultsNonComplianceCount = 0;

            foreach (var populatedSampleElement in reportPackageViewModel.SelectedSamples.Where(sampleElement => sampleElement.ChildElements.Count > 0))
            {
                foreach (var sample in populatedSampleElement.ChildElements)
                {
                    var sampleDto = _sampleService.GetSampleDetails(sample.Id);
                    foreach (var result in sampleDto.SampleResults)
                    {
                        if (result.ConcentrationResultCompliance == ResultComplianceType.Good
                            || result.ConcentrationResultCompliance == ResultComplianceType.Unknown)
                        {
                            reportSummaryViewModel.SampleResultsInComplianceCount++;
                        }
                        else if (result.ConcentrationResultCompliance == ResultComplianceType.Bad)
                        {
                            reportSummaryViewModel.SampleResultsNonComplianceCount++;
                        }

                        if (result.IsCalcMassLoading && result.MassLoadingValue != null)
                        {
                            if (result.MassResultCompliance == ResultComplianceType.Good
                                || result.MassResultCompliance == ResultComplianceType.Unknown)
                            {
                                reportSummaryViewModel.SampleResultsInComplianceCount++;
                            }
                            else if (result.MassResultCompliance == ResultComplianceType.Bad)
                            {
                                reportSummaryViewModel.SampleResultsNonComplianceCount++;
                            }
                        }

                    }
                }
            }

            //
            //  Sampling Requirements Summary
            //
            var sampleRequirementsDtos = _sampleService.GetSampleRequirements(reportPackageViewModel.PeriodStartDateTimeLocal, reportPackageViewModel.PeriodEndDateTimeLocal, reportPackageViewModel.OrganizationRegulatoryProgramId);
            foreach (var sampleRequirementDto in sampleRequirementsDtos.OrderBy(sr => sr.MonitoringPointName).ThenBy(sr => sr.ParameterName))
            {
                //Need to count the number of results for this parameter at this monitoring point
                //amongst the samples that are included in this report package
                int includedSampleCount = 0;
                if (reportPackageViewModel.SelectedSamples != null)
                {
                    foreach (var sampleElementType in reportPackageViewModel.SelectedSamples)
                    {
                        if (sampleElementType.ChildElements != null && sampleElementType.ChildElements.Count > 0)
                        {
                            foreach (var sampleChildElement in sampleElementType.ChildElements)
                            {
                                var sampleId = sampleChildElement.Id;
                                var sample = _sampleService.GetSampleDetails(sampleId);

                                if (sample.MonitoringPointId == sampleRequirementDto.MonitoringPointId)
                                {
                                    //Iterate through this selected sample's results
                                    foreach (var result in sample.SampleResults)
                                    {
                                        if (result.ParameterId == sampleRequirementDto.ParameterId)
                                        {
                                            includedSampleCount++;
                                        }
                                    }

                                }

                            }

                        }
                    }

                }
                

                reportSummaryViewModel.SamplingRequirementsItems.Add(
                    new SamplingRequirementsItem()
                    {
                        MonitoringPointName = sampleRequirementDto.MonitoringPointName,
                        ParameterName = sampleRequirementDto.ParameterName,
                        ExpectedSampleCount = sampleRequirementDto.TotalSamplesRequiredCount,
                        IncludedSampleCount = includedSampleCount
                    });
            }


            return reportSummaryViewModel;
        }

        [Route(template:"{id:int}/Details")]
        public ActionResult ReportPackageDetails(int id)
        {
            var viewModel = PrepareReportPackageDetails(id:id);
            
            ViewBag.ShowSuccessMessage = TempData[key:"ShowSuccessMessage"] ?? false;
            ViewBag.SuccessMessage = TempData[key:"SuccessMessage"] ?? "";

            ViewBag.ShowSubmissionConfirmationMessage = TempData[key:"ShowSubmissionConfirmationMessage"] ?? false;
            ViewBag.SubmissionConfirmationMessage = TempData[key:"SubmissionConfirmationMessage"] ?? "";

            ViewBag.ShowRepudiationConfirmationMessage = TempData[key:"ShowRepudiationConfirmationMessage"] ?? false;
            ViewBag.RepudiationConfirmationMessage = TempData[key:"RepudiationConfirmationMessage"] ?? "";

            ViewBag.ShowSubmissionReviewConfirmationMessage = TempData[key:"ShowSubmissionReviewConfirmationMessage"] ?? false;
            ViewBag.SubmissionReviewConfirmationMessage = TempData[key:"SubmissionReviewConfirmationMessage"] ?? "";

            ViewBag.ShowRepudiationReviewConfirmationMessage = TempData[key:"ShowRepudiationReviewConfirmationMessage"] ?? false;
            ViewBag.RepudiationReviewConfirmationMessage = TempData[key:"RepudiationReviewConfirmationMessage"] ?? "";

            ViewBag.ShowSendToLinkoCTSSuccessMessage = TempData[key:"ShowSendToLinkoCTSSuccessMessage"] ?? false;
            ViewBag.SendToLinkoCTSSuccessMessage = TempData[key:"SendToLinkoCTSSuccessMessage"] ?? "";

            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"{id:int}/Details")]
        public ActionResult ReportPackageDetails(int id, ReportPackageViewModel model, string selectedSamples, string selectedAttachments, string includedCertifications)
        {
            try
            {
                var vm = _reportPackageService.GetReportPackage(reportPackageId:id, isIncludeAssociatedElementData:true, isAuthorizationRequired:true);

                vm.Comments = model.Comments;

                var serializer = new JavaScriptSerializer();
                var samples = serializer.Deserialize<List<SelectedParentChildCombination>>(input:selectedSamples);
                var attachments = serializer.Deserialize<List<SelectedParentChildCombination>>(input:selectedAttachments);
                var certifications = serializer.Deserialize<IDictionary<string, bool>>(input:includedCertifications);    //key = ReportPackageElementTypeId, value = Certification "Is Included"
                
                vm.SamplesAndResultsTypes = samples.Select(p => new ReportPackageElementTypeDto
                                                                {
                                                                    ReportPackageElementTypeId = p.Id,
                                                                    Samples = p.ChildElements.Select(c => new SampleDto {SampleId = c.Id}).ToList()
                                                                }).ToList();

                vm.AttachmentTypes = attachments.Select(p => new ReportPackageElementTypeDto
                                                             {
                                                                 ReportPackageElementTypeId = p.Id,
                                                                 FileStores = p.ChildElements.Select(c => new FileStoreDto {FileStoreId = c.Id}).ToList()
                                                             }).ToList();

                vm.CertificationTypes = certifications.Select(p => new ReportPackageElementTypeDto
                                                                   {
                                                                       ReportPackageElementTypeId = Convert.ToInt32(p.Key),
                                                                       IsIncluded = (bool)p.Value
                                                                   }).ToList();

                _reportPackageService.SaveReportPackage(reportPackageDto:vm, isUseTransaction:true);

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

        public ActionResult Samples_Read([CustomDataSourceRequest] DataSourceRequest request, int reportPackageElementTypeId, ReportStatusName reportStatusName)
        {
            var dtos = _reportPackageService.GetSamplesForSelection(reportPackageElementTypeId:reportPackageElementTypeId);

            if (reportStatusName != ReportStatusName.Draft)
            {
                dtos = dtos.Where(c => c.IsAssociatedWithReportPackage).ToList();
            }

            var viewModels = dtos.Select(vm => new SampleViewModel
                                               {
                                                   Id = vm.SampleId,
                                                   MonitoringPointName = vm.MonitoringPointName,
                                                   CtsEventTypeName = vm.CtsEventTypeName,
                                                   CollectionMethodName = vm.CollectionMethodName,
                                                   StartDateTimeLocal = vm.StartDateTimeLocal,
                                                   EndDateTimeLocal = vm.EndDateTimeLocal,
                                                   LabSampleIdentifier = vm.LabSampleIdentifier,
                                                   IsAssociatedWithReportPackage = vm.IsAssociatedWithReportPackage,
                                                   LastSubmitted = vm.LastSubmissionDateTimeLocal,
                                                   SampleOverallCompliance = (vm.SampleResults.Any(sr => sr.ConcentrationResultCompliance == ResultComplianceType.Bad || sr.MassResultCompliance == ResultComplianceType.Bad) ? "Bad" : "Good"),
                                                });

            var result = viewModels.ToDataSourceResult(request:request, selector:vm => new
                                                                                       {
                                                                                           vm.Id,
                                                                                           vm.MonitoringPointName,
                                                                                           vm.CtsEventTypeName,
                                                                                           vm.CollectionMethodName,
                                                                                           StartDateTimeLocal = vm.StartDateTimeLocal.ToString(provider:CultureInfo.CurrentCulture),
                                                                                           EndDateTimeLocal = vm.EndDateTimeLocal.ToString(provider:CultureInfo.CurrentCulture),
                                                                                           vm.LabSampleIdentifier,
                                                                                           vm.IsAssociatedWithReportPackage,
                                                                                           LastSubmitted = vm.LastSubmitted.ToString(),
                                                                                           SampleOverallCompliance = vm.SampleOverallCompliance,
                                                                                           SampleOverallComplianceComment = vm.SampleOverallCompliance == ResultComplianceType.Good.ToString() ? Message.OverallSampleComplianceGood : Message.OverallSampleComplianceBad
            });

            return Json(data:result);
        }

        public ActionResult SampleResults_Read([CustomDataSourceRequest] DataSourceRequest request, int sampleId)
        {
            var dto = _sampleService.GetSampleDetails(sampleId:sampleId);

            var sampleresults = dto.SampleResults.Select(sr => new SampleResultViewModel
                                                               {
                                                                   AnalysisDateTimeLocal = sr.AnalysisDateTimeLocal,
                                                                   AnalysisMethod = sr.AnalysisMethod,
                                                                   EnteredMethodDetectionLimit = sr.EnteredMethodDetectionLimit,
                                                                   Id = sr.ConcentrationSampleResultId,
                                                                   IsCalcMassLoading = sr.IsCalcMassLoading,
                                                                   MassLoadingSampleResultId = sr.MassLoadingSampleResultId,
                                                                   MassLoadingQualifier = sr.MassLoadingQualifier,
                                                                   MassLoadingUnitId = sr.MassLoadingUnitId,
                                                                   MassLoadingUnitName = sr.MassLoadingUnitName,
                                                                   MassLoadingValue = sr.MassLoadingValue,
                                                                   ParameterId = sr.ParameterId,
                                                                   ParameterName = sr.ParameterName,
                                                                   Qualifier = sr.Qualifier,
                                                                   UnitId = sr.UnitId,
                                                                   Value = sr.EnteredValue,
                                                                   UnitName = sr.UnitName,
                                                                   ConcentrationResultCompliance = sr.ConcentrationResultCompliance.ToString(),
                                                                   ConcentrationResultComplianceComment = sr.ConcentrationResultComplianceComment,
                                                                   MassResultCompliance = sr.MassResultCompliance.ToString(),
                                                                   MassResultComplianceComment = sr.MassResultComplianceComment
                                                                }).ToList();

            var result = sampleresults.ToDataSourceResult(request:request, selector:vm => new
                                                                                          {
                                                                                              vm.Id,
                                                                                              vm.ParameterName,
                                                                                              Value =
                                                                                              string.IsNullOrWhiteSpace(value:vm.Value)
                                                                                                  ? $"{vm.Qualifier}"
                                                                                                  : $"{vm.Qualifier} {vm.Value} {vm.UnitName}",
                                                                                              MassLoadingValue =
                                                                                              string.IsNullOrWhiteSpace(value:vm.MassLoadingValue)
                                                                                                  ? ""
                                                                                                  : $"{vm.MassLoadingQualifier} {vm.MassLoadingValue} {vm.MassLoadingUnitName}",
                                                                                              vm.AnalysisMethod,
                                                                                              vm.EnteredMethodDetectionLimit,
                                                                                              AnalysisDateTimeLocal = vm.AnalysisDateTimeLocal.ToString(),
                                                                                              vm.ConcentrationResultCompliance,
                                                                                              vm.ConcentrationResultComplianceComment,
                                                                                              vm.MassResultCompliance,
                                                                                              vm.MassResultComplianceComment,
                                                                                              vm.IsCalcMassLoading
                                                                                            });

            return Json(data:result);
        }

        public ActionResult Attachments_Read([CustomDataSourceRequest] DataSourceRequest request, int reportPackageElementTypeId, ReportStatusName reportStatusName)
        {
            var dtos = _reportPackageService.GetFilesForSelection(reportPackageElementTypeId:reportPackageElementTypeId);

            if (reportStatusName != ReportStatusName.Draft)
            {
                dtos = dtos.Where(c => c.IsAssociatedWithReportPackage).ToList();
            }

            var viewModels = dtos.Select(vm => new AttachmentViewModel
                                               {
                                                   Id = vm.FileStoreId,
                                                   Name = vm.Name,
                                                   OriginalFileName = vm.OriginalFileName,
                                                   Description = vm.Description,
                                                   UploadDateTimeLocal = vm.UploadDateTimeLocal,
                                                   UploaderUserFullName = vm.UploaderUserFullName,
                                                   IsAssociatedWithReportPackage = vm.IsAssociatedWithReportPackage,
                                                   LastSubmitted = vm.LastSubmissionDateTimeLocal
                                               });

            var result = viewModels.ToDataSourceResult(request:request, selector:vm => new
                                                                                       {
                                                                                           vm.Id,
                                                                                           vm.Name,
                                                                                           vm.OriginalFileName,
                                                                                           vm.Description,
                                                                                           UploadDateTimeLocal =
                                                                                           vm.UploadDateTimeLocal.ToString(provider:CultureInfo.CurrentCulture),
                                                                                           vm.UploaderUserFullName,
                                                                                           vm.IsAssociatedWithReportPackage,
                                                                                           LastSubmitted = vm.LastSubmitted.ToString()
                                                                                       });

            return Json(data:result);
        }

        [Route(template:"{id:int}/Delete")]
        [PortalAuthorize("industry")]
        public ActionResult DeleteReportPackage(int id)
        {
            try
            {
                var reportPackageDeleted = _reportPackageService.DeleteReportPackage(reportPackageId:id);
                TempData[key:"ReportPackageDeletedSucceed"] = true;
                return RedirectToAction(actionName:"ReportPackages", routeValues:new {reportStatus = reportPackageDeleted.ReportStatusName});
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
        [PortalAuthorize("industry")]
        public ActionResult ReadyToSubmit(int id, ReportPackageViewModel model, string selectedSamples, string selectedAttachments, string includedCertifications)
        {
            var serializer = new JavaScriptSerializer();
            var samples = serializer.Deserialize<List<SelectedParentChildCombination>>(input: selectedSamples);
            var attachments = serializer.Deserialize<List<SelectedParentChildCombination>>(input: selectedAttachments);
            var certifications = serializer.Deserialize<IDictionary<string, bool>>(input: includedCertifications);

            try
            {
                using (var transaction = _dbContext.BeginTransaction())
                {
                    try
                    {
                        var reportPackageDto = _reportPackageService.GetReportPackage(reportPackageId:id, isIncludeAssociatedElementData:true, isAuthorizationRequired:true);

                        reportPackageDto.Comments = model.Comments;

                        reportPackageDto.SamplesAndResultsTypes = samples.Select(p => new ReportPackageElementTypeDto
                                                                        {
                                                                            ReportPackageElementTypeId = p.Id,
                                                                            Samples = p.ChildElements.Select(c => new SampleDto {SampleId = c.Id}).ToList()
                                                                        }).ToList();

                        reportPackageDto.AttachmentTypes = attachments.Select(p => new ReportPackageElementTypeDto
                                                                     {
                                                                         ReportPackageElementTypeId = p.Id,
                                                                         FileStores = p.ChildElements.Select(c => new FileStoreDto {FileStoreId = c.Id}).ToList()
                                                                     }).ToList();

                        reportPackageDto.CertificationTypes = certifications.Select(p => new ReportPackageElementTypeDto
                                                                     {
                                                                         ReportPackageElementTypeId = Convert.ToInt32(p.Key),
                                                                         IsIncluded = (bool)p.Value
                                                                     }).ToList();

                        _reportPackageService.SaveReportPackage(reportPackageDto: reportPackageDto, isUseTransaction:false);
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

            var viewModel = PrepareReportPackageDetails(id: id);
            viewModel.SelectedSamples = samples;
            viewModel.SelectedAttachments = attachments;
            viewModel.IncludedCertifications = certifications;

            //Overwrite the Report Summary view model set in "PrepareReportPackageDetails" with "selected" (but maybe not saved) items
            viewModel.ReportSummaryViewModel = GetReportSummaryViewModel(viewModel);

            return View(viewName: "ReportPackageDetails", model: viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"{id:int}/EnableReportPackage")]
        [PortalAuthorize("industry")]
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
        [PortalAuthorize("industry")]
        public ActionResult SignAndSubmit(int id, ReportPackageViewModel model)
        {
            var isValid = true;
            try
            {
                if (model.Password == null || model.Password.Trim().Length == 0)
                {
                    ModelState.AddModelError(key: "Password", errorMessage: @"Password is required.");
                    isValid = false;
                    ViewBag.ShowSubmissionValidationErrorMessage = true;
                }

                if (model.Answer == null || model.Answer.Trim().Length == 0)
                {
                    ModelState.AddModelError(key: "Answer", errorMessage: @"KBQ answer is required.");
                    isValid = false;
                    ViewBag.ShowSubmissionValidationErrorMessage = true;
                }

                if (isValid)
                {
                    var failedCountPassword = model.FailedCountPassword;
                    var failedCountKbq = model.FailedCountKbq;
                    var result = _authenticationService.ValidatePasswordAndKbq(
                                                                               password: model.Password,
                                                                               userQuestionAnswerId: model.QuestionAnswerId,
                                                                               kbqAnswer: model.Answer,
                                                                               failedPasswordCount: failedCountPassword,
                                                                               failedKbqCount: failedCountKbq,
                                                                               reportOperation: ReportOperation.SignAndSubmit,
                                                                               reportPackageId: id);
                    ModelState.Remove(key: "FailedCountPassword"); // if you don't remove then hidden field does not update on post-back 
                    ModelState.Remove(key: "FailedCountKbq"); // if you don't remove then hidden field does not update on post-back 
                    switch (result.PasswordAndKbqValidationResult)
                    {
                        case PasswordAndKbqValidationResult.Success: break;
                        case PasswordAndKbqValidationResult.IncorrectKbqAnswer:
                            isValid = false;
                            model.FailedCountPassword = failedCountPassword;
                            model.FailedCountKbq = failedCountKbq + 1;
                            ViewBag.ShowSubmissionValidationErrorMessage = true;
                            ViewBag.SubmissionValidationErrorMessage = "Password or KBQ answer is incorrect. Please try again.";
                            break;
                        case PasswordAndKbqValidationResult.InvalidPassword:
                            isValid = false;
                            model.FailedCountPassword = failedCountPassword + 1;
                            model.FailedCountKbq = failedCountKbq;
                            ViewBag.ShowSubmissionValidationErrorMessage = true;
                            ViewBag.SubmissionValidationErrorMessage = "Password or KBQ answer is incorrect. Please try again.";
                            break;
                        case PasswordAndKbqValidationResult.UserLocked_KBQ:
                        case PasswordAndKbqValidationResult.UserLocked_Password:
                            TempData[key: "RegulatoryList"] = result.RegulatoryList;
                            return RedirectToAction(actionName: "AccountLocked", controllerName: "Account");
                        default: throw new ArgumentOutOfRangeException();
                    }
                }
            }
            catch (RuleViolationException rve)
            {
                isValid = false;
                ViewBag.ShowSubmissionValidationErrorMessage = true;
                ViewBag.SubmissionValidationErrorMessage = rve.ValidationIssues[index:0].ErrorMessage;
            }

            try
            {
                if (isValid)
                {
                    _reportPackageService.SignAndSubmitReportPackage(reportPackageId:id);

                    TempData[key:"ShowSubmissionConfirmationMessage"] = true;
                    TempData[key:"SubmissionConfirmationMessage"] = "Report package submitted successfully!";
                    return Redirect(url:Url.Action(actionName:"ReportPackageDetails", controllerName:"ReportPackage", routeValues:new {id}) + "#divSubmissionConfirmation");
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                ViewBag.ShowSubmissionValidationErrorMessage = true;
            }

            model = PrepareReportPackageDetails(id:id, failedCountPassword:model.FailedCountPassword, failedCountKbq:model.FailedCountKbq);
            ModelState.Remove(key:"QuestionAnswerId"); // if you don't remove then hidden field does not update on post-back 
            ModelState.SetModelValue(key:"Answer", value:null); // Remove the old KBQ answer 

            return View(viewName:"ReportPackageDetails", model:model);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"{id:int}/RepudiateReport")]
        [PortalAuthorize("industry")]
        public ActionResult RepudiateReport(int id, ReportPackageViewModel model)
        {
            var isValid = true;
            try
            {
                if (model.Password == null || model.Password.Trim().Length == 0)
                {
                    ModelState.AddModelError(key: "Password", errorMessage: @"Password is required.");
                    isValid = false;
                    ViewBag.ShowRepudiateValidationErrorMessage = true;
                }

                if (model.Answer == null || model.Answer.Trim().Length == 0)
                {
                    ModelState.AddModelError(key: "Answer", errorMessage: @"KBQ answer is required.");
                    isValid = false;
                    ViewBag.ShowRepudiateValidationErrorMessage = true;
                }

                if (isValid)
                {
                    var failedCountPassword = model.FailedCountPassword;
                    var failedCountKbq = model.FailedCountKbq;
                    var result = _authenticationService.ValidatePasswordAndKbq(
                                                                               password: model.Password,
                                                                               userQuestionAnswerId: model.QuestionAnswerId,
                                                                               kbqAnswer: model.Answer,
                                                                               failedPasswordCount: failedCountPassword,
                                                                               failedKbqCount: failedCountKbq,
                                                                               reportOperation: ReportOperation.Repudiation,
                                                                               reportPackageId: id);

                    ModelState.Remove(key: "FailedCountPassword"); // if you don't remove then hidden field does not update on post-back 
                    ModelState.Remove(key: "FailedCountKbq"); // if you don't remove then hidden field does not update on post-back 

                    switch (result.PasswordAndKbqValidationResult)
                    {
                        case PasswordAndKbqValidationResult.Success: break;
                        case PasswordAndKbqValidationResult.IncorrectKbqAnswer:
                            isValid = false;
                            model.FailedCountPassword = failedCountPassword;
                            model.FailedCountKbq = failedCountKbq + 1;
                            ViewBag.ShowRepudiateValidationErrorMessage = true;
                            ViewBag.RepudiateValidationErrorMessage = "Password or KBQ answer is incorrect. Please try again.";
                            break;
                        case PasswordAndKbqValidationResult.InvalidPassword:
                            isValid = false;
                            model.FailedCountPassword = failedCountPassword + 1;
                            model.FailedCountKbq = failedCountKbq;
                            ViewBag.ShowRepudiateValidationErrorMessage = true;
                            ViewBag.RepudiateValidationErrorMessage = "Password or KBQ answer is incorrect. Please try again.";
                            break;
                        case PasswordAndKbqValidationResult.UserLocked_KBQ:
                        case PasswordAndKbqValidationResult.UserLocked_Password:
                            TempData[key: "RegulatoryList"] = result.RegulatoryList;
                            return RedirectToAction(actionName: "AccountLocked", controllerName: "Account");
                        default: throw new ArgumentOutOfRangeException();
                    }
                }
            }
            catch (RuleViolationException rve)
            {
                isValid = false;
                ViewBag.ShowRepudiateValidationErrorMessage = true;
                ViewBag.RepudiateValidationErrorMessage = rve.ValidationIssues[index:0].ErrorMessage;
            }

            try
            {
                if (isValid)
                {
                    _reportPackageService.RepudiateReport(reportPackageId:id, repudiationReasonId:model.RepudiationReasonId ?? 0, repudiationReasonName:model.RepudiationReasonName,
                                                          comments:model.RepudiationComments);

                    var confirmationMessage = "Report Repudiated!<br /><br />";
                    confirmationMessage += "The entire contents of the report including results, if present, are repudiated.<br />";
                    confirmationMessage += "A repudiation receipt email has been sent.<br />";

                    TempData[key:"ShowRepudiationConfirmationMessage"] = true;
                    TempData[key:"RepudiationConfirmationMessage"] = confirmationMessage;
                    return Redirect(url:Url.Action(actionName:"ReportPackageDetails", controllerName:"ReportPackage", routeValues:new {id}) + "#divRepudiationConfirmation");
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                ViewBag.ShowRepudiateValidationErrorMessage = true;
            }

            model = PrepareReportPackageDetails(id:id, failedCountPassword:model.FailedCountPassword, failedCountKbq:model.FailedCountKbq);
            ModelState.Remove(key:"QuestionAnswerId"); // if you don't remove then hidden field does not update on post-back 
            ModelState.SetModelValue(key:"Answer", value:null); // Remove the old KBQ answer 

            return View(viewName:"ReportPackageDetails", model:model);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"{id:int}/ReviewSubmission")]
        [PortalAuthorize("authority")]
        public ActionResult ReviewSubmission(int id, ReportPackageViewModel model)
        {
            try
            {
                _reportPackageService.ReviewSubmission(reportPackageId:id, comments:model.SubmissionReviewComments);

                TempData[key:"ShowSubmissionReviewConfirmationMessage"] = true;
                TempData[key:"SubmissionReviewConfirmationMessage"] = "Report Package submission review completed successfully!";

                ModelState.Clear();
                return Redirect(url:Url.Action(actionName:"ReportPackageDetails", controllerName:"ReportPackage", routeValues:new {id}) + "#divSubmissionReview");
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(viewName:"ReportPackageDetails", model:PrepareReportPackageDetails(id:id));
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"{id:int}/ReviewRepudiation")]
        [PortalAuthorize("authority")]
        public ActionResult ReviewRepudiation(int id, ReportPackageViewModel model)
        {
            try
            {
                if (model.RepudiationReviewComments == null || model.RepudiationReviewComments.Trim().Length == 0)
                {
                    ModelState.AddModelError(key:"RepudiationReviewComments", errorMessage:@"Repudiation review comments are required.");
                }
                else
                {
                    _reportPackageService.ReviewRepudiation(reportPackageId:id, comments:model.RepudiationReviewComments);

                    TempData[key:"ShowRepudiationReviewConfirmationMessage"] = true;
                    TempData[key:"RepudiationReviewConfirmationMessage"] = "Report Package repudiation review completed successfully!";

                    ModelState.Clear();
                    return Redirect(url:Url.Action(actionName:"ReportPackageDetails", controllerName:"ReportPackage", routeValues:new {id}) + "#divRepudiationReview");
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(viewName:"ReportPackageDetails", model:PrepareReportPackageDetails(id:id));
        }

        [PortalAuthorize("authority")]
        public bool IsSimilarReportPackageSubmittedAfter(int id)
        {
            var isNewerReportPackageExist = false;
            try
            {
                isNewerReportPackageExist = _reportPackageService.IsSimilarReportPackageSubmittedAfter(reportPackageId:id);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }
            return isNewerReportPackageExist;
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"{id:int}/SendToLinkoCts")]
        [PortalAuthorize("authority")]
        public ActionResult SendToLinkoCts(int id, ReportPackageViewModel model)
        {
            try
            {
                _syncService.SendSubmittedReportPackageToCts(reportPackageId:id);

                TempData[key:"ShowSendToLinkoCTSSuccessMessage"] = true;
                TempData[key:"SendToLinkoCTSSuccessMessage"] = "Report Package queued for LinkoCTS.";

                ModelState.Clear();
                return Redirect(url:Url.Action(actionName:"ReportPackageDetails", controllerName:"ReportPackage", routeValues:new {id}) + "#divSendToLinkoCTS");
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(viewName:"ReportPackageDetails", model:PrepareReportPackageDetails(id:id));
        }

        [Route(template:"{id:int}/Details/Preview")]
        [PortalAuthorize("industry")]
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
                var verifyResult = _reportPackageService.VerifyCopyOfRecord(reportPackageId:id);
                ViewBag.ShowValidateCorMessage = true;
                ViewBag.VerifyResult = verifyResult;
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(viewName:"ReportPackageDetails", model:PrepareReportPackageDetails(id:id));
        }

        public ActionResult DownloadAttachment(int id)
        {
            var fileStore = _fileStoreService.GetFileStoreById(fileStoreId:id, includingFileData:true);
            var fileDownloadName = fileStore.Name;
            var contentType = $"application/${fileStore.MediaType}";
            var fileStream = new MemoryStream(buffer:fileStore.Data) {Position = 0};

            return File(fileStream:fileStream, contentType:contentType, fileDownloadName:fileDownloadName);
        }

        private ReportPackageViewModel PrepareReportPackageDetails(int id, int failedCountPassword = 0, int failedCountKbq = 0)
        {
            var viewModel = new ReportPackageViewModel();

            try
            {
                var vm = _reportPackageService.GetReportPackage(reportPackageId:id, isIncludeAssociatedElementData:true, isAuthorizationRequired:true);

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
                                CtsEventTypeName = string.IsNullOrWhiteSpace(value:vm.CtsEventTypeName) ? "" : $@"{vm.CtsEventTypeName} ({vm.CtsEventCategoryName})",
                                CtsEventTypeId = vm.CtsEventTypeId,
                                ReportPackageTemplateElementCategories = vm.ReportPackageElementCategories,
                                Comments = vm.Comments,
                                SamplesAndResultsTypes = vm.SamplesAndResultsTypes?.Select(t => new ReportElementTypeViewModel
                                                                                                {
                                                                                                    Id = t.ReportPackageElementTypeId,
                                                                                                    Name = t.ReportElementTypeName,
                                                                                                    IsRequired = t.IsRequired
                                                                                                }).ToList(),
                                SelectedSamples = vm.SamplesAndResultsTypes?.Where(t => t.Samples.Count > 0).Select(t => new SelectedParentChildCombination
                                                                                                                         {
                                                                                                                             Id = t.ReportPackageElementTypeId,
                                                                                                                             ChildElements = t
                                                                                                                                 .Samples?.Select(s => new ChildElement
                                                                                                                                                       {
                                                                                                                                                           Id =
                                                                                                                                                               s.SampleId
                                                                                                                                                               ?? 0
                                                                                                                                                       }).ToList()
                                                                                                                         }).ToList(),
                                AttachmentTypes = vm.AttachmentTypes?.Select(t => new ReportElementTypeViewModel
                                                                                  {
                                                                                      Id = t.ReportPackageElementTypeId,
                                                                                      Name = t.ReportElementTypeName,
                                                                                      IsRequired = t.IsRequired
                                                                                  }).ToList(),
                                SelectedAttachments = vm.AttachmentTypes?.Where(t => t.FileStores.Count > 0).Select(t => new SelectedParentChildCombination
                                                                                                                         {
                                                                                                                             Id = t.ReportPackageElementTypeId,
                                                                                                                             ChildElements = t
                                                                                                                                 .FileStores?.Select(f => new ChildElement
                                                                                                                                                          {
                                                                                                                                                              Id = f.FileStoreId ?? 0
                                                                                                                                                          }).ToList()
                                                                                                                         }).ToList(),
                                CertificationTypes = vm.CertificationTypes?.Select(t => new ReportElementTypeViewModel
                                                                                        {
                                                                                            Id = t.ReportPackageElementTypeId,
                                                                                            Name = t.ReportElementTypeName,
                                                                                            Content = t.ReportElementTypeContent,
                                                                                            IsRequired = t.IsRequired,
                                                                                            IsIncluded = t.IsIncluded,
                                                                                        }).ToList(),

                                IncludedCertifications = vm.CertificationTypes?.ToDictionary(t => t.ReportPackageElementTypeId.ToString(), t => t.IsIncluded),

                                IsSubmissionBySignatoryRequired = vm.IsSubmissionBySignatoryRequired,
                                SubmitterFirstName = vm.SubmitterFirstName,
                                SubmitterLastName = vm.SubmitterLastName,
                                SubmissionDateTimeLocal = vm.SubmissionDateTimeLocal,
                                SubmitterTitleRole = vm.SubmitterTitleRole,
                                SubmissionReviewerFirstName = vm.SubmissionReviewerFirstName,
                                SubmissionReviewerLastName = vm.SubmissionReviewerLastName,
                                SubmissionReviewComments = vm.SubmissionReviewComments,
                                SubmissionReviewDateTimeLocal = vm.SubmissionReviewDateTimeLocal,
                                LastSenderFirstName = vm.LastSenderFirstName,
                                LastSenderLastName = vm.LastSenderLastName,
                                LastSentDateTimeLocal = vm.LastSentDateTimeLocal,
                                RepudiatorFirstName = vm.RepudiatorFirstName,
                                RepudiatorLastName = vm.RepudiatorLastName,
                                RepudiatorTitleRole = vm.RepudiatorTitleRole,
                                RepudiationDateTimeLocal = vm.RepudiationDateTimeLocal,
                                RepudiationReasonId = vm.RepudiationReasonId,
                                RepudiationReasonName = vm.RepudiationReasonName,
                                RepudiationComments = vm.RepudiationComments,
                                RepudiationReviewerFirstName = vm.RepudiationReviewerFirstName,
                                RepudiationReviewerLastName = vm.RepudiationReviewerLastName,
                                RepudiationReviewDateTimeLocal = vm.RepudiationReviewDateTimeLocal,
                                RepudiationReviewComments = vm.RepudiationReviewComments,
                                CanCurrentUserSubmitAndReputiate = false,
                                FailedCountPassword = failedCountPassword,
                                FailedCountKbq = failedCountKbq
                            };

                viewModel.IsCurrentPortalAuthority = _httpContextService.GetClaimValue(claimType:CacheKey.PortalName).ToLower().Equals(value:"authority");

                if (!viewModel.IsCurrentPortalAuthority && (viewModel.Status == ReportStatusName.ReadyToSubmit || viewModel.Status == ReportStatusName.Submitted))
                {
                    var currentOrganizationRegulatoryProgramUserId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramUserId));
                    var isCurrentUserSignatory = _userService.GetOrganizationRegulatoryProgramUser(orgRegProgUserId:currentOrganizationRegulatoryProgramUserId).IsSignatory;

                    viewModel.CanCurrentUserSubmitAndReputiate = !viewModel.IsSubmissionBySignatoryRequired || viewModel.IsSubmissionBySignatoryRequired && isCurrentUserSignatory;

                    var currentUserProfileId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId));
                    var userQuestion = _questionAnswerService.GetRandomQuestionAnswerFromUserProfileId(userProfileId:currentUserProfileId, questionType:QuestionTypeName.KBQ);

                    viewModel.QuestionAnswerId = userQuestion.Answer.UserQuestionAnswerId ?? 0;
                    viewModel.Question = userQuestion.Question.Content;
                    viewModel.Answer = "";
                    viewModel.Password = "";

                    if (viewModel.Status == ReportStatusName.Submitted)
                    {
                        if (viewModel.Id != null)
                        {
                            viewModel.HasRepudiationTimeExpired = !_reportPackageService.CanRepudiateReportPackage(reportPackageId:viewModel.Id.Value);
                        }
                        var repudiationReasons = _reportPackageService.GetRepudiationReasons();
                        viewModel.AvailableRepudiationReasonNames = repudiationReasons.Select(c => new SelectListItem
                                                                                                   {
                                                                                                       Text = c.Name,
                                                                                                       Value = c.RepudiationReasonId.ToString()
                                                                                                   }).OrderBy(c => c.Text).ToList();

                        viewModel.AvailableRepudiationReasonNames.Insert(index:0,
                                                                         item:new SelectListItem
                                                                              {
                                                                                  Text = Label.ResourceManager.GetString(name:"SelectReason"),
                                                                                  Value = "0",
                                                                                  Disabled = true
                                                                              });
                        viewModel.RepudiationReasonId = viewModel.RepudiationReasonId ?? 0;
                    }
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            viewModel.ReportSummaryViewModel = GetReportSummaryViewModel(viewModel);

            return viewModel;
        }

        #endregion
    }
}