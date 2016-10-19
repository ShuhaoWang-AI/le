namespace Linko.LinkoExchange.Core.Enum
{
    public enum EmailType
    {
        //TODO to remove
        RegistrationConfirmation,
        ResetPasswordConfirmation,

        // UC-5.1 registration denied, for authority
        Registration_AuthorityRegistrationDenied,

        // UC-5.1 registration denied, for industry
        Registration_IndustryRegistrationDenied,

        // UC-5.1 registration approved, for industry
        Registration_IndustryRegistrationApproved,

        // UC-5.1 registration approved, for authority
        Registration_AuthorityRegistrationApproved,

        // UC-5.5 Lock/UnLock  (for authority user account / and IU User Account)
        UserAccess_AccountLockOut,

        // UC-5.5 Lock/UnLock (for Authority Sys Admin / IU Authority sys Admin
        UserAccess_LockOutToSysAdmins,

        // UC-5.6 UC-7.6 Reset Authority User Account/Reset IU User Account
        Registration_RegistratioinResetRequired,

        // UC-5.7  Authority Invites Authority User 
        Registration_InviteAuthorityUser,

        // UC-7.7  Authority Invites Industry User
        Registration_AuthorityInviteIndustryUser,

        // UC-7.8 Grant/Remove Signatory 
        Signature_SignatoryGranted,

        // UC-7.8 Grant/Revoke Signatory
        Signature_SignatoryRevoked,

        // UC-30 Manage My Profile-Profile LockOut
        Profile_KBQFailedLockOut,

        // UC-30 Manage My Profile-Profile Changed
        Profile_ProfileChanged,

        // UC-30 Manage My Profile-Email Changed
        Profile_ProfileEmailChanged,

        // UC-30 Manage My Profile-KBQs Changed
        Profile_KBQChanged,

        // UC-30 Manage My Profile-SQs Changed
        Profile_SecurityQuestionsChanged,

        // UC-30 Manage My Profile-Password Changed
        Profile_ProfilePasswordChanged,

        // UC-33 Forgot Password 
        ForgotPassword_ForgotPassword,

        // UC-35 Reset Profile from Account Reset
        Profile_ResetProfileRequired,

        // UC-42.2.a Registration From invitation -- Email to Registration Approvers, industry user
        Registration_IndustryUserRegistrationPendingToApprovers,

        // UC-42.2.a, Registration From invitation -- Email to Registration Approvers, authority user
        Registration_AuthorityUserRegistrationPendingToApprovers,

        //  UC-34 ForgotUserName
        ForgotUserName_ForgotUserName,

        // UC-35 RegistrationResetPending
        Registration_RegistrationResetPending,

        // UC-5.7 Industry invites industry user 
        Registration_IndustryInviteIndustryUser
    }
}
