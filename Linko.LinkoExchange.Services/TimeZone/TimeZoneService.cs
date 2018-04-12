using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Settings;
using NLog;

namespace Linko.LinkoExchange.Services.TimeZone
{
    public class TimeZoneService : ITimeZoneService
    {
        #region fields

        private readonly IApplicationCache _appCache;
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper;
        private readonly ISettingService _settings;
        private readonly ILogger _logger;

        #endregion

        #region constructors and destructor

        public TimeZoneService(LinkoExchangeContext dbContext, ISettingService settings, IMapHelper mapHelper, IApplicationCache appCache, ILogger logger)
        {
            _dbContext = dbContext;
            _settings = settings;
            _mapHelper = mapHelper;
            _appCache = appCache;
            _logger = logger;
        }

        #endregion

        #region interface implementations

        public string GetTimeZoneName(int timeZoneId)
        {
            var cacheKey = $"TimeZoneId-{timeZoneId}";
            if (_appCache.Get(key:cacheKey) == null)
            {
                var cacheItem = _dbContext.TimeZones.Single(t => t.TimeZoneId == timeZoneId).Name;
                var cacheDurationHours = int.Parse(s:ConfigurationManager.AppSettings[name:"TimeZoneNameCacheDurationHours"]);
                _appCache.Insert(key:cacheKey, item:cacheItem, hours:cacheDurationHours);
            }

            return (string) _appCache.Get(key:cacheKey);
        }

        public TimeZoneDto GetTimeZone(int timeZoneId)
        {
            var timeZone = _dbContext.TimeZones
                                     .Single(t => t.TimeZoneId == timeZoneId);

            var timeZoneDto = new TimeZoneDto
                              {
                                  Name = timeZone.Name,
                                  StandardAbbreviation = timeZone.StandardAbbreviation,
                                  DaylightAbbreviation = timeZone.DaylightAbbreviation
                              };

            return timeZoneDto;
        }

        public ICollection<TimeZoneDto> GetTimeZones()
        {
            var dtos = new List<TimeZoneDto>();
            foreach (var timeZone in _dbContext.TimeZones)
            {
                dtos.Add(item:_mapHelper.GetTimeZoneDtoFromTimeZone(timeZone:timeZone));
            }

            dtos = dtos.OrderBy(t => TimeZoneInfo.FindSystemTimeZoneById(id:t.Name).BaseUtcOffset).ToList();
            return dtos;
        }

        public DateTime GetLocalizedDateTimeUsingSettingForThisOrg(DateTime utcDateTime, int orgId, int regProgramId)
        {
            var timeZoneId = Convert.ToInt32(value:_settings.GetOrganizationSettingValue(organizationId:orgId, regProgramId:regProgramId, settingType:SettingType.TimeZone));
            var authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(id:GetTimeZoneName(timeZoneId:timeZoneId));
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime:utcDateTime, destinationTimeZone:authorityLocalZone);
        }

        public DateTime GetLocalizedDateTimeUsingSettingForThisOrg(DateTime utcDateTime, int orgRegProgramId)
        {
            var timeZoneId = Convert.ToInt32(value:_settings.GetOrganizationSettingValue(orgRegProgramId:orgRegProgramId, settingType:SettingType.TimeZone));
            var authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(id:GetTimeZoneName(timeZoneId:timeZoneId));
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime:utcDateTime, destinationTimeZone:authorityLocalZone);
        }

        public string GetTimeZoneNameUsingSettingForThisOrg(int orgRegProgramId, DateTime datetime, bool abbreviationName)
        {
            var timeZoneId = Convert.ToInt32(value:_settings.GetOrganizationSettingValue(orgRegProgramId:orgRegProgramId, settingType:SettingType.TimeZone));

            var leTimeZone = _dbContext.TimeZones.Single(t => t.TimeZoneId == timeZoneId);

            return GetTimeZoneNameUsingThisTimeZone(leTimeZone:leTimeZone, datetime:datetime, abbreviationName:abbreviationName);
        }

        public string GetTimeZoneNameUsingThisTimeZone(Core.Domain.TimeZone leTimeZone, DateTime datetime, bool abbreviationName)
        {
            if (abbreviationName)
            {
                if (TimeZoneInfo.Local.IsAmbiguousTime(dateTime:datetime) || TimeZoneInfo.Local.IsDaylightSavingTime(dateTime:datetime))
                {
                    return leTimeZone.DaylightAbbreviation;
                }

                return leTimeZone.StandardAbbreviation;
            }

            var timezoneInfo = TimeZoneInfo.FindSystemTimeZoneById(id:leTimeZone.Name);
            return timezoneInfo.DisplayName.Substring(startIndex:timezoneInfo.DisplayName.Contains(value:" ")
                                                                     ? timezoneInfo.DisplayName.IndexOf(value:" ", comparisonType:StringComparison.Ordinal) + 1
                                                                     : 0
                                                     );
        }

        public DateTime GetLocalizedDateTimeUsingThisTimeZoneId(DateTime utcDateTime, int timeZoneId)
        {
            var authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(id:GetTimeZoneName(timeZoneId:timeZoneId));
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime:utcDateTime, destinationTimeZone:authorityLocalZone);
        }

        public DateTimeOffset GetDateTimeOffsetFromLocalUsingThisTimeZoneId(DateTime localDateTime, int timeZoneId)
        {
            _logger.Info(message:$"Start: TimeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId. localDateTime={localDateTime}, timeZoneId={timeZoneId}");
            var authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(id:GetTimeZoneName(timeZoneId:timeZoneId));
            var serverTimeZone = TimeZoneInfo.Local;

            while (authorityLocalZone.IsAmbiguousTime(dateTime:localDateTime) || authorityLocalZone.IsInvalidTime(dateTime:localDateTime))
            {
                localDateTime = localDateTime.AddHours(value:1);
            }

            var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime:localDateTime, sourceTimeZone:authorityLocalZone);

            _logger.Info(message:"End: TimeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId.");

            return new DateTimeOffset(dateTime:utcDateTime).ToOffset(offset:serverTimeZone.GetUtcOffset(dateTime:utcDateTime));
        }

        public DateTimeOffset GetDateTimeOffsetFromLocalUsingThisOrg(DateTime localDateTime, int orgRegProgramId)
        {
            var timeZoneId = Convert.ToInt32(value:_settings.GetOrganizationSettingValue(orgRegProgramId:orgRegProgramId, settingType:SettingType.TimeZone));
            return GetDateTimeOffsetFromLocalUsingThisTimeZoneId(localDateTime:localDateTime, timeZoneId:timeZoneId);
        }

        #endregion
    }
}