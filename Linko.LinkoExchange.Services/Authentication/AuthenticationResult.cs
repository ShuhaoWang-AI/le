using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Authentication
{
    public class AuthenticationResult
    {
        public bool Success
        {
            get; set;
        }
        public IEnumerable<string> Errors
        {
            get; set;
        }
    }
}