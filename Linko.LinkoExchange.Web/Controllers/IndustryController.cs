using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.DataSource;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.FileStore;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.MonitoringPoint;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Parameter;
using Linko.LinkoExchange.Services.Permission;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Sample;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.Unit;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.Mvc;
using Linko.LinkoExchange.Web.ViewModels.Industry;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using Microsoft.Ajax.Utilities;
using NLog;

namespace Linko.LinkoExchange.Web.Controllers
{
    [PortalAuthorize("industry")]
    [RoutePrefix(prefix:"Industry")]
    public class IndustryController : BaseController
    {
        #region fields

        private readonly IDataSourceService _dataSourceService;

        private readonly IFileStoreService _fileStoreService;
        private readonly IHttpContextService _httpContextService;
        private readonly IInvitationService _invitationService;
        private readonly ILogger _logger;
        private readonly IMonitoringPointService _monitoringPointService;
        private readonly IOrganizationService _organizationService;
        private readonly IParameterService _parameterService;
        private readonly IPermissionService _permissionService;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IReportElementService _reportElementService;
        private readonly IReportTemplateService _reportTemplateService;
        private readonly ISampleService _sampleService;
        private readonly ISettingService _settingService;
        private readonly IUnitService _unitService;
        private readonly IUserService _userService;

        #endregion

        #region constructors and destructor

        public IndustryController(IOrganizationService organizationService, IUserService userService, IInvitationService invitationService,
                                  IQuestionAnswerService questionAnswerService, IPermissionService permissionService, ILogger logger, IHttpContextService httpContextService,
                                  IFileStoreService fileStoreService, IReportElementService reportElementService, ISampleService sampleService, IUnitService unitService,
                                  IMonitoringPointService monitoringPointService, IReportTemplateService reportTemplateService, ISettingService settingService,
                                  IParameterService parameterService, IReportPackageService reportPackageService, IDataSourceService dataSourceService)
            : base(httpContextService:httpContextService, userService:userService, reportPackageService:reportPackageService, sampleService:sampleService)
        {
            _fileStoreService = fileStoreService;
            _httpContextService = httpContextService;
            _invitationService = invitationService;
            _logger = logger;
            _monitoringPointService = monitoringPointService;
            _organizationService = organizationService;
            _parameterService = parameterService;
            _permissionService = permissionService;
            _questionAnswerService = questionAnswerService;
            _reportElementService = reportElementService;
            _reportTemplateService = reportTemplateService;
            _sampleService = sampleService;
            _settingService = settingService;
            _unitService = unitService;
            _userService = userService;
            _dataSourceService = dataSourceService;
        }

        #endregion

        #region default action

        // GET: Industry
        public ActionResult Index()
        {
            return RedirectToAction(actionName:"Samples", controllerName:"Industry", routeValues:new {sampleStatus = SampleStatusName.Draft});
        }

        #endregion

        #region industry details

        // GET: /Industry/Settings
        [Route(template:"Settings")]
        public ActionResult Settings()
        {
            var currentOrganizationRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var industry = _organizationService.GetOrganizationRegulatoryProgram(orgRegProgId:currentOrganizationRegulatoryProgramId);

            var viewModel = new IndustryViewModel
                            {
                                Id = industry.OrganizationRegulatoryProgramId,
                                IndustryNo = industry.OrganizationDto.OrganizationId,
                                ReferenceNumber = industry.ReferenceNumber,
                                IndustryName = industry.OrganizationDto.OrganizationName,
                                AddressLine1 = industry.OrganizationDto.AddressLine1,
                                AddressLine2 = industry.OrganizationDto.AddressLine2,
                                CityName = industry.OrganizationDto.CityName,
                                State = industry.OrganizationDto.State,
                                ZipCode = industry.OrganizationDto.ZipCode,
                                PhoneNumber = industry.OrganizationDto.PhoneNumber,
                                PhoneExt = industry.OrganizationDto.PhoneExt,
                                FaxNumber = industry.OrganizationDto.FaxNumber,
                                WebsiteUrl = industry.OrganizationDto.WebsiteURL
                            };

            return View(model:viewModel);
        }

        // POST: /Industry/Settings
        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [Route(template:"Settings")]
        public ActionResult Settings(IndustryViewModel model)
        {
            return View(model:model);
        }

        #endregion

        #region industry users

        // GET: /Industry/Users
        [Route(template:"Users")]
        public ActionResult IndustryUsers()
        {
            var id = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var industry = _organizationService.GetOrganizationRegulatoryProgram(orgRegProgId:id);
            ViewBag.Title = string.Format(format:"{0} Users", arg0:industry.OrganizationDto.OrganizationName);

            var remainingUserLicenseCount = _organizationService.GetRemainingUserLicenseCount(orgRegProgramId:id);
            ViewBag.CanInvite = _httpContextService.GetClaimValue(claimType:CacheKey.UserRole).IsCaseInsensitiveEqual(comparing:UserRole.Administrator.ToString())
                                && remainingUserLicenseCount > 0;
            return View();
        }

