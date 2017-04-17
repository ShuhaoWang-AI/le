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

        public string GetAbbreviationTimeZoneNameUsingSettingForThisOrg(int orgRegProgramId)
        {
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(orgRegProgramId, SettingType.TimeZone));
            var timeZoneName = GetTimeZoneName(timeZoneId);
            //var t = TimeZoneInfo.FindSystemTimeZoneById(timeZoneFullName);   
            switch (timeZoneName)
            {
                case "Hawaiian Standard Time":
                    return "HST";
                case "Alaskan Standard Time":
                    return "AST";
                case "Pacific Standard Time":
                    return "PST";
                case "Mountain Standard Time":
                    return "MST";
                case "Central Standard Time":
                    return "CST";
                case "Eastern Standard Time":
                    return "EST";
                case "Atlantic Standard Time":
                    return "AST";
                case "Newfoundland Standard Time":
                    return "NST";
                default:
                    return timeZoneName;
            }
        }

        public DateTime GetLocalizedDateTimeUsingThisTimeZoneId(DateTime utcDateTime, int timeZoneId)
        {
            TimeZoneInfo authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(this.GetTimeZoneName(timeZoneId));
            return (TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, authorityLocalZone));
        }

        public DateTimeOffset GetUTCDateTimeUsingSettingForThisOrg(DateTime localDateTime, int orgRegProgramId)
        {
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(orgRegProgramId, SettingType.TimeZone));
            TimeZoneInfo orgRegProgramLocalZone = TimeZoneInfo.FindSystemTimeZoneById(this.GetTimeZoneName(timeZoneId));
            return (TimeZoneInfo.ConvertTimeToUtc(localDateTime, orgRegProgramLocalZone));
        }

        public DateTimeOffset GetUTCDateTimeUsingThisTimeZoneId(DateTime localDateTime, int timeZoneId)
        {
            localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
            TimeZoneInfo orgRegProgramLocalZone = TimeZoneInfo.FindSystemTimeZoneById(this.GetTimeZoneName(timeZoneId));
            return (TimeZoneInfo.ConvertTimeToUtc(localDateTime, orgRegProgramLocalZone));
        }

    }
}
