using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.FileStore;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Permission;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.Mvc;
using Linko.LinkoExchange.Web.ViewModels.Industry;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using NLog;

namespace Linko.LinkoExchange.Web.Controllers
{
    [RoutePrefix(prefix:"Industry")]
    public class IndustryController:Controller
    {
        #region default action

        // GET: Industry
        public ActionResult Index()
        {
            return RedirectToAction(actionName:"PendingUserApprovals", controllerName:"Industry");
        }

        #endregion

        #region constructor

        private readonly IOrganizationService _organizationService;
        private readonly IUserService _userService;
        private readonly IInvitationService _invitationService;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IPermissionService _permissionService;
        private readonly ISessionCache _sessionCache;
        private readonly ILogger _logger;
        private readonly IHttpContextService _httpContextService;
        private readonly IFileStoreService _fileStoreService;
        private readonly IReportElementService _reportElementService;

        public IndustryController(IOrganizationService organizationService, IUserService userService, IInvitationService invitationService,
                                  IQuestionAnswerService questionAnswerService, IPermissionService permissionService, ISessionCache sessionCache, ILogger logger, IHttpContextService httpContextService
                                  , IFileStoreService fileStoreService, IReportElementService reportElementService)
        {
            _organizationService = organizationService;
            _userService = userService;
            _invitationService = invitationService;
            _questionAnswerService = questionAnswerService;
            _permissionService = permissionService;
            _sessionCache = sessionCache;
            _logger = logger;
            _httpContextService = httpContextService;
            _fileStoreService = fileStoreService;
            _reportElementService = reportElementService;
        }

        #endregion

        #region Show Industry Details

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

        #region Show Industry Users

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

        public ActionResult IndustryUsers_Read([DataSourceRequest] DataSourceRequest request)
        {
            var organizationRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var users = _userService.GetUserProfilesForOrgRegProgram(orgRegProgramId:organizationRegulatoryProgramId, isRegApproved:true, isRegDenied:false, isEnabled:null, isRemoved:false);

            var viewModels = users.Select(vm => new IndustryUserViewModel
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

        // GET: /Industry/User/{id}/Details
        [Route(template:"User/{id:int}/Details")]
        [AuthorizeIndustryAdminsOnly]
        public ActionResult IndustryUserDetails(int id)
        {
            var viewModel = PrepareIndustryUserDetails(id:id);

            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"User/{id:int}/Details")]
        [AuthorizeIndustryAdminsOnly]
        public ActionResult IndustryUserDetails(int id, IndustryUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model:model);
            }
            try
            {
                _userService.UpdateUserPermissionGroupId(orgRegProgUserId:model.Id, permissionGroupId:model.Role);
                _userService.UpdateUserSignatoryStatus(orgRegProgUserId:model.Id, isSignatory:model.IsSignatory);
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
        [AuthorizeIndustryAdminsOnly]
        public ActionResult IndustryUserRemove(int id, IndustryUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(viewName:"IndustryUserDetails", model:model);
            }
            try
            {
                var result = _userService.RemoveUser(orgRegProgUserId:model.Id);

                if (result)
                {
                    return RedirectToAction(actionName:"IndustryUserRemoved", controllerName:"Industry");
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
            }

            return View(viewName:"IndustryUserDetails", model:model);
        }

        // user remove successfully
        // GET: /Industry/IndustryUserRemoved
        public ActionResult IndustryUserRemoved()
        {
            var model = new ConfirmationViewModel();
            model.Title = "User Remove Status";
            model.Message = "User Removed!";

            return View(viewName:"Confirmation", model:model);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"User/{id:int}/Details/ChangeStatus")]
        [AuthorizeIndustryAdminsOnly]
        public ActionResult IndustryUserChangeStatus(int id, IndustryUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(viewName:"IndustryUserDetails", model:model);
            }
            try
            {
                _userService.EnableDisableUserAccount(orgRegProgramUserId:model.Id, isAttemptingDisable:model.Status);

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

        private IndustryUserViewModel PrepareIndustryUserDetails(int id)
        {
            var user = _userService.GetOrganizationRegulatoryProgramUser(orgRegProgUserId:id);
            var userQuesAns = _questionAnswerService.GetUsersQuestionAnswers(userProfileId:user.UserProfileId, questionType:QuestionTypeName.SQ);
            var currentUserRole = _httpContextService.GetClaimValue(claimType:CacheKey.UserRole) ?? "";
            var currentUserProfileId = _httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId);

            ViewBag.HasPermissionForUpdate = currentUserRole.IsCaseInsensitiveEqual(comparing:UserRole.Administrator.ToString()) &&
                                             !currentUserProfileId.IsCaseInsensitiveEqual(comparing:user.UserProfileId.ToString());

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

        #region Show Pending User Approvals

        // GET: /Industry/PendingUserApprovals
        [Route(template:"PendingUserApprovals")]
        public ActionResult PendingUserApprovals()
        {
            return View();
        }

        public ActionResult PendingUserApprovals_Read([DataSourceRequest] DataSourceRequest request)
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

        #region Show Pending User Approval Details

        // GET: /Industry/PendingUserApprovals
        [Route(template:"PendingUserApprovals/{id:int}/Details")]
        [AuthorizeIndustryAdminsOnly]
        public ActionResult PendingUserApprovalDetails(int id)
        {
            var viewModel = PreparePendingUserApprovalDetails(id:id);
            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route(template:"PendingUserApprovals/{id:int}/Details/PendingUserApprove")]
        [AuthorizeIndustryAdminsOnly]
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
                            _logger.Info(message:string.Format(format:"PendingUserApprove. User={0} - id={1} Registration Approved!", arg0:model.UserName, arg1:model.Id));
                            break;
                        case RegistrationResult.NoMoreUserLicensesForIndustry:
                            _logger.Info(message:string.Format(format:"PendingUserApprove. User={0} - id={1} No more user licenses", arg0:model.UserName, arg1:model.Id));
                            ModelState.AddModelError(key:"", errorMessage:"No more User Licenses are available for this Industry. Disable another User and try again");
                            break;
                        default:
                            _logger.Info(message:string.Format(format:"PendingUserApprove. User={0} - id={1} Registration Approval Failed!", arg0:model.UserName, arg1:model.Id));
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
        [AuthorizeIndustryAdminsOnly]
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
                            _logger.Info(message:string.Format(format:"PendingUserDeny. User={0} - id={1} Registration Denied!", arg0:model.UserName, arg1:model.Id));
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
            ViewBag.HasPermissionForApproveDeny = currentUserRole.IsCaseInsensitiveEqual(comparing:UserRole.Administrator.ToString()); // TODO: call service when implement

            return viewModel;
        }

