using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public enum AccountLockoutFailureReason
    {
        CannotLockoutSupportRole,
        CannotLockoutOwnAccount
    }

    public class AccountLockoutResultDto
    {
        public bool IsSuccess { get; set; }
        public AccountLockoutFailureReason FailureReason { get; set; }
    }
}
