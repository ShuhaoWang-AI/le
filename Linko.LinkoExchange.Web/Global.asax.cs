using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using FluentValidation.Mvc;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Web.Controllers;
using Microsoft.Practices.Unity;

namespace Linko.LinkoExchange.Web
{
    /// <summary>
    /// 
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer<LinkoExchangeContext>(strategy:null);
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(routes:RouteTable.Routes);
            BundleConfig.RegisterBundles(bundles:BundleTable.Bundles);
            FluentValidationModelValidatorProvider.Configure();
            UnityConfig.RegisterTypes(container:UnityConfig.GetConfiguredContainer());
            FilterConfig.RegisterGlobalFilters(filters:GlobalFilters.Filters);
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
        }

        protected void Application_BeginRequest()
        {
            Response.Cache.SetCacheability(cacheability:HttpCacheability.NoCache);
            Response.Cache.SetExpires(date:DateTime.UtcNow.AddHours(value:-1));
            Response.Cache.SetNoStore();
        }

        /// <summary>
        ///     Handles errors that happens outside of MVC framework.
        /// </summary>
        protected void Application_Error(object sender, EventArgs e)
        {
            // To know the controller and action that handled the request we have to access the GetRouteData method of RouteTable.Routes passing the httpcontext
            var httpContext = ((MvcApplication) sender).Context;

            if (!httpContext.IsCustomErrorEnabled)
            {
                return;
            }

            var currentController = " ";
            var currentAction = " ";
            var currentRouteData = RouteTable.Routes.GetRouteData(httpContext:new HttpContextWrapper(httpContext:httpContext));
            if (currentRouteData != null)
            {
                if (currentRouteData.Values[key:"controller"] != null && !string.IsNullOrEmpty(value:currentRouteData.Values[key:"controller"].ToString()))
                {
                    currentController = currentRouteData.Values[key:"controller"].ToString();
                }

                if (currentRouteData.Values[key:"action"] != null && !string.IsNullOrEmpty(value:currentRouteData.Values[key:"action"].ToString()))
                {
                    currentAction = currentRouteData.Values[key:"action"].ToString();
                }
            }

            // Instantiate the common controller and invoke it by calling the Execute() method passing the HandleErrorInfo model 
            // This is the model used by the HandleError filter as well
            var ex = Server.GetLastError();
            var controller = UnityConfig.GetConfiguredContainer().Resolve<CommonController>();
            var routeData = new RouteData();
            var action = "DefaultError";
            if (ex is UnauthorizedAccessException)
            {
                action = "Unauthorized";
            }
            else if (ex is HttpException)
            {
                var httpEx = ex as HttpException;

                switch (httpEx.GetHttpCode())
                {
                    case 404:
                        action = "PageNotFound";
                        break;
                }
            }

            httpContext.ClearError();
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = ex is HttpException ? ((HttpException) ex).GetHttpCode() : 500;
            httpContext.Response.TrySkipIisCustomErrors = true;

            routeData.Values[key:"controller"] = "Common";
            routeData.Values[key:"action"] = action;

            controller.ViewData.Model = new HandleErrorInfo(exception:ex, controllerName:currentController, actionName:currentAction);
            ((IController) controller).Execute(requestContext:new RequestContext(httpContext:new HttpContextWrapper(httpContext:httpContext), routeData:routeData));
        }
    }
}