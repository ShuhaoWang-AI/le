using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.MonitoringPoint
{
    public class MonitoringPointService : IMonitoringPointService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContext;
        private readonly IOrganizationService _orgService;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ISettingService _settings;

        public MonitoringPointService(LinkoExchangeContext dbContext,
            IHttpContextService httpContext,
            IOrganizationService orgService,
            IMapHelper mapHelper,
            ILogger logger,
            ITimeZoneService timeZoneService,
            ISettingService settings)
        {
            _dbContext = dbContext;
            _httpContext = httpContext;
            _mapHelper = mapHelper;
            _logger = logger;
            _timeZoneService = timeZoneService;
            _settings = settings;
            _orgService = orgService;
        }

        public IEnumerable<MonitoringPointDto> GetMonitoringPoints()
        {
            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            var monPointDtos = new List<MonitoringPointDto>();
            var foundMonPoints = _dbContext.MonitoringPoints
                                           .Where(mp => mp.OrganizationRegulatoryProgramId == currentOrgRegProgramId
                                                        && mp.IsEnabled == true && mp.IsRemoved == false)
                                           .ToList();

            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));
            foreach (var mp in foundMonPoints)
            {
                var dto = _mapHelper.GetMonitoringPointDtoFromMonitoringPoint(mp);

                //Set LastModificationDateTimeLocal
                dto.LastModificationDateTimeLocal = _timeZoneService
                        .GetLocalizedDateTimeUsingThisTimeZoneId(mp.LastModificationDateTimeUtc?.DateTime ?? mp.CreationDateTimeUtc.DateTime, timeZoneId);

                if (mp.LastModifierUserId.HasValue)
                {
                    var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == mp.LastModifierUserId.Value);
                    dto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                }
                else
                {
                    dto.LastModifierFullName = "N/A";
                }

                monPointDtos.Add(dto);
            }
            return monPointDtos;
        }

        public MonitoringPointDto GetMonitoringPoint(int monitoringPointId)
        {
            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            
            var foundMonPoint = _dbContext.MonitoringPoints
                .Single(mp => mp.MonitoringPointId == monitoringPointId);

            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));
            var dto = _mapHelper.GetMonitoringPointDtoFromMonitoringPoint(foundMonPoint); // TODO: if null what will happen to following code?

            //Set LastModificationDateTimeLocal
            dto.LastModificationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(foundMonPoint.LastModificationDateTimeUtc?.DateTime ?? foundMonPoint.CreationDateTimeUtc.DateTime, timeZoneId);

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
    }
}
