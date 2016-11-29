using AutoMapper;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.TimeZone
{
    public class TimeZoneService : ITimeZoneService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ISettingService _settings;

        public TimeZoneService(LinkoExchangeContext dbContext, IMapper mapper, ISettingService settings)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _settings = settings;
        }

        public string GetTimeZoneName(int timeZoneId)
        {
            return (_dbContext.TimeZones.Single(t => t.TimeZoneId == timeZoneId).Name);
        }

        public ICollection<TimeZoneDto> GetTimeZones()
        {
            var dtos = _mapper.Map<IEnumerable<Core.Domain.TimeZone>, ICollection<TimeZoneDto>>(_dbContext.TimeZones);
            dtos = dtos.OrderBy(t => TimeZoneInfo.FindSystemTimeZoneById(t.Name).BaseUtcOffset).ToList();
            return dtos;
        }

        public DateTime GetLocalizedDateTimeUsingSettingForThisOrg(DateTime utcDateTime, int orgId, int regProgramId)
        {
            var timeZoneId =  Convert.ToInt32(_settings.GetOrganizationSettingValue(orgId, regProgramId, SettingType.TimeZone));
            TimeZoneInfo authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(this.GetTimeZoneName(timeZoneId));
            return (TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, authorityLocalZone));
        }

        public DateTime GetLocalizedDateTimeUsingSettingForThisOrg(DateTime utcDateTime, int orgRegProgramId)
        {
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(orgRegProgramId, SettingType.TimeZone));
            TimeZoneInfo authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(this.GetTimeZoneName(timeZoneId));
            return (TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, authorityLocalZone));
        }


    }
}
