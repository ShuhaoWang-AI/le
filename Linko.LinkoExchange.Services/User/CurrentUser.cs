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
        public void SetCurrentOrgRegProgUserId(int orgRegProgUserId)
        {
            if (HttpContext.Current != null)
                HttpContext.Current.User.Identity.SetCurrentOrgRegProgUserId(orgRegProgUserId);
        }

        public int? GetCurrentOrgRegProgUserId()
        {
            return HttpContext.Current != null ? HttpContext.Current.User.Identity.GetOrgRegProgUserId() : (int?)null;
        }

        //public void SetCurrentOrgRegProgramId(int orgRegProgramId);
        //public int? GetCurrentOrgRegProgramId();
        //public void SetCurrentOrganizationId(int organizationId);
        //public int? GetCurrentOrganizationId();

    }
}
