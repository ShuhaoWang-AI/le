namespace Linko.LinkoExchange.Services.Dto
{
    public enum SettingType
    {
        FailedPasswordAttemptMaxCount = 1,
        FailedKBQAttemptMaxCount = 2, 
        InvitationExpiredHours = 3,
        PasswordChangeRequiredDays = 4,
        PasswordHistoryMaxCount = 6,

        AuthorityUserLicenseTotalCount = 6,
        IndustryUserLicenseTotalCount = 16, 
        IndustryUserLicenseUsedCount = 17,

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
        EmailContactInfoEmailAddress = 15, 
        AuthorityUserLicenseUsedCount,
             
        TimeZone, 
        MassLoadingResultToUseLessThanSign,
        MassLoadingCalculationDecimalPlaces, 
        UserPerIndustryMaxCount

    }

    public class SettingDto
    {
        public SettingType Type { get; set; }
        public string Value { get; set; }
    }
}
