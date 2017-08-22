using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Validation
{
    /// <summary>
    ///     When there is at least 1 rule violation, a RuleViolationException is raised.
    /// </summary>
    [Serializable]
    public class RuleViolationException : Exception
    {
        #region constructors and destructor

        public RuleViolationException(string message, List<RuleViolation> validationIssues) : base(message:message)
        {
            ValidationIssues = validationIssues;
        }

        #endregion

        #region public properties

        public List<RuleViolation> ValidationIssues { get; }

        #endregion
    }
}