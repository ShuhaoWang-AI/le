using System.Web.Mvc;
using Linko.LinkoExchange.Services.Cache;
using System.Web.Routing;
using Linko.LinkoExchange.Services.Organization;
using Microsoft.Practices.Unity;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Web.Mvc
{
    public class AuthorizeCorrectAuthorityOnly : ActionFilterAttribute
    {

        [Dependency]
        public IOrganizationService _orgService { get; set; }
        [Dependency]
        public IUserService _userService { get; set; }
        [Dependency]
        public ISessionCache _sessionCache { get; set; }

        //If true   : passed in "id" is an Organization Regulatory Program User Id
        //If false  : passed in "id" is an Organization Regulatory Program Id (Industry)
        bool _isIdParameterForUser;

        public AuthorizeCorrectAuthorityOnly(bool isIdParameterForUser)
        {
            _isIdParameterForUser = isIdParameterForUser;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewResult result;
            bool isCorrectAuthority = true;

            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                //Case 1: Both "id" and "iid" exist in URL ("iid" is for Industry)
                //Case 2: Only "id" exists in URL ("id" can be User or Industry. Check _isIdParameterForUser)
                //
                int? targetOrgRegProgUserId = null;
                int? targetOrgRegProgramId = null;

                if (filterContext.Controller.ControllerContext.RouteData.Values["iid"] != null)
                {
                    //both Industry and User id was passed in
                    targetOrgRegProgramId = int.Parse(filterContext.Controller.ControllerContext.RouteData.Values["iid"].ToString());
                    targetOrgRegProgUserId = int.Parse(filterContext.Controller.ControllerContext.RouteData.Values["id"].ToString());
                }
                else
                {
                    if (_isIdParameterForUser)
                    {
                        //User id was passed in only
                        targetOrgRegProgUserId = int.Parse(filterContext.Controller.ControllerContext.RouteData.Values["id"].ToString());
                    }
                    else
                    {
                        //Industry id was passed in only
                        targetOrgRegProgramId = int.Parse(filterContext.Controller.ControllerContext.RouteData.Values["id"].ToString());
                    }
                }

                if (targetOrgRegProgUserId.HasValue)
                {

                    var targetUserDto = _userService.GetOrganizationRegulatoryProgramUser(targetOrgRegProgUserId.Value);
                    int usersOrgRegProgramId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
                    if (usersOrgRegProgramId != _orgService.GetAuthority(targetUserDto.OrganizationRegulatoryProgramId).OrganizationRegulatoryProgramId)
                    {
                        //This is not the authorized Authority of this Industry User
                        isCorrectAuthority = false;
                    }

                }

                if (targetOrgRegProgramId.HasValue)
                {
                    int usersOrgRegProgramId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
                    if (usersOrgRegProgramId != _orgService.GetAuthority(targetOrgRegProgramId.Value).OrganizationRegulatoryProgramId)
                    {
                        //This is not the authorized Authority of this Industry
                        isCorrectAuthority = false;
                    }

                }

            }
            else {
                //This is not the authorized Authority if this Industry
                isCorrectAuthority = false;

            }

            if (isCorrectAuthority)
            {
                return;
            }
            else
            {
                //Not authorized
                result = new ViewResult
                {
                    ViewName = "~/Views/Common/Unauthorized.cshtml",
                };
                filterContext.Result = result;

            }


        }

    }
}