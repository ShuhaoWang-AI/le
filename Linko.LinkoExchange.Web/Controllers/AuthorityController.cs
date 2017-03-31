using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Parameter;
using Linko.LinkoExchange.Services.Permission;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.Mvc;
using Linko.LinkoExchange.Web.ViewModels.Authority;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using NLog;

namespace Linko.LinkoExchange.Web.Controllers
{
    [RoutePrefix(prefix:"Authority")]
    public class AuthorityController:Controller
    {
        #region constructor

        private readonly IOrganizationService _organizationService;
        private readonly IUserService _userService;
        private readonly IInvitationService _invitationService;
        private readonly ISettingService _settingService;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IPermissionService _permissionService;
        private readonly ISessionCache _sessionCache;
        private readonly IHttpContextService _httpContextService;
        private readonly ILogger _logger;
        private readonly ICromerrAuditLogService _cromerrLogService;
        private readonly IParameterService _parameterService;
        private readonly IReportElementService _reportElementService;
        private readonly IReportTemplateService _reportTemplateService;

        public AuthorityController(IOrganizationService organizationService, IUserService userService, IInvitationService invitationService,
                                   ISettingService settingService, IQuestionAnswerService questionAnswerService, ITimeZoneService timeZoneService, IPermissionService permissionService,
                                   ISessionCache sessionCache, ILogger logger, ICromerrAuditLogService cromerrLogService, IHttpContextService httpContextService, IParameterService parameterService,
                                   IReportElementService reportElementService, IReportTemplateService reportTemplateService)
        {
            _organizationService = organizationService;
            _userService = userService;
            _invitationService = invitationService;
            _settingService = settingService;
            _questionAnswerService = questionAnswerService;
            _timeZoneService = timeZoneService;
            _permissionService = permissionService;
            _sessionCache = sessionCache;
            _logger = logger;
            _cromerrLogService = cromerrLogService;
            _httpContextService = httpContextService;
            _parameterService = parameterService;
            _reportElementService = reportElementService;
            _reportTemplateService = reportTemplateService;
        }

        #endregion
        
        #region default action

        // GET: Authority
        public ActionResult Index()
        {
            return RedirectToAction(actionName:"Industries", controllerName:"Authority");
        }

        #endregion

        #region Show Authority Settings

        // GET: /Authority/Settings
        public ActionResult Settings()
        {
            var viewModel = PrepareAuthoritySettings();

            return View(model:viewModel);
        }

        // POST: /Authority/Settings#AuthoritySettings
        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Settings(AuthoritySettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model:model);
            }

            try
            {
                var id = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                var authority = _organizationService.GetOrganizationRegulatoryProgram(orgRegProgId:id);
                var authoritySettings = _settingService.GetOrganizationSettingsById(organizationId:authority.OrganizationId).Settings;

                //FailedPasswordAttemptMaxCount
                authoritySettings.Where(s => s.TemplateName.Equals(obj:SettingType.FailedPasswordAttemptMaxCount))
                                 .ToList()
                                 .ForEach(s => s.Value = model.FailedPasswordAttemptMaxCount);
                //FailedKbqAttemptMaxCount
                authoritySettings.Where(s => s.TemplateName.Equals(obj:SettingType.FailedKBQAttemptMaxCount)).ToList().ForEach(s => s.Value = model.FailedKbqAttemptMaxCount);
                //InvitationExpiredHours
                authoritySettings.Where(s => s.TemplateName.Equals(obj:SettingType.InvitationExpiredHours)).ToList().ForEach(s => s.Value = model.InvitationExpiredHours);
                //PasswordChangeRequiredDays
                authoritySettings.Where(s => s.TemplateName.Equals(obj:SettingType.PasswordChangeRequiredDays)).ToList().ForEach(s => s.Value = model.PasswordChangeRequiredDays);
                //PasswordHistoryMaxCount
                authoritySettings.Where(s => s.TemplateName.Equals(obj:SettingType.PasswordHistoryMaxCount)).ToList().ForEach(s => s.Value = model.PasswordHistoryMaxCount);
                //TimeZone
                authoritySettings.Where(s => s.TemplateName.Equals(obj:SettingType.TimeZone)).ToList().ForEach(s => s.Value = model.TimeZone);

                _settingService.CreateOrUpdateOrganizationSettings(organizationId:authority.OrganizationId, settingDtos:authoritySettings);

                ViewBag.ShowSuccessMessageForAuthoritySettings = true;
                ViewBag.SuccessMessageForAuthoritySettings = "Save successful.";
                ModelState.Clear();
                model = PrepareAuthoritySettings();
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                ViewBag.ShowErrorMessageForAuthoritySettings = true;
                model = PrepareAuthoritySettings();
            }

