using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public enum EnableOrganizationFailureReason
    {
        TooManyIndustriesForAuthority,
        TooManyUsersForThisIndustry
    }

    public class EnableOrganizationResultDto
    {
        public bool IsSuccess { get; set; }
        public EnableOrganizationFailureReason FailureReason { get; set; }
    }
}
