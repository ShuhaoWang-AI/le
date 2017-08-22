using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Dto
{
    public class RegistrationResultDto
    {
        #region public properties

        public IEnumerable<string> Errors { get; set; }
        public RegistrationResult Result { get; set; }
        public IEnumerable<AuthorityDto> RegulatoryList { get; set; }

        #endregion
    }
}