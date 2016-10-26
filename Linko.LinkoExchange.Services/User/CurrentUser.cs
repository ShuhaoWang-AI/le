using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Extensions;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Linko.LinkoExchange.Services.User
{
    public class CurrentUser : ICurrentUser
    {
        public int UserProfileId()
        {
            if (HttpContext.Current != null)
                return Convert.ToInt32(HttpContext.Current.User.Identity.UserProfileId());
            else
                throw new Exception("ERROR: HttpContext.Current does not exist");

        }
    }
}
