namespace Linko.LinkoExchange.Core.Extensions
{
    public static class StringExtension
    {
        /// <summary>
        ///     Gets the value or empty string.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> </returns>
        public static string GetValueOrEmptyString(this string value)
        {
            return string.IsNullOrWhiteSpace(value:value) ? string.Empty : value;
        }

        /// <summary>
        ///     Nullifies an empty or whitespace string.
        /// </summary>
        /// <param name="value"> string. </param>
        /// <returns> Returns the string value if it is not null or whitespace. Null, otherwise. </returns>
        public static string GetValueOrNull(this string value)
        {
            return string.IsNullOrWhiteSpace(value:value) ? null : value;
        }
    }
}