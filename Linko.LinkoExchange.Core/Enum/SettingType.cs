using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Enum
{
    public enum SettingType
    {
        FailedPasswordAttemptMaxCount,
        FailedKBQAttemptMaxCount,
        InvitationExpiredHours,
        PasswordChangeRequiredDays,
        PasswordHistoryMaxCount,
        TimeZone,
        ReportRepudiatedDays,
        MassLoadingConversionFactorPounds,
        MassLoadingResultToUseLessThanSign,
        MassLoadingCalculationDecimalPlaces,
        EmailContactInfoName,
        EmailContactInfoPhone,
        EmailContactInfoEmailAddress,
        AuthorityUserLicenseTotalCount,
        IndustryLicenseTotalCount,
        UserPerIndustryMaxCount,
        ResultQualifiersIndustryCanUse,
        CreateSampleNamesRule
    }

}
