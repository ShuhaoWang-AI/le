using System.Web.Mvc;

namespace Linko.LinkoExchange.Web.Mvc
{
    public class CommonInfoAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // ToDo: update the values with proper service
                filterContext.Controller.ViewBag.PortalName = "PortalName";
                filterContext.Controller.ViewBag.OrganizationName = "OrganizationName";
                filterContext.Controller.ViewBag.UserName = "UserName";
                filterContext.Controller.ViewBag.UserRole = "UserRole";
            }
            else
            {
                filterContext.Controller.ViewBag.PortalName = "";
                filterContext.Controller.ViewBag.OrganizationName = "";
                filterContext.Controller.ViewBag.UserName = "";
                filterContext.Controller.ViewBag.UserRole = "";
            }
        }
    }
}