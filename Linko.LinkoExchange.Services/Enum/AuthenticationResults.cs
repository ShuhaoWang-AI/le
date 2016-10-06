using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Enum
{
    public enum AuthenticationResults  
    {
        Success,
        InvalidUserNameOrPassword,
        PasswordLockedOut,
        AccountLockedOut,
        UserIsDisabled,
        UserNotFound,
        UserNameExits,
        EmailAddressExits,
        EmailNotConfirmed
    }
}
