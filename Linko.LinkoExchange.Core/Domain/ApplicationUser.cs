using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;

namespace Linko.LinkoExchange.Core.Domain
{
    // This class is created by Owin 

    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync (this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
//            userIdentity.AddClaim(new Claim("OrganizationRegulatoryProgramUserId", this.OrganizationRegulatoryProgramUserId.ToString()));

            return userIdentity;
        }

        //        public int OrganizationRegulatoryProgramUserId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserProfileId
        {
            get; private set;
        }

        public string Email { get; set; }

        public bool IsAccountLocked { get; set; }
        public bool IsAccountResetRequired { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
