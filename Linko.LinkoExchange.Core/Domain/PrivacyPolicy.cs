using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a PrivacyPolicy
    /// </summary>
    public class PrivacyPolicy
    {
        #region public properties

        /// <summary>
        ///     Primary key
        /// </summary>
        public int PrivacyPolicyId { get; set; }

        public string Content { get; set; }
        public DateTimeOffset EffectiveDateTimeUtc { get; set; }
        public DateTimeOffset CreationDateTimeUtc { get; set; }
        public int? LastModifierUserId { get; set; }

        #endregion
    }
}