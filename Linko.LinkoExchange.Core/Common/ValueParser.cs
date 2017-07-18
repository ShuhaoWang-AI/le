namespace Linko.LinkoExchange.Core.Common
{
    /// <summary>
    /// Try to parse a according from an input string, if parsing fails, return the default value
    /// </summary>
    public class ValueParser
    {
        public static int? TryParseInt(string value, int? defaultValue)
        {
            int result;
            return int.TryParse(value, out result) ? result : defaultValue;
        }

        public static int TryParseInt(string value, int defaultValue)
        {
            int result;
            return int.TryParse(value, out result) ? result : defaultValue;
        }

        public static bool TryParseBoolean(string value, bool defaultValue)
        {
            bool result;
            return bool.TryParse(value, out result) ? result : defaultValue;
        } 
    }
}
