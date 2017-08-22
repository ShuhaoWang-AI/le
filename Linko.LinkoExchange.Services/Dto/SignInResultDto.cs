using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Dto
{
    public class SignInResultDto
    {
        #region fields

        public AuthenticationResult AutehticationResult;

        #endregion

        #region public properties

        public string OwinUserId { get; set; }
        public int UserProfileId { get; set; }
        public string Token { get; set; }
        public IEnumerable<AuthorityDto> RegulatoryList { get; set; }

        #endregion
    }
}