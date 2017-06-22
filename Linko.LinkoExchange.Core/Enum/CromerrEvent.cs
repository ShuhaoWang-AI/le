﻿namespace Linko.LinkoExchange.Core.Enum
{
    public enum CromerrEvent
    {
        Registration_InviteSent,
        Registration_InviteDeleted,
        Registration_RegistrationPending,
        Registration_RegistrationApproved,
        Registration_RegistrationDenied,
        UserAccess_Disabled,
        UserAccess_Enabled,
        UserAccess_Removed,
        UserAccess_RoleChange,
        UserAccess_ManualAccountLock,
        UserAccess_ManualAccountUnlock,
        UserAccess_AccountResetInitiated,
        UserAccess_AccountResetExpired,
        UserAccess_AccountResetSuccessful,
        Login_Success,
        Login_PasswordLockout,
        Login_AccountLocked,
        Login_AccountResetRequired,
        Login_UserDisabled,
        Login_RegistrationPending,
        Login_NoAssociation,
        ForgotPassword_Success,
        ForgotPassword_PasswordResetExpired,
        ForgotPassword_AccountLocked,
        Profile_AccountLocked,
        Profile_PasswordChanged,
        Profile_EmailChanged,
        Profile_KBQChanged,
        Profile_SQChanged,
        Signature_IdentityProofed,
        Signature_SignatoryPending,
        Signature_SignatoryGranted,
        Signature_SignatoryRevoked,
        Signature_SignFailed,
        Signature_AccountLockedPassword,
        Signature_AccountLockedKBQ,
        Report_Submitted,
        Report_Repudiated

    }
}
