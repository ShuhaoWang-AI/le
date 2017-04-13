using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.TimeZone
{
    public interface ITimeZoneService
    {
        string GetTimeZoneName(int timeZoneId);
        ICollection<TimeZoneDto> GetTimeZones();
        DateTime GetLocalizedDateTimeUsingSettingForThisOrg(DateTime utcDateTime, int orgId, int regProgramId);

        DateTime GetLocalizedDateTimeUsingSettingForThisOrg(DateTime utcDateTime, int orgRegProgramId);

        DateTime GetLocalizedDateTimeUsingThisTimeZoneId(DateTime utcDateTime, int timeZoneId);

        DateTimeOffset GetUTCDateTimeUsingSettingForThisOrg(DateTime localDateTime, int orgRegProgramId);

        DateTimeOffset GetUTCDateTimeUsingThisTimeZoneId(DateTime localDateTime, int timeZoneId);
        string GetTimeZoneNameUsingSettingForThisOrg(int orgRegProgramId);
    }
}
