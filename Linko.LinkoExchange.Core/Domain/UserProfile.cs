using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Core.Domain
{
    public class UserProfile
    {
        [Key]
        public int UserProfileId { get; private set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PasswordHash { get; set; }
        public bool IsAccountLocked { get; set; }
        public object OldEmailAddress { get; set; }
        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsPhoneNumberConfirmed { get; set; }
    }
}
