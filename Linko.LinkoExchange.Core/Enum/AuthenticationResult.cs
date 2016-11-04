﻿namespace Linko.LinkoExchange.Core.Enum
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

    public enum RegistrationResult
    {
        Success,
        Failed, 
        InvitationExpired,
        ProgramNotFound, 
        ProgramDisabled,
        BadUserProfileData,
        BadPassword,
        BadSecurityQuestionAndAnswer,
        BadKBQAndAnswer,
        NotAgreedTermsAndConditions,
        InvalidateRegistrationToken,
        UserAlreadyInTheProgram,
        MissingKBQAnswer,
        MissingSecurityQuestionAnswer,
        DuplicatedKBQAnswer,
        DuplicatedSecurityQuestionAnswer,
        DuplicatedKBQ,
        DuplicatedSecurityQuestion,
        MissingKBQ,
        MissingSecurityQuestion
    }
}
