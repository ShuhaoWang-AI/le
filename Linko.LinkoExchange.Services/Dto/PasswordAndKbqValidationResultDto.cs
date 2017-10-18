using Linko.LinkoExchange.Core.Enum;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class PasswordAndKbqValidationResultDto
    {
        #region fields

        public PasswordAndKbqValidationResult PasswordAndKbqValidationResult;

        #endregion

        #region public properties

        public IEnumerable<AuthorityDto> RegulatoryList { get; set; }

        #endregion
    }
}
