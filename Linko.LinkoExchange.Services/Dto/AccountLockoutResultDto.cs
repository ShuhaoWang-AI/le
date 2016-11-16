
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
