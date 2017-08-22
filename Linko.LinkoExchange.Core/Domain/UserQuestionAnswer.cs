using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents an answer entered by a user for a particular question.
    /// </summary>
    public class UserQuestionAnswer
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int UserQuestionAnswerId { get; set; }

        public string Content { get; set; }

        public int QuestionId { get; set; }
        public virtual Question Question { get; set; }

        public int UserProfileId { get; set; }

        //public UserProfile UserProfile { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        #endregion
    }
}