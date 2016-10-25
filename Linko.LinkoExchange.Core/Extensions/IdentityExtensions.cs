using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Extensions
{
    public static class IdentityExtensions
    {
        public static void SetCurrentOrgRegProgUserId(this IIdentity identity, int orgRegProgUserId)
        {
            ((ClaimsIdentity)identity).AddClaim(new Claim("OrgRegProgUserId", orgRegProgUserId.ToString()));
        }

        public static int GetOrgRegProgUserId(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity) identity).FindFirst("OrgRegProgUserId");
            // Test for null to avoid issues during local testing
            return (claim != null) ? Convert.ToInt32(claim.Value) : -1;
        }
    }
}
