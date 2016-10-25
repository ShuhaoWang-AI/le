using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Linko.LinkoExchange.Core.Domain
{
    public class UserProfile : IdentityUser
    {  
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserProfileId { get; private set; }

        public string FirstName { get; set; }
        public string LastName { get; set; } 
        public string BusinessName { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityName { get; set; }
        public string ZipCode { get; set; }
        public string Jurisdictionid { get; set; }

        public int? PhoneExt { get; set; }
        public bool IsAccountLocked { get; set; }
        public bool IsAccountResetRequired { get; set; }
        public bool IsIdentityProofed { get; set; }
        public bool IsInternalAccount { get; set; }
        public DateTime? LastModificationDateTimeUtc { get; set; }

        public string OldEmailAddress { get; set; }  

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<UserProfile> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            //            userIdentity.AddClaim(new Claim("OrganizationRegulatoryProgramUserId", this.OrganizationRegulatoryProgramUserId.ToString()));

            return userIdentity;
        }

        //Extended Properties
        public int? CurrentOrgRegProgUserId { get; set; }
        public int? CurrentOrgRegProgramId { get; set; }
        public int? CurrentOrganizationId { get; set; }

    }
}
