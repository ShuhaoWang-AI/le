namespace Linko.LinkoExchange.Services.Enum
{
    public enum AuthenticationResult
    {
        Success,
        Failed,
        InvalidUserNameOrPassword,
        UserIsLocked,
        UserIsDisabled,
        UserNotFound,
        UserAlreadyExists
    }
}
