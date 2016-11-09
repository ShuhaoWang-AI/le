namespace Linko.LinkoExchange.Core.Enum
{
    public enum AuthenticationResult
    {
        Success,
        Failed,
        PasswordRequirementsNotMet,
        InvalidUserNameOrPassword,
        UserIsLocked,
        UserIsDisabled,
        UserNotFound,
        EmailIsNotConfirmed,
        RegistrationApprovalPending,
        PasswordLockedOut,
        PasswordExpired,
        UserAlreadyExists,
        CanNotUseOldPassword, 
        ExpiredRegistrationToken,
        AccountIsNotAssociated,
        IncorrectAnswerToQuestion,
        AccountResetRequired
    } 
}
