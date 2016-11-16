using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a type of a question.
    /// </summary>
    public partial class QuestionType
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int QuestionTypeId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }


        // Reverse navigation
        public virtual ICollection<Question> Questions { get; set; }
    }
}
