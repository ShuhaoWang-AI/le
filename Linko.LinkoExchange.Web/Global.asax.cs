using System;
using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using FluentValidation.Mvc;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Web.Controllers;
using Microsoft.Practices.Unity;
using System.Web.Helpers;
using System.Security.Claims;
using System.Web;

namespace Linko.LinkoExchange.Web
{
    /// <summary>
    /// 
    /// </summary>
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer <LinkoExchangeContext> (strategy: null);
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            FluentValidationModelValidatorProvider.Configure();
            UnityConfig.RegisterTypes(UnityConfig.GetConfiguredContainer());
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
        }

        protected void Application_BeginRequest()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
        }

        /// <summary>
        /// Handles errors that happens outside of MVC framework.
        /// </summary>
        protected void Application_Error(object sender, EventArgs e)
        {
            // To know the controller and action that handled the request we have to access the GetRouteData method of RouteTable.Routes passing the httpcontext
            System.Web.HttpContext httpContext = ((MvcApplication) sender).Context;

            if (!httpContext.IsCustomErrorEnabled)
            {
                return;
            }

            string currentController = " ";
            string currentAction = " ";
            RouteData currentRouteData = RouteTable.Routes.GetRouteData(new System.Web.HttpContextWrapper(httpContext));
            if (currentRouteData != null)
            {
                if (currentRouteData.Values["controller"] != null && !string.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
                {
                    currentController = currentRouteData.Values["controller"].ToString();
                }

                if (currentRouteData.Values["action"] != null && !string.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
                {
                    currentAction = currentRouteData.Values["action"].ToString();
                }
            }

            // Instantiate the common controller and invoke it by calling the Execute() method passing the HandleErrorInfo model 
            // This is the model used by the HandleError filter as well
            Exception ex = Server.GetLastError();
            CommonController controller = UnityConfig.GetConfiguredContainer().Resolve<CommonController>();
            RouteData routeData = new RouteData();
            string action = "DefaultError";
            if (ex is UnauthorizedAccessException)
            {
                action = "Unauthorized";
            }
            else if (ex is System.Web.HttpException)
            {
                System.Web.HttpException httpEx = ex as System.Web.HttpException;

                switch (httpEx.GetHttpCode())
                {
                    case 404:
                        action = "PageNotFound";
                        break;
                }
            }

            httpContext.ClearError();
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = ex is System.Web.HttpException ? ((System.Web.HttpException) ex).GetHttpCode() : 500;
            httpContext.Response.TrySkipIisCustomErrors = true;

            routeData.Values["controller"] = "Common";
            routeData.Values["action"] = action;

            controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);
            ((IController) controller).Execute(new RequestContext(new System.Web.HttpContextWrapper(httpContext), routeData));
        }


    }
}
