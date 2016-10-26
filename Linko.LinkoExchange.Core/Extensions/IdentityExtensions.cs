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
        public static string UserProfileId(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("UserProfileId");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string UserFullName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("UserFullName");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
    }
}
