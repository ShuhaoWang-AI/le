﻿// Linko.LinkoExchange.Web -  Global.asax.cs
// 9/20/2016 - rsaha 

using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using FluentValidation.Mvc;
using Linko.LinkoExchange.Web.Controllers;
using Microsoft.Practices.Unity;

namespace Linko.LinkoExchange.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas ();
            RouteConfig.RegisterRoutes (RouteTable.Routes);
            BundleConfig.RegisterBundles (BundleTable.Bundles);
            FluentValidationModelValidatorProvider.Configure ();
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
            RouteData currentRouteData = RouteTable.Routes.GetRouteData (new System.Web.HttpContextWrapper (httpContext));
            if (currentRouteData != null)
            {
                if (currentRouteData.Values["controller"] != null && !string.IsNullOrEmpty (currentRouteData.Values["controller"].ToString ()))
                {
                    currentController = currentRouteData.Values["controller"].ToString ();
                }

                if (currentRouteData.Values["action"] != null && !string.IsNullOrEmpty (currentRouteData.Values["action"].ToString ()))
                {
                    currentAction = currentRouteData.Values["action"].ToString ();
                }
            }

            // Instantiate the common controller and invoke it by calling the Execute() method passing the HandleErrorInfo model 
            // This is the model used by the HandleError filter as well
            Exception ex = Server.GetLastError ();
            CommonController controller = UnityConfig.GetConfiguredContainer ().Resolve<CommonController> ();
            var routeData = new RouteData ();
            string action = "DefaultError";
            if (ex is System.Web.HttpException)
            {
                var httpEx = ex as System.Web.HttpException;

                switch (httpEx.GetHttpCode ())
                {
                    case 404:
                        action = "PageNotFound";
                        break;
                }
            }

            httpContext.ClearError ();
            httpContext.Response.Clear ();
            httpContext.Response.StatusCode = ex is System.Web.HttpException ? ((System.Web.HttpException) ex).GetHttpCode () : 500;
            httpContext.Response.TrySkipIisCustomErrors = true;

            routeData.Values["controller"] = "Common";
            routeData.Values["action"] = action;

            controller.ViewData.Model = new HandleErrorInfo (ex, currentController, currentAction);
            ((IController) controller).Execute (new RequestContext (new System.Web.HttpContextWrapper (httpContext), routeData));
        }
    }
}
