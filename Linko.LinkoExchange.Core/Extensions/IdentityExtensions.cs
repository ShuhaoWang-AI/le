﻿using System.Security.Claims;
using System.Security.Principal;

namespace Linko.LinkoExchange.Core.Extensions
{
    public static class IdentityExtensions
    {
        public static string UserProfileId(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity) identity).FindFirst(type:"UserProfileId");

            // Test for null to avoid issues during local testing
            return claim != null ? claim.Value : string.Empty;
        }

        public static string UserFullName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity) identity).FindFirst(type:"UserFullName");

            // Test for null to avoid issues during local testing
            return claim != null ? claim.Value : string.Empty;
        }
    }
}