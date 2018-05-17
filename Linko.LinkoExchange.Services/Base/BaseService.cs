using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Linko.LinkoExchange.Core.Validation;

namespace Linko.LinkoExchange.Services.Base
{
    /// <summary>
    ///     All service classes must inherit from this base class.
    ///     Currently only used to implement function-level authorization.
    /// </summary>
    public abstract class BaseService
    {
        /// <summary>
        ///     Returns a boolean value indicating authorization to execute a method.
        ///     This should be the first thing called upon entering a method.
        ///     Returning false should typically trigger an UnauthorizedAccess exception
        /// </summary>
        /// <param name="apiName"> Method name (automatically detected without explicit parameter) </param>
        /// <param name="id"> Method specific "target" id of the object which the user is acting upon </param>
        /// <returns> </returns>
        public abstract bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id);

        protected static RuleViolationException CreateRuleViolationExceptionForValidationError(string errorMessage)
        {
            var validationIssues = new List<RuleViolation> { new RuleViolation(propertyName: string.Empty, propertyValue: null, errorMessage: errorMessage) };

            return new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
        }
    }
}