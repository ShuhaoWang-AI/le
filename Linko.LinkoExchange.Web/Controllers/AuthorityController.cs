﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.ViewModels.Authority;
using NLog;

namespace Linko.LinkoExchange.Web.Controllers
{
    [RoutePrefix("Authority")]
    public class AuthorityController : Controller
    {
        #region constructor
        
        private readonly IOrganizationService _organizationService;
        private readonly ISessionCache _sessionCache;
        private readonly ILogger _logger;

        public AuthorityController(IOrganizationService organizationService, ISessionCache sessionCache, ILogger logger)
        {
            _organizationService = organizationService;
            _sessionCache = sessionCache;
            _logger = logger;
        }

        #endregion

        #region default action

        // GET: Authority
        public ActionResult Index()
        {
            return RedirectToAction(actionName: "Industries", controllerName: "Authority");
        }

        #endregion

        #region Show Industry list for current user authority

        // GET: /Authority/Industries
        public ActionResult Industries()
        {
            return View();
        }

        // POST: /Authority/Industries
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Industries(IndustryViewModel model, FormCollection collection)
        {
            ViewBag.SearchString = collection["searchString"];

            return View(model);
        }

        public ActionResult Industries_Read([DataSourceRequest] DataSourceRequest request, string searchString)
        {
            int currentOrganizationRegulatoryProgramId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var industries = _organizationService.GetChildOrganizationRegulatoryPrograms(currentOrganizationRegulatoryProgramId, searchString);

            var viewModels = industries.Select(vm => new IndustryViewModel
            {
                ID = vm.OrganizationRegulatoryProgramId,
                IndustryNo = vm.OrganizationDto.OrganizationId,
                IndustryName = vm.OrganizationDto.OrganizationName,
                AddressLine1 = vm.OrganizationDto.AddressLine1,
                AddressLine2 = vm.OrganizationDto.AddressLine2,
                CityName = vm.OrganizationDto.CityName,
                State = vm.OrganizationDto.State,
                ZipCode = vm.OrganizationDto.ZipCode,
                PhoneNumber = vm.OrganizationDto.PhoneNumber,
                PhoneExt = vm.OrganizationDto.PhoneExt,
                FaxNumber = vm.OrganizationDto.FaxNumber,
                WebsiteUrl = vm.OrganizationDto.WebsiteURL,
                IsEnabled = vm.IsEnabled,
                HasSignatory = vm.HasSignatory,
                AssignedTo = vm.AssignedTo
            });

            DataSourceResult result = viewModels.ToDataSourceResult(request, vm => new
            {
                ID = vm.ID,
                IndustryNo = vm.IndustryNo,
                IndustryName = vm.IndustryName,
                Address = vm.Address,
                IsEnabledText = vm.IsEnabledText,
                HasSignatoryText = vm.HasSignatoryText,
                AssignedTo = vm.AssignedTo
            });

            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Industries_Select(IEnumerable<IndustryViewModel> items)
        {
            try
            {
                if (items != null && ModelState.IsValid)
                {
                    var item = items.First();
                    return Json(new
                    {
                        redirect = true,
                        newurl = Url.Action(actionName: "IndustryDetails", controllerName: "Authority", routeValues: new
                        {
                            id = item.ID
                        })
                    });
                }
                else
                {
                    return Json(new
                    {
                        redirect = false,
                        message = "Please select a Industry."
                    });
                }
            }
            catch (RuleViolationException rve)
            {
                return Json(new
                {
                    redirect = false,
                    message = MvcValidationExtensions.GetViolationMessages(rve)
                });
            }
        }
        #endregion


        #region Show Industry Details

        // GET: /Authority/IndustryDetails

        [Route("Industry/{id:int}/Details")]
        public ActionResult IndustryDetails(int id)
        {
            var industry = _organizationService.GetOrganizationRegulatoryProgram(id);
            var userRole = _sessionCache.GetClaimValue(CacheKey.UserRole) ?? "";

            var viewModel = new IndustryViewModel
            {
                ID = industry.OrganizationRegulatoryProgramId,
                IndustryNo = industry.OrganizationDto.OrganizationId,
                IndustryName = industry.OrganizationDto.OrganizationName,
                AddressLine1 = industry.OrganizationDto.AddressLine1,
                AddressLine2 = industry.OrganizationDto.AddressLine2,
                CityName = industry.OrganizationDto.CityName,
                State = industry.OrganizationDto.State,
                ZipCode = industry.OrganizationDto.ZipCode,
                PhoneNumber = industry.OrganizationDto.PhoneNumber,
                PhoneExt = industry.OrganizationDto.PhoneExt,
                FaxNumber = industry.OrganizationDto.FaxNumber,
                WebsiteUrl = industry.OrganizationDto.WebsiteURL,
                IsEnabled = industry.IsEnabled,
                HasSignatory = industry.HasSignatory,
                AssignedTo = industry.AssignedTo,
                LastSubmission = DateTime.Now, //TODO: get last submission date from service when implement //industry.LastSubmission 
                HasPermissionForEnableDisable = userRole.ToLower().Equals(value: "administrator")
            };

            return View(viewModel);
        }

        // POST: /Authority/IndustryDetails
        [AcceptVerbs(HttpVerbs.Post)]
        [Route("Industry/{id:int}/Details")]
        public ActionResult IndustryDetails(IndustryViewModel model)
        {
            try
            {
                bool isUpdated = _organizationService.UpdateEnableDisableFlag(model.ID, !model.IsEnabled);

                if (isUpdated)
                {
                    return RedirectToAction(actionName: "IndustryDetails", controllerName: "Authority", routeValues: new
                    {
                        id = model.ID
                    });
                }
                else
                {
                    // model removes all information except id and IsEnabled as all fields are disabled and not in hidden fields. So need to repopulate again
                    var industry = _organizationService.GetOrganizationRegulatoryProgram(model.ID);
                    var userRole = _sessionCache.GetClaimValue(CacheKey.UserRole) ?? "";

                    model = new IndustryViewModel
                    {
                        ID = industry.OrganizationRegulatoryProgramId,
                        IndustryNo = industry.OrganizationDto.OrganizationId,
                        IndustryName = industry.OrganizationDto.OrganizationName,
                        AddressLine1 = industry.OrganizationDto.AddressLine1,
                        AddressLine2 = industry.OrganizationDto.AddressLine2,
                        CityName = industry.OrganizationDto.CityName,
                        State = industry.OrganizationDto.State,
                        ZipCode = industry.OrganizationDto.ZipCode,
                        PhoneNumber = industry.OrganizationDto.PhoneNumber,
                        PhoneExt = industry.OrganizationDto.PhoneExt,
                        FaxNumber = industry.OrganizationDto.FaxNumber,
                        WebsiteUrl = industry.OrganizationDto.WebsiteURL,
                        IsEnabled = industry.IsEnabled,
                        HasSignatory = industry.HasSignatory,
                        AssignedTo = industry.AssignedTo,
                        LastSubmission = DateTime.Now, //TODO: get last submission date from service when implement //industry.LastSubmission 
                        HasPermissionForEnableDisable = userRole.ToLower().Equals(value: "administrator")
                    };


                    List<RuleViolation> validationIssues = new List<RuleViolation>();
                    string message = "Enable Industry not allowed. No more Industry Licenses are available.  Disable another Industry and try again.";
                    validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                    throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
            }

            return View(model);
        }
        #endregion

    }
}