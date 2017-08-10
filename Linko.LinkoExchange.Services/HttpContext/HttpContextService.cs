using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using Linko.LinkoExchange.Core.Extensions;

namespace Linko.LinkoExchange.Services.HttpContext
{
    public class HttpContextService : IHttpContextService
    {
        #region interface implementations

        public System.Web.HttpContext Current => System.Web.HttpContext.Current;

        public object GetSessionValue(string key)
        {
            return Current.Session[name:key];
        }

        public void SetSessionValue(string key, object value)
        {
            Current.Session[name:key] = value;
        }

        public string GetClaimValue(string claimType)
        {
            if (Current.User.Identity.IsAuthenticated)
            {
                var identity = Current.User.Identity as ClaimsIdentity;
                var claims = identity?.Claims.ToList();

                Debug.Assert(condition:claims != null, message:"claims != null");
                var claim = claims.FirstOrDefault(i => i.Type == claimType);
                return claim != null ? claim.Value : string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }

        public string GetRequestBaseUrl()
        {
            return Current.Request.Url.Scheme
                   + "://"
                   + Current.Request.Url.Authority
                   + Current.Request.ApplicationPath?.TrimEnd('/')
                   + "/";
        }

        public int CurrentUserProfileId()
        {
            if (System.Web.HttpContext.Current?.User?.Identity != null)
            {
                var userProfileIdString = Current.User.Identity.UserProfileId();
                if (!string.IsNullOrEmpty(value:userProfileIdString))
                {
                    return Convert.ToInt32(value:userProfileIdString);
                }
            }

            return -1;
        }

        public string CurrentUserIPAddress()
        {
            var userIPAddress = Current?.Request?.UserHostAddress;
            return !string.IsNullOrEmpty(value:userIPAddress) ? userIPAddress : "";
        }

        public string CurrentUserHostName()
        {
            if (System.Web.HttpContext.Current?.Request?.UserHostAddress != null)
            {
                var userIPAddress = Current.Request.UserHostAddress;
                if (!string.IsNullOrEmpty(value:userIPAddress))
                {
                    string userHostName;
                    try
                    {
                        userHostName = Dns.GetHostEntry(hostNameOrAddress:userIPAddress)?.HostName ?? "";
                    }
                    catch
                    {
                        userHostName = null;
                    }

                    if (!string.IsNullOrEmpty(value:userHostName))
                    {
                        return userHostName;
                    }
                }
            }

            return "";
        }

        #endregion
    }
}