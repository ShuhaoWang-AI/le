using System;
using System.Collections.Generic;
using System.Linq;

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

        public RuleViolationException(string message, params RuleViolation[] validationIssues) 
            : this(message:message, validationIssues:validationIssues.ToList())
        {
        }

        #endregion

        #region public properties

        public List<RuleViolation> ValidationIssues { get; }

        #endregion

        public string GetFirstErrorMessage()
        {
            return ValidationIssues[0].ErrorMessage;
        }
    }
}