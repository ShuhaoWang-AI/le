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

        AuthorityUserLicenseTotalCount = 6, //need to sync with DB
        IndustryUserLicenseTotalCount = 16, //need to sync with DB
        IndustryLicenseTotalCount = -1, //need to sync with DB
        //CurrentUserLicenseCount = 8,

        TimeZoneId = 9,
        ReportRepudiatedDays = 9,
        PasswordRequireLength = 10,
        MassLoadingConversionFactorPounds = 10,
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
        SystemEmailLastName = 23,

        EmailContactInfoName = 13,
        EmailContactInfoPhone = 14,
        EmailContactInfoEmailAddress = 15
    }

    public class SettingDto
    {
        public SettingType Type { get; set; }
        public string Value { get; set; }
    }
}
