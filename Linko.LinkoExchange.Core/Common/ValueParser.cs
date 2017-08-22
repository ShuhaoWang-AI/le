namespace Linko.LinkoExchange.Core.Common
{
    /// <summary>
    ///     Try to parse a according from an input string, if parsing fails, return the default value
    /// </summary>
    public static class ValueParser
    {
        /// <summary>
        ///     Tries the parse int.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <param name="defaultValue"> The default value. </param>
        /// <returns> </returns>
        public static int? TryParseInt(string value, int? defaultValue)
        {
            int result;
            return int.TryParse(s:value, result:out result) ? result : defaultValue;
        }

        /// <summary>
        ///     Tries the parse int.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <param name="defaultValue"> The default value. </param>
        /// <returns> </returns>
        public static int TryParseInt(string value, int defaultValue)
        {
            int result;
            return int.TryParse(s:value, result:out result) ? result : defaultValue;
        }

        /// <summary>
        ///     Tries the parse boolean.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <param name="defaultValue"> if set to <c> true </c> [default value]. </param>
        /// <returns> </returns>
        public static bool TryParseBoolean(string value, bool defaultValue)
        {
            bool result;
            return bool.TryParse(value:value, result:out result) ? result : defaultValue;
        }
    }
}