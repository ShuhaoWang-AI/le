using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a password change history for a particular user.
    /// </summary>
    public class UserPasswordHistory
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int UserPasswordHistoryId { get; set; }

        public string PasswordHash { get; set; }

        public int UserProfileId { get; set; }

        //public UserProfile UserProfile { get; set; }

        public DateTimeOffset LastModificationDateTimeUtc { get; set; }

        #endregion
    }
}