using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Web;
using System.Web.Mvc;
using NLog;

namespace Linko.LinkoExchange.Web.Mvc
{
    /// <summary>
    ///     Custom error handling that logs the exception.
    /// </summary>
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        #region fields

        private readonly ILogger _logger;
        private readonly string _unauthorizedPagePath;

        #endregion

        #region constructors and destructor

        public CustomHandleErrorAttribute(ILogger logger)
        {
            _logger = logger;
            _unauthorizedPagePath = ConfigurationManager.AppSettings[name:"UnauthorizedPagePath"];
        }

        #endregion

        #region public methods

        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            {
                return;
            }

            var httpCode = new HttpException(message:null, innerException:filterContext.Exception).GetHttpCode();
            if (httpCode != 500 && httpCode != 401) //401 = Unauthorized Access
            {
                return;
            }

            if (!ExceptionType.IsInstanceOfType(o:filterContext.Exception))
            {
                return;
            }

            // if the request is AJAX return JSON else view
            if (filterContext.HttpContext.Request.Headers[name:"X-Requested-With"] == "XMLHttpRequest")
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
                                     ViewName = _unauthorizedPagePath
                                 };
                    filterContext.Result = result;
                }
                else
                {
                    var controllerName = (string) filterContext.RouteData.Values[key:"controller"];
                    var actionName = (string) filterContext.RouteData.Values[key:"action"];
                    var model = new HandleErrorInfo(exception:filterContext.Exception, controllerName:controllerName, actionName:actionName);
                    filterContext.Result = new ViewResult
                                           {
                                               ViewName = View,
                                               MasterName = Master,
                                               ViewData = new ViewDataDictionary<HandleErrorInfo>(model:model),
                                               TempData = filterContext.Controller.TempData
                                           };
                }
            }

            // log the error
            string errorMessage;
            if (filterContext.Exception is DbEntityValidationException)
            {
                var ex = (DbEntityValidationException) filterContext.Exception;
                var errors = new List<string> {ex.Message};

                foreach (var item in ex.EntityValidationErrors)
                {
                    var entry = item.Entry;
                    var entityTypeName = entry.Entity.GetType().Name;

                    foreach (var subItem in item.ValidationErrors)
                    {
                        var message = $"Error '{subItem.ErrorMessage}' occurred in {entityTypeName} at {subItem.PropertyName}";
                        errors.Add(item:message);
                    }
                }

                errorMessage = string.Join(separator:"," + Environment.NewLine, values:errors);
            }
            else
            {
                errorMessage = filterContext.Exception.Message;
            }

            // ReSharper disable once ArgumentsStyleNamedExpression
            _logger.Error(filterContext.Exception, errorMessage);

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;

            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }

        #endregion
    }
}