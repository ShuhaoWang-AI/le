using Linko.LinkoExchange.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Linko.LinkoExchange.Services.User
{
    public class CurrentUser : ICurrentUser
    {
        public int? GetCurrentOrgRegProgramUserId()
        {
            return HttpContext.Current != null ? HttpContext.Current.User.Identity.GetOrganizationRegulatoryProgramUserId() : (int?)null;
        }

    }
}
