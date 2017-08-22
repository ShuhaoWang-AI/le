using System;
using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;

namespace Linko.LinkoExchange.Services.MonitoringPoint
{
    public class MonitoringPointService : IMonitoringPointService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContext;
        private readonly IMapHelper _mapHelper;
        private readonly ISettingService _settings;
        private readonly ITimeZoneService _timeZoneService;

        #endregion

        #region constructors and destructor

        public MonitoringPointService(LinkoExchangeContext dbContext,
                                      IHttpContextService httpContext,
                                      IMapHelper mapHelper,
                                      ITimeZoneService timeZoneService,
                                      ISettingService settings)
        {
            _dbContext = dbContext;
            _httpContext = httpContext;
            _mapHelper = mapHelper;
            _timeZoneService = timeZoneService;
            _settings = settings;
        }

        #endregion

        #region interface implementations

        public IEnumerable<MonitoringPointDto> GetMonitoringPoints()
        {
            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

            var monPointDtos = new List<MonitoringPointDto>();
            var foundMonPoints = _dbContext.MonitoringPoints
                                           .Where(mp => mp.OrganizationRegulatoryProgramId == currentOrgRegProgramId
                                                        && mp.IsEnabled
                                                        && mp.IsRemoved == false)
                                           .ToList();

            var timeZoneId = Convert.ToInt32(value:_settings.GetOrganizationSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.TimeZone));
            foreach (var mp in foundMonPoints)
            {
                var dto = _mapHelper.GetMonitoringPointDtoFromMonitoringPoint(mp:mp);

                //Set LastModificationDateTimeLocal
                dto.LastModificationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:mp.LastModificationDateTimeUtc?.UtcDateTime ?? mp.CreationDateTimeUtc.UtcDateTime, timeZoneId:timeZoneId);

                if (mp.LastModifierUserId.HasValue)
                {
                    var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == mp.LastModifierUserId.Value);
                    dto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                }
                else
                {
                    dto.LastModifierFullName = "N/A";
                }

                monPointDtos.Add(item:dto);
            }

            return monPointDtos;
        }

        public MonitoringPointDto GetMonitoringPoint(int monitoringPointId)
        {
            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

            var foundMonPoint = _dbContext.MonitoringPoints
                                          .Single(mp => mp.MonitoringPointId == monitoringPointId);

            var timeZoneId = Convert.ToInt32(value:_settings.GetOrganizationSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.TimeZone));
            var dto = _mapHelper.GetMonitoringPointDtoFromMonitoringPoint(mp:foundMonPoint); // TODO: if null what will happen to following code?

            //Set LastModificationDateTimeLocal
            dto.LastModificationDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:foundMonPoint.LastModificationDateTimeUtc?.UtcDateTime ?? foundMonPoint.CreationDateTimeUtc.UtcDateTime,
                                                         timeZoneId:timeZoneId);

            if (foundMonPoint.LastModifierUserId.HasValue)
            {
                var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == foundMonPoint.LastModifierUserId.Value);
                dto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
            }
            else
            {
                dto.LastModifierFullName = "N/A";
            }

            return dto;
        }

        #endregion
    }
}