namespace Linko.LinkoExchange.Core.Enum
{
    public enum EventCategory
    {
        ForgotPassword,
        ForgotUserName,
        Profile,
        Registration,
        Signature,
        UserAccess
    }

    public enum EventType
    {
        AccountLockOut,
        AuthorityRegistrationApproved,
        AuthorityRegistrationDenied,
        AuthorityUserRegistrationPendingToApprovers,
        EmailChanged,
        ForgotPassword,
        ForgotUserName,
        IndustryRegistrationApproved,
        IndustryRegistrationDenied,
        IndustryUserRegistrationPendingToApprovers,
        InviteAuthorityUser,
        InviteIndustryUser,
        KBQChanged,
        KBQFailedLockOut,
        LockOutToSysAdmins,
        PasswordChanged,
        ProfileChanged,
        RegistrationResetPending,
        ResetProfileRequired,
        ResetRequired,
        SecurityQuestionsChanged,
        SignatoryGranted,
        SignatoryRevoked,
    }
}
