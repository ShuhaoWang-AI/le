using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.Mvc;
using Linko.LinkoExchange.Web.ViewModels.Authority;
using NLog;

namespace Linko.LinkoExchange.Web.Controllers
{
    [RoutePrefix("Authority")]
    public class AuthorityController : Controller
    {
        #region constructor
        
        private readonly IOrganizationService _organizationService;
        private readonly IUserService _userService;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IInvitationService _invitationService;        
        private readonly ISessionCache _sessionCache;
        private readonly ILogger _logger;

        public AuthorityController(IOrganizationService organizationService, IUserService userService, IInvitationService invitationService,
            IQuestionAnswerService questionAnswerService, ISessionCache sessionCache, ILogger logger)
        {
            _organizationService = organizationService;
            _userService = userService;
            _invitationService = invitationService;
            _questionAnswerService = questionAnswerService;
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
        public ActionResult IndustryDetails(int id, IndustryViewModel model)
        {
            try
            {
                var result = _organizationService.UpdateEnableDisableFlag(model.ID, !model.IsEnabled);
                bool isUpdated = result.IsSuccess;

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



        #region Show Industry Users

        // GET: /Authority/IndustryUsers
        [Route("Industry/{id:int}/Users")]
        public ActionResult IndustryUsers(int id)
        {
            ViewBag.IndustryId = id;
            var industry = _organizationService.GetOrganizationRegulatoryProgram(id);
            ViewBag.Title = industry.OrganizationDto.OrganizationName;
            return View();
        }
        
        public ActionResult IndustryUsers_Read([DataSourceRequest] DataSourceRequest request, string industryId)
        {
            var organizationRegulatoryProgramId = int.Parse(industryId);
            var users = _userService.GetUserProfilesForOrgRegProgram(organizationRegulatoryProgramId, isRegApproved: true, isRegDenied: false, isEnabled: null, isRemoved: false);

            var viewModels = users.Select(vm => new IndustryUserViewModel
            {
                ID = vm.OrganizationRegulatoryProgramUserId,
                IID = vm.OrganizationRegulatoryProgramId,
                PID = vm.UserProfileId,
                FirstName = vm.UserProfileDto.FirstName,
                LastName = vm.UserProfileDto.LastName,
                PhoneNumber = vm.UserProfileDto.PhoneNumber,
                Email = vm.UserProfileDto.Email,
                DateRegistered = vm.RegistrationDateTimeUtc.Value.DateTime,
                Status = vm.IsEnabled,
                AccountLocked = vm.UserProfileDto.IsAccountLocked
            });

            DataSourceResult result = viewModels.ToDataSourceResult(request, vm => new
            {
                ID = vm.ID,
                IID = vm.IID,
                PID = vm.PID,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                PhoneNumber = vm.PhoneNumber,
                Email = vm.Email,
                DateRegistered = vm.DateRegistered,
                StatusText = vm.StatusText,
                AccountLockedText = vm.AccountLockedText
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
                            iid = item.IID,
                            id = item.ID
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
            var organizationRegulatoryProgramId = int.Parse(industryId);
            var invitations = _invitationService.GetInvitationsForOrgRegProgram(organizationRegulatoryProgramId);

            var viewModels = invitations.Select(vm => new IndustryUserPendingInvitationViewModel
            {
                ID = vm.InvitationId,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.EmailAddress,
                DateInvited = vm.InvitationDateTimeUtc.DateTime,
                InviteExpires = vm.ExpiryDateTimeUtc.DateTime,
                CanInvite = true
            });

            DataSourceResult result = viewModels.ToDataSourceResult(request, vm => new
            {
                ID = vm.ID,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                DateInvited = vm.DateInvited,
                InviteExpires = vm.InviteExpires,
                CanInvite = true
            });

            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult IndustryUsers_PendingInvitations_Delete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<IndustryUserPendingInvitationViewModel> items)
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

                    _invitationService.DeleteInvitation(item.ID);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
            }

            return Json(items.ToDataSourceResult(request, ModelState));
        }
        #endregion



        #region Show Industry Users

        // GET: /Authority/IndustryUserDetails
        [Route("Industry/{iid:int}/User/{id:int}/Details")]
        public ActionResult IndustryUserDetails(int iid , int id)
        {
            IndustryUserViewModel viewModel = PrepareIndustryUserDetails(id);

            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("Industry/{iid:int}/User/{id:int}/IndustryUserUpdateSignatoryStatus")]
        public ActionResult IndustryUserUpdateSignatoryStatus(int iid, int id, IndustryUserViewModel model)
        {
            try
            {
                _userService.UpdateUserSignatoryStatus(model.ID, model.IsSignatory);
                return RedirectToAction(actionName: "IndustryUserDetails", controllerName: "Authority", routeValues: new
                {
                    iid = model.IID,
                    id = model.ID
                });
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
            }

            model = PrepareIndustryUserDetails(id);
            return View(viewName: "IndustryUserDetails", model: model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("Industry/{iid:int}/User/{id:int}/IndustryUserLockUnLock")]
        public ActionResult IndustryUserLockUnLock(int iid, int id, IndustryUserViewModel model)
        {
            try
            {
                _userService.LockUnlockUserAccount(model.PID, !model.AccountLocked, isForFailedKBQs: false);
                return RedirectToAction(actionName: "IndustryUserDetails", controllerName: "Authority", routeValues: new
                {
                    iid = model.IID,
                    id = model.ID
                });
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
            }

            model = PrepareIndustryUserDetails(id);
            return View(viewName: "IndustryUserDetails", model: model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [Route("Industry/{iid:int}/User/{id:int}/IndustryUserReset")]
        public ActionResult IndustryUserReset(int iid, int id, IndustryUserViewModel model)
        {
            string newEmail = model.ResetEmail;
            try
            {
                var result = _userService.ResetUser(model.PID, newEmail);

                if (result.IsSuccess)
                {
                    return RedirectToAction(actionName: "IndustryUserDetails", controllerName: "Authority", routeValues: new
                    {
                        iid = model.IID,
                        id = model.ID
                    });
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
                            message = "User Account Reset Failed";
                            break;
                    }

                    validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                    throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                }
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(rve, ViewData.ModelState);
            }

            model = PrepareIndustryUserDetails(id);
            model.ResetEmail = newEmail;

            return View(viewName: "IndustryUserDetails", model: model);
        }

        private IndustryUserViewModel PrepareIndustryUserDetails(int id)
        {
            var user = _userService.GetOrganizationRegulatoryProgramUser(id);
            var userQuesAns = _questionAnswerService.GetUsersQuestionAnswers(user.UserProfileId, QuestionType.Security);

            var viewModel = new IndustryUserViewModel
            {
                ID = user.OrganizationRegulatoryProgramUserId,
                IID = user.OrganizationRegulatoryProgramId,
                PID = user.UserProfileId,
                FirstName = user.UserProfileDto.FirstName,
                LastName = user.UserProfileDto.LastName,
                PhoneNumber = user.UserProfileDto.PhoneNumber,
                PhoneExt = user.UserProfileDto.PhoneExt,
                Email = user.UserProfileDto.Email,
                ResetEmail = user.UserProfileDto.Email,
                DateRegistered = user.RegistrationDateTimeUtc.Value.DateTime,
                Status = user.IsEnabled,
                AccountLocked = user.UserProfileDto.IsAccountLocked,
                Role = user.PermissionGroup.Name,
                IsSignatory = user.IsSignatory,
                SecurityQuestion1 = (userQuesAns.ElementAt(index: 0) != null) ? userQuesAns.ElementAt(index: 0).Question.Content : "",
                Answer1 = (userQuesAns.ElementAt(index: 0) != null) ? userQuesAns.ElementAt(index: 0).Answer.Content : "",
                SecurityQuestion2 = (userQuesAns.ElementAt(index: 1) != null) ? userQuesAns.ElementAt(index: 1).Question.Content : "",
                Answer2 = (userQuesAns.ElementAt(index: 1) != null) ? userQuesAns.ElementAt(index: 1).Answer.Content : "",
            };
            return viewModel;
        }
        #endregion
    }
}