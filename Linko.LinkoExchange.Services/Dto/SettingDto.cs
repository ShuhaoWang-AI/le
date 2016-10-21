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

        PasswordExpiredDays = 16,
        DaysBeforeRequirePasswordChanging = 17, 
        EmailServer = 18,
        SupportPhoneNumber = 19,
        SupportEmail = 20,
        SystemEmailEmailAddress = 21,
        SystemEmailFirstName = 22,
        SystemEmailLastName = 23
    }

    public class SettingDto
    {
        public SettingType Type { get; set; }
        public string Value { get; set; }
    }
}
