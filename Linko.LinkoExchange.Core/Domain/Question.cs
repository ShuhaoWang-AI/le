using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a possible question to ask the users.
    /// </summary>
    public partial class Question
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int QuestionId { get; set; }

        public string Content { get; set; }

        public int QuestionTypeId { get; set; }
        public virtual QuestionType QuestionType { get; set; }

        /// <summary>
        /// True: the question is displayed in the list. 
        /// Default: true.
        /// </summary>
        public bool IsActive { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }
}