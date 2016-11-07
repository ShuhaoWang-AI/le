using System.Collections.Generic; 

namespace Linko.LinkoExchange.Services.Dto
{
    public enum SettingsTemplate
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

    public class OrganizationSettingDto
    {
        public ICollection<SettingDto> Settings { get; set; }
        public int OrganizationId { get; set; }
    }
}
