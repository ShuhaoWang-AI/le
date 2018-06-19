using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Linko.LinkoExchange.Web.Shared
{
    public class RedirectToActionAnchor : RedirectToRouteResult
    {
        #region constructors and destructor

        public RedirectToActionAnchor(string action, string controller, string anchor, object routeValues)
            : base(routeValues:new RouteValueDictionary(values:routeValues))
        {
            if (string.IsNullOrWhiteSpace(value:action))
            {
                throw new ArgumentNullException(paramName:action);
            }

            Action = action;
            Anchor = anchor;
            Controller = controller;
        }

        #endregion

        #region interface implementations

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(paramName:"context");
            }

            RouteValues[key:"action"] = Action;

            if (!string.IsNullOrWhiteSpace(value:Controller))
            {
                RouteValues[key:"controller"] = Controller;
            }

            var requestContext = new RequestContext(httpContext:context.HttpContext, routeData:RouteTable.Routes.GetRouteData(httpContext:context.HttpContext));
            var vpd = RouteTable.Routes.GetVirtualPath(requestContext:requestContext, name:RouteName, values:RouteValues);

            if (vpd == null || string.IsNullOrWhiteSpace(value:vpd.VirtualPath))
            {
                throw new InvalidOperationException(message:"No route matched");
            }

            var target = vpd.VirtualPath;

            if (!string.IsNullOrWhiteSpace(value:Anchor))
            {
                // Add the anchor onto the end:
                target += string.Format(format:"#{0}", arg0:Anchor);
            }

            context.HttpContext.Response.Redirect(url:target, endResponse:false);
        }

        #endregion

        #region public properties

        public string Action { get; set; }

        public string Anchor { get; set; }

        public string Controller { get; set; }

        #endregion
    }
}