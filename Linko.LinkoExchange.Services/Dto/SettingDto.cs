namespace Linko.LinkoExchange.Services.Dto
{
    public enum SettingName
    {
        MaxPasswordAttempts = 1,
        MaxKBQAttempts = 2,
        MaxFailedPasswordAttempts = 3,
        InvitationExpiryHours = 4,
        PasswordChangeDays = 5,
        PasswordHistoryCount = 6,
        MaxUserLicenseCount = 7,
        CurrentUserLicenseCount = 8,
        TimeZoneId = 9
    }

    public class SettingDto
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
