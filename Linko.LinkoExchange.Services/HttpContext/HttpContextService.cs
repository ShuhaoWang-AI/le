using System;
using Linko.LinkoExchange.Core.Extensions;

namespace Linko.LinkoExchange.Services
{
    public class HttpContextService : IHttpContextService
    {
        public System.Web.HttpContext Current()
        {
            return System.Web.HttpContext.Current;
        }

        public object GetSessionValue(string key)
        {
            return System.Web.HttpContext.Current.Session[key];
        }

        public void SetSessionValue(string key, object value)
        {
            System.Web.HttpContext.Current.Session[key] = value;
        }

        public string GetRequestBaseUrl()
        {
            return Current().Request.Url.Scheme + "://"
             + Current().Request.Url.Authority
             + Current().Request.ApplicationPath.TrimEnd('/') + "/";
        }

        public int CurrentUserProfileId()
        {
            if (System.Web.HttpContext.Current?.User?.Identity != null)
            {
                string userProfileIdString = System.Web.HttpContext.Current.User.Identity.UserProfileId();
                if (!String.IsNullOrEmpty(userProfileIdString))
                {
                    return Convert.ToInt32(userProfileIdString);
                }
            }

            return -1;
        }

        public string CurrentUserIPAddress()
        {
            if (System.Web.HttpContext.Current?.Request?.UserHostAddress != null)
            {
                string userIPAddress = System.Web.HttpContext.Current.Request.UserHostAddress;
                if (!String.IsNullOrEmpty(userIPAddress))
                {
                    return userIPAddress;
                }
            }

            return "";
        }
        public string CurrentUserHostName()
        {
            if (System.Web.HttpContext.Current?.Request?.UserHostName != null)
            {
                string userHostName = System.Web.HttpContext.Current.Request.UserHostName;
                if (!String.IsNullOrEmpty(userHostName))
                {
                    return userHostName;
                }
            }

            return "";

        }
    }
}
