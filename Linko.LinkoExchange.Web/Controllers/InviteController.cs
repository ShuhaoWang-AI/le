using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Sample;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.Extensions;
using Linko.LinkoExchange.Web.Mvc;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using NLog;

namespace Linko.LinkoExchange.Web.Controllers
{
    [PortalAuthorize("authority","industry")]
    public class InviteController:BaseController
    {
        private readonly IHttpContextService _httpContextService;
        private readonly IInvitationService _invitationService;
        private readonly ILogger _logger;

        public InviteController(
            IInvitationService invitationService, 
            ILogger logger, 
            IHttpContextService httpContextService,
            IUserService userService,
            IReportPackageService reportPackageService, 
            ISampleService sampleService)
            :base(httpContextService,userService,reportPackageService, sampleService)
        {
            _invitationService = invitationService;
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
            
            var result = _invitationService.CheckEmailAddress(orgRegProgramId:orgRegProgramId, email:emailAddress);

            var viewModel = new InviteViewModel
                            {
                                OrgRegProgramId = orgRegProgramId,
                                IsUserActiveInSameProgram = result.IsUserActiveInSameProgram
                            };

            if (result.ExistingOrgRegProgramUser == null)
            {
                viewModel.EmailAddress = emailAddress;
                viewModel.DisplayMessage = "The user does not exist. Enter a first and last name and click the 'Send Invite' button.";
            }
            else
            {
                viewModel.FirstName = result.ExistingOrgRegProgramUser.UserProfileDto.FirstName;
                viewModel.LastName = result.ExistingOrgRegProgramUser.UserProfileDto.LastName;
                viewModel.EmailAddress = result.ExistingOrgRegProgramUser.UserProfileDto.Email;
                viewModel.BusinessName = result.ExistingOrgRegProgramUser.UserProfileDto.BusinessName;
                viewModel.PhoneNumber = result.ExistingOrgRegProgramUser.UserProfileDto.PhoneNumber;

                if (result.IsUserActiveInSameProgram)
                {
                    viewModel.DisplayMessage = "This user is already associated with this account. No invite can be sent.";
                }
                else
                {
                    viewModel.DisplayMessage = "This user is not yet associated with this account. Click the 'Send Invite' button to invite them.";
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
            
            if (viewModel.OrgRegProgramId == 0)
            {
                //Inviting to current organization
                var orgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                viewModel.OrgRegProgramId = orgRegProgramId;
            }


            try
            {
                _invitationService.SendUserInvite(orgRegProgramId:viewModel.OrgRegProgramId, email:viewModel.EmailAddress, firstName:viewModel.FirstName,
                                                  lastName:viewModel.LastName, invitationType:viewModel.InvitationType);
                _logger.Info(message:string.Format(format:"Invite successfully sent. Email={0}, FirstName={1}, LastName={2}.",
                                                   arg0:viewModel.EmailAddress, arg1:viewModel.FirstName, arg2:viewModel.LastName));
                TempData["InivteSendSucceed"] = true;

                var redirectUrl = GetRedirectUrl(invitationType:viewModel.InvitationType, orgRegProgramId:viewModel.OrgRegProgramId);
                return Redirect(redirectUrl); 
            }
            catch (RuleViolationException rve)
            {
                MvcValidationExtensions.UpdateModelStateWithViolations(ruleViolationException:rve, modelState:ViewData.ModelState);
            }

            return View(model:viewModel);
        }

        private string GetRedirectUrl(InvitationType invitationType, int orgRegProgramId )
        {
            switch (invitationType)
            {
                case InvitationType.AuthorityToAuthority: return "/Authority/Users";
                case InvitationType.IndustryToIndustry: return "/Industry/Users";
                case InvitationType.AuthorityToIndustry: return string.Format(format:"/Authority/Industry/{0}/Users", arg0:orgRegProgramId);
                default: return @"../";
            }
        } 
    }
}