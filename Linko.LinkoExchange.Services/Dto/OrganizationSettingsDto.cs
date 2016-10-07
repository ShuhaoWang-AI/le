using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class OrganizationSettingsDto
    {
        public int MaxPasswordAttempts { get; set; }
        public int MaxKBQAttempts { get; set; }
        public int MaxFailedPasswordAttempts { get; set; }
        public int InvitationExpiryHours { get; set; }
        public int PasswordChangeDays { get; set; }
        public int PasswordHistoryCount { get; set; }
        public int MaxUserLicenseCount { get; set; }
        public int CurrentUserLicenseCount { get; set; }
        public int TimeZoneId { get; set; }
    }
}
