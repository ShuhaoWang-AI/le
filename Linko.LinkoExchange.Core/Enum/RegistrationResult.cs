namespace Linko.LinkoExchange.Core.Enum
{
    public enum RegistrationResult
    {
        Success,
        Failed,
        InvitationExpired,
        NotAgreedTermsAndConditions,
        InvalidateRegistrationToken,
        UserAlreadyInTheProgram,
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