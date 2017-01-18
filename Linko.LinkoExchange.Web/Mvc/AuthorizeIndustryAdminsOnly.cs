using System.Web.Mvc;
using Linko.LinkoExchange.Services.Cache;
using System.Web.Routing;
using Microsoft.Practices.Unity;
using Linko.LinkoExchange.Services.User;
using System.Configuration;

namespace Linko.LinkoExchange.Web.Mvc
{
    public class AuthorizeIndustryAdminsOnly : ActionFilterAttribute
    {

        [Dependency]
        public IUserService _userService { get; set; }
        [Dependency]
        public ISessionCache _sessionCache { get; set; }

        private string _unauthorizedPagePath;

        public AuthorizeIndustryAdminsOnly()
        {
            _unauthorizedPagePath = ConfigurationManager.AppSettings["UnauthorizedPagePath"];
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                int urlOrgRegProgUserId = int.Parse(filterContext.Controller.ControllerContext.RouteData.Values["id"].ToString());
                var targetsOrgRegProgramUserDto = _userService.GetOrganizationRegulatoryProgramUser(urlOrgRegProgUserId);

                int usersOrgRegProgUserId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
                var thisOrgRegProgramUserDto = _userService.GetOrganizationRegulatoryProgramUser(usersOrgRegProgUserId);

                bool isAdmin = thisOrgRegProgramUserDto.PermissionGroup.Name.ToLower().StartsWith("admin")
                                && thisOrgRegProgramUserDto.PermissionGroup.OrganizationRegulatoryProgramId == targetsOrgRegProgramUserDto.OrganizationRegulatoryProgramId;

                if (isAdmin)
                {
                    //This is an authorized industry admin within the target organization regulatory program
                    return;
                }
            }

            //Not authorized
            var result = new ViewResult
            {
                ViewName = _unauthorizedPagePath,
            };
            filterContext.Result = result;

        }

    }
    
}