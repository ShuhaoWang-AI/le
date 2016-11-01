using Linko.LinkoExchange.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return Convert.ToInt32(System.Web.HttpContext.Current.User.Identity.UserProfileId());
        }
    }
}
