using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Linko.LinkoExchange.Core.Domain;
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

namespace Linko.LinkoExchange.Services.DataSource
{
    public class DataSourceService : BaseService, IDataSourceService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContext;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;
        private readonly IOrganizationService _orgService;
        private readonly ISettingService _settings;
        private readonly ITimeZoneService _timeZoneService;

        #endregion

        #region constructors and destructor

        public DataSourceService(LinkoExchangeContext dbContext,
                                 IHttpContextService httpContext,
                                 ILogger logger,
                                 IMapHelper mapHelper,
                                 IOrganizationService orgService,
                                 ISettingService settings,
                                 ITimeZoneService timeZoneService)
        {
            _dbContext = dbContext;
            _httpContext = httpContext;
            _logger = logger;
            _mapHelper = mapHelper;
            _orgService = orgService;
            _settings = settings;
            _timeZoneService = timeZoneService;
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
                default: throw new NotSupportedException(message:$"ERROR: Unhandled API authorization attempt using name = '{apiName}'");
            }

            //return retVal;
        }

        public int SaveDataSource(DataSourceDto dataSourceDto)
        {
            var dataSourceIdString = dataSourceDto.DataSourceId?.ToString() ?? "null";
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(), descripition:$"dataSourceId={dataSourceIdString}"))
            {
                var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                var currentUserProfileId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.UserProfileId));

                if (string.IsNullOrWhiteSpace(value:dataSourceDto.Name))
                {
                    throw CreateRuleViolationExceptionForValidationError(errorMessage:"Name is required.");
                }

                int parameterGroupIdToReturn;
                using (_dbContext.CreateAutoCommitScope())
                {
                    var existingDataSource = GetExistingDataSource(organizationRegulatoryProgramId:currentOrgRegProgramId, dataSourceDto:dataSourceDto);
                    Core.Domain.DataSource dataSourceToPersist;
                    if (existingDataSource != null)
                    {
                        if (existingDataSource.DataSourceId != dataSourceDto.DataSourceId)
                        {
                            throw CreateRuleViolationExceptionForValidationError(errorMessage:"A DataSource with the name already exists. Please select another name.");
                        }
                        else
                        {
                            dataSourceToPersist = _mapHelper.GetDataSourceFromDataSourceDto(dto:dataSourceDto, existingDataSource:existingDataSource);
                            dataSourceToPersist.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                            dataSourceToPersist.LastModificationDateTimeUtc = DateTimeOffset.Now;
                            dataSourceToPersist.LastModifierUserId = currentUserProfileId;
                        }
                    }
                    else
                    {
                        dataSourceToPersist = _mapHelper.GetDataSourceFromDataSourceDto(dto:dataSourceDto, existingDataSource:null);
                        dataSourceToPersist.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                        dataSourceToPersist.CreationDateTimeUtc = DateTimeOffset.Now;
                        dataSourceToPersist.LastModificationDateTimeUtc = DateTimeOffset.Now;
                        dataSourceToPersist.LastModifierUserId = currentUserProfileId;

                        _dbContext.DataSources.Add(entity:dataSourceToPersist);
                    }

                    _dbContext.SaveChanges();

                    parameterGroupIdToReturn = dataSourceToPersist.DataSourceId;
                }

                return parameterGroupIdToReturn;
            }
        }

        public void DeleteDataSource(int dataSourceId)
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(), descripition:$"dataSourceId={dataSourceId}"))
            {
                using (_dbContext.CreateAutoCommitScope())
                {
                    var foundDataSource = _dbContext.DataSources.Single(pg => pg.DataSourceId == dataSourceId);

                    _dbContext.DataSources.Remove(entity:foundDataSource);

                    _dbContext.SaveChanges();
                }
            }
        }

        public List<DataSourceDto> GetDataSources(int organizationRegulatoryProgramId)
        {
            var dataSources = _dbContext.DataSources.Where(d => d.OrganizationRegulatoryProgramId == organizationRegulatoryProgramId).ToList();

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var authOrgRegProgramId = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationRegulatoryProgramId;
            var timeZoneId = Convert.ToInt32(value:_settings.GetOrganizationSettingValue(orgRegProgramId:authOrgRegProgramId, settingType:SettingType.TimeZone));

            var dataSourceDtos = new List<DataSourceDto>();
            foreach (var ds in dataSources)
            {
                var dataSourceDto = _mapHelper.GetDataSourceDtoFroDataSource(dataSource:ds);

                dataSourceDto.LastModificationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:ds.LastModificationDateTimeUtc.HasValue
                                                                             ? ds.LastModificationDateTimeUtc.Value.UtcDateTime
                                                                             : ds.CreationDateTimeUtc.UtcDateTime, timeZoneId:timeZoneId);
                if (ds.LastModifierUserId.HasValue)
                {
                    var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == ds.LastModifierUserId.Value);
                    dataSourceDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                }
                else
                {
                    dataSourceDto.LastModifierFullName = "N/A";
                }

                dataSourceDtos.Add(item:dataSourceDto);
            }

            return dataSourceDtos;
        }

        public DataSourceDto GetDataSource(int organizationRegulatoryProgramId, string name)
        {
            var existingDataSource = GetExistingDataSourceByName(organizationRegulatoryProgramId:organizationRegulatoryProgramId, name:name);
            return existingDataSource == null ? null : _mapHelper.GetDataSourceDtoFroDataSource(dataSource:existingDataSource);
        }

        public DataSourceDto GetDataSourceById(int dataSourceId)
        {
            var existingDataSource = _dbContext.DataSources.FirstOrDefault(param => param.DataSourceId == dataSourceId);
            return existingDataSource == null ? null : _mapHelper.GetDataSourceDtoFroDataSource(dataSource:existingDataSource);
        }

        public Dictionary<string, DataSourceTranslationItemDto> GetDataSourceTranslationDict(int dataSourceId, DataSourceTranslationType translationType)
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            var caseInsensitiveDictionary = new Dictionary<string, DataSourceTranslationItemDto>(comparer:comparer);

            foreach (var dataSourceTranslationDto in GetDataSourceTranslations(dataSourceId:dataSourceId, translationType:translationType))
            {
                caseInsensitiveDictionary.Add(key: dataSourceTranslationDto.DataSourceTerm, 
                                              value: dataSourceTranslationDto.TranslationItem);
            }

            return caseInsensitiveDictionary;
        }

        public List<DataSourceTranslationDto> GetDataSourceTranslations(int dataSourceId, DataSourceTranslationType translationType)
        {
            var dataSource = _dbContext.DataSources.FirstOrDefault(param => param.DataSourceId == dataSourceId);
            if (dataSource == null)
            {
                throw CreateRuleViolationExceptionForValidationError(errorMessage:"Data Source is not found");
            }

            switch (translationType)
            {
                case DataSourceTranslationType.MonitoringPoint: return GetDataSourceMonitoringPointTranslations(dataSource: dataSource);
                case DataSourceTranslationType.SampleType: return GetDataSourceSampleTypeTranslations(dataSource: dataSource);
                case DataSourceTranslationType.CollectionMethod: return GetDataSourceCollectionMethodTranslations(dataSource: dataSource);
                case DataSourceTranslationType.Parameter: return GetDataSourceParameterTranslations(dataSource: dataSource);
                case DataSourceTranslationType.Unit: return GetDataSourceUnitTranslations(dataSource: dataSource);
                default: throw CreateRuleViolationExceptionForValidationError(errorMessage:"DataSourceType is unsupported");
            }
        }
        private List<DataSourceTranslationDto> GetDataSourceMonitoringPointTranslations(Core.Domain.DataSource dataSource) {
            return _dbContext.DataSourceMonitoringPoints.Where(d => d.DataSourceId == dataSource.DataSourceId)
                             .OrderBy(d => d.DataSourceTerm).ToList()
                             .Select(x => _mapHelper.ToDataSourceMonitoringPointDto(x))
                             .ToList();
        }
        private List<DataSourceTranslationDto> GetDataSourceSampleTypeTranslations(Core.Domain.DataSource dataSource)
        {
            return _dbContext.DataSourceCtsEventTypes.Where(d => d.DataSourceId == dataSource.DataSourceId)
                             .OrderBy(d => d.DataSourceTerm).ToList()
                             .Select(x => _mapHelper.ToDataSourceSampleTypeDto(x))
                             .ToList();
        }
        private List<DataSourceTranslationDto> GetDataSourceCollectionMethodTranslations(Core.Domain.DataSource dataSource)
        {
            return _dbContext.DataSourceCollectionMethods.Where(d => d.DataSourceId == dataSource.DataSourceId)
                             .OrderBy(d => d.DataSourceTerm).ToList()
                             .Select(x => _mapHelper.ToDataSourceCollectionMethodDto(x))
                             .ToList();
        }
        private List<DataSourceTranslationDto> GetDataSourceParameterTranslations(Core.Domain.DataSource dataSource)
        {
            return _dbContext.DataSourceParameters.Where(d => d.DataSourceId == dataSource.DataSourceId)
                             .OrderBy(d => d.DataSourceTerm).ToList()
                             .Select(x => _mapHelper.ToDataSourceParameterDto(x))
                             .ToList();
        }
        private List<DataSourceTranslationDto> GetDataSourceUnitTranslations(Core.Domain.DataSource dataSource)
        {
            return _dbContext.DataSourceUnits.Where(d => d.DataSourceId == dataSource.DataSourceId)
                             .OrderBy(d => d.DataSourceTerm).ToList()
                             .Select(x => _mapHelper.ToDataSourceUnitDto(x))
                             .ToList();
        }
        #endregion

        private Core.Domain.DataSource GetExistingDataSourceByName(int organizationRegulatoryProgramId, string name)
        {
            return _dbContext.DataSources.FirstOrDefault(ds => string.Equals(ds.Name.Trim(), name.Trim())
                                                               && ds.OrganizationRegulatoryProgramId == organizationRegulatoryProgramId);
        }

        private Core.Domain.DataSource GetExistingDataSource(int organizationRegulatoryProgramId, DataSourceDto dataSourceDto)
        {
            if (dataSourceDto.DataSourceId.HasValue)
            {
                return _dbContext.DataSources.FirstOrDefault(param => param.DataSourceId == dataSourceDto.DataSourceId);
            }

            return _dbContext.DataSources.FirstOrDefault(ds => string.Equals(ds.Name.Trim(), dataSourceDto.Name.Trim())
                                                               && ds.OrganizationRegulatoryProgramId == organizationRegulatoryProgramId);
        }
    }
}