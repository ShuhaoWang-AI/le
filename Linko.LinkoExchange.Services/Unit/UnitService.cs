using System;
using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Config;
using Linko.LinkoExchange.Services.Settings;

namespace Linko.LinkoExchange.Services.Unit
{
    public class UnitService : IUnitService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;
        private readonly IHttpContextService _httpContextService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IOrganizationService _orgService;
        private readonly IConfigSettingService _configService;
        private readonly ISettingService _settingService;

        public UnitService(
            LinkoExchangeContext dbContext,
            IMapHelper mapHelper,
            ILogger logger,
            IHttpContextService httpContextService,
            ITimeZoneService timeZoneService,
            IOrganizationService orgService,
            IConfigSettingService configService,
            ISettingService settingService)
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

            if (orgService == null)
            {
                throw new ArgumentNullException(nameof(orgService));
            }

            if (configService == null)
            {
                throw new ArgumentNullException(nameof(orgService));
            }

            if (settingService == null)
            {
                throw new ArgumentNullException(nameof(settingService));
            }

            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _logger = logger;
            _httpContextService = httpContextService;
            _timeZoneService = timeZoneService;
            _orgService = orgService;
            _configService = configService;
            _settingService = settingService;
        }

        /// <summary>
        /// Gets all available flow units for an Organization where IsFlowUnit = true in tUnit table
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UnitDto> GetFlowUnits()
        {
            _logger.Info("Enter UnitService.GetFlowUnits.");

            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var authOrganizationId = _orgService.GetAuthority(currentOrgRegProgramId).OrganizationId;

            var units = _dbContext.Units.Where(i => i.IsFlowUnit && i.OrganizationId == authOrganizationId).ToList();

            var unitDtos = UnitDtosHelper(units);

            _logger.Info("Leave UnitService.GetFlowUnits.");

            return unitDtos;
        }

        /// <summary>
        /// Reads unit labels from the Org Reg Program Setting "FlowUnitValidValues"
        /// </summary>
        /// <returns>Collection of unit dto's corresponding to the labels read from the setting</returns>
        public IEnumerable<UnitDto> GetFlowUnitValidValues()
        {
            _logger.Info("Enter UnitService.GetFlowUnitValidValues.");

            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var authOrganizationId = _orgService.GetAuthority(currentOrgRegProgramId).OrganizationId;
            var flowUnitValidValuesString = _settingService.GetOrganizationSettingValue(currentOrgRegProgramId, Core.Enum.SettingType.FlowUnitValidValues);
            var flowUnitValidValuesArray = flowUnitValidValuesString.Split(',');

            var units = _dbContext.Units
                .Where(i => i.IsFlowUnit 
                    && i.OrganizationId == authOrganizationId
                    && flowUnitValidValuesArray.Contains(i.Name)
                    ).ToList();

            var unitDtos = UnitDtosHelper(units);

            _logger.Info("Leave UnitService.GetFlowUnitValidValues.");

            return unitDtos;
        }

        /// <summary>
        /// Always ppd as per client's requirements
        /// </summary>
        /// <returns></returns>
        public UnitDto GetUnitForMassLoadingCalculations()
        {
            _logger.Info("Enter UnitService.GetUnitForMassLoadingCalculations.");

            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var authOrganizationId = _orgService.GetAuthority(currentOrgRegProgramId).OrganizationId;
            var ppdLabelForLookup = _configService.GetConfigValue("ppdUnitName");

            var units = _dbContext.Units.Where(i => i.Name == ppdLabelForLookup && i.OrganizationId == authOrganizationId).ToList();

            var unitDtos = UnitDtosHelper(units);

            _logger.Info("Leave UnitService.GetUnitForMassLoadingCalculations.");

            return unitDtos.First();
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