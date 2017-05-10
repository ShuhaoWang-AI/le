using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.TimeZone
{
    public interface ITimeZoneService
    {
        string GetTimeZoneName(int timeZoneId);

        TimeZoneDto GetTimeZone(int timeZoneId);

        ICollection<TimeZoneDto> GetTimeZones();

        DateTime GetLocalizedDateTimeUsingSettingForThisOrg(DateTime utcDateTime, int orgId, int regProgramId);

        DateTime GetLocalizedDateTimeUsingSettingForThisOrg(DateTime utcDateTime, int orgRegProgramId);

        DateTime GetLocalizedDateTimeUsingThisTimeZoneId(DateTime utcDateTime, int timeZoneId);

        string GetTimeZoneNameUsingSettingForThisOrg(int orgRegProgramId, DateTime dateTime, bool abbreviationName);

        string GetTimeZoneNameUsingThisTimeZone(Core.Domain.TimeZone leTimeZone, DateTime dateTime, bool abbreviationName);

        /// <summary>
        /// Takes a Utc DateTime and returns a DateTimeOffset with the offset value corresponding to the passed in
        /// Org Reg Program's Authority's time zone.
        /// </summary>
        /// <param name="utcDateTime">Utc DateTime</param>
        /// <param name="orgRegProgramId">Can be either the User's Org Reg Program Id or User's Authority's Org Reg Program Id</param>
        /// <returns>DateTimeOffset with the offset value corresponding to the passed Org Reg Program's Authority's time zone</returns>
        DateTimeOffset GetLocalDateTimeOffsetFromUtcUsingSettingForThisOrg(DateTime utcDateTime, int orgRegProgramId);

        /// <summary>
        /// Takes a Utc DateTime and returns a DateTimeOffset with the offset value corresponding to the passed in time zone.
        /// </summary>
        /// <param name="utcDateTime">Utc DateTime</param>
        /// <param name="timeZoneId"></param>
        /// <returns>DateTimeOffset with the offset value corresponding to the passed in time zone</returns>
        DateTimeOffset GetLocalDateTimeOffsetFromUtcUsingThisTimeZoneId(DateTime utcDateTime, int timeZoneId);

        /// <summary>
        /// Takes a local DateTime and returns a DateTimeOffset with the offset value corresponding to the passed in 
        /// Org Reg Program's Authority's time zone.
        /// </summary>
        /// <param name="localDateTime">DateTime local to the User Authority's timeZoneId</param>
        /// <param name="orgRegProgramId">Can be either the User's Org Reg Program Id or User's Authority's Org Reg Program Id</param>
        /// <returns>DateTimeOffset with the offset value corresponding to the passed Org Reg Program's Authority's time zone</returns>
        DateTimeOffset GetLocalDateTimeOffsetFromLocalUsingSettingForThisOrg(DateTime localDateTime, int orgRegProgramId);

        /// <summary>
        /// Takes a local DateTime and returns a DateTimeOffset with the offset value corresponding to the passed in time zone.
        /// </summary>
        /// <param name="localDateTime">DateTime local to the User Authority's timeZoneId</param>
        /// <param name="timeZoneId"></param>
        /// <returns>DateTimeOffset with the offset value corresponding to the passed in time zone</returns>
        DateTimeOffset GetLocalDateTimeOffsetFromLocalUsingThisTimeZoneId(DateTime localDateTime, int timeZoneId);

        /// <summary>
        /// Takes a local DateTime and returns a DateTimeOffset with the offset value corresponding to web server's local time zone.
        /// </summary>
        /// <param name="localDateTime">DateTime local to the User Authority's timeZoneId</param>
        /// <param name="timeZoneId">User Authority's local timeZoneId</param>
        /// <returns>DateTimeOffset with the offset value corresponding to the web server's local time zone</returns>
        DateTimeOffset GetServerDateTimeOffsetFromLocalUsingThisTimeZoneId(DateTime localDateTime, int timeZoneId);

        /// <summary>
        /// Takes a local DateTime and returns a DateTimeOffset with the offset value corresponding to web server's local time zone.
        /// </summary>
        /// <param name="localDateTime">DateTime local to the User Authority's timeZoneId</param>
        /// <param name="orgRegProgramId">User Authority's Org Reg Program Id</param>
        /// <returns></returns>
        DateTimeOffset GetServerDateTimeOffsetFromLocalUsingThisOrg(DateTime localDateTime, int orgRegProgramId);
    }
}