            return View(viewName:"Settings", model:model);
        }

        // POST: /Authority/Settings#ProgramSettings
        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"Settings/Program")]
        public ActionResult ProgramSettings(AuthoritySettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(viewName:"Settings", model:model);
            }

            try
            {
                var id = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                var programSettings = _settingService.GetProgramSettingsById(orgRegProgramId:id).Settings;

                //ReportRepudiatedDays
                programSettings.Where(s => s.TemplateName.Equals(obj:SettingType.ReportRepudiatedDays)).ToList().ForEach(s => s.Value = model.ReportRepudiatedDays);
                //EmailContactInfoName
                programSettings.Where(s => s.TemplateName.Equals(obj:SettingType.EmailContactInfoName)).ToList().ForEach(s => s.Value = model.EmailContactInfoName);
                //EmailContactInfoPhone
                programSettings.Where(s => s.TemplateName.Equals(obj:SettingType.EmailContactInfoPhone)).ToList().ForEach(s => s.Value = model.EmailContactInfoPhone);
                //EmailContactInfoEmailAddress
                programSettings.Where(s => s.TemplateName.Equals(obj:SettingType.EmailContactInfoEmailAddress)).ToList()
                               .ForEach(s => s.Value = "" + model.EmailContactInfoEmailAddress);

                _settingService.CreateOrUpdateProgramSettings(orgRegProgId:id, settingDtos:programSettings);

                ViewBag.ShowSuccessMessageForProgramSettings = true;
                ViewBag.SuccessMessageForProgramSettings = "Save successful.";
                ModelState.Clear();
                model = PrepareAuthoritySettings();
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                ViewBag.ShowErrorMessageForProgramSettings = true;
                model = PrepareAuthoritySettings();
            }

            return View(viewName:"Settings", model:model);
        }

        private AuthoritySettingsViewModel PrepareAuthoritySettings()
        {
            var id = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var authority = _organizationService.GetOrganizationRegulatoryProgram(orgRegProgId:id);
            var authoritySettings = _settingService.GetOrganizationSettingsById(organizationId:authority.OrganizationId);
            var programSettings = _settingService.GetProgramSettingsById(orgRegProgramId:authority.OrganizationRegulatoryProgramId);
            var userRole = _httpContextService.GetClaimValue(claimType:CacheKey.UserRole) ?? "";

            var viewModel = new AuthoritySettingsViewModel
                            {
                                Id = authority.OrganizationRegulatoryProgramId,
                                ExchangeAuthorityId = authority.OrganizationDto.OrganizationId,
                                AuthorityName = authority.OrganizationDto.OrganizationName,
                                Npdes = authority.OrganizationDto.PermitNumber,
                                Signer = authority.OrganizationDto.Signer,
                                AddressLine1 = authority.OrganizationDto.AddressLine1,
                                AddressLine2 = authority.OrganizationDto.AddressLine2,
                                CityName = authority.OrganizationDto.CityName,
                                State = authority.OrganizationDto.State,
                                ZipCode = authority.OrganizationDto.ZipCode,
                                PhoneNumber = authority.OrganizationDto.PhoneNumber,
                                PhoneExt = authority.OrganizationDto.PhoneExt,
                                FaxNumber = authority.OrganizationDto.FaxNumber,
                                WebsiteUrl = authority.OrganizationDto.WebsiteURL,
                                HasPermissionForUpdate = userRole.IsCaseInsensitiveEqual(comparing:UserRole.Administrator.ToString()),
                                FailedPasswordAttemptMaxCount = authoritySettings.Settings
                                                                                 .Where(s => s.TemplateName.Equals(obj:SettingType.FailedPasswordAttemptMaxCount))
                                                                                 .Select(s => s.Value).First(),
                                FailedPasswordAttemptMaxCountDefault = authoritySettings.Settings
                                                                                        .Where(s => s.TemplateName.Equals(obj:SettingType.FailedPasswordAttemptMaxCount))
                                                                                        .Select(s => s.DefaultValue).First(),
                                FailedKbqAttemptMaxCount = authoritySettings.Settings
                                                                            .Where(s => s.TemplateName.Equals(obj:SettingType.FailedKBQAttemptMaxCount))
                                                                            .Select(s => s.Value).First(),
                                FailedKbqAttemptMaxCountDefault = authoritySettings.Settings
                                                                                   .Where(s => s.TemplateName.Equals(obj:SettingType.FailedKBQAttemptMaxCount))
                                                                                   .Select(s => s.DefaultValue).First(),
                                InvitationExpiredHours = authoritySettings.Settings
                                                                          .Where(s => s.TemplateName.Equals(obj:SettingType.InvitationExpiredHours))
                                                                          .Select(s => s.Value).First(),
                                InvitationExpiredHoursDefault = authoritySettings.Settings
                                                                                 .Where(s => s.TemplateName.Equals(obj:SettingType.InvitationExpiredHours))
                                                                                 .Select(s => s.DefaultValue).First(),
                                PasswordChangeRequiredDays = authoritySettings.Settings
                                                                              .Where(s => s.TemplateName.Equals(obj:SettingType.PasswordChangeRequiredDays))
                                                                              .Select(s => s.Value).First(),
                                PasswordChangeRequiredDaysDefault = authoritySettings.Settings
                                                                                     .Where(s => s.TemplateName.Equals(obj:SettingType.PasswordChangeRequiredDays))
                                                                                     .Select(s => s.DefaultValue).First(),
                                PasswordHistoryMaxCount = authoritySettings.Settings
                                                                           .Where(s => s.TemplateName.Equals(obj:SettingType.PasswordHistoryMaxCount))
                                                                           .Select(s => s.Value).First(),
                                PasswordHistoryMaxCountDefault = authoritySettings.Settings
                                                                                  .Where(s => s.TemplateName.Equals(obj:SettingType.PasswordHistoryMaxCount))
                                                                                  .Select(s => s.DefaultValue).First(),
                                TimeZone = authoritySettings.Settings
                                                            .Where(s => s.TemplateName.Equals(obj:SettingType.TimeZone))
                                                            .Select(s => s.Value).First(),
                                ReportRepudiatedDays = programSettings.Settings
                                                                      .Where(s => s.TemplateName.Equals(obj:SettingType.ReportRepudiatedDays))
                                                                      .Select(s => s.Value).First(),
                                ReportRepudiatedDaysDefault = programSettings.Settings
                                                                             .Where(s => s.TemplateName.Equals(obj:SettingType.ReportRepudiatedDays))
                                                                             .Select(s => s.DefaultValue).First(),
                                MassLoadingConversionFactorPounds = programSettings.Settings
                                                                                   .Where(s => s.TemplateName.Equals(obj:SettingType.MassLoadingConversionFactorPounds))
                                                                                   .Select(s => s.Value).First(),
                                MassLoadingResultToUseLessThanSign = programSettings.Settings
                                                                                    .Where(s => s.TemplateName.Equals(obj:SettingType.MassLoadingResultToUseLessThanSign))
                                                                                    .Select(s => s.Value).First(),
                                MassLoadingCalculationDecimalPlaces = programSettings.Settings
                                                                                     .Where(s => s.TemplateName.Equals(obj:SettingType.MassLoadingCalculationDecimalPlaces))
                                                                                     .Select(s => s.Value).First(),
                                EmailContactInfoName = programSettings.Settings
                                                                      .Where(s => s.TemplateName.Equals(obj:SettingType.EmailContactInfoName))
                                                                      .Select(s => s.Value).First(),
                                EmailContactInfoNameDefault = authority.OrganizationDto.OrganizationName,
                                EmailContactInfoPhone = programSettings.Settings
                                                                       .Where(s => s.TemplateName.Equals(obj:SettingType.EmailContactInfoPhone))
                                                                       .Select(s => s.Value).First(),
                                EmailContactInfoPhoneDefault = authority.OrganizationDto.PhoneNumber,
                                EmailContactInfoEmailAddress = programSettings.Settings
                                                                              .Where(s => s.TemplateName.Equals(obj:SettingType.EmailContactInfoEmailAddress))
                                                                              .Select(s => s.Value).First(),
                                AuthorityUserLicenseTotalCount = programSettings.Settings
                                                                                .Where(s => s.TemplateName.Equals(obj:SettingType.AuthorityUserLicenseTotalCount))
                                                                                .Select(s => s.Value).First(),
                                AuthorityUserLicenseUsedCount = _organizationService.GetCurrentUserLicenseCount(orgRegProgramId:id).ToString(),
                                IndustryLicenseTotalCount = programSettings.Settings
                                                                           .Where(s => s.TemplateName.Equals(obj:SettingType.IndustryLicenseTotalCount))
                                                                           .Select(s => s.Value).First(),
                                IndustryLicenseUsedCount = _organizationService.GetCurrentIndustryLicenseCount(orgRegProgramId:id).ToString(),
                                UserPerIndustryMaxCount = programSettings.Settings
                                                                         .Where(s => s.TemplateName.Equals(obj:SettingType.UserPerIndustryMaxCount))
                                                                         .Select(s => s.Value).First()
                            };

            // Time Zones
            viewModel.AvailableTimeZones = new List<SelectListItem>();
            var timeZones = _timeZoneService.GetTimeZones();
            if (timeZones.Count > 0)
            {
                viewModel.AvailableTimeZones = timeZones.Select(tz => new SelectListItem
                                                                      {
                                                                          Text = TimeZoneInfo.FindSystemTimeZoneById(id:tz.Name).DisplayName,
                                                                          Value = tz.TimeZoneId.ToString(),
                                                                          Selected = tz.TimeZoneId.ToString().Equals(value:viewModel.TimeZone)
                                                                      }).ToList();
            }
            //viewModel.AvailableTimeZones.Insert(index: 0, item: new SelectListItem { Text = "Select Time Zone", Value = "0" });

            return viewModel;
        }

        #endregion

        #region Show Audit Logs

        // GET: /Authority/AuditLogs
        [AuthorizeAuthorityOnly]
        public ActionResult AuditLogs()
        {
            return View();
        }

        // POST: /Authority/AuditLogs
        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult AuditLogs(AuditLogViewModel model, FormCollection collection)
        {
            ViewBag.SearchString = collection[name:"searchString"];
            return View(model:model);
        }

        private void GetFilterDescriptersFromTree(IList<IFilterDescriptor> filterDescriptors, ref List<FilterDescriptor> foundFilterDescriptors)
        {
            for (var i = 0; i < filterDescriptors.Count(); i++)
            {
                var filter = filterDescriptors[index:i];
                if (filter is CompositeFilterDescriptor)
                {
                    GetFilterDescriptersFromTree(filterDescriptors:((CompositeFilterDescriptor) filter).FilterDescriptors, foundFilterDescriptors:ref foundFilterDescriptors);
                }
                else if (filter is FilterDescriptor)
                {
                    foundFilterDescriptors.Add(item:(FilterDescriptor) filter);

                    //Disable filtering that will happen automatically via ToDataSourceResult
                    //to get around bug 2099 (incorrect filtering with date only)
                    filterDescriptors.RemoveAt(index:i);
                }
            }
        }

        public ActionResult AuditLogs_Read([DataSourceRequest] DataSourceRequest request)
        {
            //Extract DateTime Range Filters to handle special case
            //where we must include all entries that take place for a given day
            DateTime? dateRangeStart = null;
            DateTime? dateRangeEnd = null;
            DateTime? dateToExclude = null;
            string eventCategoryContains = null;
            string eventTypeContains = null;
            string emailAddressContains = null;

            var foundFilterDescriptors = new List<FilterDescriptor>();
            GetFilterDescriptersFromTree(filterDescriptors:request.Filters, foundFilterDescriptors:ref foundFilterDescriptors);

            foreach (var filterDescriptor in foundFilterDescriptors)
            {
                if (filterDescriptor.Member == "LogDateTimeUtc")
                {
                    if (filterDescriptor.Operator == FilterOperator.IsEqualTo)
                    {
                        dateRangeStart = (DateTime) filterDescriptor.Value;
                        dateRangeEnd = ((DateTime) filterDescriptor.Value).AddDays(value:1);
                    }
                    else if (filterDescriptor.Operator == FilterOperator.IsGreaterThan)
                    {
                        dateRangeStart = ((DateTime) filterDescriptor.Value).AddDays(value:1);
                    }
                    else if (filterDescriptor.Operator == FilterOperator.IsGreaterThanOrEqualTo)
                    {
                        dateRangeStart = (DateTime) filterDescriptor.Value;
                    }
                    else if (filterDescriptor.Operator == FilterOperator.IsLessThan)
                    {
                        dateRangeEnd = (DateTime) filterDescriptor.Value;
                    }
                    else if (filterDescriptor.Operator == FilterOperator.IsLessThanOrEqualTo)
                    {
                        dateRangeEnd = ((DateTime) filterDescriptor.Value).AddDays(value:1);
                    }
                    else if (filterDescriptor.Operator == FilterOperator.IsNotEqualTo)
                    {
                        dateToExclude = (DateTime) filterDescriptor.Value;
                    }

                    break;
                }
                if (filterDescriptor.Member == "EventCategory")
                {
                    eventCategoryContains = filterDescriptor.Value.ToString();
                }
                else if (filterDescriptor.Member == "EventType")
                {
                    eventTypeContains = filterDescriptor.Value.ToString();
                }
                else if (filterDescriptor.Member == "EmailAddress")
                {
                    emailAddressContains = filterDescriptor.Value.ToString();
                }
            }

            var page = request.Page;
            var pageSize = request.PageSize;
            var sortColumn = "LogDateTimeUtc";
            var isSortAscending = false;

            if (request.Sorts.Any())
            {
                foreach (var sortDescriptor in request.Sorts)
                {
                    isSortAscending = sortDescriptor.SortDirection == ListSortDirection.Ascending;
                    sortColumn = sortDescriptor.Member;
                }
            }

            var organizationRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(value:_settingService.GetOrganizationSettingValue(orgRegProgramId:organizationRegulatoryProgramId, settingType:SettingType.TimeZone));
            var totalCount = 0;
            var logEntries = _cromerrLogService.GetCromerrAuditLogEntries(organizationRegulatoryProgramId:organizationRegulatoryProgramId,
                                                                          page:page, pageSize:pageSize, sortColumn:sortColumn, isSortAscending:isSortAscending,
                                                                          dateRangeStart:dateRangeStart, dateRangeEnd:dateRangeEnd, dateToExclude:dateToExclude,
                                                                          eventCategoryContains:eventCategoryContains, eventTypeContains:eventTypeContains, emailAddressContains:emailAddressContains,
                                                                          totalCount:out totalCount);

            var viewModels = logEntries.Select(dto => new AuditLogViewModel
                                                      {
                                                          CromerrAuditLogId = dto.CromerrAuditLogId,
                                                          AuditLogTemplateId = dto.AuditLogTemplateId,
                                                          RegulatoryProgramName = dto.RegulatoryProgramName,
                                                          OrganizationId = dto.OrganizationId.Value,
                                                          OrganizationName = dto.OrganizationName,
                                                          RegulatorName = dto.RegulatorOrganizationName,
                                                          EventCategory = dto.EventCategory,
                                                          EventType = dto.EventType,
                                                          UserProfileIdDisplay = dto.UserProfileId.HasValue && dto.UserProfileId > 0 ? dto.UserProfileId.ToString() : "n/a",
                                                          UserName = dto.UserName,
                                                          FirstName = dto.UserFirstName,
                                                          LastName = dto.UserLastName,
                                                          EmailAddress = dto.UserEmailAddress,
                                                          IPAddress = dto.IPAddress,
                                                          HostName = dto.HostName,
                                                          Comment = dto.Comment,
                                                          //Need to modify datetime to local
                                                          LogDateTimeUtc = _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(
                                                                                                                                    utcDateTime:dto.LogDateTimeUtc.DateTime,
                                                                                                                                    timeZoneId:timeZoneId)
                                                      });

            var result = viewModels.ToDataSourceResult(request:request, selector:vm => new
                                                                                       {
                                                                                           vm.CromerrAuditLogId,
                                                                                           vm.AuditLogTemplateId,
                                                                                           vm.RegulatoryProgramName,
                                                                                           vm.OrganizationId,
                                                                                           vm.OrganizationName,
                                                                                           vm.RegulatorName,
                                                                                           vm.EventCategory,
                                                                                           vm.EventType,
                                                                                           vm.UserProfileIdDisplay,
                                                                                           vm.UserName,
                                                                                           vm.FirstName,
                                                                                           vm.LastName,
                                                                                           vm.EmailAddress,
                                                                                           vm.IPAddress,
                                                                                           vm.HostName,
                                                                                           vm.Comment,
                                                                                           LogDateTimeUtc = vm.LogDateTimeUtc.ToString(),
                                                                                           vm.LogDateTimeUtcDetailString
                                                                                       });
            result.Total = totalCount;

            //var result = new DataSourceResult()
            //{
            //    Data = viewModels,
            //    Total = totalCount
            //};

            return Json(data:result);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult AuditLogs_Select(int cromerrAuditLogId)
        {
            try
            {
                if (cromerrAuditLogId != -1 && ModelState.IsValid)
                {
                    var logDetails = _cromerrLogService.GetCromerrAuditLogEntry(cromerrAuditLogId:cromerrAuditLogId).Comment;
                    return Json(data:new
                                     {
                                         redirect = false,
                                         details = logDetails //$"Log Entry Details for Id = {cromerrAuditLogId}"
                                     });
                }
                return Json(data:new
                                 {
                                     redirect = false,
                                     message = "Please select a log entry."
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

        [HttpPost]
        public ActionResult AuditLogs_Excel_Export_Save(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(s:base64);

            return File(fileContents:fileContents, contentType:contentType, fileDownloadName:fileName);
        }

        #endregion

        #region Show Authority Users

        // GET: /Authority/Users
        [Route(template:"Users")]
        public ActionResult AuthorityUsers()
        {
            var id = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var authority = _organizationService.GetOrganizationRegulatoryProgram(orgRegProgId:id);
            ViewBag.Title = string.Format(format:"{0} Users", arg0:authority.OrganizationDto.OrganizationName);

            var remainingUserLicenseCount = _organizationService.GetRemainingUserLicenseCount(orgRegProgramId:id);
            ViewBag.CanInvite = _httpContextService.GetClaimValue(claimType:CacheKey.UserRole).IsCaseInsensitiveEqual(comparing:UserRole.Administrator.ToString())
                                && remainingUserLicenseCount > 0;
            return View();
        }

        public ActionResult AuthorityUsers_Read([DataSourceRequest] DataSourceRequest request)
        {
            var organizationRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var users = _userService.GetUserProfilesForOrgRegProgram(orgRegProgramId:organizationRegulatoryProgramId, isRegApproved:true, isRegDenied:false, isEnabled:null, isRemoved:false);

            var viewModels = users.Select(vm => new AuthorityUserViewModel
                                                {
                                                    Id = vm.OrganizationRegulatoryProgramUserId,
                                                    PId = vm.UserProfileId,
                                                    FirstName = vm.UserProfileDto.FirstName,
                                                    LastName = vm.UserProfileDto.LastName,
                                                    PhoneNumber = vm.UserProfileDto.PhoneNumber,
                                                    Email = vm.UserProfileDto.Email,
                                                    ResetEmail = vm.UserProfileDto.Email,
                                                    DateRegistered = vm.RegistrationDateTimeUtc.Value.DateTime,
                                                    Status = vm.IsEnabled,
                                                    AccountLocked = vm.UserProfileDto.IsAccountLocked,
                                                    Role = vm.PermissionGroup.PermissionGroupId.Value,
                                                    RoleText = vm.PermissionGroup.Name
                                                });

            var result = viewModels.ToDataSourceResult(request:request, selector:vm => new
                                                                                       {
                                                                                           vm.Id,
                                                                                           vm.PId,
                                                                                           vm.FirstName,
                                                                                           vm.LastName,
                                                                                           vm.PhoneNumber,
                                                                                           vm.Email,
                                                                                           vm.ResetEmail,
                                                                                           vm.DateRegistered,
                                                                                           vm.StatusText,
                                                                                           vm.AccountLockedText,
                                                                                           vm.Role,
                                                                                           vm.RoleText
                                                                                       });

            return Json(data:result);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult AuthorityUsers_Select(IEnumerable<AuthorityUserViewModel> items)
        {
            try
            {
                if (items != null && ModelState.IsValid)
                {
                    var item = items.First();
                    return Json(data:new
                                     {
                                         redirect = true,
                                         newurl = Url.Action(actionName:"AuthorityUserDetails", controllerName:"Authority", routeValues:new
                                                                                                                                        {
                                                                                                                                            id = item.Id
                                                                                                                                        })
                                     });
                }
                return Json(data:new
                                 {
                                     redirect = false,
                                     message = "Please select an user."
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

        public ActionResult AuthorityUsers_PendingInvitations_Read([DataSourceRequest] DataSourceRequest request)
        {
            var organizationRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var invitations = _invitationService.GetInvitationsForOrgRegProgram(senderOrgRegProgramId:organizationRegulatoryProgramId, targetOrgRegProgramId:organizationRegulatoryProgramId);

            var viewModels = invitations.Select(vm => new PendingInvitationViewModel
                                                      {
                                                          Id = vm.InvitationId,
                                                          FirstName = vm.FirstName,
                                                          LastName = vm.LastName,
                                                          Email = vm.EmailAddress,
                                                          DateInvited = vm.InvitationDateTimeUtc.DateTime,
                                                          InviteExpires = vm.ExpiryDateTimeUtc.DateTime
                                                      });

            var result = viewModels.ToDataSourceResult(request:request, selector:vm => new
                                                                                       {
                                                                                           vm.Id,
                                                                                           vm.FirstName,
                                                                                           vm.LastName,
                                                                                           vm.Email,
                                                                                           vm.DateInvited,
                                                                                           vm.InviteExpires
                                                                                       });

            return Json(data:result);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult AuthorityUsers_PendingInvitations_Delete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")] IEnumerable<PendingInvitationViewModel> items)
        {
            if (!ModelState.IsValid)
            {
                return Json(data:items.ToDataSourceResult(request:request, modelState:ModelState));
            }

            try
            {
                if (items.Any())
                {
                    var item = items.First();

                    _invitationService.DeleteInvitation(invitationId:item.Id);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return Json(data:items.ToDataSourceResult(request:request, modelState:ModelState));
        }

        #endregion

        #region Show Authority User Details

        // GET: /Authority/AuthorityUserDetails
        [Route(template:"User/{id:int}/Details")]
        [AuthorizeCorrectAuthorityOnly(isIdParameterForUser:true)]
        public ActionResult AuthorityUserDetails(int id)
        {
            var viewModel = PrepareAuthorityUserDetails(id:id);

            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"User/{id:int}/Details")]
        [AuthorizeCorrectAuthorityOnly(isIdParameterForUser:true)]
        public ActionResult AuthorityUserDetails(int id, AuthorityUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model:model);
            }
            try
            {
                _userService.UpdateUserPermissionGroupId(orgRegProgUserId:model.Id, permissionGroupId:model.Role);
                ViewBag.ShowSuccessMessage = true;
                ViewBag.SuccessMessage = "User role updated successfully!";
                ModelState.Clear();
                model = PrepareAuthorityUserDetails(id:id);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                model = PrepareAuthorityUserDetails(id:id);
            }

            return View(viewName:"AuthorityUserDetails", model:model);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"User/{id:int}/Details/UserLockUnLock")]
        [AuthorizeCorrectAuthorityOnly(isIdParameterForUser:true)]
        public ActionResult AuthorityUserLockUnLock(int id, AuthorityUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(viewName:"AuthorityUserDetails", model:model);
            }
            try
            {
                _userService.LockUnlockUserAccount(userProfileId:model.PId, isAttemptingLock:!model.AccountLocked, reason:AccountLockEvent.ManualAction);

                ViewBag.ShowSuccessMessage = true;
                ViewBag.SuccessMessage = model.AccountLocked ? "User unlocked!" : "User locked!";
                ModelState.Clear();
                model = PrepareAuthorityUserDetails(id:id);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                model = PrepareAuthorityUserDetails(id:id);
            }

            return View(viewName:"AuthorityUserDetails", model:model);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"User/{id:int}/Details/ChangeStatus")]
        [AuthorizeCorrectAuthorityOnly(isIdParameterForUser:true)]
        public ActionResult AuthorityUserChangeStatus(int id, AuthorityUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(viewName:"AuthorityUserDetails", model:model);
            }
            try
            {
                _userService.EnableDisableUserAccount(orgRegProgramUserId:model.Id, isAttemptingDisable:model.Status);

                ViewBag.ShowSuccessMessage = true;
                ViewBag.SuccessMessage = model.Status ? "User disabled!" : "User enabled!";
                ModelState.Clear();
                model = PrepareAuthorityUserDetails(id:id);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                model = PrepareAuthorityUserDetails(id:id);
            }

            return View(viewName:"AuthorityUserDetails", model:model);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"User/{id:int}/Details/UserRemove")]
        [AuthorizeCorrectAuthorityOnly(isIdParameterForUser:true)]
        public ActionResult AuthorityUserRemove(int id, AuthorityUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(viewName:"AuthorityUserDetails", model:model);
            }
            try
            {
                var result = _userService.RemoveUser(orgRegProgUserId:model.Id);

                if (result)
                {
                    return RedirectToAction(actionName:"AuthorityUserRemoved", controllerName:"Authority");
                }
                var validationIssues = new List<RuleViolation>();
                var message = "Remove user failed.";
                validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                model = PrepareAuthorityUserDetails(id:id);
            }

            return View(viewName:"AuthorityUserDetails", model:model);
        }

        // user remove successfully
        // GET: /Authority/AuthorityUserRemoved
        public ActionResult AuthorityUserRemoved()
        {
            var model = new ConfirmationViewModel
                        {
                            Title = "User Remove Status",
                            Message = "User Removed!"
                        };

            return View(viewName:"Confirmation", model:model);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"User/{id:int}/Details/UserReset")]
        [AuthorizeCorrectAuthorityOnly(isIdParameterForUser:true)]
        public ActionResult AuthorityUserReset(int id, AuthorityUserViewModel model)
        {
            var newEmail = model.ResetEmail;
            if (!ModelState.IsValid)
            {
                return View(viewName:"AuthorityUserDetails", model:model);
            }
            try
            {
                var result = _userService.ResetUser(userProfileId:model.PId, newEmailAddress:newEmail);

                if (result.IsSuccess)
                {
                    ViewBag.ShowSuccessMessage = true;
                    ViewBag.SuccessMessage = "User account reset successfully!";
                    ModelState.Clear();
                    model = PrepareAuthorityUserDetails(id:id);
                }
                else
                {
                    var validationIssues = new List<RuleViolation>();
                    var message = "";

                    switch (result.FailureReason)
                    {
                        case ResetUserFailureReason.NewEmailAddressAlreadyInUse:
                            message = "Email is already in use on another account.";
                            break;
                        default:
                            message = "User account reset failed";
                            break;
                    }

                    validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                    throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);

                model = PrepareAuthorityUserDetails(id:id);
                model.ResetEmail = newEmail;
            }

            return View(viewName:"AuthorityUserDetails", model:model);
        }

        private AuthorityUserViewModel PrepareAuthorityUserDetails(int id)
        {
            var user = _userService.GetOrganizationRegulatoryProgramUser(orgRegProgUserId:id);
            var userQuesAns = _questionAnswerService.GetUsersQuestionAnswers(userProfileId:user.UserProfileId, questionType:QuestionTypeName.SQ);
            var currentUserRole = _httpContextService.GetClaimValue(claimType:CacheKey.UserRole) ?? "";
            var currentUserProfileId = _httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId);

            ViewBag.HasPermissionForUpdate = currentUserRole.IsCaseInsensitiveEqual(comparing:UserRole.Administrator.ToString()) &&
                                             !currentUserProfileId.IsCaseInsensitiveEqual(comparing:user.UserProfileId.ToString());
            ViewBag.HasPermissionForChangeRole = currentUserRole.IsCaseInsensitiveEqual(comparing:UserRole.Administrator.ToString());

            var viewModel = new AuthorityUserViewModel
                            {
                                Id = user.OrganizationRegulatoryProgramUserId,
                                PId = user.UserProfileId,
                                FirstName = user.UserProfileDto.FirstName,
                                LastName = user.UserProfileDto.LastName,
                                PhoneNumber = user.UserProfileDto.PhoneNumber,
                                PhoneExt = user.UserProfileDto.PhoneExt,
                                Email = user.UserProfileDto.Email,
                                ResetEmail = user.UserProfileDto.Email,
                                DateRegistered = user.RegistrationDateTimeUtc.Value.DateTime,
                                Status = user.IsEnabled,
                                AccountLocked = user.UserProfileDto.IsAccountLocked,
                                Role = user.PermissionGroup.PermissionGroupId.Value,
                                RoleText = user.PermissionGroup.Name,
                                SecurityQuestion1 = userQuesAns.Count > 0 && userQuesAns.ElementAt(index:0) != null ? userQuesAns.ElementAt(index:0).Question.Content : "",
                                Answer1 = userQuesAns.Count > 0 && userQuesAns.ElementAt(index:0) != null ? userQuesAns.ElementAt(index:0).Answer.Content : "",
                                SecurityQuestion2 = userQuesAns.Count > 1 && userQuesAns.ElementAt(index:1) != null ? userQuesAns.ElementAt(index:1).Question.Content : "",
                                Answer2 = userQuesAns.Count > 1 && userQuesAns.ElementAt(index:1) != null ? userQuesAns.ElementAt(index:1).Answer.Content : ""
                            };
            // Roles
            viewModel.AvailableRoles = new List<SelectListItem>();
            var roles = _permissionService.GetRoles(orgRegProgramId:user.OrganizationRegulatoryProgramId);
            if (roles.Count() > 0)
            {
                viewModel.AvailableRoles = roles.Select(r => new SelectListItem
                                                             {
                                                                 Text = r.Name,
                                                                 Value = r.PermissionGroupId.ToString(),
                                                                 Selected = Convert.ToInt32(value:r.PermissionGroupId) == viewModel.Role
                                                             }).ToList();
            }
            return viewModel;
        }

        #endregion

        #region Show Industry list

        // GET: /Authority/Industries
        public ActionResult Industries()
        {
            return View();
        }

        // POST: /Authority/Industries
        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult Industries(IndustryViewModel model, FormCollection collection)
        {
            ViewBag.SearchString = collection[name:"searchString"];

            return View(model:model);
        }

        private string GetClaimValue(string key)
        {
            var claims = ((ClaimsPrincipal) HttpContext.User).Claims;
            var claim = claims.SingleOrDefault(i => i.Type == key);
            if (claim != null)
            {
                return claim.Value;
            }

            return "";

            //_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)
        }

        public ActionResult Industries_Read([DataSourceRequest] DataSourceRequest request, string searchString)
        {
            var currentOrganizationRegulatoryProgramId = int.Parse(s:GetClaimValue(key:CacheKey.OrganizationRegulatoryProgramId));

            // int currentOrganizationRegulatoryProgramId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var industries = _organizationService.GetChildOrganizationRegulatoryPrograms(orgRegProgId:currentOrganizationRegulatoryProgramId, searchString:searchString);

            var viewModels = industries.Select(vm => new IndustryViewModel
                                                     {
                                                         Id = vm.OrganizationRegulatoryProgramId,
                                                         IndustryNo = vm.OrganizationDto.OrganizationId,
                                                         IndustryNoText = vm.OrganizationDto.OrganizationId.ToString(),
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
                                                         Classification = vm.OrganizationDto.Classification,
                                                         IsEnabled = vm.IsEnabled,
                                                         HasSignatory = vm.HasSignatory,
                                                         AssignedTo = vm.AssignedTo
                                                     });

            var result = viewModels.ToDataSourceResult(request:request, selector:vm => new
                                                                                       {
                                                                                           vm.Id,
                                                                                           vm.IndustryNo,
                                                                                           vm.IndustryNoText,
                                                                                           vm.IndustryName,
                                                                                           vm.Address,
                                                                                           vm.Classification,
                                                                                           vm.IsEnabledText,
                                                                                           vm.HasSignatoryText,
                                                                                           vm.AssignedTo
                                                                                       });

            return Json(data:result);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult Industries_Select(IEnumerable<IndustryViewModel> items)
        {
            try
            {
                if (items != null && ModelState.IsValid)
                {
                    var item = items.First();
                    return Json(data:new
                                     {
                                         redirect = true,
                                         newurl = Url.Action(actionName:"IndustryDetails", controllerName:"Authority", routeValues:new
                                                                                                                                   {
                                                                                                                                       id = item.Id
                                                                                                                                   })
                                     });
                }
                return Json(data:new
                                 {
                                     redirect = false,
                                     message = "Please select an industry."
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

        #region Show Industry Details

        // GET: /Authority/IndustryDetails
        [Route(template:"Industry/{id:int}/Details")]
        [AuthorizeCorrectAuthorityOnly(isIdParameterForUser:false)]
        public ActionResult IndustryDetails(int id)
        {
            var viewModel = PrepareIndustryDetails(id:id);

            return View(model:viewModel);
        }

        // POST: /Authority/IndustryDetails
        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [Route(template:"Industry/{id:int}/Details")]
        [AuthorizeCorrectAuthorityOnly(isIdParameterForUser:false)]
        public ActionResult IndustryDetails(int id, IndustryViewModel model)
        {
            try
            {
                var result = _organizationService.UpdateEnableDisableFlag(orgRegProgId:model.Id, isEnabled:!model.IsEnabled);
                var isUpdated = result.IsSuccess;

                if (isUpdated)
                {
                    ViewBag.ShowSuccessMessage = true;
                    ViewBag.SuccessMessage = model.IsEnabled ? "Industry Disabled!" : "Industry Enabled!";
                    ModelState.Clear();
                    model = PrepareIndustryDetails(id:id);
                }
                else
                {
                    model = PrepareIndustryDetails(id:id);

                    var validationIssues = new List<RuleViolation>();
                    var message = "Enable Industry not allowed. No more Industry Licenses are available. Disable another Industry and try again.";
                    validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                    throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(model:model);
        }

        private IndustryViewModel PrepareIndustryDetails(int id)
        {
            var industry = _organizationService.GetOrganizationRegulatoryProgram(orgRegProgId:id);
            var userRole = _httpContextService.GetClaimValue(claimType:CacheKey.UserRole) ?? "";

            var viewModel = new IndustryViewModel
                            {
                                Id = industry.OrganizationRegulatoryProgramId,
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
                                HasPermissionForEnableDisable = true //All Authority user types have permission! //userRole.ToLower().IsCaseInsensitiveEqual(UserRole.Administrator.ToString())
                            };
            return viewModel;
        }

        #endregion

        #region Show Industry Users

        // GET: /Authority/IndustryUsers
        [Route(template:"Industry/{id:int}/Users")]
        [AuthorizeCorrectAuthorityOnly(isIdParameterForUser:false)]
        public ActionResult IndustryUsers(int id)
        {
            ViewBag.IndustryId = id;
            var industry = _organizationService.GetOrganizationRegulatoryProgram(orgRegProgId:id);
            ViewBag.Title = string.Format(format:"{0} Users", arg0:industry.OrganizationDto.OrganizationName);

            //Invite button only visible if there isn't currently an active Admin for this IU
            ViewBag.CanInvite = !industry.HasAdmin;

            return View();
        }

        public ActionResult IndustryUsers_Read([DataSourceRequest] DataSourceRequest request, string industryId)
        {
            var organizationRegulatoryProgramId = int.Parse(s:industryId);
            var users = _userService.GetUserProfilesForOrgRegProgram(orgRegProgramId:organizationRegulatoryProgramId, isRegApproved:true, isRegDenied:false, isEnabled:null, isRemoved:false);

            var viewModels = users.Select(vm => new IndustryUserViewModel
                                                {
                                                    Id = vm.OrganizationRegulatoryProgramUserId,
                                                    IId = vm.OrganizationRegulatoryProgramId,
                                                    PId = vm.UserProfileId,
                                                    FirstName = vm.UserProfileDto.FirstName,
                                                    LastName = vm.UserProfileDto.LastName,
                                                    PhoneNumber = vm.UserProfileDto.PhoneNumber,
                                                    Email = vm.UserProfileDto.Email,
                                                    ResetEmail = vm.UserProfileDto.Email,
                                                    DateRegistered = vm.RegistrationDateTimeUtc.Value.DateTime,
                                                    Status = vm.IsEnabled,
                                                    AccountLocked = vm.UserProfileDto.IsAccountLocked,
                                                    Role = vm.PermissionGroup.PermissionGroupId.Value,
                                                    RoleText = vm.PermissionGroup.Name
                                                });

            var result = viewModels.ToDataSourceResult(request:request, selector:vm => new
                                                                                       {
                                                                                           vm.Id,
                                                                                           vm.IId,
                                                                                           vm.PId,
                                                                                           vm.FirstName,
                                                                                           vm.LastName,
                                                                                           vm.PhoneNumber,
                                                                                           vm.Email,
                                                                                           vm.ResetEmail,
                                                                                           vm.DateRegistered,
                                                                                           vm.StatusText,
                                                                                           vm.AccountLockedText,
                                                                                           vm.Role,
                                                                                           vm.RoleText
                                                                                       });

            return Json(data:result);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult IndustryUsers_Select(IEnumerable<IndustryUserViewModel> items)
        {
            try
            {
                if (items != null && ModelState.IsValid)
                {
                    var item = items.First();
                    return Json(data:new
                                     {
                                         redirect = true,
                                         newurl = Url.Action(actionName:"IndustryUserDetails", controllerName:"Authority", routeValues:new
                                                                                                                                       {
                                                                                                                                           iid = item.IId,
                                                                                                                                           id = item.Id
                                                                                                                                       })
                                     });
                }
                return Json(data:new
                                 {
                                     redirect = false,
                                     message = "Please select an user."
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

        public ActionResult IndustryUsers_PendingInvitations_Read([DataSourceRequest] DataSourceRequest request, string industryId)
        {
            var industryOrgRegProgramId = int.Parse(s:industryId);
            var senderOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var invitations = _invitationService.GetInvitationsForOrgRegProgram(senderOrgRegProgramId:senderOrgRegProgramId, targetOrgRegProgramId:industryOrgRegProgramId);

            var viewModels = invitations.Select(vm => new PendingInvitationViewModel
                                                      {
                                                          Id = vm.InvitationId,
                                                          FirstName = vm.FirstName,
                                                          LastName = vm.LastName,
                                                          Email = vm.EmailAddress,
                                                          DateInvited = vm.InvitationDateTimeUtc.DateTime,
                                                          InviteExpires = vm.ExpiryDateTimeUtc.DateTime
                                                      });

            var result = viewModels.ToDataSourceResult(request:request, selector:vm => new
                                                                                       {
                                                                                           vm.Id,
                                                                                           vm.FirstName,
                                                                                           vm.LastName,
                                                                                           vm.Email,
                                                                                           vm.DateInvited,
                                                                                           vm.InviteExpires
                                                                                       });

            return Json(data:result);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult IndustryUsers_PendingInvitations_Delete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")] IEnumerable<PendingInvitationViewModel> items)
        {
            if (!ModelState.IsValid)
            {
                return Json(data:items.ToDataSourceResult(request:request, modelState:ModelState));
            }

            try
            {
                if (items.Any())
                {
                    var item = items.First();

                    _invitationService.DeleteInvitation(invitationId:item.Id);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return Json(data:items.ToDataSourceResult(request:request, modelState:ModelState));
        }

        #endregion

        #region Show Industry User Details

        // GET: /Authority/IndustryUserDetails
        [Route(template:"Industry/{iid:int}/User/{id:int}/Details")]
        [AuthorizeCorrectAuthorityOnly(isIdParameterForUser:true)]
        public ActionResult IndustryUserDetails(int iid, int id)
        {
            var viewModel = PrepareIndustryUserDetails(id:id);

            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"Industry/{iid:int}/User/{id:int}/Details/UpdateSignatoryStatus")]
        [AuthorizeCorrectAuthorityOnly(isIdParameterForUser:true)]
        public ActionResult IndustryUserUpdateSignatoryStatus(int iid, int id, IndustryUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(viewName:"IndustryUserDetails", model:model);
            }
            try
            {
                _userService.UpdateUserSignatoryStatus(orgRegProgUserId:model.Id, isSignatory:model.IsSignatory);
                ViewBag.ShowSuccessMessage = true;
                ViewBag.SuccessMessage = model.IsSignatory ? "User signatory permission granted!" : "User signatory permission removed!";
                ModelState.Clear();
                model = PrepareIndustryUserDetails(id:id);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                model = PrepareIndustryUserDetails(id:id);
            }

            return View(viewName:"IndustryUserDetails", model:model);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"Industry/{iid:int}/User/{id:int}/Details/UserLockUnLock")]
        [AuthorizeCorrectAuthorityOnly(isIdParameterForUser:true)]
        public ActionResult IndustryUserLockUnLock(int iid, int id, IndustryUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(viewName:"IndustryUserDetails", model:model);
            }
            try
            {
                _userService.LockUnlockUserAccount(userProfileId:model.PId, isAttemptingLock:!model.AccountLocked, reason:AccountLockEvent.ManualAction);

                ViewBag.ShowSuccessMessage = true;
                ViewBag.SuccessMessage = model.AccountLocked ? "User unlocked!" : "User locked!";
                ModelState.Clear();
                model = PrepareIndustryUserDetails(id:id);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                model = PrepareIndustryUserDetails(id:id);
            }

            return View(viewName:"IndustryUserDetails", model:model);
        }

        //[AcceptVerbs(HttpVerbs.Post)]
        //[ValidateAntiForgeryToken]
        //[Route("Industry/{iid:int}/User/{id:int}/Details/ChangeStatus")]
        //[AuthorizeCorrectAuthorityOnly(true)]
        //public ActionResult IndustryUserChangeStatus(int iid, int id, IndustryUserViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }
        //    try
        //    {
        //        _userService.EnableDisableUserAccount(model.Id, model.Status);

        //        ViewBag.ShowSuccessMessage = true;
        //        ViewBag.SuccessMessage = model.Status ? "User disabled!" : "User enabled!";
        //        ModelState.Clear();
        //        model = PrepareIndustryUserDetails(id);
        //    }
        //    catch (RuleViolationException rve)
        //    {
        //        MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
        //        model = PrepareIndustryUserDetails(id);
        //    }

        //    return View(viewName: "IndustryUserDetails", model: model);
        //}

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"Industry/{iid:int}/User/{id:int}/Details/UserReset")]
        [AuthorizeCorrectAuthorityOnly(isIdParameterForUser:true)]
        public ActionResult IndustryUserReset(int iid, int id, IndustryUserViewModel model)
        {
            var newEmail = model.ResetEmail;
            if (!ModelState.IsValid)
            {
                return View(viewName:"IndustryUserDetails", model:model);
            }
            try
            {
                var result = _userService.ResetUser(userProfileId:model.PId, newEmailAddress:newEmail, targetOrgRegProgramId:model.IId);

                if (result.IsSuccess)
                {
                    ViewBag.ShowSuccessMessage = true;
                    ViewBag.SuccessMessage = "User account reset successfully!";
                    ModelState.Clear();
                    model = PrepareIndustryUserDetails(id:id);
                }
                else
                {
                    var validationIssues = new List<RuleViolation>();
                    var message = "";

                    switch (result.FailureReason)
                    {
                        case ResetUserFailureReason.NewEmailAddressAlreadyInUse:
                            message = "Email is already in use on another account.";
                            break;
                        default:
                            message = "User account reset failed";
                            break;
                    }

                    validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                    throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);

                model = PrepareIndustryUserDetails(id:id);
                model.ResetEmail = newEmail;
            }

            return View(viewName:"IndustryUserDetails", model:model);
        }

        private IndustryUserViewModel PrepareIndustryUserDetails(int id)
        {
            var user = _userService.GetOrganizationRegulatoryProgramUser(orgRegProgUserId:id);
            var userQuesAns = _questionAnswerService.GetUsersQuestionAnswers(userProfileId:user.UserProfileId, questionType:QuestionTypeName.SQ);

            var viewModel = new IndustryUserViewModel
                            {
                                Id = user.OrganizationRegulatoryProgramUserId,
                                IId = user.OrganizationRegulatoryProgramId,
                                PId = user.UserProfileId,
                                FirstName = user.UserProfileDto.FirstName,
                                LastName = user.UserProfileDto.LastName,
                                PhoneNumber = user.UserProfileDto.PhoneNumber,
                                PhoneExt = user.UserProfileDto.PhoneExt,
                                Email = user.UserProfileDto.Email,
                                ResetEmail = user.UserProfileDto.Email,
                                DateRegistered = user.RegistrationDateTimeUtc.Value.DateTime,
                                Status = user.IsEnabled,
                                AccountLocked = user.UserProfileDto.IsAccountLocked,
                                Role = user.PermissionGroup.PermissionGroupId.Value,
                                RoleText = user.PermissionGroup.Name,
                                IsSignatory = user.IsSignatory,
                                SecurityQuestion1 = userQuesAns.Count > 0 && userQuesAns.ElementAt(index:0) != null ? userQuesAns.ElementAt(index:0).Question.Content : "",
                                Answer1 = userQuesAns.Count > 0 && userQuesAns.ElementAt(index:0) != null ? userQuesAns.ElementAt(index:0).Answer.Content : "",
                                SecurityQuestion2 = userQuesAns.Count > 1 && userQuesAns.ElementAt(index:1) != null ? userQuesAns.ElementAt(index:1).Question.Content : "",
                                Answer2 = userQuesAns.Count > 1 && userQuesAns.ElementAt(index:1) != null ? userQuesAns.ElementAt(index:1).Answer.Content : ""
                            };
            return viewModel;
        }

        #endregion

        #region Show Pending User Approvals

        // GET: /Authority/PendingUserApprovals
        [Route(template:"PendingUserApprovals")]
        public ActionResult PendingUserApprovals()
        {
            return View();
        }

        public ActionResult PendingUserApprovals_Read([DataSourceRequest] DataSourceRequest request)
        {
            var organizationRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var users = _userService.GetPendingRegistrationProgramUsers(orgRegProgramId:organizationRegulatoryProgramId);

            var viewModels = users.Select(vm => new PendingUserApprovalViewModel
                                                {
                                                    Id = vm.OrganizationRegulatoryProgramUserId,
                                                    PId = vm.UserProfileId,
                                                    RegisteredOrgName = vm.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName,
                                                    Type = vm.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationType.Name,
                                                    UserName = vm.UserProfileDto.UserName,
                                                    FirstName = vm.UserProfileDto.FirstName,
                                                    LastName = vm.UserProfileDto.LastName,
                                                    BusinessName = vm.UserProfileDto.BusinessName,
                                                    PhoneNumber = vm.UserProfileDto.PhoneNumber,
                                                    Email = vm.UserProfileDto.Email,
                                                    DateRegistered = vm.RegistrationDateTimeUtc.Value.DateTime
                                                });

            var result = viewModels.ToDataSourceResult(request:request, selector:vm => new
                                                                                       {
                                                                                           vm.Id,
                                                                                           vm.PId,
                                                                                           vm.RegisteredOrgName,
                                                                                           vm.Type,
                                                                                           vm.UserName,
                                                                                           vm.FirstName,
                                                                                           vm.LastName,
                                                                                           vm.BusinessName,
                                                                                           vm.PhoneNumber,
                                                                                           vm.Email,
                                                                                           vm.DateRegistered,
                                                                                           Role = 1 // role need to be more than 0 otherwise ModelState.IsValid = false 
                                                                                       });

            return Json(data:result);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult PendingUserApprovals_Select(IEnumerable<PendingUserApprovalViewModel> items)
        {
            try
            {
                if (items != null && ModelState.IsValid)
                {
                    var item = items.First();
                    return Json(data:new
                                     {
                                         redirect = true,
                                         newurl = Url.Action(actionName:"PendingUserApprovalDetails", controllerName:"Authority", routeValues:new
                                                                                                                                              {
                                                                                                                                                  id = item.Id
                                                                                                                                              })
                                     });
                }
                return Json(data:new
                                 {
                                     redirect = false,
                                     message = "Please select an user."
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

        #region Show Pending User Approval Details

        // GET: /Authority/PendingUserApprovals
        [Route(template:"PendingUserApprovals/{id:int}/Details")]
        [AuthorizeCorrectAuthorityOnly(isIdParameterForUser:true)]
        public ActionResult PendingUserApprovalDetails(int id)
        {
            var viewModel = PreparePendingUserApprovalDetails(id:id);
            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"PendingUserApprovals/{id:int}/Details/PendingUserApprove")]
        [AuthorizeCorrectAuthorityOnly(isIdParameterForUser:true)]
        public ActionResult PendingUserApprove(int id, PendingUserApprovalViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = _userService.ApprovePendingRegistration(orgRegProgUserId:model.Id, permissionGroupId:model.Role.Value, isApproved:true);
                    switch (result.Result)
                    {
                        case RegistrationResult.Success:
                            ViewBag.ShowSuccessMessage = true;
                            ViewBag.SuccessMessage = "Registration Approved! An email has been sent to the registrant.";
                            ModelState.Clear();
                            _logger.Info(message:$"PendingUserApprove. User={model.UserName} - id={model.Id} Registration Approved!");
                            break;
                        case RegistrationResult.NoMoreUserLicensesForIndustry:
                            _logger.Info(message:$"PendingUserApprove. User={model.UserName} - id={model.Id} No more user licenses");
                            ModelState.AddModelError(key:"", errorMessage:"No more User Licenses are available for this Industry. Disable another User and try again");
                            break;
                        case RegistrationResult.NoMoreUserLicensesForAuthority:
                            _logger.Info(message:$"PendingUserApprove. User={model.UserName} - id={model.Id} No more user licenses");
                            ModelState.AddModelError(key:"", errorMessage:"No more User Licenses are available for this Authority. Disable another User and try again");
                            break;
                        default:
                            _logger.Info(message:$"PendingUserApprove. User={model.UserName} - id={model.Id} Registration Approval Failed!");
                            ModelState.AddModelError(key:"", errorMessage:"Registration Approval Failed");
                            break;
                    }
                }
                catch (RuleViolationException rve)
                {
                    MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                }
            }
            model = PreparePendingUserApprovalDetails(id:id);
            return View(viewName:"PendingUserApprovalDetails", model:model);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"PendingUserApprovals/{id:int}/Details/PendingUserDeny")]
        [AuthorizeCorrectAuthorityOnly(isIdParameterForUser:true)]
        public ActionResult PendingUserDeny(int id, PendingUserApprovalViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = _userService.ApprovePendingRegistration(orgRegProgUserId:model.Id, permissionGroupId:model.Role.Value, isApproved:false);
                    switch (result.Result)
                    {
                        case RegistrationResult.Success:
                            ViewBag.ShowSuccessMessage = true;
                            ViewBag.SuccessMessage = "Registration Denied!";
                            ModelState.Clear();
                            _logger.Info(message:$"PendingUserDeny. User={model.UserName} - id={model.Id} Registration Denied!");
                            break;
                        default:
                            _logger.Info(message:string.Format(format:"PendingUserDeny. User={0} - id={1} Registration Denial Failed!", arg0:model.UserName, arg1:model.Id));
                            ModelState.AddModelError(key:"", errorMessage:"Registration Denial Failed");
                            break;
                    }
                }
                catch (RuleViolationException rve)
                {
                    MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                }
            }

            model = PreparePendingUserApprovalDetails(id:id);
            return View(viewName:"PendingUserApprovalDetails", model:model);
        }

        private PendingUserApprovalViewModel PreparePendingUserApprovalDetails(int id)
        {
            var result = _userService.GetOrganizationRegulatoryProgramUser(orgRegProgUserId:id);

            var viewModel = new PendingUserApprovalViewModel
                            {
                                Id = result.OrganizationRegulatoryProgramUserId,
                                PId = result.UserProfileId,
                                RegisteredOrgName = result.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName,
                                Type = result.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationType.Name,
                                UserName = result.UserProfileDto.UserName,
                                FirstName = result.UserProfileDto.FirstName,
                                LastName = result.UserProfileDto.LastName,
                                BusinessName = result.UserProfileDto.BusinessName,
                                TitleRole = result.UserProfileDto.TitleRole,
                                AddressLine1 = result.UserProfileDto.AddressLine1,
                                AddressLine2 = result.UserProfileDto.AddressLine2,
                                CityName = result.UserProfileDto.CityName,
                                State = result.UserProfileDto.Jurisdiction.Code,
                                ZipCode = result.UserProfileDto.ZipCode,
                                Email = result.UserProfileDto.Email,
                                PhoneNumber = result.UserProfileDto.PhoneNumber,
                                PhoneExt = result.UserProfileDto.PhoneExt,
                                DateRegistered = result.RegistrationDateTimeUtc.Value.DateTime,
                                Role = result.PermissionGroup == null ? 0 : result.PermissionGroup.PermissionGroupId,
                                RoleText = result.PermissionGroup == null ? "" : result.PermissionGroup.Name
                            };
            // Roles
            viewModel.AvailableRoles = new List<SelectListItem>();
            var roles = _permissionService.GetRoles(orgRegProgramId:result.OrganizationRegulatoryProgramId);

            if (roles.Count() > 0)
            {
                viewModel.AvailableRoles = roles.Select(r => new SelectListItem
                                                             {
                                                                 Text = r.Name,
                                                                 Value = r.PermissionGroupId.ToString(),
                                                                 Selected = Convert.ToInt32(value:r.PermissionGroupId) == viewModel.Role
                                                             }).ToList();
            }
            viewModel.AvailableRoles.Insert(index:0, item:new SelectListItem {Text = "Select User Role", Value = "0"});

            var currentUserRole = _httpContextService.GetClaimValue(claimType:CacheKey.UserRole) ?? "";
            ViewBag.HasPermissionForApproveDeny = false;
            if (viewModel.Type.IsCaseInsensitiveEqual(comparing:OrganizationTypeName.Industry.ToString()))
            {
                ViewBag.HasPermissionForApproveDeny = true;
            }
            else
            {
                // For authority user registration request, only authority admin can approve 
                if (currentUserRole.IsCaseInsensitiveEqual(comparing:UserRole.Administrator.ToString()))
                {
                    ViewBag.HasPermissionForApproveDeny = true;
                }
            }

            ViewBag.CanChangeRole = viewModel.Type.IsCaseInsensitiveEqual(comparing:OrganizationTypeName.Authority.ToString());

            if (viewModel.Type.IsCaseInsensitiveEqual(comparing:OrganizationTypeName.Industry.ToString())
                && (!viewModel.Role.HasValue || viewModel.Role.Value == 0))
            {
                viewModel.Role = roles.Where(r => r.Name.IsCaseInsensitiveEqual(comparing:UserRole.Administrator.ToString())).First().PermissionGroupId;
                viewModel.RoleText = UserRole.Administrator.ToString();
            }

            return viewModel;
        }

        #endregion

        #region Show Static Parameter Group List

        // GET: /Authority/ParameterGroups
        public ActionResult ParameterGroups()
        {
            return View();
        }

        public ActionResult ParameterGroups_Read([DataSourceRequest] DataSourceRequest request)
        {
            var parameterGroups = _parameterService.GetStaticParameterGroups();

            var viewModels = parameterGroups.Select(vm => new ParameterGroupViewModel
                                                          {
                                                              Id = vm.ParameterGroupId,
                                                              Name = vm.Name,
                                                              Description = vm.Description,
                                                              IsActive = vm.IsActive,
                                                              LastModificationDateTimeLocal = vm.LastModificationDateTimeLocal,
                                                              LastModifierUserName = vm.LastModifierFullName
                                                          });

            var result = viewModels.ToDataSourceResult(request:request, selector:vm => new
                                                                                       {
                                                                                           vm.Id,
                                                                                           vm.Name,
                                                                                           vm.Description,
                                                                                           vm.Status,
                                                                                           vm.LastModificationDateTimeLocal,
                                                                                           vm.LastModifierUserName
                                                                                       });

            return Json(data:result);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult ParameterGroups_Select(IEnumerable<ParameterGroupViewModel> items)
        {
            try
            {
                if (items != null)
                {
                    var item = items.First();
                    return Json(data:new
                                     {
                                         redirect = true,
                                         newurl = Url.Action(actionName:"ParameterGroupDetails", controllerName:"Authority", routeValues:new {id = item.Id})
                                     });
                }
                return Json(data:new
                                 {
                                     redirect = false,
                                     message = "Please select a parameter group."
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

        #region Show Static Parameter Group Details

        [Route(template:"ParameterGroup/Details")]
        public ActionResult ParameterGroupDetails(int? id)
        {
            var viewModel = new ParameterGroupViewModel();
            try
            {
                viewModel = PrepareParameterGroupDetails(id:id);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }
            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        //[ValidateAntiForgeryToken]
        [Route(template:"ParameterGroup/Details")]
        public ActionResult ParameterGroupDetails(ParameterGroupViewModel model)
        {
            return SaveParameterGroupDetails(model:model);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult DeleteParameterGroup(int id)
        {
            try
            {
                _parameterService.DeleteParameterGroup(parameterGroupId:id);

                return View(viewName:"Confirmation", model:new ConfirmationViewModel
                                                           {
                                                               Title = "Delete Confirmation",
                                                               Message = "Parameter group deleted successfully."
                                                           });
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(viewName:"ParameterGroupDetails", model:PrepareParameterGroupDetails(id:id));
        }

        private ActionResult SaveParameterGroupDetails(ParameterGroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = PrepareParameterGroupDetails(id:model.Id);

                foreach (var issue in ModelState[key:"."].Errors)
                {
                    ModelState.AddModelError(key:string.Empty, errorMessage:issue.ErrorMessage);
                }

                return View(viewName:"ParameterGroupDetails", model:model);
            }
            try
            {
                var parameterGroupDto = new ParameterGroupDto();

                if (model.Id.HasValue)
                {
                    parameterGroupDto = _parameterService.GetParameterGroup(parameterGroupId:model.Id.Value);
                }

                parameterGroupDto.ParameterGroupId = model.Id;
                parameterGroupDto.Name = model.Name;
                parameterGroupDto.Description = model.Description;
                parameterGroupDto.IsActive = model.IsActive;
                parameterGroupDto.Parameters = model.Parameters.Select(p => new ParameterDto {ParameterId = p.Id}).ToList();

                var id = _parameterService.SaveParameterGroup(parameterGroup:parameterGroupDto);

                ViewBag.ShowSuccessMessage = true;
                ViewBag.SuccessMessage = $"Parameter Group {(model.Id.HasValue ? "updated" : "created")} successfully!";
                ModelState.Clear();
                model = PrepareParameterGroupDetails(id:id);
                return View(viewName:"ParameterGroupDetails", model:model);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                model = PrepareParameterGroupDetails(id:model.Id);
            }

            return View(viewName:"ParameterGroupDetails", model:model);
        }

        private ParameterGroupViewModel PrepareParameterGroupDetails(int? id = null)
        {
            var viewModel = new ParameterGroupViewModel();
            if (id.HasValue)
            {
                ViewBag.Satus = "Edit";
                var parameterGroup = _parameterService.GetParameterGroup(parameterGroupId:id.Value);
                viewModel = new ParameterGroupViewModel
                            {
                                Id = parameterGroup.ParameterGroupId,
                                Name = parameterGroup.Name,
                                Description = parameterGroup.Description,
                                IsActive = parameterGroup.IsActive,
                                LastModificationDateTimeLocal = parameterGroup.LastModificationDateTimeLocal,
                                LastModifierUserName = parameterGroup.LastModifierFullName,
                                Parameters = parameterGroup.Parameters.Select(p => new ParameterViewModel
                                                                                   {
                                                                                       Id = p.ParameterId,
                                                                                       Name = p.Name,
                                                                                       Description = p.Description,
                                                                                       DefaultUnitId = p.DefaultUnit.UnitId,
                                                                                       DefaultUnitName = p.DefaultUnit.Name,
                                                                                       IsRemoved = p.IsRemoved
                                                                                   }).ToList()
                            };
            }
            else
            {
                ViewBag.Satus = "New";
                viewModel.Parameters = new List<ParameterViewModel>();
            }

            ViewBag.GlobalParameters = _parameterService.GetGlobalParameters().Select(p => new ParameterViewModel
                                                                                           {
                                                                                               Id = p.ParameterId,
                                                                                               Name = p.Name,
                                                                                               Description = p.Description,
                                                                                               DefaultUnitId = p.DefaultUnit.UnitId,
                                                                                               DefaultUnitName = p.DefaultUnit.Name,
                                                                                               IsRemoved = p.IsRemoved
                                                                                           }).ToList();
            return viewModel;
        }

        #endregion

        #region Show Report Element Type List

        // GET: /Authority/ReportElementTypes
        public ActionResult ReportElementTypes()
        {
            return View();
        }

        public ActionResult ReportElementTypes_Read([DataSourceRequest] DataSourceRequest request, ReportElementCategoryName categoryName)
        {
            var reportElementTypes = _reportElementService.GetReportElementTypes(categoryName:categoryName);

            var viewModels = reportElementTypes.Select(vm => new ReportElementTypeViewModel
                                                             {
                                                                 Id = vm.ReportElementTypeId,
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
        public ActionResult ReportElementTypes_Select(IEnumerable<ReportElementTypeViewModel> items)
        {
            try
            {
                if (items != null)
                {
                    var item = items.First();
                    return Json(data:new
                                     {
                                         redirect = true,
                                         newurl = Url.Action(actionName:"ReportElementTypeDetails", controllerName:"Authority", routeValues:new {id = item.Id})
                                     });
                }
                return Json(data:new
                                 {
                                     redirect = false,
                                     message = "Please select a report element type."
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

        #region Show Report Element Type Details

        [Route(template:"ReportElementType/New")]
        public ActionResult NewReportElementTypeDetails(ReportElementCategoryName categoryName)
        {
            var viewModel = new ReportElementTypeViewModel();

            try
            {
                viewModel = PrepareReportElementTypeDetails();
                viewModel.CategoryName = categoryName;
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }
            return View(viewName:"ReportElementTypeDetails", model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"ReportElementType/New")]
        public ActionResult NewReportElementTypeDetails(ReportElementCategoryName categoryName, ReportElementTypeViewModel model)
        {
            return SaveReportElementTypeDetails(model:model);
        }

        [Route(template:"ReportElementType/{id:int}/Details")]
        public ActionResult ReportElementTypeDetails(int id)
        {
            var viewModel = new ReportElementTypeViewModel();

            try
            {
                viewModel = PrepareReportElementTypeDetails(id:id);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            ViewBag.ShowSuccessMessage = TempData[key:"ShowSuccessMessage"] ?? false;
            ViewBag.SuccessMessage = TempData[key:"SuccessMessage"] ?? "";

            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"ReportElementType/{id:int}/Details")]
        public ActionResult ReportElementTypeDetails(int id, ReportElementTypeViewModel model)
        {
            return SaveReportElementTypeDetails(model:model);
        }

        public ActionResult DeleteReportElementType(int id)
        {
            try
            {
                _reportElementService.DeleteReportElementType(reportElementTypeId:id);

                return View(viewName:"Confirmation", model:new ConfirmationViewModel
                                                           {
                                                               Title = "Delete Confirmation",
                                                               Message = "Report element type deleted successfully."
                                                           });
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(viewName:"ReportElementTypeDetails", model:PrepareReportElementTypeDetails(id:id));
        }

        private ReportElementTypeViewModel PrepareReportElementTypeDetails(int? id = null)
        {
            var viewModel = new ReportElementTypeViewModel();

            if (id.HasValue)
            {
                ViewBag.Satus = "Edit";
                var reportElementType = _reportElementService.GetReportElementType(reportElementTypeId:id.Value);
                viewModel = new ReportElementTypeViewModel
                            {
                                Id = reportElementType.ReportElementTypeId,
                                Name = reportElementType.Name,
                                Description = reportElementType.Description,
                                CategoryName = reportElementType.ReportElementCategory,
                                Content = reportElementType.Content,
                                IsContentProvided = reportElementType.IsContentProvided,
                                CtsEventTypeId = reportElementType.CtsEventType?.CtsEventTypeId ?? 0,
                                CtsEventTypeName = reportElementType.CtsEventType?.Name ?? "",
                                LastModificationDateTimeLocal = reportElementType.LastModificationDateTimeLocal,
                                LastModifierUserName = reportElementType.LastModifierFullName
                            };
            }
            else
            {
                ViewBag.Satus = "New";
            }

            // CtsEventTypes
            viewModel.AvailableCtsEventTypes = new List<SelectListItem>();
            viewModel.AvailableCtsEventTypes = _reportTemplateService.GetCtsEventTypes().Select(c => new SelectListItem
                                                                                                     {
                                                                                                         Text = c.Name,
                                                                                                         Value = c.CtsEventTypeId.ToString(),
                                                                                                         Selected = c.CtsEventTypeId.Equals(obj:viewModel.CtsEventTypeId)
                                                                                                     }).ToList();

            viewModel.AvailableCtsEventTypes.Insert(index:0, item:new SelectListItem {Text = @"Select CTS Event Type", Value = "0"});

            return viewModel;
        }

        private ActionResult SaveReportElementTypeDetails(ReportElementTypeViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var reportElementTypeDto = new ReportElementTypeDto();

                    if (model.Id.HasValue)
                    {
                        reportElementTypeDto = _reportElementService.GetReportElementType(reportElementTypeId:model.Id.Value);
                    }

                    reportElementTypeDto.ReportElementTypeId = model.Id;
                    reportElementTypeDto.Name = model.Name;
                    reportElementTypeDto.Description = model.Description;
                    reportElementTypeDto.CtsEventType = model.CtsEventTypeId == 0 ? null : _reportTemplateService.GetCtsEventType(ctsEventTypeId:model.CtsEventTypeId);
                    reportElementTypeDto.Content = model.Content;
                    reportElementTypeDto.ReportElementCategory = model.CategoryName;
                    reportElementTypeDto.IsContentProvided = model.CategoryName == ReportElementCategoryName.Certifications;

                    var id = _reportElementService.SaveReportElementType(reportElementType:reportElementTypeDto);

                    TempData[key:"ShowSuccessMessage"] = true;
                    TempData[key:"SuccessMessage"] = $"Report element type {(model.Id.HasValue ? "updated" : "created")} successfully!";

                    ModelState.Clear();
                    return RedirectToAction(actionName:"ReportElementTypeDetails", controllerName:"Authority", routeValues:new {id});
                }
                catch (RuleViolationException rve)
                {
                    MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                }
            }

            var viewModel = PrepareReportElementTypeDetails(id:model.Id);
            viewModel.CategoryName = model.CategoryName;

            return View(viewName:"ReportElementTypeDetails", model:viewModel);
        }

        #endregion
        
        #region Show Report Package Template List

        // GET: /Authority/ReportPackageTemplates
        public ActionResult ReportPackageTemplates()
        {
            return View();
        }

        public ActionResult ReportPackageTemplates_Read([DataSourceRequest] DataSourceRequest request)
        {
            var reportPackageTemplates = _reportTemplateService.GetReportPackageTemplates(includeChildObjects:false).ToList();

            var viewModels = reportPackageTemplates.Select(vm => new ReportPackageTemplateViewModel
                                                          {
                                                              Id = vm.ReportPackageTemplateId,
                                                              Name = vm.Name,
                                                              Description = vm.Description,
                                                              IsActive = vm.IsActive,
                                                              EffectiveDateTimeLocal = vm.EffectiveDateTimeLocal,
                                                              LastModificationDateTimeLocal = vm.LastModificationDateTimeLocal,
                                                              LastModifierUserName = vm.LastModifierFullName
                                                          });

            var result = viewModels.ToDataSourceResult(request:request, selector:vm => new
                                                                                       {
                                                                                           vm.Id,
                                                                                           vm.Name,
                                                                                           vm.Description,
                                                                                           vm.Status,
                                                                                           vm.EffectiveDateTimeLocal,
                                                                                           vm.LastModificationDateTimeLocal,
                                                                                           vm.LastModifierUserName
                                                                                       });

            return Json(data:result);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult ReportPackageTemplates_Select(IEnumerable<ReportPackageTemplateViewModel> items)
        {
            try
            {
                if (items != null)
                {
                    var item = items.First();
                    return Json(data:new
                                     {
                                         redirect = true,
                                         newurl = Url.Action(actionName:"ReportPackageTemplateDetails", controllerName:"Authority", routeValues:new {id = item.Id})
                                     });
                }
                return Json(data:new
                                 {
                                     redirect = false,
                                     message = "Please select a report package template."
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

        #region Show Report Package Template Details

        public ActionResult NewReportPackageTemplateDetails()
        {
           
            var viewModel = new ReportPackageTemplateViewModel();

            try
            {
                viewModel = PrepareReportPackageTemplateDetails();
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }
            return View(viewName:"ReportPackageTemplateDetails", model:viewModel);
        }

        public ActionResult ReportPackageTemplateDetails(int? id)
        {
           
            var viewModel = new ReportPackageTemplateViewModel();

            try
            {
                viewModel = PrepareReportPackageTemplateDetails(id:id);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            ViewBag.ShowSuccessMessage = TempData[key:"ShowSuccessMessage"] ?? false;
            ViewBag.SuccessMessage = TempData[key:"SuccessMessage"] ?? "";

            return View(model:viewModel);
        }
        public ActionResult DeleteReportPackageTemplate(int id)
        {
            try
            {
                _reportTemplateService.DeleteReportPackageTemplate(reportPackageTemplateId:id);

                return View(viewName:"Confirmation", model:new ConfirmationViewModel
                                                           {
                                                               Title = "Delete Confirmation",
                                                               Message = "Report package template has been deleted successfully."
                                                           });
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(viewName:"ReportPackageTemplateDetails", model:PrepareReportPackageTemplateDetails(id:id));
        }

        private ReportPackageTemplateViewModel PrepareReportPackageTemplateDetails(int? id = null)
        {
            var viewModel = new ReportPackageTemplateViewModel();

            if (id.HasValue)
            {
                ViewBag.Satus = "Edit";

                var reportPackageTemplate = _reportTemplateService.GetReportPackageTemplate(reportPackageTemplateId:id.Value);

                viewModel = new ReportPackageTemplateViewModel
                            {
                                Id = reportPackageTemplate.ReportPackageTemplateId,
                                Name = reportPackageTemplate.Name,
                                Description = reportPackageTemplate.Description,
                                IsActive = reportPackageTemplate.IsActive,
                                EffectiveDateTimeLocal = reportPackageTemplate.EffectiveDateTimeLocal,
                                LastModificationDateTimeLocal = reportPackageTemplate.LastModificationDateTimeLocal,
                                LastModifierUserName = reportPackageTemplate.LastModifierFullName
                            };
            }
            else
            {
                ViewBag.Satus = "New";
            }

            
            // CtsEventTypes
            viewModel.AvailableCtsEventTypes = new List<SelectListItem>();
            viewModel.AvailableCtsEventTypes = _reportTemplateService.GetCtsEventTypes().Select(c => new SelectListItem
                                                                                                     {
                                                                                                         Text = c.Name,
                                                                                                         Value = c.CtsEventTypeId.ToString(),
                                                                                                         Selected = c.CtsEventTypeId.Equals(obj:viewModel.CtsEventTypeId)
                                                                                                     }).ToList();

            viewModel.AvailableCtsEventTypes.Insert(index:0, item:new SelectListItem {Text = @"Select CTS Event Type", Value = "0"});


            return viewModel;
        }

        #endregion

    }
}