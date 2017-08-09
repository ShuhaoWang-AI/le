﻿using System;
using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.HttpContext;

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
        private readonly ISettingService _settingService;
        private readonly IRequestCache _requestCache;

        public UnitService(
            LinkoExchangeContext dbContext,
            IMapHelper mapHelper,
            ILogger logger,
            IHttpContextService httpContextService,
            ITimeZoneService timeZoneService,
            IOrganizationService orgService,
            ISettingService settingService,
            IRequestCache requestCache)
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

            if (settingService == null)
            {
                throw new ArgumentNullException(nameof(settingService));
            }

            if (requestCache == null)
            {
                throw new ArgumentNullException(nameof(requestCache));
            }

            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _logger = logger;
            _httpContextService = httpContextService;
            _timeZoneService = timeZoneService;
            _orgService = orgService;
            _settingService = settingService;
            _requestCache = requestCache;
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
            var flowUnitValidValuesString = _settingService.GetOrgRegProgramSettingValue(currentOrgRegProgramId, Core.Enum.SettingType.FlowUnitValidValues);
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
        /// Reads unit labels from passed in comma delimited string
        /// </summary>
        /// <param name="commaDelimitedString"></param>
        /// <returns>Collection of unit dto's corresponding to the labels read from passed in string</returns>
        public IEnumerable<UnitDto> GetFlowUnitsFromCommaDelimitedString(string commaDelimitedString, bool isLoggingEnabled = true)
        {
            if (isLoggingEnabled)
                _logger.Info("Enter UnitService.GetFlowUnitsFromCommaDelimitedString.");

            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            string cacheKey = $"GetFlowUnitsFromCommaDelimitedString-{commaDelimitedString}";
            if (_requestCache.GetValue(cacheKey) == null)
            {
                var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms
                    .Single(orp => orp.OrganizationRegulatoryProgramId == currentOrgRegProgramId);
                var authorityOrganizationId = orgRegProgram.RegulatorOrganizationId ?? orgRegProgram.OrganizationId;
                var flowUnitsArray = commaDelimitedString.Split(',');

                var units = _dbContext.Units
                    .Where(i => i.IsFlowUnit
                        && i.OrganizationId == authorityOrganizationId
                        && flowUnitsArray.Contains(i.Name)
                        ).ToList();

                var unitDtos = UnitDtosHelper(units);
                _requestCache.SetValue(cacheKey, unitDtos);
            }

            if (isLoggingEnabled)
                _logger.Info("Leave UnitService.GetFlowUnitsFromCommaDelimitedString.");

            return (List<UnitDto>)_requestCache.GetValue(cacheKey);
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
            var ppdLabelForLookup = _settingService.GetGlobalSettings()[SystemSettingType.MassLoadingUnitName];

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
                        ? unit.LastModificationDateTimeUtc.Value.UtcDateTime
                        : unit.CreationDateTimeUtc.UtcDateTime), currentOrgRegProgramId);

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