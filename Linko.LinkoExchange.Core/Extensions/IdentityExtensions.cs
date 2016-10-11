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
        public static int GetOrganizationRegulatoryProgramUserId(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity) identity).FindFirst("OrganizationRegulatoryProgramUserId");
            // Test for null to avoid issues during local testing
            return (claim != null) ? Convert.ToInt32(claim.Value) : -1;
        }
    }
}
