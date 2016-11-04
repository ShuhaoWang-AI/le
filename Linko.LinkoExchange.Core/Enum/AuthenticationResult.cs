namespace Linko.LinkoExchange.Core.Enum
{
    public enum AuthenticationResult
    {
        Success,
        Failed,
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
