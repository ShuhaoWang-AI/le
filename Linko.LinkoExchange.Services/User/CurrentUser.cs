using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Extensions;
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
        private readonly ApplicationUserManager _userManager;
        private readonly ISessionCache _sessionCache;

        public CurrentUser(ApplicationUserManager userManage, ISessionCache sessionCache)
        {
            _userManager = userManage;
            _sessionCache = sessionCache;
        }

        public string GetClaimsValue(CurrentUserInfo claimType)
        {
            string claimsValue = null;
           
            var claims = _sessionCache.GetValue("claims") as IEnumerable<Claim>;
            if (claims == null)
            {
                var userId = _sessionCache.GetValue("userId") as string;
                claims = string.IsNullOrWhiteSpace(userId) ? null : _userManager.GetClaims(userId);
                _sessionCache.SetValue("claims", claims);
            }

            //return claims?.ToList();
            if (claims != null)
            {
                string typeString = claimType.ToString();
                var claim = claims.FirstOrDefault(c => c.Type == typeString);
                if (claim != null)
                    claimsValue = claim.Value;
            }

            return claimsValue;
        }

        public void SetClaimsValue(CurrentUserInfo claimType, string value)
        {
            //TODO
        }

        public void SetClaimsForOrgRegProgramSelection(int orgRegProgId)
        {
            //TODO
        }

    }
}
