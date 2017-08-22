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
        #region public properties

        public bool IsSuccess { get; set; }
        public ResetUserFailureReason FailureReason { get; set; }

        #endregion
    }
}