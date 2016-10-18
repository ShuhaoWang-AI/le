using System.Web.Mvc;
using NLog;

namespace Linko.LinkoExchange.Web.Mvc
{
    /// <summary>
    /// Custom error handling that logs the exception.
    /// </summary>
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        #region private members

        private readonly ILogger _logger;

        #endregion


        #region constructor

        public CustomHandleErrorAttribute(ILogger logger)
        {
            _logger = logger;
        }

        #endregion


        #region public methods

        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            {
                return;
            }

            if (new System.Web.HttpException(message: null, innerException: filterContext.Exception).GetHttpCode() != 500)
            {
                return;
            }

            if (!ExceptionType.IsInstanceOfType(filterContext.Exception))
            {
                return;
            }

            // if the request is AJAX return JSON else view
            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        error = true,
                        message = filterContext.Exception.Message
                    }
                };
            }
            else
            {
                string controllerName = (string) filterContext.RouteData.Values["controller"];
                string actionName = (string) filterContext.RouteData.Values["action"];
                HandleErrorInfo model = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);
                filterContext.Result = new ViewResult
                {
                    ViewName = View,
                    MasterName = Master,
                    ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                    TempData = filterContext.Controller.TempData
                };
            }

            // log the error
            _logger.Error(filterContext.Exception, filterContext.Exception.Message);

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;

            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }

        #endregion
    }
}