namespace Linko.LinkoExchange.Core.Extensions
{
    public static class StringExtension
    {
        public static string GetValueOrEmptyString(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value;
        }

        /// <summary>
        /// Nullifies an empty or whitespace string.
        /// </summary>
        /// <param name="value">string.</param>
        /// <returns>Returns the string value if it is not null or whitespace. Null, otherwise.</returns>
        public static string GetValueOrNull(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            else
            {
                return value;
            }
        }
    }
}
