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
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.ViewModels.Industry;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using NLog;

namespace Linko.LinkoExchange.Web.Controllers
{
    [RoutePrefix("Industry")]
    public class IndustryController : Controller
    {
        #region constructor
        
        private readonly IOrganizationService _organizationService;
        private readonly IUserService _userService;
        private readonly IInvitationService _invitationService;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IPermissionService _permissionService;
        private readonly ISessionCache _sessionCache;
        private readonly ILogger _logger;

        public IndustryController(IOrganizationService organizationService, IUserService userService, IInvitationService invitationService,
            IQuestionAnswerService questionAnswerService, IPermissionService permissionService, ISessionCache sessionCache, ILogger logger)
        {
            _organizationService = organizationService;
            _userService = userService;
            _invitationService = invitationService;
            _questionAnswerService = questionAnswerService;
            _permissionService = permissionService;
            _sessionCache = sessionCache;
            _logger = logger;
        }

        #endregion

        #region default action

        // GET: Industry
        public ActionResult Index()
        {
            return RedirectToAction(actionName: "PendingUserApprovals", controllerName: "Industry");
        }
        #endregion

        #region Show Industry Details

        // GET: /Industry/Settings
        [Route("Settings")]
        public ActionResult Settings()
        {
            int currentOrganizationRegulatoryProgramId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var industry = _organizationService.GetOrganizationRegulatoryProgram(currentOrganizationRegulatoryProgramId);

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

            return View(viewModel);
        }

        // POST: /Industry/Settings
        [AcceptVerbs(HttpVerbs.Post)]
        [Route("Settings")]
        public ActionResult Settings(IndustryViewModel model)
        {
            return View(model);
        }
        #endregion

        #region Show Industry Users

        // GET: /Industry/Users
        [Route("Users")]
        public ActionResult IndustryUsers()
        {
            int id = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var industry = _organizationService.GetOrganizationRegulatoryProgram(id);
            ViewBag.Title = string.Format(format: "{0} Users", arg0: industry.OrganizationDto.OrganizationName);
            ViewBag.CanInvite = _sessionCache.GetClaimValue(CacheKey.UserRole).IsCaseInsensitiveEqual(UserRole.Administrator.ToString());
            return View();
        }

        public ActionResult IndustryUsers_Read([DataSourceRequest] DataSourceRequest request)
        {
            var organizationRegulatoryProgramId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var users = _userService.GetUserProfilesForOrgRegProgram(organizationRegulatoryProgramId, isRegApproved: true, isRegDenied: false, isEnabled: null, isRemoved: false);

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
                        newurl = Url.Action(actionName: "IndustryUserDetails", controllerName: "Industry", routeValues: new
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

        public ActionResult IndustryUsers_PendingInvitations_Read([DataSourceRequest] DataSourceRequest request)
        {
            var organizationRegulatoryProgramId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var invitations = _invitationService.GetInvitationsForOrgRegProgram(organizationRegulatoryProgramId);

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

        // GET: /Industry/User/{id}/Details
        [Route("User/{id:int}/Details")]
        public ActionResult IndustryUserDetails(int id)
        {
            IndustryUserViewModel viewModel = PrepareIndustryUserDetails(id);

            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("User/{id:int}/Details")]
        public ActionResult IndustryUserDetails(int id, IndustryUserViewModel model)
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
        [Route("User/{id:int}/Details/UserRemove")]
        public ActionResult IndustryUserRemove(int id, IndustryUserViewModel model)
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
                    return RedirectToAction(actionName: "IndustryUserRemoved", controllerName: "Industry");
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
                model = PrepareIndustryUserDetails(id);
            }

            return View(viewName: "AuthorityUserDetails", model: model);
        }

        // user remove successfully
        // GET: /Industry/IndustryUserRemoved
        public ActionResult IndustryUserRemoved()
        {
            ConfirmationViewModel model = new ConfirmationViewModel();
            model.Title = "User Remove Status";
            model.Message = "User Removed!";

            return View(viewName: "Confirmation", model: model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("User/{id:int}/Details/ChangeStatus")]
        public ActionResult IndustryUserChangeStatus(int id, IndustryUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                _userService.EnableDisableUserAccount(model.Id, !model.Status);

                ViewBag.ShowSuccessMessage = true;
                ViewBag.SuccessMessage = model.Status ? "User Disabled!" : "User Enabled!";
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

        private IndustryUserViewModel PrepareIndustryUserDetails(int id)
        {
            var user = _userService.GetOrganizationRegulatoryProgramUser(id);
            var userQuesAns = _questionAnswerService.GetUsersQuestionAnswers(user.UserProfileId, QuestionTypeName.SQ);
            var currentUserRole = _sessionCache.GetClaimValue(CacheKey.UserRole) ?? "";
            var currentUserProfileId = _sessionCache.GetClaimValue(CacheKey.UserProfileId);

            ViewBag.HasPermissionForUpdate = currentUserRole.IsCaseInsensitiveEqual(UserRole.Administrator.ToString()) &&
                !currentUserProfileId.IsCaseInsensitiveEqual(user.UserProfileId.ToString());

            ViewBag.HasPermissionForChangeRole = currentUserRole.IsCaseInsensitiveEqual(UserRole.Administrator.ToString());

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

        #region Show Pending User Approvals

        // GET: /Industry/PendingUserApprovals
        [Route("PendingUserApprovals")]
        public ActionResult PendingUserApprovals()
        {
            return View();
        }

        public ActionResult PendingUserApprovals_Read([DataSourceRequest] DataSourceRequest request)
        {
            var organizationRegulatoryProgramId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var users = _userService.GetPendingRegistrationProgramUsers(organizationRegulatoryProgramId);

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
                        newurl = Url.Action(actionName: "PendingUserApprovalDetails", controllerName: "Industry", routeValues: new
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

        // GET: /Industry/PendingUserApprovals
        [Route("PendingUserApprovals/{id:int}/Details")]
        public ActionResult PendingUserApprovalDetails(int id)
        {
            var viewModel = PreparePendingUserApprovalDetails(id);
            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("PendingUserApprovals/{id:int}/Details/PendingUserApprove")]
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
                State = result.UserProfileDto.ZipCode,
                ZipCode = result.UserProfileDto.AddressLine1,
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

            var currentUserRole = _sessionCache.GetClaimValue(CacheKey.UserRole) ?? "";
            ViewBag.HasPermissionForApproveDeny = currentUserRole.IsCaseInsensitiveEqual(UserRole.Administrator.ToString()); // TODO: call service when implement
            
            return viewModel;
        }
        #endregion
    }
}