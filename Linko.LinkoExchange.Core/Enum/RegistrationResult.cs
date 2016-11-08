namespace Linko.LinkoExchange.Core.Enum
{
    public enum RegistrationResult
    {
        Success,
        Failed,
        InvitationExpired,
        NotAgreedTermsAndConditions,
        InvalidRegistrationToken,
        CanNotUseLastNumberOfPasswords,
        ProgramNotFound,
        ProgramDisabled,
        BadUserProfileData,
        BadPassword,
        BadSecurityQuestionAndAnswer,
        BadKBQAndAnswer, 
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