        public ActionResult IndustryUsers_Read([CustomDataSourceRequest] DataSourceRequest request)
        {
            var organizationRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var users = _userService.GetUserProfilesForOrgRegProgram(orgRegProgramId:organizationRegulatoryProgramId, isRegApproved:true, isRegDenied:false, isEnabled:null,
                                                                     isRemoved:false);

            var viewModels = users.Select(vm => new IndustryUserViewModel
                                                {
                                                    Id = vm.OrganizationRegulatoryProgramUserId,
                                                    PId = vm.UserProfileId,
                                                    FirstName = vm.UserProfileDto.FirstName,
                                                    LastName = vm.UserProfileDto.LastName,
                                                    PhoneNumber = vm.UserProfileDto.PhoneNumber,
                                                    Email = vm.UserProfileDto.Email,
                                                    ResetEmail = vm.UserProfileDto.Email,
                                                    DateRegistered = vm.RegistrationDateTimeUtc?.DateTime,
                                                    Status = vm.IsEnabled,
                                                    AccountLocked = vm.UserProfileDto.IsAccountLocked,
                                                    Role = vm.PermissionGroup.PermissionGroupId ?? 0,
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
                                                                                           DateRegistered = vm.DateRegistered.ToString(),
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
                                         newurl = Url.Action(actionName:"IndustryUserDetails", controllerName:"Industry", routeValues:new
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

        public ActionResult IndustryUsers_PendingInvitations_Read([DataSourceRequest] DataSourceRequest request)
        {
            var organizationRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var invitations = _invitationService.GetInvitationsForOrgRegProgram(senderOrgRegProgramId:organizationRegulatoryProgramId,
                                                                                targetOrgRegProgramId:organizationRegulatoryProgramId);

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
                                                                                           DateInvited = vm.DateInvited.ToString(),
                                                                                           InviteExpires = vm.InviteExpires.ToString()
                                                                                       });

            return Json(data:result, behavior:JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult IndustryUsers_PendingInvitations_Delete([DataSourceRequest] DataSourceRequest request,
                                                                    [Bind(Prefix = "models")] IEnumerable<PendingInvitationViewModel> items)
        {
            if (!ModelState.IsValid)
            {
                return Json(data:items.ToDataSourceResult(request:request, modelState:ModelState));
            }

            var viewModels = items as IList<PendingInvitationViewModel> ?? items.ToList();
            try
            {
                if (viewModels.Any())
                {
                    var item = viewModels.First();

                    _invitationService.DeleteInvitation(invitationId:item.Id);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return Json(data:viewModels.ToDataSourceResult(request:request, modelState:ModelState));
        }

        #endregion

        #region industry user details

        // GET: /Industry/User/{id}/Details
        [Route(template:"User/{id:int}/Details")]
        public ActionResult IndustryUserDetails(int id)
        {
            var viewModel = PrepareIndustryUserDetails(id:id, isAuthorizationRequired:true);

            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"User/{id:int}/Details")]
        public ActionResult IndustryUserDetails(int id, IndustryUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model:model);
            }

            try
            {
                _userService.UpdateUserPermissionGroupId(orgRegProgUserId:model.Id, permissionGroupId:model.Role, isAuthorizationRequired:true);
                ViewBag.ShowSuccessMessage = true;
                ViewBag.SuccessMessage = "User updated successfully!";
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
        [Route(template:"User/{id:int}/Details/UserRemove")]
        public ActionResult IndustryUserRemove(int id, IndustryUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(viewName:"IndustryUserDetails", model:model);
            }

            try
            {
                var result = _userService.RemoveUser(orgRegProgUserId:model.Id, isAuthorizationRequired:true);
                if (result)
                {
                    TempData[key:"DeleteUserSucceed"] = true;
                    return RedirectToAction(actionName:"IndustryUsers");
                }

                var validationIssues = new List<RuleViolation>();
                var message = "Remove user failed.";
                validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                model = PrepareIndustryUserDetails(id:id);
                return View(viewName:"IndustryUserDetails", model:model);
            }
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"User/{id:int}/Details/ChangeStatus")]
        public ActionResult IndustryUserChangeStatus(int id, IndustryUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(viewName:"IndustryUserDetails", model:model);
            }

            try
            {
                _userService.EnableDisableUserAccount(orgRegProgramUserId:model.Id, isAttemptingDisable:model.Status, isAuthorizationRequired:true);

                ViewBag.ShowSuccessMessage = true;
                ViewBag.SuccessMessage = model.Status ? "User Disabled!" : "User Enabled!";
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

        private IndustryUserViewModel PrepareIndustryUserDetails(int id, bool isAuthorizationRequired = false)
        {
            var user = _userService.GetOrganizationRegulatoryProgramUser(orgRegProgUserId:id, isAuthorizationRequired:isAuthorizationRequired);

            var userQuesAns = _questionAnswerService.GetUsersQuestionAnswers(userProfileId:user.UserProfileId, questionType:QuestionTypeName.SQ);
            var currentUserRole = _httpContextService.GetClaimValue(claimType:CacheKey.UserRole) ?? "";
            var currentUserProfileId = _httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId);

            ViewBag.HasPermissionForUpdate = currentUserRole.IsCaseInsensitiveEqual(comparing:UserRole.Administrator.ToString())
                                             && !currentUserProfileId.IsCaseInsensitiveEqual(comparing:user.UserProfileId.ToString());

            ViewBag.HasPermissionForChangeRole = currentUserRole.IsCaseInsensitiveEqual(comparing:UserRole.Administrator.ToString());

            var viewModel = new IndustryUserViewModel
                            {
                                Id = user.OrganizationRegulatoryProgramUserId,
                                PId = user.UserProfileId,
                                FirstName = user.UserProfileDto.FirstName,
                                LastName = user.UserProfileDto.LastName,
                                PhoneNumber = user.UserProfileDto.PhoneNumber,
                                PhoneExt = user.UserProfileDto.PhoneExt,
                                Email = user.UserProfileDto.Email,
                                ResetEmail = user.UserProfileDto.Email,
                                DateRegistered = user.RegistrationDateTimeUtc?.DateTime,
                                Status = user.IsEnabled,
                                AccountLocked = user.UserProfileDto.IsAccountLocked,
                                Role = user.PermissionGroup.PermissionGroupId ?? 0,
                                RoleText = user.PermissionGroup.Name,
                                IsSignatory = user.IsSignatory,
                                SecurityQuestion1 = userQuesAns.Count > 0 && userQuesAns.ElementAt(index:0) != null ? userQuesAns.ElementAt(index:0).Question.Content : "",
                                Answer1 = userQuesAns.Count > 0 && userQuesAns.ElementAt(index:0) != null ? userQuesAns.ElementAt(index:0).Answer.Content : "",
                                SecurityQuestion2 = userQuesAns.Count > 1 && userQuesAns.ElementAt(index:1) != null ? userQuesAns.ElementAt(index:1).Question.Content : "",
                                Answer2 = userQuesAns.Count > 1 && userQuesAns.ElementAt(index:1) != null ? userQuesAns.ElementAt(index:1).Answer.Content : ""
                            };

            // Roles
            viewModel.AvailableRoles = new List<SelectListItem>();
            var roles = _permissionService.GetRoles(orgRegProgramId:user.OrganizationRegulatoryProgramId);
            var permissionGroupDtos = roles as IList<PermissionGroupDto> ?? roles.ToList();
            if (permissionGroupDtos.Any())
            {
                viewModel.AvailableRoles = permissionGroupDtos.Select(r => new SelectListItem
                                                                           {
                                                                               Text = r.Name,
                                                                               Value = r.PermissionGroupId.ToString(),
                                                                               Selected = Convert.ToInt32(value:r.PermissionGroupId) == viewModel.Role
                                                                           }).ToList();
            }
            return viewModel;
        }

        #endregion

        #region pending user approvals

        // GET: /Industry/PendingUserApprovals
        [Route(template:"PendingUserApprovals")]
        public ActionResult PendingUserApprovals()
        {
            return View();
        }

        public ActionResult PendingUserApprovals_Read([CustomDataSourceRequest] DataSourceRequest request)
        {
            var organizationRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var users = _userService.GetPendingRegistrationProgramUsers(orgRegProgramId:organizationRegulatoryProgramId);

            // TODO: Change service as not including industry users
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
                                                    DateRegistered = vm.RegistrationDateTimeUtc?.DateTime
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
                                                                                           DateRegistered = vm.DateRegistered.ToString(),
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
                                         newurl = Url.Action(actionName:"PendingUserApprovalDetails", controllerName:"Industry", routeValues:new
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

        #region pending user approval details

        // GET: /Industry/PendingUserApprovals
        [Route(template:"PendingUserApprovals/{id:int}/Details")]
        public ActionResult PendingUserApprovalDetails(int id)
        {
            var viewModel = PreparePendingUserApprovalDetails(id:id, isAuthorizationRequired:true);
            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"PendingUserApprovals/{id:int}/Details/PendingUserApprove")]
        public ActionResult PendingUserApprove(int id, PendingUserApprovalViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = _userService.ApprovePendingRegistration(orgRegProgUserId:model.Id, permissionGroupId:model.Role ?? 0, isApproved:true,
                                                                         isAuthorizationRequired:true);
                    switch (result.Result)
                    {
                        case RegistrationResult.Success:
                            ViewBag.ShowSuccessMessage = true;
                            ViewBag.SuccessMessage = "Registration Approved! An email has been sent to the registrant.";
                            ModelState.Clear();
                            _logger.Info(message:string.Format(format:"PendingUserApprove. User={0} - id={1} Registration Approved!", arg0:model.UserName, arg1:model.Id));
                            break;
                        case RegistrationResult.NoMoreUserLicensesForIndustry:
                            _logger.Info(message:string.Format(format:"PendingUserApprove. User={0} - id={1} No more user licenses", arg0:model.UserName, arg1:model.Id));
                            ModelState.AddModelError(key:"", errorMessage:@"No more User Licenses are available for this Industry. Disable another User and try again.");
                            break;
                        default:
                            _logger.Info(message:string.Format(format:"PendingUserApprove. User={0} - id={1} Registration Approval Failed!", arg0:model.UserName,
                                                               arg1:model.Id));
                            ModelState.AddModelError(key:"", errorMessage:@"Registration Approval Failed!");
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
        public ActionResult PendingUserDeny(int id, PendingUserApprovalViewModel model)
        {
            //No need to validate since we are denying
            try
            {
                var result = _userService.ApprovePendingRegistration(orgRegProgUserId:model.Id, permissionGroupId:model.Role ?? 0, isApproved:false);
                switch (result.Result)
                {
                    case RegistrationResult.Success:
                        ViewBag.ShowSuccessMessage = true;
                        ViewBag.SuccessMessage = "Registration Denied!";
                        ModelState.Clear();
                        _logger.Info(message:string.Format(format:"PendingUserDeny. User={0} - id={1} Registration Denied!", arg0:model.UserName, arg1:model.Id));
                        break;
                    default:
                        _logger.Info(message:string.Format(format:"PendingUserDeny. User={0} - id={1} Registration Denial Failed!", arg0:model.UserName, arg1:model.Id));
                        ModelState.AddModelError(key:"", errorMessage:@"Registration Denial Failed!");
                        break;
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            model = PreparePendingUserApprovalDetails(id:id);
            return View(viewName:"PendingUserApprovalDetails", model:model);
        }

        private PendingUserApprovalViewModel PreparePendingUserApprovalDetails(int id, bool isAuthorizationRequired = false)
        {
            var result = _userService.GetOrganizationRegulatoryProgramUser(orgRegProgUserId:id, isAuthorizationRequired:isAuthorizationRequired);

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
                                DateRegistered = result.RegistrationDateTimeUtc?.DateTime,
                                Role = result.PermissionGroup?.PermissionGroupId ?? 0,
                                RoleText = result.PermissionGroup == null ? "" : result.PermissionGroup.Name
                            };

            // Roles
            viewModel.AvailableRoles = new List<SelectListItem>();
            var roles = _permissionService.GetRoles(orgRegProgramId:result.OrganizationRegulatoryProgramId);

            var permissionGroupDtos = roles as IList<PermissionGroupDto> ?? roles.ToList();
            if (permissionGroupDtos.Any())
            {
                viewModel.AvailableRoles = permissionGroupDtos.Select(r => new SelectListItem
                                                                           {
                                                                               Text = r.Name,
                                                                               Value = r.PermissionGroupId.ToString(),
                                                                               Selected = Convert.ToInt32(value:r.PermissionGroupId) == viewModel.Role
                                                                           }).ToList();
            }
            viewModel.AvailableRoles.Insert(index:0, item:new SelectListItem {Text = @"Select User Role", Value = "0", Disabled = true});

            var currentUserRole = _httpContextService.GetClaimValue(claimType:CacheKey.UserRole) ?? "";
            ViewBag.HasPermissionForApproveDeny = currentUserRole.IsCaseInsensitiveEqual(comparing:UserRole.Administrator.ToString()); // TODO: call service when implement

            return viewModel;
        }

        #endregion

        #region attachment list

        // GET: /Industry/Attachments
        public ActionResult Attachments()
        {
            return View();
        }

        public ActionResult Attachments_Read([CustomDataSourceRequest] DataSourceRequest request)
        {
            var dtos = _fileStoreService.GetFileStores();

            var viewModels = dtos.Select(vm => new AttachmentViewModel
                                               {
                                                   Id = vm.FileStoreId,
                                                   Name = vm.Name,
                                                   OriginalFileName = vm.OriginalFileName,
                                                   Description = vm.Description,
                                                   ReportElementTypeName = vm.ReportElementTypeName,
                                                   UploadDateTimeLocal = vm.UploadDateTimeLocal,
                                                   UploaderUserFullName = vm.UploaderUserFullName,
                                                   UsedByReports = vm.UsedByReports
                                               });

            var result = viewModels.ToDataSourceResult(request:request, selector:vm => new
                                                                                       {
                                                                                           vm.Id,
                                                                                           vm.Name,
                                                                                           vm.OriginalFileName,
                                                                                           vm.Description,
                                                                                           vm.ReportElementTypeName,
                                                                                           UploadDateTimeLocal =
                                                                                           vm.UploadDateTimeLocal.ToString(provider:CultureInfo.CurrentCulture),
                                                                                           vm.UploaderUserFullName,
                                                                                           vm.Status
                                                                                       });

            return Json(data:result);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult Attachments_Select(IEnumerable<AttachmentViewModel> items)
        {
            try
            {
                if (items != null)
                {
                    var item = items.First();
                    return Json(data:new
                                     {
                                         redirect = true,
                                         newurl = Url.Action(actionName:"AttachmentDetails", controllerName:"Industry", routeValues:new {id = item.Id})
                                     });
                }

                return Json(data:new
                                 {
                                     redirect = false,
                                     message = "Please select an attachment."
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

        #region attachment details

        [Route(template:"Attachment/New")]
        public ActionResult NewAttachmentDetails()
        {
            var viewModel = new AttachmentViewModel();

            try
            {
                viewModel = PrepareAttachmentDetails();
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(viewName:"AttachmentDetails", model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"Attachment/New")]
        public ActionResult NewAttachmentDetails(AttachmentViewModel model, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int id;

                    if (upload != null && upload.ContentLength > 0)
                    {
                        using (var reader = new BinaryReader(input:upload.InputStream))
                        {
                            var content = reader.ReadBytes(count:upload.ContentLength);

                            var fileStoreDto = new FileStoreDto
                                               {
                                                   FileStoreId = model.Id,
                                                   OriginalFileName = upload.FileName,
                                                   ReportElementTypeId = model.ReportElementTypeId,
                                                   ReportElementTypeName = _reportElementService.GetReportElementTypes(categoryName:ReportElementCategoryName.Attachments)
                                                                                                .Where(c => c.ReportElementTypeId == model.ReportElementTypeId)
                                                                                                .Select(c => c.Name)
                                                                                                .FirstOrDefault(),
                                                   Description = model.Description,
                                                   Data = content,
                                                   MediaType = upload.ContentType
                                               };
                            id = _fileStoreService.CreateFileStore(fileStoreDto:fileStoreDto);
                        }
                    }
                    else
                    {
                        var validationIssues = new List<RuleViolation>();
                        var message = "No file was selected.";
                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                        throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                    }

                    TempData[key:"ShowSuccessMessage"] = true;
                    TempData[key:"SuccessMessage"] = $"Attachment {(model.Id.HasValue ? "updated" : "created")} successfully!";

                    ModelState.Clear();
                    return RedirectToAction(actionName:"AttachmentDetails", controllerName:"Industry", routeValues:new {id});
                }
                catch (RuleViolationException rve)
                {
                    MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                }
            }

            var viewModel = PrepareAttachmentDetails(id:model.Id);

            return View(viewName:"AttachmentDetails", model:viewModel);
        }

        [Route(template:"Attachment/{id:int}/Details")]
        public ActionResult AttachmentDetails(int id)
        {
            var viewModel = new AttachmentViewModel();

            try
            {
                viewModel = PrepareAttachmentDetails(id:id);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }
            var previousUri = HttpContext.Request.UrlReferrer;
            ViewBag.fromCreateAttachment = previousUri != null
                                           && previousUri.AbsolutePath.Equals(value:Url.Action(actionName:"NewAttachmentDetails", controllerName:"Industry"),
                                                                              comparisonType:StringComparison.OrdinalIgnoreCase);
            ViewBag.ShowSuccessMessage = TempData[key:"ShowSuccessMessage"] ?? false;
            ViewBag.SuccessMessage = TempData[key:"SuccessMessage"] ?? "";

            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"Attachment/{id:int}/Details")]
        public ActionResult AttachmentDetails(int id, AttachmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var fileStoreDto = _fileStoreService.GetFileStoreById(fileStoreId:id);

                    fileStoreDto.ReportElementTypeId = model.ReportElementTypeId;
                    fileStoreDto.ReportElementTypeName = model.ReportElementTypeName;
                    fileStoreDto.Description = model.Description;

                    _fileStoreService.UpdateFileStore(fileStoreDto:fileStoreDto);

                    TempData[key:"ShowSuccessMessage"] = true;
                    TempData[key:"SuccessMessage"] = $"Attachment {(model.Id.HasValue ? "updated" : "created")} successfully!";

                    ModelState.Clear();
                    return RedirectToAction(actionName:"AttachmentDetails", controllerName:"Industry", routeValues:new {id});
                }
                catch (RuleViolationException rve)
                {
                    MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                }
            }

            var viewModel = PrepareAttachmentDetails(id:model.Id);

            return View(viewName:"AttachmentDetails", model:viewModel);
        }

        [Route(template:"Attachment/{id:int}/Details/Download")]
        public ActionResult DownloadAttachment(int id)
        {
            var fileStore = _fileStoreService.GetFileStoreById(fileStoreId:id, includingFileData:true);
            var fileDownloadName = fileStore.Name;
            var contentType = $"application/${fileStore.MediaType}";
            var fileStream = new MemoryStream(buffer:fileStore.Data) {Position = 0};

            return File(fileStream:fileStream, contentType:contentType, fileDownloadName:fileDownloadName);
        }

        [Route(template:"Attachment/{id:int}/Details/Delete")]
        public ActionResult DeleteAttachment(int id)
        {
            try
            {
                _fileStoreService.DeleteFileStore(fileStoreId:id);
                TempData[key:"AttachmentDeletedSucceed"] = true;
                return RedirectToAction(actionName:"Attachments");
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(viewName:"AttachmentDetails", model:PrepareAttachmentDetails(id:id));
        }

        private AttachmentViewModel PrepareAttachmentDetails(int? id = null)
        {
            var viewModel = new AttachmentViewModel();

            if (id.HasValue)
            {
                ViewBag.Satus = "Edit";

                var vm = _fileStoreService.GetFileStoreById(fileStoreId:id.Value);
                viewModel = new AttachmentViewModel
                            {
                                Id = vm.FileStoreId,
                                Name = vm.Name,
                                OriginalFileName = vm.OriginalFileName,
                                Description = vm.Description,
                                MediaType = vm.MediaType,
                                ReportElementTypeId = vm.ReportElementTypeId,
                                ReportElementTypeName = vm.ReportElementTypeName,
                                UploadDateTimeLocal = vm.UploadDateTimeLocal,
                                UploaderUserFullName = vm.UploaderUserFullName,
                                UsedByReports = vm.UsedByReports
                            };
            }
            else
            {
                ViewBag.Satus = "New";
                ViewBag.MaxFileSize = _fileStoreService.GetMaxFileSize();
                viewModel.UsedByReports = false;
                viewModel.ReportElementTypeId = 0;
            }

            // ReportElementTypes
            viewModel.AvailableReportElementTypes = new List<SelectListItem>();
            viewModel.AvailableReportElementTypes = _reportElementService.GetReportElementTypes(categoryName:ReportElementCategoryName.Attachments)
                                                                         .Select(c => new SelectListItem
                                                                                      {
                                                                                          Text = c.Name,
                                                                                          Value = c.ReportElementTypeId.ToString(),
                                                                                          Selected = c.ReportElementTypeId.Equals(other:viewModel.ReportElementTypeId)
                                                                                                     && c.Name.Equals(value:viewModel.ReportElementTypeName)
                                                                                      }).ToList();

            if (viewModel.Id.HasValue && !viewModel.AvailableReportElementTypes.Any(c => c.Selected))
            {
                // If previously selected one is not in the list then add that
                viewModel.AvailableReportElementTypes.Add(item:new SelectListItem
                                                               {
                                                                   Text = viewModel.ReportElementTypeName,
                                                                   Value = viewModel.ReportElementTypeId.ToString(),
                                                                   Selected = true
                                                               });
            }

            viewModel.AvailableReportElementTypes.Insert(index:0, item:new SelectListItem {Text = @"Select Attachment Type", Value = "0", Disabled = true});

            viewModel.AllowedFileExtensions = string.Join(separator:",", values:_fileStoreService.GetValidAttachmentFileExtensions());

            return viewModel;
        }

        #endregion

        #region sample list

        // GET: /Industry/Samples
        [Route(template:"Samples/{sampleStatus}")]
        public ActionResult Samples(SampleStatusName sampleStatus)
        {
            ViewBag.SampleStatusName = sampleStatus;
            return View();
        }

        public ActionResult Samples_Read([CustomDataSourceRequest] DataSourceRequest request, SampleStatusName sampleStatus)
        {
            var dtos = _sampleService.GetSamples(status:sampleStatus);

            var viewModels = dtos.Select(vm => new SampleViewModel
                                               {
                                                   Id = vm.SampleId,
                                                   MonitoringPointName = vm.MonitoringPointName,
                                                   CtsEventTypeName = vm.CtsEventTypeName,
                                                   CollectionMethodName = vm.CollectionMethodName,
                                                   StartDateTimeLocal = vm.StartDateTimeLocal,
                                                   EndDateTimeLocal = vm.EndDateTimeLocal,
                                                   LastModificationDateTimeLocal = vm.LastModificationDateTimeLocal,
                                                   LastModifierUserName = vm.LastModifierFullName
                                               });

            var result = viewModels.ToDataSourceResult(request:request, selector:vm => new
                                                                                       {
                                                                                           vm.Id,
                                                                                           vm.MonitoringPointName,
                                                                                           vm.CtsEventTypeName,
                                                                                           vm.CollectionMethodName,
                                                                                           StartDateTimeLocal =
                                                                                           vm.StartDateTimeLocal.ToString(provider:CultureInfo.CurrentCulture),
                                                                                           EndDateTimeLocal = vm.EndDateTimeLocal.ToString(provider:CultureInfo.CurrentCulture),
                                                                                           LastModificationDateTimeLocal =
                                                                                           vm.LastModificationDateTimeLocal.ToString(provider:CultureInfo.CurrentCulture),
                                                                                           vm.LastModifierUserName
                                                                                       });

            return Json(data:result);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult Samples_Select(IEnumerable<SampleViewModel> items)
        {
            try
            {
                if (items != null)
                {
                    var item = items.First();
                    return Json(data:new
                                     {
                                         redirect = true,
                                         newurl = Url.Action(actionName:"SampleDetails", controllerName:"Industry", routeValues:new {id = item.Id})
                                     });
                }

                return Json(data:new
                                 {
                                     redirect = false,
                                     message = "Please select a Sample."
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

        #region sample details

        [Route(template:"Sample/New/Step1")]
        public ActionResult NewSampleDetailsStep1()
        {
            var monitoringPoints = _monitoringPointService.GetMonitoringPoints().Select(vm => new MonitoringPointViewModel
                                                                                              {
                                                                                                  Id = vm.MonitoringPointId,
                                                                                                  Name = vm.Name,
                                                                                                  Description = vm.Description
                                                                                              }).ToList();

            var viewModel = new NewSampleStep1ViewModel {AllMonitoringPoints = monitoringPoints};

            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"Sample/New/Step1")]
        public ActionResult NewSampleDetailsStep1(NewSampleStep1ViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.EndDateTimeLocal < model.StartDateTimeLocal)
                    {
                        var validationIssues = new List<RuleViolation>();
                        var message = "End Date must be on or after Start date.";
                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                        throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                    }

                    TempData[key:"NewSampleStep1ViewModel"] = model;
                    return RedirectToAction(actionName:"NewSampleDetailsStep2");
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

            var monitoringPoints = _monitoringPointService.GetMonitoringPoints().Select(vm => new MonitoringPointViewModel
                                                                                              {
                                                                                                  Id = vm.MonitoringPointId,
                                                                                                  Name = vm.Name,
                                                                                                  Description = vm.Description
                                                                                              }).ToList();
            model.AllMonitoringPoints = monitoringPoints;
            return View(model:model);
        }

        [Route(template:"Sample/New/Step2")]
        public ActionResult NewSampleDetailsStep2()
        {
            if (TempData[key:"NewSampleStep1ViewModel"] == null)
            {
                return RedirectToAction(actionName:"NewSampleDetailsStep1");
            }
            else
            {
                var newSampleStep1ViewModel = TempData[key:"NewSampleStep1ViewModel"] as NewSampleStep1ViewModel;
                TempData.Keep(key:"NewSampleStep1ViewModel");

                if (newSampleStep1ViewModel == null)
                {
                    return RedirectToAction(actionName:"NewSampleDetailsStep1");
                }
                else
                {
                    ViewBag.Satus = "New";
                    var monitoringPoint = _monitoringPointService.GetMonitoringPoint(monitoringPointId:newSampleStep1ViewModel.SelectedMonitoringPointId);
                    var currentOrganizationRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                    var programSettings =
                        _settingService.GetProgramSettingsById(orgRegProgramId:_settingService
                                                                   .GetAuthority(orgRegProgramId:currentOrganizationRegulatoryProgramId).OrganizationRegulatoryProgramId);
                    var viewModelMassLoadingMassLoadingUnitId = _unitService.GetUnitForMassLoadingCalculations();

                    Debug.Assert(condition:newSampleStep1ViewModel.StartDateTimeLocal != null, message:"newSampleStep1ViewModel.StartDateTimeLocal != null");
                    Debug.Assert(condition:newSampleStep1ViewModel.EndDateTimeLocal != null, message:"newSampleStep1ViewModel.EndDateTimeLocal != null");

                    var viewModel = new SampleViewModel
                                    {
                                        MonitoringPointId = monitoringPoint.MonitoringPointId,
                                        MonitoringPointName = monitoringPoint.Name,
                                        StartDateTimeLocal = newSampleStep1ViewModel.StartDateTimeLocal.Value,
                                        EndDateTimeLocal = newSampleStep1ViewModel.EndDateTimeLocal.Value,
                                        MassLoadingMassLoadingUnitId = viewModelMassLoadingMassLoadingUnitId.UnitId,
                                        MassLoadingMassLoadingUnitName = viewModelMassLoadingMassLoadingUnitId.Name,
                                        SampleStatusName = SampleStatusName.Draft,
                                        AvailableCollectionMethods = new List<SelectListItem>(),
                                        AvailableCtsEventTypes = new List<SelectListItem>(),
                                        SampleResults = new List<SampleResultViewModel>(),
                                        ParameterGroups = new List<ParameterGroupViewModel>(),
                                        AllParameters = new List<ParameterViewModel>()
                                    };

                    if (newSampleStep1ViewModel.IsClonedSample)
                    {
                        //
                        // Cloning a sample, set copied parameters here
                        //

                        if (newSampleStep1ViewModel.SampleResults != null)
                        {
                            //Need to scrub sample results for any potential existing id's from copied sample
                            foreach (var sampleResult in newSampleStep1ViewModel.SampleResults)
                            {
                                sampleResult.Id = null;
                                sampleResult.MassLoadingSampleResultId = null;
                            }

                            viewModel.SampleResults = newSampleStep1ViewModel.SampleResults;
                        }

                        viewModel.CollectionMethodId = newSampleStep1ViewModel.CollectionMethodId;
                        viewModel.LabSampleIdentifier = newSampleStep1ViewModel.LabSampleIdentifier;
                        viewModel.CtsEventTypeId = newSampleStep1ViewModel.CtsEventTypeId;
                        viewModel.FlowValue = newSampleStep1ViewModel.FlowValue;
                        viewModel.FlowUnitId = newSampleStep1ViewModel.FlowUnitId;
                        viewModel.FlowUnitValidValues = newSampleStep1ViewModel.FlowUnitValidValues;
                    }
                    else
                    {
                        viewModel.FlowUnitValidValues = _unitService.GetFlowUnitValidValues();
                    }

                    viewModel.ResultQualifierValidValues = programSettings
                        .Settings.Where(s => s.TemplateName.Equals(obj:SettingType.ResultQualifierValidValues)).Select(s => s.Value).First();
                    viewModel.MassLoadingConversionFactorPounds =
                        double.Parse(s:programSettings.Settings.Where(s => s.TemplateName.Equals(obj:SettingType.MassLoadingConversionFactorPounds))
                                                      .Select(s => s.Value)
                                                      .First()
                                                      .IfNullOrWhiteSpace(defaultValue:"0.0"));
                    viewModel.MassLoadingCalculationDecimalPlaces =
                        int.Parse(s:programSettings.Settings.Where(s => s.TemplateName.Equals(obj:SettingType.MassLoadingCalculationDecimalPlaces))
                                                   .Select(s => s.Value)
                                                   .First()
                                                   .IfNullOrWhiteSpace(defaultValue:"0"));
                    viewModel.IsMassLoadingResultToUseLessThanSign =
                        bool.Parse(value:programSettings.Settings.Where(s => s.TemplateName.Equals(obj:SettingType.MassLoadingResultToUseLessThanSign))
                                                        .Select(s => s.Value)
                                                        .First()
                                                        .IfNullOrWhiteSpace(defaultValue:"true"));

                    AddAdditionalPropertyToSampleDetails(viewModel:viewModel);

                    ViewBag.ShowSuccessMessage = TempData[key:"ShowSuccessMessage"] ?? false;
                    ViewBag.SuccessMessage = TempData[key:"SuccessMessage"] ?? "";

                    return View(viewName:"SampleDetails", model:viewModel);
                }
            }
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"Sample/New/Step2")]
        public ActionResult NewSampleDetailsStep2(SampleViewModel model, FormCollection collection)
        {
            try
            {
                var objJavascript = new JavaScriptSerializer();

                model.FlowUnitValidValues = objJavascript.Deserialize<IEnumerable<UnitDto>>(input:collection[name:"FlowUnitValidValues"]);
                model.SampleResults = objJavascript.Deserialize<IEnumerable<SampleResultViewModel>>(input:HttpUtility.HtmlDecode(s:collection[name:"SampleResults"]));

                // don't check ModelState.IsValid as SampleResults is always invalid and it is re-populated later from the FormCollection[name: "SampleResults"]
                if (!ModelState.IsValidField(key:"FlowUnitId") || !ModelState.IsValidField(key:"StartDateTimeLocal") || !ModelState.IsValidField(key:"EndDateTimeLocal"))
                {
                    if (ModelState[key:"."] != null)
                    {
                        foreach (var issue in ModelState[key:"."].Errors)
                        {
                            ModelState.AddModelError(key:string.Empty, errorMessage:issue.ErrorMessage);
                        }
                    }
                }
                else
                {
                    var vm = ConvertSampleViewModelToDto(model:model);

                    var id = _sampleService.SaveSample(sample:vm);

                    TempData[key:"ShowSuccessMessage"] = true;
                    TempData[key:"SuccessMessage"] = "Sample updated successfully!";

                    ModelState.Clear();

                    return RedirectToAction(actionName:"SampleDetails", controllerName:"Industry", routeValues:new {id});
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            ViewBag.Satus = "New";
            model.FlowUnitValidValues = _unitService.GetFlowUnitValidValues();
            AddAdditionalPropertyToSampleDetails(viewModel:model);
            return View(viewName:"SampleDetails", model:model);
        }

        [Route(template:"Sample/{id:int}/Details")]
        public ActionResult SampleDetails(int id)
        {
            var viewModel = PrepareSampleDetails(id:id);

            ViewBag.ShowSuccessMessage = TempData[key:"ShowSuccessMessage"] ?? false;
            ViewBag.SuccessMessage = TempData[key:"SuccessMessage"] ?? "";

            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"Sample/{id:int}/Details")]
        public ActionResult SampleDetails(int id, SampleViewModel model, FormCollection collection)
        {
            try
            {
                if (model.Id != null)
                {
                    var objJavascript = new JavaScriptSerializer();

                    model.FlowUnitValidValues = objJavascript.Deserialize<IEnumerable<UnitDto>>(input:collection[name:"FlowUnitValidValues"]);
                    model.SampleResults = objJavascript.Deserialize<IEnumerable<SampleResultViewModel>>(input:HttpUtility.HtmlDecode(s:collection[name:"SampleResults"]));

                    // don't check ModelState.IsValid as SampleResults is always invalid and it is re-populated later from the FormCollection[name: "SampleResults"]
                    if (!ModelState.IsValidField(key:"FlowUnitId") || !ModelState.IsValidField(key:"StartDateTimeLocal") || !ModelState.IsValidField(key:"EndDateTimeLocal"))
                    {
                        ViewBag.Satus = "Edit";
                        AddAdditionalPropertyToSampleDetails(viewModel:model);
                        return View(viewName:"SampleDetails", model:model);
                    }

                    var vm = ConvertSampleViewModelToDto(model:model);

                    _sampleService.SaveSample(sample:vm);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);

                ViewBag.Satus = "Edit";
                AddAdditionalPropertyToSampleDetails(viewModel:model);
                return View(viewName:"SampleDetails", model:model);
            }

            TempData[key:"ShowSuccessMessage"] = true;
            TempData[key:"SuccessMessage"] = "Sample updated successfully!";

            ModelState.Clear();
            return RedirectToAction(actionName:"SampleDetails", controllerName:"Industry", routeValues:new {model.Id});
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"Sample/{id:int}/Details/ReadyToReport/{isReadyToReport:bool}")]
        public ActionResult EnableSample(bool isReadyToReport, SampleViewModel model, FormCollection collection)
        {
            try
            {
                if (model.Id != null)
                {
                    var objJavascript = new JavaScriptSerializer();

                    model.FlowUnitValidValues = objJavascript.Deserialize<IEnumerable<UnitDto>>(input:collection[name:"FlowUnitValidValues"]);
                    model.SampleResults = objJavascript.Deserialize<IEnumerable<SampleResultViewModel>>(input:HttpUtility.HtmlDecode(s:collection[name:"SampleResults"]));

                    if (isReadyToReport)
                    {
                        // don't check ModelState.IsValid as SampleResults is always invalid and it is re-populated later from the FormCollection[name: "SampleResults"]
                        if (!ModelState.IsValidField(key:"FlowUnitId") || !ModelState.IsValidField(key:"StartDateTimeLocal") || !ModelState.IsValidField(key:"EndDateTimeLocal"))
                        {
                            ViewBag.Satus = "Edit";
                            AddAdditionalPropertyToSampleDetails(viewModel:model);
                            return View(viewName:"SampleDetails", model:model);
                        }
                    }

                    var vm = ConvertSampleViewModelToDto(model:model); // _sampleService.GetSampleDetails(sampleId:model.Id.Value);
                    vm.IsReadyToReport = isReadyToReport;

                    _sampleService.SaveSample(sample:vm);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);

                ViewBag.Satus = "Edit";
                AddAdditionalPropertyToSampleDetails(viewModel:model);
                return View(viewName:"SampleDetails", model:model);
            }

            TempData[key:"ShowSuccessMessage"] = true;
            TempData[key:"SuccessMessage"] = "Sample updated successfully!";

            ModelState.Clear();
            return RedirectToAction(actionName:"SampleDetails", controllerName:"Industry", routeValues:new {model.Id});
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult CopySample(SampleViewModel model, FormCollection collection)
        {
            var objJavascript = new JavaScriptSerializer();

            model.FlowUnitValidValues = objJavascript.Deserialize<IEnumerable<UnitDto>>(input:collection[name:"FlowUnitValidValues"]);
            model.SampleResults = objJavascript.Deserialize<IEnumerable<SampleResultViewModel>>(input:HttpUtility.HtmlDecode(s:collection[name:"SampleResults"]));

            // don't check ModelState.IsValid as SampleResults is always invalid and it is re-populated later from the FormCollection[name: "SampleResults"]
            if (!ModelState.IsValidField(key:"FlowUnitId") || !ModelState.IsValidField(key:"StartDateTimeLocal") || !ModelState.IsValidField(key:"EndDateTimeLocal"))
            {
                ViewBag.Satus = "Edit";
                AddAdditionalPropertyToSampleDetails(viewModel:model);
                return View(viewName:"SampleDetails", model:model);
            }

            var viewModel = new NewSampleStep1ViewModel
                            {
                                IsClonedSample = true,
                                StartDateTimeLocal = model.StartDateTimeLocal,
                                EndDateTimeLocal = model.EndDateTimeLocal,
                                SelectedMonitoringPointId = model.MonitoringPointId,
                                FlowUnitValidValues = model.FlowUnitValidValues,
                                SampleResults = model.SampleResults,
                                CollectionMethodId = model.CollectionMethodId,
                                LabSampleIdentifier = model.LabSampleIdentifier,
                                CtsEventTypeId = model.CtsEventTypeId,
                                FlowValue = model.FlowValue,
                                FlowUnitId = model.FlowUnitId
                            };

            TempData[key:"NewSampleStep1ViewModel"] = viewModel;
            TempData[key:"ShowSuccessMessage"] = true;
            TempData[key:"SuccessMessage"] = "Sample was successfully copied and is displayed below. Please update accordingly.";

            return RedirectToAction(actionName:"NewSampleDetailsStep2");
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult GetParameterGroupsForSample(int monitoringPointId,
                                                        DateTime startDateTime, DateTime endDateTime, int collectionMethodId)
        {
            try
            {
                var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                var complianceDeterminationDate =
                    _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.ComplianceDeterminationDate);
                DateTime sampleDateTimeLocal;
                if (complianceDeterminationDate == ComplianceDeterminationDate.StartDateSampled.ToString())
                {
                    sampleDateTimeLocal = startDateTime;
                }
                else
                {
                    sampleDateTimeLocal = endDateTime;
                }

                List<ParameterGroupViewModel> parameterGroups;
                if (monitoringPointId > 0 && collectionMethodId > 0)
                {
                    parameterGroups =
                        _parameterService.GetAllParameterGroups(monitoringPointId:monitoringPointId,
                                                                sampleDateTimeLocal:sampleDateTimeLocal,
                                                                collectionMethodId:collectionMethodId)
                                         .Select(c => new ParameterGroupViewModel
                                                      {
                                                          Id = c.ParameterGroupId,
                                                          Name = c.Name,
                                                          Description = c.Description,
                                                          ParameterIds = string.Join(separator:",", values:c.Parameters.Select(p => p.ParameterId).ToList())
                                                      }).OrderBy(c => c.Name).ToList();
                }
                else
                {
                    parameterGroups = new List<ParameterGroupViewModel>();
                }

                var allParameters = _parameterService.GetGlobalParameters(monitoringPointId:monitoringPointId, sampleDateTimeLocal:sampleDateTimeLocal)
                                                     .Select(c => new ParameterViewModel
                                                                  {
                                                                      Id = c.ParameterId,
                                                                      Name = c.Name,
                                                                      DefaultUnitId = c.DefaultUnit.UnitId,
                                                                      DefaultUnitName = c.DefaultUnit.Name,
                                                                      IsCalcMassLoading = c.IsCalcMassLoading,
                                                                      ConcentrationMaxValue = c.ConcentrationMaxValue,
                                                                      ConcentrationMinValue = c.ConcentrationMinValue,
                                                                      MassLoadingMaxValue = c.MassLoadingMaxValue,
                                                                      MassLoadingMinValue = c.MassLoadingMinValue
                                                                  }).OrderBy(c => c.Name).ToList();
                return Json(data:new
                                 {
                                     hasError = false,
                                     parameterGroups,
                                     allParameters
                                 });
            }
            catch (RuleViolationException rve)
            {
                return Json(data:new
                                 {
                                     hasError = true,
                                     message = MvcValidationExtensions.GetViolationMessages(ruleViolationException:rve)
                                 });
            }
        }

        [Route(template:"Sample/{id:int}/Details/Delete")]
        public ActionResult DeleteSample(int id)
        {
            try
            {
                var sampleDto = _sampleService.DeleteSample(sampleId:id);
                TempData[key:"SampleDeleteSucceed"] = true;
                return RedirectToAction(actionName:"Samples", routeValues:new {sampleStatus = sampleDto.SampleStatusName});
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
                return View(viewName:"SampleDetails", model:PrepareSampleDetails(id:id));
            }
        }


		// GET: /Industry/PermitLimits
	    [Route(template:"PermitLimits")]
	    public FileResult PermitLimits()
	    { 
		    var data = _parameterService.GetIndustryDischargeLimitReport();
		    const string contentType = "application/pdf";
		    var fileDownloadName = "Discharge Permit Limits.pdf"; 

			return File(data, contentType, fileDownloadName);
	    }

	    private SampleDto ConvertSampleViewModelToDto(SampleViewModel model)
        {
            var dto = new SampleDto
                      {
                          SampleId = model.Id,
                          Name = model.Name,
                          MonitoringPointId = model.MonitoringPointId,
                          MonitoringPointName = model.MonitoringPointName,
                          CtsEventTypeId = model.CtsEventTypeId,
                          CtsEventTypeName = model.CtsEventTypeName,
                          CtsEventCategoryName = model.CtsEventCategoryName,
                          CollectionMethodId = model.CollectionMethodId,
                          CollectionMethodName = model.CollectionMethodName,
                          IsReadyToReport = model.IsReadyToReport,
                          SampleStatusName = model.SampleStatusName,
                          LabSampleIdentifier = model.LabSampleIdentifier,
                          FlowUnitValidValues = model.FlowUnitValidValues,
                          ResultQualifierValidValues = model.ResultQualifierValidValues,
                          MassLoadingConversionFactorPounds = model.MassLoadingConversionFactorPounds,
                          MassLoadingCalculationDecimalPlaces = model.MassLoadingCalculationDecimalPlaces,
                          IsMassLoadingResultToUseLessThanSign = model.IsMassLoadingResultToUseLessThanSign,
                          SampleResults = model.SampleResults.Select(c => new SampleResultDto
                                                                          {
                                                                              AnalysisDateTimeLocal = c.AnalysisDateTimeLocal,
                                                                              AnalysisMethod = c.AnalysisMethod,
                                                                              EnteredMethodDetectionLimit = c.EnteredMethodDetectionLimit,
                                                                              ConcentrationSampleResultId = c.Id,
                                                                              IsApprovedEPAMethod = true,
                                                                              IsCalcMassLoading = c.IsCalcMassLoading,
                                                                              MassLoadingSampleResultId = c.MassLoadingSampleResultId,
                                                                              MassLoadingQualifier = c.MassLoadingQualifier,
                                                                              MassLoadingUnitId = c.MassLoadingUnitId,
                                                                              MassLoadingUnitName = c.MassLoadingUnitName,
                                                                              MassLoadingValue =
                                                                                  string.IsNullOrWhiteSpace(value:c.MassLoadingValue) ? string.Empty : c.MassLoadingValue,
                                                                              ParameterId = c.ParameterId,
                                                                              ParameterName = c.ParameterName,
                                                                              Qualifier = c.Qualifier,
                                                                              UnitId = c.UnitId,
                                                                              EnteredValue = string.IsNullOrWhiteSpace(value:c.Value) ? string.Empty : c.Value,
                                                                              UnitName = c.UnitName
                                                                          }),
                          FlowUnitId = model.FlowUnitId,
                          FlowUnitName = model.FlowUnitName,
                          FlowEnteredValue = string.IsNullOrWhiteSpace(value:model.FlowValue) ? string.Empty : model.FlowValue,
                          StartDateTimeLocal = model.StartDateTimeLocal,
                          EndDateTimeLocal = model.EndDateTimeLocal

                          //LastModificationDateTimeLocal = 
                          //StartDateTime = 
                          //ByOrganizationTypeName = 
                          //EndDateTime = 
                          //LastModifierFullName = 
                      };

            return dto;
        }

        private SampleViewModel PrepareSampleDetails(int id)
        {
            var viewModel = new SampleViewModel();

            try
            {
                ViewBag.Satus = "Edit";

                var vm = _sampleService.GetSampleDetails(sampleId:id);
                var viewModelMassLoadingMassLoadingUnitId = _unitService.GetUnitForMassLoadingCalculations();

                viewModel = new SampleViewModel
                            {
                                AvailableCollectionMethods = new List<SelectListItem>(),
                                AvailableCtsEventTypes = new List<SelectListItem>(),
                                CollectionMethodId = vm.CollectionMethodId,
                                CollectionMethodName = vm.CollectionMethodName,
                                CtsEventTypeId = vm.CtsEventTypeId,
                                CtsEventTypeName = vm.CtsEventTypeName,
                                EndDateTimeLocal = vm.EndDateTimeLocal,
                                FlowUnitId = vm.FlowUnitId,
                                FlowUnitName = vm.FlowUnitName,
                                FlowUnitValidValues = vm.FlowUnitValidValues,
                                FlowValue = vm.FlowEnteredValue,
                                Id = vm.SampleId,
                                IsMassLoadingResultToUseLessThanSign = vm.IsMassLoadingResultToUseLessThanSign,
                                IsReadyToReport = vm.IsReadyToReport,
                                LabSampleIdentifier = vm.LabSampleIdentifier,
                                LastModificationDateTimeLocal = vm.LastModificationDateTimeLocal,
                                LastModifierUserName = vm.LastModifierFullName,
                                MassLoadingCalculationDecimalPlaces = vm.MassLoadingCalculationDecimalPlaces ?? 0,
                                MassLoadingConversionFactorPounds = vm.MassLoadingConversionFactorPounds ?? 0.0,
                                MassLoadingMassLoadingUnitId = viewModelMassLoadingMassLoadingUnitId.UnitId,
                                MassLoadingMassLoadingUnitName = viewModelMassLoadingMassLoadingUnitId.Name,
                                MonitoringPointId = vm.MonitoringPointId,
                                MonitoringPointName = vm.MonitoringPointName,
                                Name = vm.Name,
                                ParameterGroups = new List<ParameterGroupViewModel>(),
                                ResultQualifierValidValues = vm.ResultQualifierValidValues,
                                SampleResults = new List<SampleResultViewModel>(),
                                SampleStatusName = vm.SampleStatusName,
                                StartDateTimeLocal = vm.StartDateTimeLocal,
                                AllParameters = new List<ParameterViewModel>()
                            };

                viewModel.SampleResults = vm.SampleResults.Select(c => new SampleResultViewModel
                                                                       {
                                                                           AnalysisDateTimeLocal = c.AnalysisDateTimeLocal,
                                                                           AnalysisMethod = c.AnalysisMethod,
                                                                           EnteredMethodDetectionLimit = c.EnteredMethodDetectionLimit,
                                                                           Id = c.ConcentrationSampleResultId,
                                                                           IsCalcMassLoading = c.IsCalcMassLoading,
                                                                           MassLoadingSampleResultId = c.MassLoadingSampleResultId,
                                                                           MassLoadingQualifier = c.MassLoadingQualifier,
                                                                           MassLoadingUnitId = c.MassLoadingUnitId,
                                                                           MassLoadingUnitName = c.MassLoadingUnitName,
                                                                           MassLoadingValue = c.MassLoadingValue,
                                                                           ParameterId = c.ParameterId,
                                                                           ParameterName = c.ParameterName,
                                                                           Qualifier = c.Qualifier,
                                                                           UnitId = c.UnitId,
                                                                           Value = c.EnteredValue,
                                                                           UnitName = c.UnitName
                                                                       });

                AddAdditionalPropertyToSampleDetails(viewModel:viewModel);
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }
            return viewModel;
        }

        private void AddAdditionalPropertyToSampleDetails(SampleViewModel viewModel)
        {
            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var complianceDeterminationDate =
                _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.ComplianceDeterminationDate);
            DateTime sampleDateTimeLocal;
            if (complianceDeterminationDate == ComplianceDeterminationDate.StartDateSampled.ToString())
            {
                sampleDateTimeLocal = viewModel.StartDateTimeLocal;
            }
            else
            {
                sampleDateTimeLocal = viewModel.EndDateTimeLocal;
            }

            if (viewModel.CollectionMethodId > 0)
            {
                viewModel.ParameterGroups =
                    _parameterService.GetAllParameterGroups(monitoringPointId:viewModel.MonitoringPointId,
                                                            sampleDateTimeLocal:sampleDateTimeLocal,
                                                            collectionMethodId:viewModel.CollectionMethodId)
                                     .Select(c => new ParameterGroupViewModel
                                                  {
                                                      Id = c.ParameterGroupId,
                                                      Name = c.Name,
                                                      Description = c.Description,
                                                      ParameterIds = string.Join(separator:",", values:c.Parameters.Select(p => p.ParameterId).ToList())
                                                  }).OrderBy(c => c.Name).ToList();
            }
            else
            {
                viewModel.ParameterGroups = new List<ParameterGroupViewModel>();
            }

            viewModel.AvailableCollectionMethods = _sampleService.GetCollectionMethods().Select(c => new SelectListItem
                                                                                                     {
                                                                                                         Text = c.Name,
                                                                                                         Value = c.CollectionMethodId.ToString(),
                                                                                                         Selected = c.CollectionMethodId.Equals(obj:viewModel.CollectionMethodId)
                                                                                                                    && c.Name.Equals(obj:viewModel.CollectionMethodName)
                                                                                                     }).OrderBy(c => c.Text).ToList();

            if (viewModel.Id.HasValue && !viewModel.AvailableCollectionMethods.Any(c => c.Selected))
            {
                // If previously selected one is not in the list then add that
                viewModel.AvailableCollectionMethods.Add(item:new SelectListItem
                                                              {
                                                                  Text = viewModel.CollectionMethodName,
                                                                  Value = viewModel.CollectionMethodId.ToString(),
                                                                  Selected = true
                                                              });
            }

            viewModel.AvailableCollectionMethods.Insert(index:0, item:new SelectListItem {Text = @"Select Collection Method", Value = "0", Disabled = true});

            var ctsEventTypeDtos = _reportTemplateService.GetCtsEventTypes(isForSample:true);
            var sampleTypes = ctsEventTypeDtos as IList<CtsEventTypeDto> ?? ctsEventTypeDtos.ToList();
            viewModel.CtsEventCategoryName =
                sampleTypes.Count > 0 ? sampleTypes.First().CtsEventCategoryName : "sample"; //if there is no active sampleTypes then hard code the CtsEventCategoryName as sample
            viewModel.AvailableCtsEventTypes = sampleTypes.Select(c => new SelectListItem
                                                                       {
                                                                           Text = c.Name,
                                                                           Value = c.CtsEventTypeId.ToString(),
                                                                           Selected = c.CtsEventTypeId.Equals(obj:viewModel.CtsEventTypeId)
                                                                                      && c.Name.Equals(obj:viewModel.CtsEventTypeName)
                                                                       }).OrderBy(c => c.Text).ToList();

            if (viewModel.Id.HasValue && !viewModel.AvailableCtsEventTypes.Any(c => c.Selected))
            {
                // If previously selected one is not in the list then add that
                viewModel.AvailableCtsEventTypes.Add(item:new SelectListItem
                                                          {
                                                              Text = viewModel.CtsEventTypeName,
                                                              Value = viewModel.CtsEventTypeId.ToString(),
                                                              Selected = true
                                                          });
            }

            viewModel.AvailableCtsEventTypes.Insert(index:0, item:new SelectListItem {Text = @"Select Sample Type", Value = "0", Disabled = true});

            viewModel.AllParameters = _parameterService.GetGlobalParameters(monitoringPointId:viewModel.MonitoringPointId, sampleDateTimeLocal:sampleDateTimeLocal)
                                                       .Select(c => new ParameterViewModel
                                                                    {
                                                                        Id = c.ParameterId,
                                                                        Name = c.Name,
                                                                        DefaultUnitId = c.DefaultUnit.UnitId,
                                                                        DefaultUnitName = c.DefaultUnit.Name,
                                                                        IsCalcMassLoading = c.IsCalcMassLoading,
                                                                        ConcentrationMaxValue = c.ConcentrationMaxValue,
                                                                        ConcentrationMinValue = c.ConcentrationMinValue,
                                                                        MassLoadingMaxValue = c.MassLoadingMaxValue,
                                                                        MassLoadingMinValue = c.MassLoadingMinValue
                                                                    }).OrderBy(c => c.Name).ToList();
        }

        #endregion

        #region DataSources List View

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
            var dataSources = _dataSourceService.GetDataSources(organziationRegulatoryProgramId:organizationRegulatoryProgramId);

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

        #region DataSource Details View
        [Route(template: "DataSource/New")]
        public ActionResult NewDataSourceDetails()
        {
            var viewModel = new DataSourceViewModel();
            return View(viewName: "DataSourceDetails", model: viewModel);
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
                TempData[key: "DeleteDataSourceSucceed"] = true;
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

        #endregion
    }
}