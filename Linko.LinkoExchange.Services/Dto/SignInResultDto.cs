using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Dto
{
    public class SignInResultDto
    {
        public string Token { get; set; }
        public AuthenticationResult AutehticationResult; 
    }
}
