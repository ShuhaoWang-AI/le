using System;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNet.Identity;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a merged entity between .NET identity and application.
    /// There is currently no alternate key support in EF 6.1.3 that will enable any navigation properties to this entity.
    /// </summary>
    public partial class UserProfile : IdentityUser
    {
        /// <summary>
        /// Identity column. Alternate key.
        /// </summary>
        public int UserProfileId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string TitleRole { get; set; }

        public string BusinessName { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string CityName { get; set; }

        public string ZipCode { get; set; }

        public int? JurisdictionId { get; set; }
        public virtual Jurisdiction Jurisdiction { get; set; }

        public int? PhoneExt { get; set; }

        public bool IsAccountLocked { get; set; }

        public bool IsAccountResetRequired { get; set; }

        public bool IsIdentityProofed { get; set; }

        public bool IsInternalAccount { get; set; }

        public string OldEmailAddress { get; set; }

        public int TermConditionId { get; set; }

        public DateTimeOffset TermConditionAgreedDateTimeUtc { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        // Reverse navigation
        //public virtual ICollection<UserPasswordHistory> UserPasswordHistories { get; set; }
        //public virtual ICollection<UserQuestionAnswer> UserQuestionAnswers { get; set; }


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<UserProfile> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here
            userIdentity.AddClaim(new Claim("UserFullName", this.FirstName + " " + this.LastName));
            userIdentity.AddClaim(new Claim("UserProfileId", this.UserProfileId.ToString()));
            userIdentity.AddClaim(new Claim("UserName", this.UserName));
            userIdentity.AddClaim(new Claim("Email", this.Email));

            return userIdentity;
        }

        // IdentityUser properties

        /// <summary>
        /// Primary key.
        /// </summary>
        //public string Id { get; set; }

        //public string Email { get; set; }

        /// <summary>
        /// True: email address has been confirmed by the user. Default: True.
        /// </summary>
        //public bool EmailConfirmed { get; set; }

        //public string PasswordHash { get; set; }

        //public string SecurityStamp { get; set; }

        //public string PhoneNumber { get; set; }

        //public bool PhoneNumberConfirmed { get; set; }

        //public bool TwoFactorEnabled { get; set; }

        /// <summary>
        ///  Used to lock out the user until such period has passed or the user reset the password.
        /// </summary>
        //public DateTime? LockoutEndDateUtc { get; set; }

        /// <summary>
        /// True: the user will be locked out after the pre-determined nth attempt. Default: False.
        /// </summary>
        //public bool LockoutEnabled { get; set; }

        /// <summary>
        /// .NET Identity. An unsuccessful login attempt with the associated email address will increase the counter. 
        /// After n failed attempts "IsLocked" is set. The count will be reset to 0 after a successful login.
        /// </summary>
        //public int AccessFailedCount { get; set; }

        //public string UserName { get; set; }
    }
}