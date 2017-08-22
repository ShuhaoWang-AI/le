using System;
using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;

namespace Linko.LinkoExchange.Services.Unit
{
    public class UnitService : IUnitService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContextService;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;
        private readonly IOrganizationService _orgService;
        private readonly IRequestCache _requestCache;
        private readonly ISettingService _settingService;
        private readonly ITimeZoneService _timeZoneService;

        #endregion

        #region constructors and destructor

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
                throw new ArgumentNullException(paramName:nameof(dbContext));
            }

            if (mapHelper == null)
            {
                throw new ArgumentNullException(paramName:nameof(mapHelper));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(paramName:nameof(logger));
            }

            if (httpContextService == null)
            {
                throw new ArgumentNullException(paramName:nameof(httpContextService));
            }

            if (timeZoneService == null)
            {
                throw new ArgumentNullException(paramName:nameof(timeZoneService));
            }

            if (orgService == null)
            {
                throw new ArgumentNullException(paramName:nameof(orgService));
            }

            if (settingService == null)
            {
                throw new ArgumentNullException(paramName:nameof(settingService));
            }

            if (requestCache == null)
            {
                throw new ArgumentNullException(paramName:nameof(requestCache));
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

        #endregion

        #region interface implementations

        /// <summary>
        ///     Gets all available flow units for an Organization where IsFlowUnit = true in tUnit table
        /// </summary>
        /// <returns> </returns>
        public IEnumerable<UnitDto> GetFlowUnits()
        {
            _logger.Info(message:"Enter UnitService.GetFlowUnits.");

            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var authOrganizationId = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationId;

            var units = _dbContext.Units.Where(i => i.IsFlowUnit && i.OrganizationId == authOrganizationId).ToList();

            var unitDtos = UnitDtosHelper(units:units);

            _logger.Info(message:"Leave UnitService.GetFlowUnits.");

            return unitDtos;
        }

        /// <summary>
        ///     Reads unit labels from the Org Reg Program Setting "FlowUnitValidValues"
        /// </summary>
        /// <returns> Collection of unit dto's corresponding to the labels read from the setting </returns>
        public IEnumerable<UnitDto> GetFlowUnitValidValues()
        {
            _logger.Info(message:"Enter UnitService.GetFlowUnitValidValues.");

            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var authOrganizationId = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationId;
            var flowUnitValidValuesString = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.FlowUnitValidValues);
            var flowUnitValidValuesArray = flowUnitValidValuesString.Split(',');

            var units = _dbContext.Units
                                  .Where(i => i.IsFlowUnit
                                              && i.OrganizationId == authOrganizationId

                                              // ReSharper disable once ArgumentsStyleNamedExpression
                                              && flowUnitValidValuesArray.Contains(i.Name)
                                        ).ToList();

            var unitDtos = UnitDtosHelper(units:units);

            _logger.Info(message:"Leave UnitService.GetFlowUnitValidValues.");

            return unitDtos;
        }

        /// <summary>
        ///     Reads unit labels from passed in comma delimited string
        /// </summary>
        /// <param name="commaDelimitedString"> </param>
        /// <param name="isLoggingEnabled"> </param>
        /// <returns>
        ///     Collection of unit dto's corresponding to the labels read from passed in string
        /// </returns>
        public IEnumerable<UnitDto> GetFlowUnitsFromCommaDelimitedString(string commaDelimitedString, bool isLoggingEnabled = true)
        {
            if (isLoggingEnabled)
            {
                _logger.Info(message:"Enter UnitService.GetFlowUnitsFromCommaDelimitedString.");
            }

            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

            var cacheKey = $"GetFlowUnitsFromCommaDelimitedString-{commaDelimitedString}";
            if (_requestCache.GetValue(key:cacheKey) == null)
            {
                var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms
                                              .Single(orp => orp.OrganizationRegulatoryProgramId == currentOrgRegProgramId);
                var authorityOrganizationId = orgRegProgram.RegulatorOrganizationId ?? orgRegProgram.OrganizationId;
                var flowUnitsArray = commaDelimitedString.Split(',');

                var units = _dbContext.Units.Where(i => i.IsFlowUnit
                                                        && i.OrganizationId == authorityOrganizationId

                                                        // ReSharper disable once ArgumentsStyleNamedExpression
                                                        && flowUnitsArray.Contains(i.Name)).ToList();

                var unitDtos = UnitDtosHelper(units:units);
                _requestCache.SetValue(key:cacheKey, value:unitDtos);
            }

            if (isLoggingEnabled)
            {
                _logger.Info(message:"Leave UnitService.GetFlowUnitsFromCommaDelimitedString.");
            }

            return (List<UnitDto>) _requestCache.GetValue(key:cacheKey);
        }

        /// <summary>
        ///     Always ppd as per client's requirements
        /// </summary>
        /// <returns> </returns>
        public UnitDto GetUnitForMassLoadingCalculations()
        {
            _logger.Info(message:"Enter UnitService.GetUnitForMassLoadingCalculations.");

            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var authOrganizationId = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationId;
            var ppdLabelForLookup = _settingService.GetGlobalSettings()[key:SystemSettingType.MassLoadingUnitName];

            var units = _dbContext.Units.Where(i => i.Name == ppdLabelForLookup && i.OrganizationId == authOrganizationId).ToList();

            var unitDtos = UnitDtosHelper(units:units);

            _logger.Info(message:"Leave UnitService.GetUnitForMassLoadingCalculations.");

            return unitDtos.First();
        }

        #endregion

        private List<UnitDto> UnitDtosHelper(List<Core.Domain.Unit> units)
        {
            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

            var unitDtos = new List<UnitDto>();
            foreach (var unit in units)
            {
                var unitDto = _mapHelper.GetUnitDtoFromUnit(unit:unit);

                unitDto.LastModificationDateTimeLocal =
                    _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:unit.LastModificationDateTimeUtc?.UtcDateTime ?? unit.CreationDateTimeUtc.UtcDateTime,
                                                                                orgRegProgramId:currentOrgRegProgramId);

                if (unit.LastModifierUserId.HasValue)
                {
                    var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == unit.LastModifierUserId.Value);
                    unitDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                }
                else
                {
                    unitDto.LastModifierFullName = "N/A";
                }

                unitDtos.Add(item:unitDto);
            }

            return unitDtos;
        }
    }
}