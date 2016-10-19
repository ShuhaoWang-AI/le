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
        PasswordLockedOut,
        PasswordExpired,
        UserAlreadyExists,
        CanNotUseOldPassword,
        InvalidateRegistrationToken
    }
}
