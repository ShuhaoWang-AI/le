using Linko.LinkoExchange.Services.Enum;

namespace Linko.LinkoExchange.Services.Dto
{
    public class SignInResult
    {
        public string Token { get; set; }
        public AuthenticationResult AutehticationResult;
    }
}
