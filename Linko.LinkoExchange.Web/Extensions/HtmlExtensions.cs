using System.Linq;
using System.Web.Mvc;

namespace Linko.LinkoExchange.Web.Extensions
{
    public static class HtmlExtensions
    {
        public static string IsActive(this HtmlHelper htmlHelper, string controllers = "", string actions = "", string cssClass = "active")
        {
            var viewContext = htmlHelper.ViewContext;
            var isChildAction = viewContext.Controller.ControllerContext.IsChildAction;

            if (isChildAction)
            {
                viewContext = htmlHelper.ViewContext.ParentActionViewContext;
            }

            var routeValues = viewContext.RouteData.Values;
            var currentAction = routeValues[key:"action"].ToString();
            var currentController = routeValues[key:"controller"].ToString();

            if (string.IsNullOrEmpty(value:actions))
            {
                actions = currentAction;
            }

            if (string.IsNullOrEmpty(value:controllers))
            {
                controllers = currentController;
            }

            var acceptedActions = actions.Trim().Split(',').Distinct().ToArray();
            var acceptedControllers = controllers.Trim().Split(',').Distinct().ToArray();

            return acceptedActions.Contains(value:currentAction) && acceptedControllers.Contains(value:currentController) ? cssClass : string.Empty;
        }
    }
}