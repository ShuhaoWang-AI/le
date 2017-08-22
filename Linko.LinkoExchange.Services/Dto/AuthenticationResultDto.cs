using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Dto
{
    public class AuthenticationResultDto
    {
        #region constructors and destructor

        public AuthenticationResultDto()
        {
            Success = true;
            Errors = null;
        }

        #endregion

        #region public properties

        public bool Success { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public AuthenticationResult Result { get; set; }
        public IEnumerable<AuthorityDto> RegulatoryList { get; set; }

        #endregion
    }
}