namespace Linko.LinkoExchange.Core.Enum
{
    public enum EmailType
    {
        // UC-5.1 registration denied, for authority
        AuthorityRegistrationDenied,

        // UC-5.1 registration denied, for industry
        IndustryRegistrationDenied,

        // UC-5.1 registration approved, for industry
        IndustryRegistrationApproved,

        // UC-5.1 registration approved, for authority
        AuthorityRegistrationApproved,

        // UC-5.5 Lock/UnLock  (for authority user account / and IU User Account)
        AccountLockOut,

        // UC-5.5 Lock/UnLock (for Authority Sys Admin / IU Authority sys Admin
        LockOutToSysAdmins,

        // UC-5.6 UC-7.6 Reset Authority User Account/Reset IU User Account
        RegistratioinResetRequired,

        // UC-5.7  Authority Invites Authority User 
        InviteAuthorityUser,

        // UC-7.7  Authority Invites Industry User 
        InviteIndustryUser,

        // UC-7.8 Grant/Remove Signatory 
        SignatoryGranted,

        // UC-7.8 Grant/Revoke Signatory
        SignatoryRevoked,

        // UC-30 Manage My Profile-Profile LockOut
        KBQFailedLockOut,

        // UC-30 Manage My Profile-Profile Changed
        ProfileChanged,

        // UC-30 Manage My Profile-Email Changed
        ProfileEmailChanged,

        // UC-30 Manage My Profile-KBQs Changed
        KBQChanged,

        // UC-30 Manage My Profile-SQs Changed
        SecurityQuestionsChanged,

        // UC-30 Manage My Profile-Password Changed
        ProfilePasswordChanged,

        // UC-33 Forgot Password 
        ForgotPassword,

        // UC-35 Reset Profile from Account Reset
        ResetProfileRequired,

        // UC-42.2.a Registration From invitation -- Email to Registration Approvers, industry user
        IndustryUserRegistrationPendingToApprovers,

        // UC-42.2.a, Registration From invitation -- Email to Registration Approvers, authority user
        AuthorityUserRegistrationPendingToApprovers,

        //  UC-34 ForgotUserName
        ForgotUserName,

        // UC-35 RegistrationResetPending
        RegistrationResetPending
    }
}
