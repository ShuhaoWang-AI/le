using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Linko.LinkoExchange.Core.Validation;

namespace Linko.LinkoExchange.Web.Extensions
{
    public static class MvcValidationExtensions
    {
        /// <summary>
        /// Copies all of the rule violations to a controller class's ModelState.
        /// </summary>
        /// <param name="ruleViolationException"> </param>
        /// <param name="modelState"> </param>
        public static void UpdateModelStateWithViolations(RuleViolationException ruleViolationException, ModelStateDictionary modelState)
        {
            foreach (var issue in ruleViolationException.ValidationIssues)
            {
                if (DoesModelStateContainSamePropertyErrorMessage(modelState:modelState, issue:issue))
                {
                    continue;
                }

                //var value = issue.PropertyValue ?? string.Empty;
                modelState.AddModelError(key:issue.PropertyName, errorMessage:issue.ErrorMessage);
            }
        }

        private static bool DoesModelStateContainSamePropertyErrorMessage(ModelStateDictionary modelState, RuleViolation issue)
        {
            ModelState value;
            return modelState.TryGetValue(key:issue.PropertyName, value:out value) && 
                   value.Errors.ToList().Any(e => issue.ErrorMessage.Equals(value:e.ErrorMessage, comparisonType:StringComparison.Ordinal));
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

        public static List<string> GetViolationErrors(RuleViolationException ruleViolationException)
        {
            var errors = new List<string>();

            foreach (var issue in ruleViolationException.ValidationIssues)
            {
                errors.Add(item:issue.ErrorMessage);
            }

            return errors;
        }
    }
}