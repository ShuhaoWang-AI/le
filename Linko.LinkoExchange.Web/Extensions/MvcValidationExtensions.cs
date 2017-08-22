using System.Web.Mvc;
using Linko.LinkoExchange.Core.Validation;

namespace Linko.LinkoExchange.Web.Extensions
{
    public static class MvcValidationExtensions
    {
        /// <summary>
        ///     Copies all of the rule violations to a controller class's ModelState.
        /// </summary>
        /// <param name="ruleViolationException"> </param>
        /// <param name="modelState"> </param>
        public static void UpdateModelStateWithViolations(RuleViolationException ruleViolationException, ModelStateDictionary modelState)
        {
            foreach (var issue in ruleViolationException.ValidationIssues)
            {
                //var value = issue.PropertyValue ?? string.Empty;
                modelState.AddModelError(key:issue.PropertyName, errorMessage:issue.ErrorMessage);
            }
        }

        public static string GetViolationMessages(RuleViolationException ruleViolationException)
        {
            var errorMsgs = "";

            foreach (var issue in ruleViolationException.ValidationIssues)
            {
                errorMsgs = string.Format(format:"{0} \n {1}", arg0:errorMsgs, arg1:issue.ErrorMessage);
            }

            return errorMsgs;
        }
    }
}