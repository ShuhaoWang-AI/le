namespace Linko.LinkoExchange.Services.Dto
{
    public enum AccountLockoutFailureReason
    {
        CannotLockoutSupportRole,
        CannotLockoutOwnAccount
    }

    public class AccountLockoutResultDto
    {
        #region public properties

        public bool IsSuccess { get; set; }
        public AccountLockoutFailureReason FailureReason { get; set; }

        #endregion
    }
}