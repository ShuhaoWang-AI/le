using System;
using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Core.Domain
{
    public class UserPasswordHistory
    {
        [Key]
        public int UserPasswordHistoryId { get; set; }
        public string PasswordHash  { get; set; }
        public int UserProfileId { get; set; }
        public DateTimeOffset LastModificationDateTimeUtc { get; set; }
    }
}
