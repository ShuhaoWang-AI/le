using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using NLog;

namespace Linko.LinkoExchange.Web.Controllers
{
    public class InviteController:Controller
    {
        private readonly IHttpContextService _httpContextService;
        private readonly IInvitationService _invitationService;
        private readonly ILogger _logger;
        private readonly ISessionCache _sessionCache;

        public InviteController(IInvitationService invitationService, ISessionCache sessionCache, ILogger logger, IHttpContextService httpContextService)
        {
            _invitationService = invitationService;
            _sessionCache = sessionCache;
            _logger = logger;
            _httpContextService = httpContextService;
        }

        public ActionResult Invite(string industryOrgRegProgramId, string invitationType)
        {
            var model = new InviteViewModel();
            if (!string.IsNullOrEmpty(value:industryOrgRegProgramId))
            {
                model.OrgRegProgramId = Convert.ToInt32(value:industryOrgRegProgramId);
            }

            model.InvitationType = (InvitationType) Enum.Parse(enumType:typeof(InvitationType), value:invitationType);
            return View(model:model);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult InviteCheckEmail(string emailAddress, string orgRegProgramIdString)
        {
            int orgRegProgramId;
            if (string.IsNullOrEmpty(value:orgRegProgramIdString) || int.Parse(s:orgRegProgramIdString) < 1)
            {
                orgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            }
            else
            {
                orgRegProgramId = int.Parse(s:orgRegProgramIdString);
            }

            var viewModel = new InviteViewModel();
            var foundUsers = _invitationService.CheckEmailAddress(orgRegProgramId:orgRegProgramId, email:emailAddress);
            if (foundUsers.ExistingUserSameProgram != null)
            {
                viewModel = new InviteViewModel
                            {
                                DisplayMessage = "This user is already associated with this account. No invite can be sent.",
                                IsExistingProgramUser = true,
                                FirstName = foundUsers.ExistingUserSameProgram.UserProfileDto.FirstName,
                                LastName = foundUsers.ExistingUserSameProgram.UserProfileDto.LastName,
                                EmailAddress = foundUsers.ExistingUserSameProgram.UserProfileDto.Email,
                                BusinessName = foundUsers.ExistingUserSameProgram.UserProfileDto.BusinessName,
                                PhoneNumber = foundUsers.ExistingUserSameProgram.UserProfileDto.PhoneNumber
                            };
            }
            else if (foundUsers.ExistingUsersDifferentPrograms != null)
            {
                viewModel = new InviteViewModel
                            {
                                DisplayMessage = "This user is not yet associated with this account. Click the 'Send Invite' button to invite them.",
                                IsExistingProgramUser = false,
                                ExistingUsers = new List<InviteExistingUserViewModel>()
                            };

                foreach (var distinctExistingUser in
                    foundUsers.ExistingUsersDifferentPrograms
                              .GroupBy(user => user.UserProfileId)
                              .Select(user => user.First())
                              .ToList())
                {
                    viewModel.ExistingUsers.Add(item:new InviteExistingUserViewModel
                                                     {
                                                         OrgRegProgramUserId = distinctExistingUser.OrganizationRegulatoryProgramUserId,
                                                         FirstName = distinctExistingUser.UserProfileDto.FirstName,
                                                         LastName = distinctExistingUser.UserProfileDto.LastName,
                                                         EmailAddress = distinctExistingUser.UserProfileDto.Email,
                                                         BusinessName = distinctExistingUser.UserProfileDto.BusinessName,
                                                         PhoneNumber = distinctExistingUser.UserProfileDto.PhoneNumber
                                                     });
                }
            }

            return Json(data:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult Invite(InviteViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(model:viewModel);
            }

            var orgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            if (viewModel.OrgRegProgramId > 0)
            {
                //Inviting Admin user to UI
                orgRegProgramId = viewModel.OrgRegProgramId;
            }

            var redirectUrl = GetRedirectUrl(invitationType:viewModel.InvitationType, industryOrgRegProgramId:orgRegProgramId);

            var result = _invitationService.SendUserInvite(orgRegProgramId:orgRegProgramId, email:viewModel.EmailAddress, firstName:viewModel.FirstName, lastName:viewModel.LastName,
                                                           invitationType:viewModel.InvitationType);
            if (result.Success)
            {
                _logger.Info(message:string.Format(format:"Invite successfully sent. Email={0}, FirstName={1}, LastName={2}.",
                                                   arg0:viewModel.EmailAddress, arg1:viewModel.FirstName, arg2:viewModel.LastName));

                //return new RedirectResult(redirectUrl);
                return View(viewName:"Confirmation", model:new ConfirmationViewModel
                                                           {
                                                               Title = "Invitation Confirmation",
                                                               Message = "An invitation has been sent",
                                                               HtmlStr = "<p><a href=\"#\" onclick=\"location.href='" + redirectUrl + "'\" class=\"btn btn-sm btn-primary\">OK</a></p>"
                                                           });
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(key:string.Empty, errorMessage:error);
            }

            viewModel.EmailAddress = "";
            viewModel.FirstName = "";
            viewModel.LastName = "";
            return View(model:viewModel);
        }

        [AcceptVerbs(verbs:HttpVerbs.Post)]
        public ActionResult InviteExistingUser(string orgRegProgUserIdString, string industryOrgRegProgramId, string invitationType)
        {
            var orgRegProgramUserId = int.Parse(s:orgRegProgUserIdString);
            var orgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var thisInvitationType = (InvitationType) Enum.Parse(enumType:typeof(InvitationType), value:invitationType);
            if (!string.IsNullOrEmpty(value:industryOrgRegProgramId) && int.Parse(s:industryOrgRegProgramId) > 0)
            {
                //Inviting Admin user to UI
                orgRegProgramId = int.Parse(s:industryOrgRegProgramId);
            }

            var redirectUrl = GetRedirectUrl(invitationType:thisInvitationType, industryOrgRegProgramId:orgRegProgramId);

            var result = _invitationService.SendUserInvite(orgRegProgramId:orgRegProgramId, email:"", firstName:"", lastName:"", invitationType:thisInvitationType,
                                                           existingOrgRegProgramUserId:orgRegProgramUserId);
            if (result.Success)
            {
                _logger.Info(message:
                             string.Format(format:"Invite successfully sent to existing user in different program. OrgRegProgUserId={0} from ProgramId={1}", arg0:orgRegProgramUserId,
                                           arg1:orgRegProgramId));
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    _logger.Info(message:
                                 string.Format(format:"Invite failed to send to existing user {0} in different program. Error={1} from ProgramId={2}", arg0:orgRegProgramUserId, arg1:error,
                                               arg2:orgRegProgramId));
                }
            }

            //return new RedirectResult(redirectUrl);
            return View(viewName:"Confirmation", model:new ConfirmationViewModel
                                                       {
                                                           Title = "Invitation Confirmation",
                                                           Message = "An invitation has been sent",
                                                           HtmlStr = "<p><a href=\"#\" onclick=\"location.href='" + redirectUrl + "'\" class=\"btn btn-sm btn-primary\">OK</a></p>"
                                                       });
        }

        private string GetRedirectUrl(InvitationType invitationType, int? industryOrgRegProgramId = null)
        {
            switch (invitationType)
            {
                case InvitationType.AuthorityToAuthority:
                    return "/Authority/Users";
                case InvitationType.IndustryToIndustry:
                    return "/Industry/Users";
                case InvitationType.AuthorityToIndustry:
                    if (industryOrgRegProgramId.HasValue && industryOrgRegProgramId.Value > 0)
                    {
                        return string.Format(format:"/Authority/Industry/{0}/Users", arg0:industryOrgRegProgramId.Value);
                    }
                    break;
            }

            return @"../";
        }
    }
}