using System.Web.Mvc;
using NLog;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using System.Collections.Generic;
using System;
using System.Configuration;

namespace Linko.LinkoExchange.Web.Mvc
{
    /// <summary>
    /// Custom error handling that logs the exception.
    /// </summary>
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        #region private members

        private readonly ILogger _logger;
        private string _unauthorizedPagePath;

        #endregion


        #region constructor

        public CustomHandleErrorAttribute(ILogger logger)
        {
            _logger = logger;
            _unauthorizedPagePath = ConfigurationManager.AppSettings["UnauthorizedPagePath"];
        }

        #endregion


        #region public methods

        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            {
                return;
            }

            var httpCode = new System.Web.HttpException(message: null, innerException: filterContext.Exception).GetHttpCode();
            if (httpCode != 500 && httpCode != 401) //401 = Unauthorized Access
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
                if (filterContext.Exception is UnauthorizedAccessException)
                {
                    var result = new ViewResult
                    {
                        ViewName = _unauthorizedPagePath,
                    };
                    filterContext.Result = result;
                }
                else {

                    string controllerName = (string)filterContext.RouteData.Values["controller"];
                    string actionName = (string)filterContext.RouteData.Values["action"];
                    HandleErrorInfo model = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);
                    filterContext.Result = new ViewResult
                    {
                        ViewName = View,
                        MasterName = Master,
                        ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                        TempData = filterContext.Controller.TempData
                    };
                }
               
            }

            // log the error
            string errorMessage;
            if (filterContext.Exception is DbEntityValidationException)
            {
                var ex = (DbEntityValidationException)filterContext.Exception;
                var errors = new List<string>() { ex.Message };

                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;

                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        string message = string.Format("Error '{0}' occurred in {1} at {2}", subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                        errors.Add(message);
                    }
                }

                errorMessage = String.Join("," + Environment.NewLine, errors);
            }
            else
            {
                errorMessage = filterContext.Exception.Message;
            }

            _logger.Error(filterContext.Exception, errorMessage);

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;

            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }

        #endregion
    }
}