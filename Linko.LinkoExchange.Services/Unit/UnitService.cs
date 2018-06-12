using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Base;
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
    public class UnitService : BaseService, IUnitService
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

        /// <inheritdoc />
        public override bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id)
        {
            //var retVal = false;

            //var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            //var currentPortalName = _httpContextService.GetClaimValue(claimType:CacheKey.PortalName);
            //currentPortalName = string.IsNullOrWhiteSpace(value:currentPortalName) ? "" : currentPortalName.Trim().ToLower();

            switch (apiName)
            {
                default: throw new Exception(message:$"ERROR: Unhandled API authorization attempt using name = '{apiName}'");
            }

            //return retVal;
        }

        /// <inheritdoc />
        public IEnumerable<UnitDto> GetUnits()
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod()))
            {
                var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                var authOrganizationId = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationId;

                var units = _dbContext.Units.Where(i => i.OrganizationId == authOrganizationId).ToList();

                var unitDtos = UnitDtosHelper(units:units);

                return unitDtos;
            }
        }

        /// <inheritdoc />
        public UnitDto GetUnit(int unitId)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(), descripition:$"unitId={unitId}"))
            {
                var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                var authOrganizationId = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationId;

                var unit = _dbContext.Units.SingleOrDefault(i => i.OrganizationId == authOrganizationId && i.UnitId == unitId);

                if (unit == null)
                {
                    return null;
                }
                else
                {
                    var unitDto = _mapHelper.ToDto(fromDomainObject:unit);

                    unitDto.LastModificationDateTimeLocal =
                        _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:unit.LastModificationDateTimeUtc?.UtcDateTime
                                                                                                ?? unit.CreationDateTimeUtc.UtcDateTime, orgRegProgramId:currentOrgRegProgramId);

                    if (unit.LastModifierUserId.HasValue)
                    {
                        var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == unit.LastModifierUserId.Value);
                        unitDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                    }
                    else
                    {
                        unitDto.LastModifierFullName = "N/A";
                    }

                    return unitDto;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<SystemUnitDto> GetSystemUnits()
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod()))
            {
                var systemUnits = _dbContext.SystemUnits.Include(x => x.UnitDimension).ToList();

                var systemUnitDtos = systemUnits.Select(i => _mapHelper.ToDto(fromDomainObject:i));

                return systemUnitDtos;
            }
        }

        /// <inheritdoc />
        public SystemUnitDto GetSystemUnit(int systemUnitId)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod()))
            {
                var systemUnit = _dbContext.SystemUnits.Include(x => x.UnitDimension).SingleOrDefault(i => i.SystemUnitId == systemUnitId);

                if (systemUnit == null)
                {
                    return null;
                }
                else
                {
                    var systemUnitDto = _mapHelper.ToDto(fromDomainObject:systemUnit);

                    return systemUnitDto;
                }
            }
        }

        /// <summary>
        /// Gets all available flow units for an Organization where IsFlowUnit = true in tUnit table
        /// </summary>
        /// <returns> </returns>
        public IEnumerable<UnitDto> GetFlowUnits()
        {
            _logger.Info(message:"Start: UnitService.GetFlowUnits.");

            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var authOrganizationId = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationId;

            var units = _dbContext.Units.Where(i => i.IsFlowUnit && i.OrganizationId == authOrganizationId).ToList();

            var unitDtos = UnitDtosHelper(units:units);

            _logger.Info(message:"End: UnitService.GetFlowUnits.");

            return unitDtos;
        }

        /// <summary>
        /// Reads unit labels from the Org Reg Program Setting "FlowUnitValidValues"
        /// </summary>
        /// <returns> Collection of unit dto's corresponding to the labels read from the setting </returns>
        public IEnumerable<UnitDto> GetFlowUnitValidValues()
        {
            _logger.Info(message:"Start: UnitService.GetFlowUnitValidValues.");

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

            _logger.Info(message:"End: UnitService.GetFlowUnitValidValues.");

            return unitDtos;
        }

        /// <summary>
        /// Reads unit labels from passed in comma delimited string
        /// </summary>
        /// <param name="commaDelimitedString"> </param>
        /// <param name="isLoggingEnabled"> </param>
        /// <returns>
        /// Collection of unit dto's corresponding to the labels read from passed in string
        /// </returns>
        public IEnumerable<UnitDto> GetFlowUnitsFromCommaDelimitedString(string commaDelimitedString, bool isLoggingEnabled = true)
        {
            if (isLoggingEnabled)
            {
                _logger.Info(message:"Start: UnitService.GetFlowUnitsFromCommaDelimitedString.");
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
                _logger.Info(message:"End: UnitService.GetFlowUnitsFromCommaDelimitedString.");
            }

            return (List<UnitDto>) _requestCache.GetValue(key:cacheKey);
        }

        /// <summary>
        /// Always ppd as per client's requirements
        /// </summary>
        /// <returns> </returns>
        public UnitDto GetUnitForMassLoadingCalculations()
        {
            _logger.Info(message:"Start: UnitService.GetUnitForMassLoadingCalculations.");

            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var authOrganizationId = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationId;
            var ppdLabelForLookup = _settingService.GetGlobalSettings()[key:SystemSettingType.MassLoadingUnitName];

            var units = _dbContext.Units.Where(i => i.Name == ppdLabelForLookup && i.OrganizationId == authOrganizationId).ToList();

            var unitDtos = UnitDtosHelper(units:units);

            _logger.Info(message:"End: UnitService.GetUnitForMassLoadingCalculations.");

            return unitDtos.First();
        }

        /// <inheritdoc />
        public int GetMissingAuthorityUnitToSystemUnitTranslationCount()
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod()))
            {
                var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                var authOrganizationId = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationId;

                return _dbContext.Units.Where(i => i.OrganizationId == authOrganizationId && !i.IsReviewed).ToList().Count;
            }
        }

        /// <inheritdoc />
        public void UpdateAuthorityUnit(UnitDto unitDto)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod()))
            {
                var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                var authOrganizationId = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationId;
                var currentUserId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId));

                var unitToPersist = _dbContext.Units.SingleOrDefault(i => i.OrganizationId == authOrganizationId && i.UnitId == unitDto.UnitId);
                unitToPersist = _mapHelper.ToDomainObject(fromDto:unitDto, existingDomainObject:unitToPersist);

                unitToPersist.LastModificationDateTimeUtc = DateTimeOffset.Now;
                unitToPersist.LastModifierUserId = currentUserId;

                _dbContext.SaveChanges();
            }
        }

        /// <inheritdoc />
        public double? ConvertResultToTargetUnit(double? result, Core.Domain.Unit currentAuthorityUnit, Core.Domain.Unit targetAuthorityUnit)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(),
                                    descripition:$"current unitId:{currentAuthorityUnit}, target unitId:{targetAuthorityUnit}"))
            {
                return ConvertResultToTargetUnit(result:result, currentAuthorityUnit:_mapHelper.ToDto(fromDomainObject:currentAuthorityUnit),
                                                 targetAuthorityUnit:_mapHelper.ToDto(fromDomainObject:targetAuthorityUnit));
            }
        }

        /// <inheritdoc />
        public double? ConvertResultToTargetUnit(double? result, UnitDto currentAuthorityUnit, UnitDto targetAuthorityUnit)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(),
                                    descripition:$"current unit:{currentAuthorityUnit?.UnitId.ToString() ?? "null"}, target unit:{targetAuthorityUnit?.UnitId.ToString() ?? "null"}"))
            {
                var currentSystemUnit = currentAuthorityUnit?.SystemUnit;
                var targetSystemUnit = targetAuthorityUnit?.SystemUnit;

                if (currentSystemUnit == null)
                {
                    throw new ArgumentNullException(paramName: nameof(currentAuthorityUnit), message: ErrorConstants.Unit.PropertySystemUnitCannotBeNull);
                }
                if (targetSystemUnit == null)
                {
                    throw new ArgumentNullException(paramName: nameof(targetAuthorityUnit), message: ErrorConstants.Unit.PropertySystemUnitCannotBeNull);
                }

                if (currentSystemUnit.UnitDimensionId == targetSystemUnit.UnitDimensionId)
                {
                    return ConvertResultToTargetUnit(result:result, currentUnitConversionFactor:currentSystemUnit.ConversionFactor,
                                                     currentUnitAdditiveFactor:currentSystemUnit.AdditiveFactor,
                                                     targetUnitConversionFactor:targetSystemUnit.ConversionFactor, targetUnitAdditiveFactor:targetSystemUnit.AdditiveFactor);
                }
                else
                {
                    throw CreateRuleViolationExceptionForValidationError(errorMessage:ErrorConstants.Unit.UnsupportedUnitConversion);
                }
            }
        }

        #endregion

        private double? ConvertResultToTargetUnit(double? result, double currentUnitConversionFactor, double currentUnitAdditiveFactor, double targetUnitConversionFactor,
                                                  double targetUnitAdditiveFactor)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(),
                                    descripition:$"currentUnitConversionFactor:{currentUnitConversionFactor}, "
                                                 + $"currentUnitAdditiveFactor:{currentUnitAdditiveFactor}, "
                                                 + $"targetUnitConversionFactor:{targetUnitConversionFactor}, "
                                                 + $"targetUnitAdditiveFactor:{targetUnitAdditiveFactor}")
            )
            {
                return (double?) (((decimal?) result * (decimal) currentUnitConversionFactor + (decimal) currentUnitAdditiveFactor - (decimal) targetUnitAdditiveFactor)
                                  / (decimal) targetUnitConversionFactor);
            }
        }

        private List<UnitDto> UnitDtosHelper(List<Core.Domain.Unit> units)
        {
            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

            var unitDtos = new List<UnitDto>();
            foreach (var unit in units)
            {
                var unitDto = _mapHelper.ToDto(fromDomainObject:unit);

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