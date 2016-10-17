using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class OrganizationSettingsDto
    {
        public SettingsTemplate SettingName { get; set; }
        public string SettingValue { get; set; }
    }
}
