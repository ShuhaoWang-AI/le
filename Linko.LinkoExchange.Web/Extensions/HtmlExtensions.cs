using System.Linq;
using System.Web.Mvc;

namespace Linko.LinkoExchange.Web.Extensions
{
    public static class HtmlExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="htmlHelper"> </param>
        /// <param name="controllers"> </param>
        /// <param name="actions"> </param>
        /// <param name="routeValueTag"> </param>
        /// <param name="cssClass"> </param>
        /// <returns> </returns>
        public static string IsActive(this HtmlHelper htmlHelper, string controllers = "", string actions = "", string routeValueTag = null, string cssClass = "active")
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

            if (string.IsNullOrEmpty(value:routeValueTag))
            {
                return acceptedActions.Contains(value:currentAction) && acceptedControllers.Contains(value:currentController) ? cssClass : string.Empty;
            }
            else
            {
                var routeKeyValuePair = routeValueTag.Split(':').ToArray();
                var routeValue = routeValues[key:routeKeyValuePair[0]]?.ToString();

                if (string.IsNullOrEmpty(value:routeValue))
                {
                    return acceptedActions.Contains(value:currentAction) && acceptedControllers.Contains(value:currentController) ? cssClass : string.Empty;
                }
                else
                {
                    return acceptedActions.Contains(value:currentAction) && acceptedControllers.Contains(value:currentController) && routeValue.Equals(value:routeKeyValuePair[1])
                               ? cssClass
                               : string.Empty;
                }
            }
        }
    }
}