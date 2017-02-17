using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Permission;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.ViewModels.Authority;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using NLog;
using Linko.LinkoExchange.Web.Mvc;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services;

namespace Linko.LinkoExchange.Web.Controllers
{
    [RoutePrefix("Authority")]
    public class AuthorityController : Controller
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


        public AuthorityController(IOrganizationService organizationService, IUserService userService, IInvitationService invitationService,
            ISettingService settingService, IQuestionAnswerService questionAnswerService, ITimeZoneService timeZoneService, IPermissionService permissionService,
            ISessionCache sessionCache, ILogger logger, ICromerrAuditLogService cromerrLogService, IHttpContextService httpContextService)
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
        }

        #endregion

        #region default action

        // GET: Authority
        public ActionResult Index()
        {
            return RedirectToAction(actionName: "Industries", controllerName: "Authority");
        }

        #endregion

        #region Show Authority Settings

        // GET: /Authority/Settings
        public ActionResult Settings()
        {
            AuthoritySettingsViewModel viewModel = PrepareAuthoritySettings();

            return View(viewModel);
        }

        // POST: /Authority/Settings#AuthoritySettings
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Settings(AuthoritySettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                int id = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
                var authority = _organizationService.GetOrganizationRegulatoryProgram(id);
                var authoritySettings = _settingService.GetOrganizationSettingsById(authority.OrganizationId).Settings;

                //FailedPasswordAttemptMaxCount
                authoritySettings.Where(s => s.TemplateName.Equals(SettingType.FailedPasswordAttemptMaxCount)).ToList().ForEach(s => s.Value = model.FailedPasswordAttemptMaxCount);
                //FailedKbqAttemptMaxCount
                authoritySettings.Where(s => s.TemplateName.Equals(SettingType.FailedKBQAttemptMaxCount)).ToList().ForEach(s => s.Value = model.FailedKbqAttemptMaxCount);
                //InvitationExpiredHours
                authoritySettings.Where(s => s.TemplateName.Equals(SettingType.InvitationExpiredHours)).ToList().ForEach(s => s.Value = model.InvitationExpiredHours);
                //PasswordChangeRequiredDays
                authoritySettings.Where(s => s.TemplateName.Equals(SettingType.PasswordChangeRequiredDays)).ToList().ForEach(s => s.Value = model.PasswordChangeRequiredDays);
                //PasswordHistoryMaxCount
                authoritySettings.Where(s => s.TemplateName.Equals(SettingType.PasswordHistoryMaxCount)).ToList().ForEach(s => s.Value = model.PasswordHistoryMaxCount);
                //TimeZone
                authoritySettings.Where(s => s.TemplateName.Equals(SettingType.TimeZone)).ToList().ForEach(s => s.Value = model.TimeZone);

                _settingService.CreateOrUpdateOrganizationSettings(authority.OrganizationId, authoritySettings);

                ViewBag.ShowSuccessMessageForAuthoritySettings = true;
                ViewBag.SuccessMessageForAuthoritySettings = "Save successful.";
                ModelState.Clear();
                model = PrepareAuthoritySettings();

            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
                ViewBag.ShowErrorMessageForAuthoritySettings = true;
                model = PrepareAuthoritySettings();
            }

            return View(viewName: "Settings", model: model);
        }

        // POST: /Authority/Settings#ProgramSettings
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("Settings/Program")]
        public ActionResult ProgramSettings(AuthoritySettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                int id = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
                var programSettings = _settingService.GetProgramSettingsById(id).Settings;

                //ReportRepudiatedDays
                programSettings.Where(s => s.TemplateName.Equals(SettingType.ReportRepudiatedDays)).ToList().ForEach(s => s.Value = model.ReportRepudiatedDays);
                //EmailContactInfoName
                programSettings.Where(s => s.TemplateName.Equals(SettingType.EmailContactInfoName)).ToList().ForEach(s => s.Value = model.EmailContactInfoName);
                //EmailContactInfoPhone
                programSettings.Where(s => s.TemplateName.Equals(SettingType.EmailContactInfoPhone)).ToList().ForEach(s => s.Value = model.EmailContactInfoPhone);
                //EmailContactInfoEmailAddress
                programSettings.Where(s => s.TemplateName.Equals(SettingType.EmailContactInfoEmailAddress)).ToList()
                    .ForEach(s => s.Value = ("" + model.EmailContactInfoEmailAddress));

                _settingService.CreateOrUpdateProgramSettings(id, programSettings);

                ViewBag.ShowSuccessMessageForProgramSettings = true;
                ViewBag.SuccessMessageForProgramSettings = "Save successful.";
                ModelState.Clear();
                model = PrepareAuthoritySettings();

            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
                ViewBag.ShowErrorMessageForProgramSettings = true;
                model = PrepareAuthoritySettings();
            }

            return View(viewName: "Settings", model: model);
        }

        private AuthoritySettingsViewModel PrepareAuthoritySettings()
        {
            int id = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var authority = _organizationService.GetOrganizationRegulatoryProgram(id);
            var authoritySettings = _settingService.GetOrganizationSettingsById(authority.OrganizationId);
            var programSettings = _settingService.GetProgramSettingsById(authority.OrganizationRegulatoryProgramId);
            var userRole = _httpContextService.GetClaimValue(CacheKey.UserRole) ?? "";

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
                HasPermissionForUpdate = userRole.IsCaseInsensitiveEqual(UserRole.Administrator.ToString()),

                FailedPasswordAttemptMaxCount = authoritySettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.FailedPasswordAttemptMaxCount))
                                                                 .Select(s => s.Value).First(),
                FailedPasswordAttemptMaxCountDefault = authoritySettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.FailedPasswordAttemptMaxCount))
                                                                 .Select(s => s.DefaultValue).First(),
                FailedKbqAttemptMaxCount = authoritySettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.FailedKBQAttemptMaxCount))
                                                                 .Select(s => s.Value).First(),
                FailedKbqAttemptMaxCountDefault = authoritySettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.FailedKBQAttemptMaxCount))
                                                                 .Select(s => s.DefaultValue).First(),
                InvitationExpiredHours = authoritySettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.InvitationExpiredHours))
                                                                 .Select(s => s.Value).First(),
                InvitationExpiredHoursDefault = authoritySettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.InvitationExpiredHours))
                                                                 .Select(s => s.DefaultValue).First(),
                PasswordChangeRequiredDays = authoritySettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.PasswordChangeRequiredDays))
                                                                 .Select(s => s.Value).First(),
                PasswordChangeRequiredDaysDefault = authoritySettings.Settings
                                                                .Where(s => s.TemplateName.Equals(SettingType.PasswordChangeRequiredDays))
                                                                 .Select(s => s.DefaultValue).First(),
                PasswordHistoryMaxCount = authoritySettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.PasswordHistoryMaxCount))
                                                                 .Select(s => s.Value).First(),
                PasswordHistoryMaxCountDefault = authoritySettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.PasswordHistoryMaxCount))
                                                                 .Select(s => s.DefaultValue).First(),
                TimeZone = authoritySettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.TimeZone))
                                                                 .Select(s => s.Value).First(),

                ReportRepudiatedDays = programSettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.ReportRepudiatedDays))
                                                                 .Select(s => s.Value).First(),
                ReportRepudiatedDaysDefault = programSettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.ReportRepudiatedDays))
                                                                 .Select(s => s.DefaultValue).First(),
                MassLoadingConversionFactorPounds = programSettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.MassLoadingConversionFactorPounds))
                                                                 .Select(s => s.Value).First(),
                MassLoadingResultToUseLessThanSign = programSettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.MassLoadingResultToUseLessThanSign))
                                                                 .Select(s => s.Value).First(),
                MassLoadingCalculationDecimalPlaces = programSettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.MassLoadingCalculationDecimalPlaces))
                                                                 .Select(s => s.Value).First(),
                EmailContactInfoName = programSettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.EmailContactInfoName))
                                                                 .Select(s => s.Value).First(),
                EmailContactInfoNameDefault = authority.OrganizationDto.OrganizationName,
                EmailContactInfoPhone = programSettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.EmailContactInfoPhone))
                                                                 .Select(s => s.Value).First(),
                EmailContactInfoPhoneDefault = authority.OrganizationDto.PhoneNumber,
                EmailContactInfoEmailAddress = programSettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.EmailContactInfoEmailAddress))
                                                                 .Select(s => s.Value).First(),
                AuthorityUserLicenseTotalCount = programSettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.AuthorityUserLicenseTotalCount))
                                                                 .Select(s => s.Value).First(),
                AuthorityUserLicenseUsedCount = _organizationService.GetCurrentUserLicenseCount(id).ToString(),
                IndustryLicenseTotalCount = programSettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.IndustryLicenseTotalCount))
                                                                 .Select(s => s.Value).First(),
                IndustryLicenseUsedCount = _organizationService.GetCurrentIndustryLicenseCount(id).ToString(),
                UserPerIndustryMaxCount = programSettings.Settings
                                                                 .Where(s => s.TemplateName.Equals(SettingType.UserPerIndustryMaxCount))
                                                                 .Select(s => s.Value).First(),
            };

            // Time Zones
            viewModel.AvailableTimeZones = new List<SelectListItem>();
            var timeZones = _timeZoneService.GetTimeZones();
            if (timeZones.Count > 0)
            {
                viewModel.AvailableTimeZones = timeZones.Select(tz => new SelectListItem
                {
                    Text = TimeZoneInfo.FindSystemTimeZoneById(tz.Name).DisplayName,
                    Value = tz.TimeZoneId.ToString(),
                    Selected = (tz.TimeZoneId.ToString().Equals(viewModel.TimeZone))
                }).ToList();
            }
            //viewModel.AvailableTimeZones.Insert(index: 0, item: new SelectListItem { Text = "Select Time Zone", Value = "0" });

            return viewModel;
        }

        #endregion

        #region Show Audit Logs

        // GET: /Authority/AuditLogs
        public ActionResult AuditLogs()
        {
            return View();
        }

        // POST: /Authority/AuditLogs
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AuditLogs(AuditLogViewModel model, FormCollection collection)
        {
            ViewBag.SearchString = collection["searchString"];
            return View(model);
        }

        public ActionResult AuditLogs_Read([DataSourceRequest] DataSourceRequest request, string searchString)
        {
            var organizationRegulatoryProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            int timeZoneId = Convert.ToInt32(_settingService.GetOrganizationSettingValue(organizationRegulatoryProgramId, SettingType.TimeZone));
            var logEntries = _cromerrLogService.GetCromerrAuditLogEntries(organizationRegulatoryProgramId, searchString);

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
                                dto.LogDateTimeUtc.DateTime, timeZoneId)

            });

            DataSourceResult result = viewModels.ToDataSourceResult(request, vm => new
            {
                CromerrAuditLogId = vm.CromerrAuditLogId,
                AuditLogTemplateId = vm.AuditLogTemplateId,
                RegulatoryProgramName = vm.RegulatoryProgramName,
                OrganizationId = vm.OrganizationId,
                OrganizationName = vm.OrganizationName,
                RegulatorName = vm.RegulatorName,
                EventCategory = vm.EventCategory,
                EventType = vm.EventType,
                UserProfileIdDisplay = vm.UserProfileIdDisplay,
                UserName = vm.UserName,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                EmailAddress = vm.EmailAddress,
                IPAddress = vm.IPAddress,
                HostName = vm.HostName,
                Comment = vm.Comment,
                LogDateTimeUtc = vm.LogDateTimeUtc.ToString(),
                LogDateTimeUtcDetailString = vm.LogDateTimeUtcDetailString
            });

            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AuditLogs_Select(int cromerrAuditLogId)
        {
            try
            {
                if (cromerrAuditLogId != -1 && ModelState.IsValid)
                {
                    var logDetails = _cromerrLogService.GetCromerrAuditLogEntry(cromerrAuditLogId).Comment;
                    return Json(new
                    {
                        redirect = false,
                        details = logDetails //$"Log Entry Details for Id = {cromerrAuditLogId}"
                    });
                }
                else
                {
                    return Json(new
                    {
                        redirect = false,
                        message = "Please select a log entry."
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

        [HttpPost]
        public ActionResult AuditLogs_Excel_Export_Save(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }

        #endregion

        #region Show Authority Users

        // GET: /Authority/Users
        [Route("Users")]
        public ActionResult AuthorityUsers()
        {
            int id = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var authority = _organizationService.GetOrganizationRegulatoryProgram(id);
            ViewBag.Title = string.Format(format: "{0} Users", arg0: authority.OrganizationDto.OrganizationName);

            var remainingUserLicenseCount = _organizationService.GetRemainingUserLicenseCount(id);
            ViewBag.CanInvite = _httpContextService.GetClaimValue(CacheKey.UserRole).IsCaseInsensitiveEqual(UserRole.Administrator.ToString())
                                && remainingUserLicenseCount > 0;
            return View();
        }

        public ActionResult AuthorityUsers_Read([DataSourceRequest] DataSourceRequest request)
        {
            var organizationRegulatoryProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var users = _userService.GetUserProfilesForOrgRegProgram(organizationRegulatoryProgramId, isRegApproved: true, isRegDenied: false, isEnabled: null, isRemoved: false);

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

            DataSourceResult result = viewModels.ToDataSourceResult(request, vm => new
            {
                Id = vm.Id,
                PId = vm.PId,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                PhoneNumber = vm.PhoneNumber,
                Email = vm.Email,
                ResetEmail = vm.ResetEmail,
                DateRegistered = vm.DateRegistered,
                StatusText = vm.StatusText,
                AccountLockedText = vm.AccountLockedText,
                Role = vm.Role,
                RoleText = vm.RoleText
            });

            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AuthorityUsers_Select(IEnumerable<AuthorityUserViewModel> items)
        {
            try
            {
                if (items != null && ModelState.IsValid)
                {
                    var item = items.First();
                    return Json(new
                    {
                        redirect = true,
                        newurl = Url.Action(actionName: "AuthorityUserDetails", controllerName: "Authority", routeValues: new
                        {
                            id = item.Id
                        })
                    });
                }
                else
                {
                    return Json(new
                    {
                        redirect = false,
                        message = "Please select an user."
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

        public ActionResult AuthorityUsers_PendingInvitations_Read([DataSourceRequest] DataSourceRequest request)
        {
            var organizationRegulatoryProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var invitations = _invitationService.GetInvitationsForOrgRegProgram(organizationRegulatoryProgramId, organizationRegulatoryProgramId);

            var viewModels = invitations.Select(vm => new PendingInvitationViewModel
            {
                Id = vm.InvitationId,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.EmailAddress,
                DateInvited = vm.InvitationDateTimeUtc.DateTime,
                InviteExpires = vm.ExpiryDateTimeUtc.DateTime
            });

            DataSourceResult result = viewModels.ToDataSourceResult(request, vm => new
            {
                Id = vm.Id,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                DateInvited = vm.DateInvited,
                InviteExpires = vm.InviteExpires
            });

            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AuthorityUsers_PendingInvitations_Delete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<PendingInvitationViewModel> items)
        {
            if (!ModelState.IsValid)
            {
                return Json(items.ToDataSourceResult(request, ModelState));
            }

            try
            {
                if (items.Any())
                {
                    var item = items.First();

                    _invitationService.DeleteInvitation(item.Id);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
            }

            return Json(items.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region Show Authority User Details

        // GET: /Authority/AuthorityUserDetails
        [Route("User/{id:int}/Details")]
        [AuthorizeCorrectAuthorityOnly(true)]
        public ActionResult AuthorityUserDetails(int id)
        {
            AuthorityUserViewModel viewModel = PrepareAuthorityUserDetails(id);

            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("User/{id:int}/Details")]
        [AuthorizeCorrectAuthorityOnly(true)]
        public ActionResult AuthorityUserDetails(int id, AuthorityUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                _userService.UpdateUserPermissionGroupId(model.Id, model.Role);
                ViewBag.ShowSuccessMessage = true;
                ViewBag.SuccessMessage = "User role updated successfully!";
                ModelState.Clear();
                model = PrepareAuthorityUserDetails(id);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
                model = PrepareAuthorityUserDetails(id);
            }

            return View(viewName: "AuthorityUserDetails", model: model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("User/{id:int}/Details/UserLockUnLock")]
        [AuthorizeCorrectAuthorityOnly(true)]
        public ActionResult AuthorityUserLockUnLock(int id, AuthorityUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                _userService.LockUnlockUserAccount(model.PId, !model.AccountLocked, AccountLockEvent.ManualAction);

                ViewBag.ShowSuccessMessage = true;
                ViewBag.SuccessMessage = model.AccountLocked ? "User unlocked!" : "User locked!";
                ModelState.Clear();
                model = PrepareAuthorityUserDetails(id);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
                model = PrepareAuthorityUserDetails(id);
            }

            return View(viewName: "AuthorityUserDetails", model: model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("User/{id:int}/Details/ChangeStatus")]
        [AuthorizeCorrectAuthorityOnly(true)]
        public ActionResult AuthorityUserChangeStatus(int id, AuthorityUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                _userService.EnableDisableUserAccount(model.Id, model.Status);

                ViewBag.ShowSuccessMessage = true;
                ViewBag.SuccessMessage = model.Status ? "User disabled!" : "User enabled!";
                ModelState.Clear();
                model = PrepareAuthorityUserDetails(id);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
                model = PrepareAuthorityUserDetails(id);
            }

            return View(viewName: "AuthorityUserDetails", model: model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("User/{id:int}/Details/UserRemove")]
        [AuthorizeCorrectAuthorityOnly(true)]
        public ActionResult AuthorityUserRemove(int id, AuthorityUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var result = _userService.RemoveUser(model.Id);

                if (result)
                {
                    return RedirectToAction(actionName: "AuthorityUserRemoved", controllerName: "Authority");
                }
                else
                {
                    List<RuleViolation> validationIssues = new List<RuleViolation>();
                    string message = "Remove user failed.";
                    validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                    throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
                model = PrepareAuthorityUserDetails(id);
            }

            return View(viewName: "AuthorityUserDetails", model: model);
        }

        // user remove successfully
        // GET: /Authority/AuthorityUserRemoved
        public ActionResult AuthorityUserRemoved()
        {
            ConfirmationViewModel model = new ConfirmationViewModel();
            model.Title = "User Remove Status";
            model.Message = "User Removed!";

            return View(viewName: "Confirmation", model: model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("User/{id:int}/Details/UserReset")]
        [AuthorizeCorrectAuthorityOnly(true)]
        public ActionResult AuthorityUserReset(int id, AuthorityUserViewModel model)
        {
            string newEmail = model.ResetEmail;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var result = _userService.ResetUser(model.PId, newEmail);

                if (result.IsSuccess)
                {
                    ViewBag.ShowSuccessMessage = true;
                    ViewBag.SuccessMessage = "User account reset successfully!";
                    ModelState.Clear();
                    model = PrepareAuthorityUserDetails(id);
                }
                else
                {
                    List<RuleViolation> validationIssues = new List<RuleViolation>();
                    string message = "";

                    switch (result.FailureReason)
                    {
                        case ResetUserFailureReason.NewEmailAddressAlreadyInUse:
                            message = "Email is already in use on another account.";
                            break;
                        default:
                            message = "User account reset failed";
                            break;
                    }

                    validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                    throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);

                model = PrepareAuthorityUserDetails(id);
                model.ResetEmail = newEmail;
            }

            return View(viewName: "AuthorityUserDetails", model: model);
        }

        private AuthorityUserViewModel PrepareAuthorityUserDetails(int id)
        {
            var user = _userService.GetOrganizationRegulatoryProgramUser(id);
            var userQuesAns = _questionAnswerService.GetUsersQuestionAnswers(user.UserProfileId, QuestionTypeName.SQ);
            var currentUserRole = _httpContextService.GetClaimValue(CacheKey.UserRole) ?? "";
            var currentUserProfileId = _httpContextService.GetClaimValue(CacheKey.UserProfileId);

            ViewBag.HasPermissionForUpdate = currentUserRole.IsCaseInsensitiveEqual(UserRole.Administrator.ToString()) &&
                !currentUserProfileId.IsCaseInsensitiveEqual(user.UserProfileId.ToString());
            ViewBag.HasPermissionForChangeRole = currentUserRole.IsCaseInsensitiveEqual(UserRole.Administrator.ToString());

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
                SecurityQuestion1 = (userQuesAns.Count > 0 && userQuesAns.ElementAt(index: 0) != null) ? userQuesAns.ElementAt(index: 0).Question.Content : "",
                Answer1 = (userQuesAns.Count > 0 && userQuesAns.ElementAt(index: 0) != null) ? userQuesAns.ElementAt(index: 0).Answer.Content : "",
                SecurityQuestion2 = (userQuesAns.Count > 1 && userQuesAns.ElementAt(index: 1) != null) ? userQuesAns.ElementAt(index: 1).Question.Content : "",
                Answer2 = (userQuesAns.Count > 1 && userQuesAns.ElementAt(index: 1) != null) ? userQuesAns.ElementAt(index: 1).Answer.Content : "",
            };
            // Roles
            viewModel.AvailableRoles = new List<SelectListItem>();
            var roles = _permissionService.GetRoles(user.OrganizationRegulatoryProgramId);
            if (roles.Count() > 0)
            {
                viewModel.AvailableRoles = roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.PermissionGroupId.ToString(),
                    Selected = (Convert.ToInt32(r.PermissionGroupId) == viewModel.Role)
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
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Industries(IndustryViewModel model, FormCollection collection)
        {
            ViewBag.SearchString = collection["searchString"];

            return View(model);
        }

        private string GetClaimValue(string key)
        {
            var claims = ((System.Security.Claims.ClaimsPrincipal) HttpContext.User).Claims;
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
            int currentOrganizationRegulatoryProgramId = int.Parse(GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            // int currentOrganizationRegulatoryProgramId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var industries = _organizationService.GetChildOrganizationRegulatoryPrograms(currentOrganizationRegulatoryProgramId, searchString);

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
                AssignedTo = vm.AssignedTo,
            });

            DataSourceResult result = viewModels.ToDataSourceResult(request, vm => new
            {
                Id = vm.Id,
                IndustryNo = vm.IndustryNo,
                IndustryNoText = vm.IndustryNoText,
                IndustryName = vm.IndustryName,
                Address = vm.Address,
                Classification = vm.Classification,
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
                            id = item.Id
                        })
                    });
                }
                else
                {
                    return Json(new
                    {
                        redirect = false,
                        message = "Please select an industry."
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
        [AuthorizeCorrectAuthorityOnly(false)]
        public ActionResult IndustryDetails(int id)
        {
            IndustryViewModel viewModel = PrepareIndustryDetails(id);

            return View(viewModel);
        }

        // POST: /Authority/IndustryDetails
        [AcceptVerbs(HttpVerbs.Post)]
        [Route("Industry/{id:int}/Details")]
        [AuthorizeCorrectAuthorityOnly(false)]
        public ActionResult IndustryDetails(int id, IndustryViewModel model)
        {
            try
            {
                var result = _organizationService.UpdateEnableDisableFlag(model.Id, !model.IsEnabled);
                bool isUpdated = result.IsSuccess;

                if (isUpdated)
                {
                    ViewBag.ShowSuccessMessage = true;
                    ViewBag.SuccessMessage = model.IsEnabled ? "Industry Disabled!" : "Industry Enabled!";
                    ModelState.Clear();
                    model = PrepareIndustryDetails(id);
                }
                else
                {
                    model = PrepareIndustryDetails(id);

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

        private IndustryViewModel PrepareIndustryDetails(int id)
        {
            var industry = _organizationService.GetOrganizationRegulatoryProgram(id);
            var userRole = _httpContextService.GetClaimValue(CacheKey.UserRole) ?? "";

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
        [Route("Industry/{id:int}/Users")]
        [AuthorizeCorrectAuthorityOnly(false)]
        public ActionResult IndustryUsers(int id)
        {
            ViewBag.IndustryId = id;
            var industry = _organizationService.GetOrganizationRegulatoryProgram(id);
            ViewBag.Title = string.Format(format: "{0} Users", arg0: industry.OrganizationDto.OrganizationName);

            //Invite button only visible if there isn't currently an active Admin for this IU
            ViewBag.CanInvite = !industry.HasAdmin;

            return View();
        }

        public ActionResult IndustryUsers_Read([DataSourceRequest] DataSourceRequest request, string industryId)
        {
            var organizationRegulatoryProgramId = int.Parse(industryId);
            var users = _userService.GetUserProfilesForOrgRegProgram(organizationRegulatoryProgramId, isRegApproved: true, isRegDenied: false, isEnabled: null, isRemoved: false);

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

            DataSourceResult result = viewModels.ToDataSourceResult(request, vm => new
            {
                Id = vm.Id,
                IId = vm.IId,
                PId = vm.PId,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                PhoneNumber = vm.PhoneNumber,
                Email = vm.Email,
                ResetEmail = vm.ResetEmail,
                DateRegistered = vm.DateRegistered,
                StatusText = vm.StatusText,
                AccountLockedText = vm.AccountLockedText,
                Role = vm.Role,
                RoleText = vm.RoleText
            });

            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult IndustryUsers_Select(IEnumerable<IndustryUserViewModel> items)
        {
            try
            {
                if (items != null && ModelState.IsValid)
                {
                    var item = items.First();
                    return Json(new
                    {
                        redirect = true,
                        newurl = Url.Action(actionName: "IndustryUserDetails", controllerName: "Authority", routeValues: new
                        {
                            iid = item.IId,
                            id = item.Id
                        })
                    });
                }
                else
                {
                    return Json(new
                    {
                        redirect = false,
                        message = "Please select an user."
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

        public ActionResult IndustryUsers_PendingInvitations_Read([DataSourceRequest] DataSourceRequest request, string industryId)
        {
            var industryOrgRegProgramId = int.Parse(industryId);
            int senderOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var invitations = _invitationService.GetInvitationsForOrgRegProgram(senderOrgRegProgramId, industryOrgRegProgramId);

            var viewModels = invitations.Select(vm => new PendingInvitationViewModel
            {
                Id = vm.InvitationId,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.EmailAddress,
                DateInvited = vm.InvitationDateTimeUtc.DateTime,
                InviteExpires = vm.ExpiryDateTimeUtc.DateTime
            });

            DataSourceResult result = viewModels.ToDataSourceResult(request, vm => new
            {
                Id = vm.Id,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                DateInvited = vm.DateInvited,
                InviteExpires = vm.InviteExpires
            });

            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult IndustryUsers_PendingInvitations_Delete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<PendingInvitationViewModel> items)
        {
            if (!ModelState.IsValid)
            {
                return Json(items.ToDataSourceResult(request, ModelState));
            }

            try
            {
                if (items.Any())
                {
                    var item = items.First();

                    _invitationService.DeleteInvitation(item.Id);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
            }

            return Json(items.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region Show Industry User Details

        // GET: /Authority/IndustryUserDetails
        [Route("Industry/{iid:int}/User/{id:int}/Details")]
        [AuthorizeCorrectAuthorityOnly(true)]
        public ActionResult IndustryUserDetails(int iid, int id)
        {
            IndustryUserViewModel viewModel = PrepareIndustryUserDetails(id);

            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("Industry/{iid:int}/User/{id:int}/Details/UpdateSignatoryStatus")]
        [AuthorizeCorrectAuthorityOnly(true)]
        public ActionResult IndustryUserUpdateSignatoryStatus(int iid, int id, IndustryUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                _userService.UpdateUserSignatoryStatus(model.Id, model.IsSignatory);
                ViewBag.ShowSuccessMessage = true;
                ViewBag.SuccessMessage = model.IsSignatory ? "User signatory permission granted!" : "User signatory permission removed!";
                ModelState.Clear();
                model = PrepareIndustryUserDetails(id);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
                model = PrepareIndustryUserDetails(id);
            }

            return View(viewName: "IndustryUserDetails", model: model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("Industry/{iid:int}/User/{id:int}/Details/UserLockUnLock")]
        [AuthorizeCorrectAuthorityOnly(true)]
        public ActionResult IndustryUserLockUnLock(int iid, int id, IndustryUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                _userService.LockUnlockUserAccount(model.PId, !model.AccountLocked, AccountLockEvent.ManualAction);

                ViewBag.ShowSuccessMessage = true;
                ViewBag.SuccessMessage = model.AccountLocked ? "User unlocked!" : "User locked!";
                ModelState.Clear();
                model = PrepareIndustryUserDetails(id);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
                model = PrepareIndustryUserDetails(id);
            }

            return View(viewName: "IndustryUserDetails", model: model);
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

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("Industry/{iid:int}/User/{id:int}/Details/UserReset")]
        [AuthorizeCorrectAuthorityOnly(true)]
        public ActionResult IndustryUserReset(int iid, int id, IndustryUserViewModel model)
        {
            string newEmail = model.ResetEmail;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var result = _userService.ResetUser(model.PId, newEmail, model.IId);

                if (result.IsSuccess)
                {
                    ViewBag.ShowSuccessMessage = true;
                    ViewBag.SuccessMessage = "User account reset successfully!";
                    ModelState.Clear();
                    model = PrepareIndustryUserDetails(id);
                }
                else
                {
                    List<RuleViolation> validationIssues = new List<RuleViolation>();
                    string message = "";

                    switch (result.FailureReason)
                    {
                        case ResetUserFailureReason.NewEmailAddressAlreadyInUse:
                            message = "Email is already in use on another account.";
                            break;
                        default:
                            message = "User account reset failed";
                            break;
                    }

                    validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                    throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);

                model = PrepareIndustryUserDetails(id);
                model.ResetEmail = newEmail;
            }

            return View(viewName: "IndustryUserDetails", model: model);
        }

        private IndustryUserViewModel PrepareIndustryUserDetails(int id)
        {
            var user = _userService.GetOrganizationRegulatoryProgramUser(id);
            var userQuesAns = _questionAnswerService.GetUsersQuestionAnswers(user.UserProfileId, QuestionTypeName.SQ);

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
                SecurityQuestion1 = (userQuesAns.Count > 0 && userQuesAns.ElementAt(index: 0) != null) ? userQuesAns.ElementAt(index: 0).Question.Content : "",
                Answer1 = (userQuesAns.Count > 0 && userQuesAns.ElementAt(index: 0) != null) ? userQuesAns.ElementAt(index: 0).Answer.Content : "",
                SecurityQuestion2 = (userQuesAns.Count > 1 && userQuesAns.ElementAt(index: 1) != null) ? userQuesAns.ElementAt(index: 1).Question.Content : "",
                Answer2 = (userQuesAns.Count > 1 && userQuesAns.ElementAt(index: 1) != null) ? userQuesAns.ElementAt(index: 1).Answer.Content : "",
            };
            return viewModel;
        }
        #endregion

        #region Show Pending User Approvals

        // GET: /Authority/PendingUserApprovals
        [Route("PendingUserApprovals")]
        public ActionResult PendingUserApprovals()
        {
            return View();
        }

        public ActionResult PendingUserApprovals_Read([DataSourceRequest] DataSourceRequest request)
        {
            var organizationRegulatoryProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var users = _userService.GetPendingRegistrationProgramUsers(organizationRegulatoryProgramId);

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

            DataSourceResult result = viewModels.ToDataSourceResult(request, vm => new
            {
                Id = vm.Id,
                PId = vm.PId,
                RegisteredOrgName = vm.RegisteredOrgName,
                Type = vm.Type,
                UserName = vm.UserName,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                BusinessName = vm.BusinessName,
                PhoneNumber = vm.PhoneNumber,
                Email = vm.Email,
                DateRegistered = vm.DateRegistered,
                Role = 1 // role need to be more than 0 otherwise ModelState.IsValid = false 
            });

            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult PendingUserApprovals_Select(IEnumerable<PendingUserApprovalViewModel> items)
        {
            try
            {
                if (items != null && ModelState.IsValid)
                {
                    var item = items.First();
                    return Json(new
                    {
                        redirect = true,
                        newurl = Url.Action(actionName: "PendingUserApprovalDetails", controllerName: "Authority", routeValues: new
                        {
                            id = item.Id
                        })
                    });
                }
                else
                {
                    return Json(new
                    {
                        redirect = false,
                        message = "Please select an user."
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

        #region Show Pending User Approval Details

        // GET: /Authority/PendingUserApprovals
        [Route("PendingUserApprovals/{id:int}/Details")]
        [AuthorizeCorrectAuthorityOnly(true)]
        public ActionResult PendingUserApprovalDetails(int id)
        {
            var viewModel = PreparePendingUserApprovalDetails(id);
            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("PendingUserApprovals/{id:int}/Details/PendingUserApprove")]
        [AuthorizeCorrectAuthorityOnly(true)]
        public ActionResult PendingUserApprove(int id, PendingUserApprovalViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = _userService.ApprovePendingRegistration(model.Id, model.Role.Value, isApproved: true);
                    switch (result.Result)
                    {
                        case RegistrationResult.Success:
                            ViewBag.ShowSuccessMessage = true;
                            ViewBag.SuccessMessage = "Registration Approved!";
                            ModelState.Clear();
                            _logger.Info(string.Format(format: "PendingUserApprove. User={0} - id={1} Registration Approved!", arg0: model.UserName, arg1: model.Id));
                            break;
                        case RegistrationResult.NoMoreUserLicensesForIndustry:
                            _logger.Info(string.Format(format: "PendingUserApprove. User={0} - id={1} No more user licenses", arg0: model.UserName, arg1: model.Id));
                            ModelState.AddModelError(key: "", errorMessage: "No more User Licenses are available for this Industry.  Disable another User and try again");
                            break;
                        case RegistrationResult.NoMoreUserLicensesForAuthority:
                            _logger.Info(string.Format(format: "PendingUserApprove. User={0} - id={1} No more user licenses", arg0: model.UserName, arg1: model.Id));
                            ModelState.AddModelError(key: "", errorMessage: "No more User Licenses are available for this Authority.  Disable another User and try again");
                            break;
                        default:
                            _logger.Info(string.Format(format: "PendingUserApprove. User={0} - id={1} Registration Approval Failed!", arg0: model.UserName, arg1: model.Id));
                            ModelState.AddModelError(key: "", errorMessage: "Registration Approval Failed");
                            break;
                    }
                }
                catch (RuleViolationException rve)
                {
                    MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
                }
            }
            model = PreparePendingUserApprovalDetails(id);
            return View(viewName: "PendingUserApprovalDetails", model: model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("PendingUserApprovals/{id:int}/Details/PendingUserDeny")]
        [AuthorizeCorrectAuthorityOnly(true)]
        public ActionResult PendingUserDeny(int id, PendingUserApprovalViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = _userService.ApprovePendingRegistration(model.Id, model.Role.Value, isApproved: false);
                    switch (result.Result)
                    {
                        case RegistrationResult.Success:
                            ViewBag.ShowSuccessMessage = true;
                            ViewBag.SuccessMessage = "Registration Denied!";
                            ModelState.Clear();
                            _logger.Info(string.Format(format: "PendingUserDeny. User={0} - id={1} Registration Denied!", arg0: model.UserName, arg1: model.Id));
                            break;
                        default:
                            _logger.Info(string.Format(format: "PendingUserDeny. User={0} - id={1} Registration Denial Failed!", arg0: model.UserName, arg1: model.Id));
                            ModelState.AddModelError(key: "", errorMessage: "Registration Denial Failed");
                            break;
                    }
                }
                catch (RuleViolationException rve)
                {
                    MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
                }
            }

            model = PreparePendingUserApprovalDetails(id);
            return View(viewName: "PendingUserApprovalDetails", model: model);
        }

        private PendingUserApprovalViewModel PreparePendingUserApprovalDetails(int id)
        {
            var result = _userService.GetOrganizationRegulatoryProgramUser(id);

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
                Role = (result.PermissionGroup == null) ? 0 : result.PermissionGroup.PermissionGroupId,
                RoleText = (result.PermissionGroup == null) ? "" : result.PermissionGroup.Name
            };
            // Roles
            viewModel.AvailableRoles = new List<SelectListItem>();
            var roles = _permissionService.GetRoles(result.OrganizationRegulatoryProgramId);

            if (roles.Count() > 0)
            {
                viewModel.AvailableRoles = roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.PermissionGroupId.ToString(),
                    Selected = (Convert.ToInt32(r.PermissionGroupId) == viewModel.Role)
                }).ToList();
            }
            viewModel.AvailableRoles.Insert(index: 0, item: new SelectListItem { Text = "Select User Role", Value = "0" });

            var currentUserRole = _httpContextService.GetClaimValue(CacheKey.UserRole) ?? "";
            ViewBag.HasPermissionForApproveDeny = false;
            if (viewModel.Type.IsCaseInsensitiveEqual(OrganizationTypeName.Industry.ToString()))
            {
                ViewBag.HasPermissionForApproveDeny = true;
            }
            else
            {
                // For authority user registration request, only authority admin can approve 
                if (currentUserRole.IsCaseInsensitiveEqual(UserRole.Administrator.ToString()))
                {
                    ViewBag.HasPermissionForApproveDeny = true;
                }
            }

            ViewBag.CanChangeRole = viewModel.Type.IsCaseInsensitiveEqual(OrganizationTypeName.Authority.ToString());

            if (viewModel.Type.IsCaseInsensitiveEqual(OrganizationTypeName.Industry.ToString())
                && (!viewModel.Role.HasValue || viewModel.Role.Value == 0))
            {
                viewModel.Role = roles.Where(r => r.Name.IsCaseInsensitiveEqual(UserRole.Administrator.ToString())).First().PermissionGroupId;
                viewModel.RoleText = UserRole.Administrator.ToString();
            }

            return viewModel;
        }
        #endregion
    }
}