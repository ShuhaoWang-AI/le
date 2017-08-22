namespace Linko.LinkoExchange.Core.Validation
{
    /// <summary>
    ///     Captures information about a business rule that is being violated.
    ///     Typical usage:
    ///     A collection of RuleViolations is used to represent all of the validation errors that result when submitting a
    ///     form.
    /// </summary>
    public class RuleViolation
    {
        #region constructors and destructor

        public RuleViolation(string propertyName, object propertyValue, string errorMessage)
        {
            PropertyName = propertyName;
            PropertyValue = propertyValue;
            ErrorMessage = errorMessage;
        }

        #endregion

        #region properties

        public string PropertyName { get; private set; }

        public object PropertyValue { get; private set; }

        public string ErrorMessage { get; private set; }

        #endregion
    }
}