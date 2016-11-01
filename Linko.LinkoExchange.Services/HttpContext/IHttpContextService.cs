using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services
{
    public interface IHttpContextService
    {
        System.Web.HttpContext Current();
        string GetRequestBaseUrl();

        object GetSessionValue(string key);
        void SetSessionValue(string key, object value);

        int CurrentUserProfileId();
    } 
}
