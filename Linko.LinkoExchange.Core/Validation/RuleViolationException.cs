using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Validation
{
    /// <summary>
    /// When there is at least 1 rule violation, a RuleViolationException is raised.
    /// </summary>
    [Serializable]
    public class RuleViolationException : Exception
    {
        public List<RuleViolation> ValidationIssues
        {
            get; private set;
        }

        public RuleViolationException(string message, List<RuleViolation> validationIssues) : base(message)
        {
            ValidationIssues = validationIssues;
        }
    }
}
