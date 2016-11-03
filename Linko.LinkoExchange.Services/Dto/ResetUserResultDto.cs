using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public enum ResetUserFailureReason
    {
        CannotResetSupportRole,
        CannotResetOwnAccount,
        NewEmailAddressAlreadyInUse
    }

    public class ResetUserResultDto
    {
        public bool IsSuccess { get; set; }
        public ResetUserFailureReason FailureReason { get; set; }
    }
}
