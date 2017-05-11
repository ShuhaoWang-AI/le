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

        /// <summary>
        /// Converts a UTC date/time to a local date/time in the time zone that the organization's authority is in for a given Regulatory Program
        /// </summary>
        /// <param name="utcDateTime"></param>
        /// <param name="orgId">Can be either the User's Organization Id or User's Authority's Organization Id</param>
        /// <param name="regProgramId">The User's Regulatory Program Id</param>
        /// <returns>DateTime in the local time zone</returns>
        DateTime GetLocalizedDateTimeUsingSettingForThisOrg(DateTime utcDateTime, int orgId, int regProgramId);

        /// <summary>
        /// Converts a UTC date/time to a local date/time in the time zone that the Org Reg Program's Authority is in.
        /// </summary>
        /// <param name="utcDateTime"></param>
        /// <param name="orgRegProgramId">Can be either the User's Org Reg Program Id or User's Authority's Org Reg Program Id</param>
        /// <returns>DateTime in the local time zone</returns>
        DateTime GetLocalizedDateTimeUsingSettingForThisOrg(DateTime utcDateTime, int orgRegProgramId);

        /// <summary>
        /// Converts a UTC date/time to a local date/time in a specified time zone.
        /// </summary>
        /// <param name="utcDateTime"></param>
        /// <param name="timeZoneId">Target time zone</param>
        /// <returns>DateTime in the specified time zone</returns>
        DateTime GetLocalizedDateTimeUsingThisTimeZoneId(DateTime utcDateTime, int timeZoneId);

        /// <summary>
        ///  Gets the time zone's name associated with a Org Reg Program on a given date.
        /// </summary>
        /// <param name="orgRegProgramId"></param>
        /// <param name="dateTime"></param>
        /// <param name="abbreviationName">If true, returns the abbreviated time zone name</param>
        /// <returns></returns>
        string GetTimeZoneNameUsingSettingForThisOrg(int orgRegProgramId, DateTime dateTime, bool abbreviationName);

        /// <summary>
        /// Gets the time zone's name associated with a passed in Time Zone object on a given date.
        /// </summary>
        /// <param name="leTimeZone"></param>
        /// <param name="dateTime"></param>
        /// <param name="abbreviationName">If true, returns the abbreviated time zone name</param>
        /// <returns></returns>
        string GetTimeZoneNameUsingThisTimeZone(Core.Domain.TimeZone leTimeZone, DateTime dateTime, bool abbreviationName);

        /// <summary>
        /// Takes a local DateTime and returns a DateTimeOffset with the offset value corresponding to web server's local time zone.
        /// </summary>
        /// <param name="localDateTime">DateTime local to the User Authority's timeZoneId</param>
        /// <param name="timeZoneId">User Authority's local timeZoneId</param>
        /// <returns>DateTimeOffset with the offset value corresponding to the web server's local time zone</returns>
        DateTimeOffset GetDateTimeOffsetFromLocalUsingThisTimeZoneId(DateTime localDateTime, int timeZoneId);

        /// <summary>
        /// Takes a local DateTime and returns a DateTimeOffset with the offset value corresponding to web server's local time zone.
        /// </summary>
        /// <param name="localDateTime">DateTime local to the User Authority's timeZoneId</param>
        /// <param name="orgRegProgramId">Can be either the User's Org Reg Program Id or User's Authority's Org Reg Program Id</param>
        /// <returns></returns>
        DateTimeOffset GetDateTimeOffsetFromLocalUsingThisOrg(DateTime localDateTime, int orgRegProgramId);
    }
}
