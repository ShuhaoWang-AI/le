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
    }
}
