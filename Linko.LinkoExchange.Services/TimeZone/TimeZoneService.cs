using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Linko.LinkoExchange.Services.TimeZone
{
    public class TimeZoneService : ITimeZoneService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly ISettingService _settings;
        private readonly IMapHelper _mapHelper;

        public TimeZoneService(LinkoExchangeContext dbContext, ISettingService settings, IMapHelper mapHelper)
        {
            _dbContext = dbContext;
            _settings = settings;
            _mapHelper = mapHelper;
        }

        public string GetTimeZoneName(int timeZoneId)
        {
            return (_dbContext.TimeZones.Single(t => t.TimeZoneId == timeZoneId).Name);
        }

        public TimeZoneDto GetTimeZone(int timeZoneId)
        {
            var timeZone = _dbContext.TimeZones
                .Single(t => t.TimeZoneId == timeZoneId);

            var timeZoneDto = new TimeZoneDto() {
                Name = timeZone.Name,
                StandardAbbreviation = timeZone.StandardAbbreviation,
                DaylightAbbreviation = timeZone.DaylightAbbreviation
            };

            return (timeZoneDto);
        }

        public ICollection<TimeZoneDto> GetTimeZones()
        {
            var dtos = new List<TimeZoneDto>();
            foreach (Core.Domain.TimeZone timeZone in _dbContext.TimeZones)
            {
                dtos.Add(_mapHelper.GetTimeZoneDtoFromTimeZone(timeZone));
            }
            dtos = dtos.OrderBy(t => TimeZoneInfo.FindSystemTimeZoneById(t.Name).BaseUtcOffset).ToList();
            return dtos;
        }

        public DateTime GetLocalizedDateTimeUsingSettingForThisOrg(DateTime utcDateTime, int orgId, int regProgramId)
        {
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(orgId, regProgramId, SettingType.TimeZone));
            TimeZoneInfo authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(this.GetTimeZoneName(timeZoneId));
            return (TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, authorityLocalZone));
        }

        public DateTime GetLocalizedDateTimeUsingSettingForThisOrg(DateTime utcDateTime, int orgRegProgramId)
        {
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(orgRegProgramId, SettingType.TimeZone));
            TimeZoneInfo authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(this.GetTimeZoneName(timeZoneId));
            return (TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, authorityLocalZone));
        }

        public string GetTimeZoneNameUsingSettingForThisOrg(int orgRegProgramId, DateTime datetime, bool abbreviationName)
        {
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(orgRegProgramId, SettingType.TimeZone));

            var leTimeZone = _dbContext.TimeZones.Single(t => t.TimeZoneId == timeZoneId);

            return GetTimeZoneNameUsingThisTimeZone(leTimeZone, datetime, abbreviationName);
        }

        public string GetTimeZoneNameUsingThisTimeZone(Core.Domain.TimeZone leTimeZone, DateTime datetime, bool abbreviationName)
        {
            if (abbreviationName)
            {
                if (TimeZoneInfo.Local.IsAmbiguousTime(datetime) || TimeZoneInfo.Local.IsDaylightSavingTime(datetime))
                {
                    return leTimeZone.DaylightAbbreviation;
                }
                return leTimeZone.StandardAbbreviation;
            }

            var timezoneInfo = TimeZoneInfo.FindSystemTimeZoneById(leTimeZone.Name);
            return timezoneInfo.DisplayName.Substring(timezoneInfo.DisplayName.IndexOf(" ") + 1);
        }

        public DateTime GetLocalizedDateTimeUsingThisTimeZoneId(DateTime utcDateTime, int timeZoneId)
        {
            TimeZoneInfo authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(this.GetTimeZoneName(timeZoneId));
            return (TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, authorityLocalZone));
        }

        public DateTimeOffset GetLocalDateTimeOffsetFromUtcUsingSettingForThisOrg(DateTime utcDateTime, int orgRegProgramId)
        {
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(orgRegProgramId, SettingType.TimeZone));
            return (GetLocalDateTimeOffsetFromUtcUsingThisTimeZoneId(utcDateTime, timeZoneId));
        }

        public DateTimeOffset GetLocalDateTimeOffsetFromUtcUsingThisTimeZoneId(DateTime utcDateTime, int timeZoneId)
        {
            TimeZoneInfo authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(this.GetTimeZoneName(timeZoneId));
            return (new DateTimeOffset(utcDateTime).ToOffset(authorityLocalZone.GetUtcOffset(utcDateTime)));
        }

        public DateTimeOffset GetLocalDateTimeOffsetFromLocalUsingSettingForThisOrg(DateTime localDateTime, int orgRegProgramId)
        {
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(orgRegProgramId, SettingType.TimeZone));
            return (GetLocalDateTimeOffsetFromLocalUsingThisTimeZoneId(localDateTime, timeZoneId));
        }

        public DateTimeOffset GetLocalDateTimeOffsetFromLocalUsingThisTimeZoneId(DateTime localDateTime, int timeZoneId)
        {
            TimeZoneInfo authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(this.GetTimeZoneName(timeZoneId));

            while (authorityLocalZone.IsAmbiguousTime(localDateTime) || authorityLocalZone.IsInvalidTime(localDateTime))
            {
                localDateTime = localDateTime.AddHours(1);
            }

            var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, authorityLocalZone);
            return (new DateTimeOffset(utcDateTime).ToOffset(authorityLocalZone.GetUtcOffset(utcDateTime)));
        }

        public DateTimeOffset GetServerDateTimeOffsetFromLocalUsingThisTimeZoneId(DateTime localDateTime, int timeZoneId)
        {
            TimeZoneInfo authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(this.GetTimeZoneName(timeZoneId));
            TimeZoneInfo serverTimeZone = TimeZoneInfo.Local;

            while (authorityLocalZone.IsAmbiguousTime(localDateTime) || authorityLocalZone.IsInvalidTime(localDateTime))
            {
                localDateTime = localDateTime.AddHours(1);
            }

            var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, authorityLocalZone);
            return (new DateTimeOffset(utcDateTime).ToOffset(serverTimeZone.GetUtcOffset(utcDateTime)));

        }

        public DateTimeOffset GetServerDateTimeOffsetFromLocalUsingThisOrg(DateTime localDateTime, int orgRegProgramId)
        {
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(orgRegProgramId, SettingType.TimeZone));
            return (GetServerDateTimeOffsetFromLocalUsingThisTimeZoneId(localDateTime, timeZoneId));
        }

    }
}
