using Linko.LinkoExchange.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
