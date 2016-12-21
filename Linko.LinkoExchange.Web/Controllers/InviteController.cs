using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Web.ViewModels.Authority;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Linko.LinkoExchange.Web.Controllers
{
    public class InviteController : Controller
    {
        private readonly IInvitationService _invitationService;
        private readonly ISessionCache _sessionCache;
        private readonly ILogger _logger;

        public InviteController(IInvitationService invitationService, ISessionCache sessionCache, ILogger logger)
        {
            _invitationService = invitationService;
            _sessionCache = sessionCache;
            _logger = logger;
        }

        public ActionResult Invite(string industryOrgRegProgramId, string invitationType)
        {
            var model = new InviteViewModel();
            if (!String.IsNullOrEmpty(industryOrgRegProgramId))
            {
                model.OrgRegProgramUserId = Convert.ToInt32(industryOrgRegProgramId);
            }

            model.InvitationType = (InvitationType)Enum.Parse(typeof(InvitationType), invitationType);
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult InviteCheckEmail(string emailAddress, string orgRegProgramIdString)
        {
            int orgRegProgramId;
            if (String.IsNullOrEmpty(orgRegProgramIdString) || int.Parse(orgRegProgramIdString) < 1)
            {
                orgRegProgramId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            }
            else
            {
                orgRegProgramId = int.Parse(orgRegProgramIdString);
            }

            InviteViewModel viewModel = new InviteViewModel();
            var foundUsers = _invitationService.CheckEmailAddress(orgRegProgramId, emailAddress);
            if (foundUsers.ExistingUserSameProgram != null)
            {
                viewModel = new InviteViewModel()
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
                viewModel = new InviteViewModel()
                {
                    DisplayMessage = "This user is not yet associated with this account. Click the 'Send Invite' button to invite them.",
                    IsExistingProgramUser = false,
                };

                viewModel.ExistingUsers = new List<InviteExistingUserViewModel>();
                foreach (var distinctExistingUser in 
                    foundUsers.ExistingUsersDifferentPrograms
                    .GroupBy(user => user.UserProfileId)
                    .Select(user => user.First())
                    .ToList())
                {
                    viewModel.ExistingUsers.Add(new InviteExistingUserViewModel()
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


            return Json(viewModel);

        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Invite(InviteViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var orgRegProgramId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            if (viewModel.OrgRegProgramUserId > 0)
            {
                //Inviting Admin user to UI
                orgRegProgramId = viewModel.OrgRegProgramUserId;
            }

            string redirectUrl = GetRedirectUrl(viewModel.InvitationType, orgRegProgramId);

            var result = _invitationService.SendUserInvite(orgRegProgramId, viewModel.EmailAddress, viewModel.FirstName, viewModel.LastName, viewModel.InvitationType);
            if (result.Success)
            {
                _logger.Info(string.Format("Invite successfully sent. Email={0}, FirstName={1}, LastName={2}.",
                    viewModel.EmailAddress, viewModel.FirstName, viewModel.LastName));

                //return new RedirectResult(redirectUrl);
                return View("Confirmation", new ConfirmationViewModel()
                {
                    Title = "Invitation Confirmation",
                    Message = "An invitation has been sent",
                    HtmlStr = "<p><a href=\"#\" onclick=\"location.href='" + redirectUrl + "'\" class=\"btn btn-sm btn-primary\">OK</a></p>"
                });
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }

            viewModel.EmailAddress = "";
            viewModel.FirstName = "";
            viewModel.LastName = "";
            return View(viewModel);

        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult InviteExistingUser(string orgRegProgUserIdString, string industryOrgRegProgramId, string invitationType)
        {
            var orgRegProgramUserId = int.Parse(orgRegProgUserIdString);
            var orgRegProgramId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            InvitationType thisInvitationType = (InvitationType)Enum.Parse(typeof(InvitationType), invitationType);
            if (!String.IsNullOrEmpty(industryOrgRegProgramId) && int.Parse(industryOrgRegProgramId) > 0)
            {
                //Inviting Admin user to UI
                orgRegProgramId = int.Parse(industryOrgRegProgramId);
            }

            string redirectUrl = GetRedirectUrl(thisInvitationType, orgRegProgramId);

            var result = _invitationService.SendUserInvite(orgRegProgramId, "", "", "", thisInvitationType, orgRegProgramUserId);
            if (result.Success)
            {
                _logger.Info(string.Format("Invite successfully sent to existing user in different program. OrgRegProgUserId={0} from ProgramId={1}", orgRegProgramUserId, orgRegProgramId));
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    _logger.Info(string.Format("Invite failed to send to existing user {0} in different program. Error={1} from ProgramId={2}", orgRegProgramUserId, error, orgRegProgramId));
                }
            }

            //return new RedirectResult(redirectUrl);
            return View("Confirmation", new ConfirmationViewModel()
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
                        return string.Format("/Authority/Industry/{0}/Users", industryOrgRegProgramId.Value);
                    }
                    break;
            }

            return @"../";

        }
    }
}