namespace Linko.LinkoExchange.Services.Dto
{
    public enum SettingType
    {
        MaxPasswordAttempts = 1,
        MaxKBQAttempts = 2,
        MaxFailedPasswordAttempts = 3,
        InvitationExpiryHours = 4,
        PasswordChangeDays = 5,
        PasswordHistoryCount = 6,
        MaxUserLicenseCount = 7,
        CurrentUserLicenseCount = 8,
        TimeZoneId = 9,
        PasswordRequireLength = 10,
        PasswordRequireDigit = 11,
        PasswordRequireLowerCase = 12,
        PasswordRequireUpperCase = 13,
        UserLockoutEnabledByDefault = 14,
        DefaultAccountLockoutTimeSpan = 15,
        MaxFailedAccessAttemptsBeforeLockout = 16,
        PasswordExpiredDays = 17,
        DaysBeforeRequirePasswordChanging = 18,
        NumberOfPasswordsInHistory = 19
    }

    public class SettingDto
    {
        public SettingType Type { get; set; }
        public string Value { get; set; }
    }
}