        #endregion

        #region Show Attachment List

        // GET: /Industry/Attachments
        public ActionResult Attachments()
        {
            return View();
        }

        public ActionResult Attachments_Read([DataSourceRequest] DataSourceRequest request)
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
                                                                                           vm.UploadDateTimeLocal,
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

        #region Show Attachment Details

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
                    fileStoreDto.ReportElementTypeName = _reportElementService.GetReportElementTypes(categoryName:ReportElementCategoryName.Attachments)
                                                                                                .Where(c => c.ReportElementTypeId == model.ReportElementTypeId)
                                                                                                .Select(c => c.Name)
                                                                                                .FirstOrDefault();
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

        [Route(template:"Attachment/{id:int}/Download")]
        public ActionResult DownloadAttachment(int id)
        {
            var fileStore = _fileStoreService.GetFileStoreById(fileStoreId:id, includingFileData:true);
            var fileDownloadName = fileStore.Name;
            var contentType = $"application/${fileStore.MediaType}";
            var fileStream = new MemoryStream(buffer:fileStore.Data) {Position = 0};

            return File(fileStream:fileStream, contentType:contentType, fileDownloadName:fileDownloadName);
        }

        public ActionResult DeleteAttachment(int id)
        {
            try
            {
                _fileStoreService.DeleteFileStore(fileStoreId:id);

                return View(viewName:"Confirmation", model:new ConfirmationViewModel
                                                           {
                                                               Title = "Delete Confirmation",
                                                               Message = "Attachment deleted successfully."
                                                           });
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
                                                                                          Selected = c.ReportElementTypeId.Equals( other:viewModel.ReportElementTypeId)
                                                                                      }).ToList();

            viewModel.AvailableReportElementTypes.Insert(index:0, item:new SelectListItem {Text = @"Select Attachment Type", Value = "0"});

            viewModel.AllowedFileExtensions = string.Join(separator:",", values:_fileStoreService.GetValidAttachmentFileExtensions());

            return viewModel;
        }

        #endregion
    }
}