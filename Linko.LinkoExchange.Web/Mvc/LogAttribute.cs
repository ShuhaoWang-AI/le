using System.Web.Mvc;
using NLog;

namespace Linko.LinkoExchange.Web.Mvc
{
    public class LogAttribute : ActionFilterAttribute
    {
        #region fields

        private readonly ILogger _logger;

        #endregion

        #region constructors and destructor

        public LogAttribute(ILogger logger)
        {
            _logger = logger;
        }

        #endregion

        #region default action

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controllerName = filterContext.RouteData.Values[key:"controller"];
            var actionName = filterContext.RouteData.Values[key:"action"];
            var url = filterContext.HttpContext.Request.Url;
            var message = $"Executing - controller:{controllerName} action:{actionName}";

            _logger.Info(message:message);
            _logger.Info(message:$"Url:{url}");
            base.OnActionExecuting(filterContext:filterContext);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var controllerName = filterContext.RouteData.Values[key:"controller"];
            var actionName = filterContext.RouteData.Values[key:"action"];
            var message = $"Executed - controller:{controllerName} action:{actionName}";

            _logger.Info(message:message);
            base.OnResultExecuted(filterContext:filterContext);
        }

        #endregion
    }
}