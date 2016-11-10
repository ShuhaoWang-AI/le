using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class UserDto
    { 
        public int UserProfileId { get; set; }

        public string TitleRole { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string BusinessName { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityName { get; set; }
        public string ZipCode { get; set; }
        public int JurisdictionId { get; set; } 

        public int? PhoneExt { get; set; }
        public bool IsAccountLocked { get; set; }
        public bool IsAccountResetRequired { get; set; }
        public bool IsIdentityProofed { get; set; }
        public bool IsInternalAccount { get; set; }
        public DateTimeOffset? CreationDateTimeUtc { get; set; }
        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }
        
        public string Email { get; set; }
        public string OldEmailAddress { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; } 
        public string PhoneNumber { get; set; } 
        public DateTime LockoutEndDateUtc { get; set; } 
        public bool LockoutEnabled { get; set; }

        public string Password { get; set; }
        public bool AgreeTermsAndConditions { get; set; }
    }
}
