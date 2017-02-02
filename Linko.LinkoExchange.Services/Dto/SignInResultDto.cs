using Linko.LinkoExchange.Core.Enum;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class SignInResultDto
    {
        public string OwinUserId { get;set;}
        public int UserProfileId { get;set;}
        public string Token { get; set; }
        public AuthenticationResult AutehticationResult;
        public IEnumerable<AuthorityDto> RegulatoryList { get; set; } 
    }
}
