using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Enum
{
    public enum SettingType
    {

        FailedPasswordAttemptMaxCount = 1,
        FailedKBQAttemptMaxCount = 2,
        InvitationExpiredHours = 3,
        PasswordChangeRequiredDays = 4,
        PasswordHistoryMaxCount = 5,
        AuthorityUserLicenseTotalCount = 6,
        AuthorityUserLicenseUsedCount = 7,
        TimeZone = 8,
        ReportRepudiatedDays = 9,
        MassLoadingConversionFactorPounds = 10,
        MassLoadingResultToUseLessThanSign = 11,
        MassLoadingCalculationDecimalPlaces = 12,
        EmailContactInfoName = 13,
        EmailContactInfoPhone = 14,
        EmailContactInfoEmailAddress = 15,
        IndustryUserLicenseTotalCount = 16,
        IndustryUserLicenseUsedCount = 17,
        UserPerIndustryMaxCount = 18


        //TimeZoneId = 9,
        
        //UserLockoutEnabledByDefault = 14,
        //DefaultAccountLockoutTimeSpan = 15,

        //PasswordExpiredDays = 16,
        //DaysBeforeRequirePasswordChanging = 17,
        //EmailServer = 18,  
    }

}
