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

        public string GetRequestBaseUrl()
        {
            return Current().Request.Url.Scheme + "://"
             + Current().Request.Url.Authority
             + Current().Request.ApplicationPath.TrimEnd('/') + "/";
        }
    }
}
