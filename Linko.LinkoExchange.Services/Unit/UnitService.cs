using System;
using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;

namespace Linko.LinkoExchange.Services.Unit
{
    public class UnitService : IUnitService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;
        private readonly IHttpContextService _httpContextService;
        private readonly ITimeZoneService _timeZoneService;

        public UnitService(
            LinkoExchangeContext dbContext,
            IMapHelper mapHelper,
            ILogger logger,
            IHttpContextService httpContextService,
            ITimeZoneService timeZoneService)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (mapHelper == null)
            {
                throw new ArgumentNullException(nameof(mapHelper));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (httpContextService == null)
            {
                throw new ArgumentNullException(nameof(httpContextService));
            }

            if (timeZoneService == null)
            {
                throw new ArgumentNullException(nameof(timeZoneService));
            }

            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _logger = logger;
            _httpContextService = httpContextService;
            _timeZoneService = timeZoneService;
        }

        public List<UnitDto> GetFlowUnits()
        {
            _logger.Info("Enter UnitService.GetFlowUnits.");

            var units = _dbContext.Units.Where(i => i.IsFlowUnit == true).ToList();

            var unitDtos = UnitDtosHelper(units);

            _logger.Info("Leave UnitService.GetFlowUnits.");

            return unitDtos;
        }

        private List<UnitDto> UnitDtosHelper(List<Core.Domain.Unit> units)
        {
            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            var unitDtos = new List<UnitDto>();
            foreach (var unit in units)
            {
                var unitDto = _mapHelper.GetUnitDtoFromUnit(unit);

                unitDto.LastModificationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingSettingForThisOrg(
                    (unit.LastModificationDateTimeUtc.HasValue
                        ? unit.LastModificationDateTimeUtc.Value.DateTime
                        : unit.CreationDateTimeUtc.DateTime), currentOrgRegProgramId);

                if (unit.LastModifierUserId.HasValue)
                {
                    var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == unit.LastModifierUserId.Value);
                    unitDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                }
                else
                {
                    unitDto.LastModifierFullName = "N/A";
                }

                unitDtos.Add(unitDto);
            }
            return unitDtos;
        }
    }
}