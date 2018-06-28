using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
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
                using (_dbContext.BeginTransactionScope(from:MethodBase.GetCurrentMethod()))
                {
                    var existingDataSourceByName = GetExistingDataSourceByName(organizationRegulatoryProgramId:currentOrgRegProgramId, name:dataSourceDto.Name);
                    var existingDataSourceById = GetExistingDataSourceById(dataSourceId:dataSourceDto.DataSourceId);

                    var doesNewDataSourceWithAlreadyExistName = existingDataSourceById == null && existingDataSourceByName != null;
                    var doesUpdateDataSourceWithAlreadyExistName = existingDataSourceById != null && existingDataSourceByName != null
                                                                   && existingDataSourceById.DataSourceId != existingDataSourceByName.DataSourceId;
                    if (doesNewDataSourceWithAlreadyExistName || doesUpdateDataSourceWithAlreadyExistName)
                    {
                        throw CreateRuleViolationExceptionForValidationError(errorMessage: "A Data Provider with the name already exists. Please select another name.");
                    }

                    Core.Domain.DataSource dataSourceToPersist;
                    if (existingDataSourceById != null)
                    {
                        dataSourceToPersist = _mapHelper.GetDataSourceFromDataSourceDto(dto:dataSourceDto, existingDataSource: existingDataSourceById);
                        dataSourceToPersist.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                        dataSourceToPersist.LastModificationDateTimeUtc = DateTime.UtcNow;
                        dataSourceToPersist.LastModifierUserId = currentUserProfileId;
                    }
                    else
                    {
                        dataSourceToPersist = _mapHelper.GetDataSourceFromDataSourceDto(dto:dataSourceDto, existingDataSource:null);
                        dataSourceToPersist.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                        dataSourceToPersist.CreationDateTimeUtc = DateTime.UtcNow;
                        dataSourceToPersist.LastModificationDateTimeUtc = DateTime.UtcNow;
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
                using (_dbContext.BeginTransactionScope(from:MethodBase.GetCurrentMethod()))
                {
                    var foundDataSource = _dbContext.DataSources
                                                    .Include(ds => ds.DataSourceMonitoringPoints)
                                                    .Include(ds => ds.DataSourceCollectionMethods)
                                                    .Include(ds => ds.DataSourceCtsEventTypes)
                                                    .Include(ds => ds.DataSourceParameters)
                                                    .Include(ds => ds.DataSourceUnits)
                                                    .First(ds => ds.DataSourceId == dataSourceId);

                    RemoveEntities(entitySet:_dbContext.DataSourceMonitoringPoints, entitiesToRemove:foundDataSource.DataSourceMonitoringPoints);
                    RemoveEntities(entitySet:_dbContext.DataSourceCollectionMethods, entitiesToRemove:foundDataSource.DataSourceCollectionMethods);
                    RemoveEntities(entitySet:_dbContext.DataSourceCtsEventTypes, entitiesToRemove:foundDataSource.DataSourceCtsEventTypes);
                    RemoveEntities(entitySet:_dbContext.DataSourceParameters, entitiesToRemove:foundDataSource.DataSourceParameters);
                    RemoveEntities(entitySet:_dbContext.DataSourceUnits, entitiesToRemove:foundDataSource.DataSourceUnits);
                    RemoveEntity(entitySet:_dbContext.DataSources, entityToRemove:foundDataSource);

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

        public DataSourceDto GetDataSourceById(int dataSourceId, bool withDataTranslations = false)
        {
            var existingDataSource = withDataTranslations
                                         ? _dbContext.DataSources
                                                     .Include(ds => ds.DataSourceMonitoringPoints)
                                                     .Include(ds => ds.DataSourceMonitoringPoints.Select(x => x.MonitoringPoint))
                                                     .Include(ds => ds.DataSourceCollectionMethods)
                                                     .Include(ds => ds.DataSourceCollectionMethods.Select(x => x.CollectionMethod))
                                                     .Include(ds => ds.DataSourceCtsEventTypes)
                                                     .Include(ds => ds.DataSourceCtsEventTypes.Select(x => x.CtsEventType))
                                                     .Include(ds => ds.DataSourceParameters)
                                                     .Include(ds => ds.DataSourceParameters.Select(x => x.Parameter))
                                                     .Include(ds => ds.DataSourceUnits)
                                                     .Include(ds => ds.DataSourceUnits.Select(x => x.Unit))
                                                     .FirstOrDefault(ds => ds.DataSourceId == dataSourceId)
                                         : _dbContext.DataSources.FirstOrDefault(param => param.DataSourceId == dataSourceId);

            return existingDataSource == null ? null : _mapHelper.GetDataSourceDtoFroDataSource(dataSource: existingDataSource);
        }

        public Dictionary<string, DataSourceTranslationItemDto> GetDataSourceTranslationDict(int dataSourceId, DataSourceTranslationType translationType)
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            var caseInsensitiveDictionary = new Dictionary<string, DataSourceTranslationItemDto>(comparer:comparer);

            foreach (var dataSourceTranslationDto in GetDataSourceTranslations(dataSourceId:dataSourceId, translationType:translationType))
            {
                caseInsensitiveDictionary.Add(key:dataSourceTranslationDto.DataSourceTerm,
                                              value:dataSourceTranslationDto.TranslationItem);
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
                case DataSourceTranslationType.MonitoringPoint: return GetDataSourceMonitoringPointTranslations(dataSource:dataSource);
                case DataSourceTranslationType.SampleType: return GetDataSourceSampleTypeTranslations(dataSource:dataSource);
                case DataSourceTranslationType.CollectionMethod: return GetDataSourceCollectionMethodTranslations(dataSource:dataSource);
                case DataSourceTranslationType.Parameter: return GetDataSourceParameterTranslations(dataSource:dataSource);
                case DataSourceTranslationType.Unit: return GetDataSourceUnitTranslations(dataSource:dataSource);
                default: throw CreateRuleViolationExceptionForValidationError(errorMessage:$"DataSourceTranslationType {translationType} is unsupported");
            }
        }

        public int SaveDataSourceTranslation(DataSourceTranslationDto dataSourceTranslation, DataSourceTranslationType translationType)
        {
            using (_dbContext.BeginTransactionScope(from:MethodBase.GetCurrentMethod()))
            {
                ThrowRuleViolationErrorIfDataSourceNotExist(dataSourceTranslation: dataSourceTranslation, translationType: translationType);
                ThrowRuleViolationErrorIfDataSourceTermIsAlreadyExist(dataSourceTranslation:dataSourceTranslation, translationType:translationType);
                ThrowRuleViolationErrorIfTranslationItemIsInvalid(translationItem:dataSourceTranslation.TranslationItem, translationType: translationType);
                switch (translationType)
                {
                    case DataSourceTranslationType.MonitoringPoint:
                        var existingDataSourceMonitoringPoint =
                            _dbContext.DataSourceMonitoringPoints.FirstOrDefault(x => x.DataSourceMonitoringPointId == dataSourceTranslation.Id);
                        var dataSourceMonitoringPointToPersist =
                            _mapHelper.ToDataSourceMonitoringPoint(from:dataSourceTranslation, existingDomainObject:existingDataSourceMonitoringPoint);
                        if (existingDataSourceMonitoringPoint == null)
                        {
                            _dbContext.DataSourceMonitoringPoints.Add(entity:dataSourceMonitoringPointToPersist);
                        }

                        _dbContext.SaveChanges();
                        return dataSourceMonitoringPointToPersist.DataSourceMonitoringPointId;
                    case DataSourceTranslationType.SampleType:
                        var existingDataSourceCtsEventType = _dbContext.DataSourceCtsEventTypes.FirstOrDefault(x => x.DataSourceCtsEventTypeId == dataSourceTranslation.Id);
                        var dataSourceCtsEventTypeToPersist = _mapHelper.ToDataSourceSampleType(from:dataSourceTranslation, existingDomainObject:existingDataSourceCtsEventType);
                        if (existingDataSourceCtsEventType == null)
                        {
                            _dbContext.DataSourceCtsEventTypes.Add(entity:dataSourceCtsEventTypeToPersist);
                        }

                        _dbContext.SaveChanges();
                        return dataSourceCtsEventTypeToPersist.DataSourceCtsEventTypeId;
                    case DataSourceTranslationType.CollectionMethod:
                        var existingDataSourceCollectionMethod =
                            _dbContext.DataSourceCollectionMethods.FirstOrDefault(x => x.DataSourceCollectionMethodId == dataSourceTranslation.Id);
                        var dataSourceCollectionMethodToPersist =
                            _mapHelper.ToDataSourceCollectionMethod(from:dataSourceTranslation, existingDomainObject:existingDataSourceCollectionMethod);
                        if (existingDataSourceCollectionMethod == null)
                        {
                            _dbContext.DataSourceCollectionMethods.Add(entity:dataSourceCollectionMethodToPersist);
                        }

                        _dbContext.SaveChanges();
                        return dataSourceCollectionMethodToPersist.DataSourceCollectionMethodId;
                    case DataSourceTranslationType.Parameter:
                        var existingDataSourceParameter = _dbContext.DataSourceParameters.FirstOrDefault(x => x.DataSourceParameterId == dataSourceTranslation.Id);
                        var dataSourceParameterToPersist = _mapHelper.ToDataSourceParameter(from:dataSourceTranslation, existingDomainObject:existingDataSourceParameter);
                        if (existingDataSourceParameter == null)
                        {
                            _dbContext.DataSourceParameters.Add(entity:dataSourceParameterToPersist);
                        }

                        _dbContext.SaveChanges();
                        return dataSourceParameterToPersist.DataSourceParameterId;
                    case DataSourceTranslationType.Unit:
                        var existingDataSourceUnit = _dbContext.DataSourceUnits.FirstOrDefault(x => x.DataSourceUnitId == dataSourceTranslation.Id);
                        var dataSourceUnitToPersist = _mapHelper.ToDataSourceUnit(from:dataSourceTranslation, existingDomainObject:existingDataSourceUnit);
                        if (existingDataSourceUnit == null)
                        {
                            _dbContext.DataSourceUnits.Add(entity:dataSourceUnitToPersist);
                        }

                        _dbContext.SaveChanges();
                        return dataSourceUnitToPersist.DataSourceUnitId;
                    default: throw CreateRuleViolationExceptionForValidationError(errorMessage:$"DataSourceTranslationType {translationType} is unsupported");
                }
            }
        }

        public void SaveDataSourceTranslations(int dataSourceId, IEnumerable<DataSourceTranslationItemDto> dataSourceTranslations)
        {
            using (_dbContext.BeginTransactionScope(from: MethodBase.GetCurrentMethod()))
            {
                foreach (var translationItem in dataSourceTranslations)
                {
                    switch (translationItem.TranslationType)
                    {
                        case DataSourceTranslationType.MonitoringPoint:
                            var dataSourceMonitoringPoint = new DataSourceMonitoringPoint
                                                            {
                                                                DataSourceId = dataSourceId,
                                                                DataSourceTerm = translationItem.TranslationName,
                                                                MonitoringPointId = translationItem.TranslationId
                                                            };
                            _dbContext.DataSourceMonitoringPoints.Add(entity: dataSourceMonitoringPoint);
                            break;
                        case DataSourceTranslationType.SampleType:
                            var dataSourceCtsEventType = new DataSourceCtsEventType
                                                         {
                                                             DataSourceId = dataSourceId,
                                                             DataSourceTerm = translationItem.TranslationName,
                                                             CtsEventTypeId = translationItem.TranslationId
                                                         };
                            _dbContext.DataSourceCtsEventTypes.Add(entity: dataSourceCtsEventType);
                            break;
                        case DataSourceTranslationType.CollectionMethod:
                            var dataSourceCollectionMethod = new DataSourceCollectionMethod
                                                             {
                                                                 DataSourceId = dataSourceId,
                                                                 DataSourceTerm = translationItem.TranslationName,
                                                                 CollectionMethodId = translationItem.TranslationId
                                                             };
                            _dbContext.DataSourceCollectionMethods.Add(entity: dataSourceCollectionMethod);
                            break;
                        case DataSourceTranslationType.Parameter:
                            var dataSourceParameter = new DataSourceParameter
                                                      {
                                                          DataSourceId = dataSourceId,
                                                          DataSourceTerm = translationItem.TranslationName,
                                                          ParameterId = translationItem.TranslationId
                                                      };
                            _dbContext.DataSourceParameters.Add(entity: dataSourceParameter);
                            break;
                        case DataSourceTranslationType.Unit:
                            var dataSourceUnit = new DataSourceUnit
                                                 {
                                                     DataSourceId = dataSourceId,
                                                     DataSourceTerm = translationItem.TranslationName,
                                                     UnitId = translationItem.TranslationId
                                                 };
                            _dbContext.DataSourceUnits.Add(entity: dataSourceUnit);
                            break;
                        default: throw new InternalServerError(message:$"DataSourceTranslationType {translationItem.TranslationType} is unsupported");
                    }
                }

                _dbContext.SaveChanges();
            }
        }

        public void DeleteDataSourceTranslation(DataSourceTranslationDto dataSourceTranslation, DataSourceTranslationType translationType)
        {
            using (_dbContext.BeginTransactionScope(from:MethodBase.GetCurrentMethod()))
            {
                switch (translationType)
                {
                    case DataSourceTranslationType.MonitoringPoint:
                        var dataSourceMonitoringPointToDelete = _dbContext.DataSourceMonitoringPoints.Single(x => x.DataSourceMonitoringPointId == dataSourceTranslation.Id);
                        _dbContext.DataSourceMonitoringPoints.Remove(entity:dataSourceMonitoringPointToDelete);
                        break;
                    case DataSourceTranslationType.SampleType:
                        var dataSourceCtsEventTypeToDelete = _dbContext.DataSourceCtsEventTypes.Single(x => x.DataSourceCtsEventTypeId == dataSourceTranslation.Id);
                        _dbContext.DataSourceCtsEventTypes.Remove(entity:dataSourceCtsEventTypeToDelete);
                        break;
                    case DataSourceTranslationType.CollectionMethod:
                        var dataSourceCollectionMethodToDelete = _dbContext.DataSourceCollectionMethods.Single(x => x.DataSourceCollectionMethodId == dataSourceTranslation.Id);
                        _dbContext.DataSourceCollectionMethods.Remove(entity:dataSourceCollectionMethodToDelete);
                        break;
                    case DataSourceTranslationType.Parameter:
                        var dataSourceParameterToDelete = _dbContext.DataSourceParameters.Single(x => x.DataSourceParameterId == dataSourceTranslation.Id);
                        _dbContext.DataSourceParameters.Remove(entity:dataSourceParameterToDelete);
                        break;
                    case DataSourceTranslationType.Unit:
                        var dataSourceUnitToDelete = _dbContext.DataSourceUnits.Single(x => x.DataSourceUnitId == dataSourceTranslation.Id);
                        _dbContext.DataSourceUnits.Remove(entity:dataSourceUnitToDelete);
                        break;
                    default: throw new InternalServerError(message: $"DataSourceTranslationType {translationType} is unsupported");
                }
                _dbContext.SaveChanges();
            }
        }

        public void DeleteInvalidDataSourceTranslations(int dataSourceId)
        {
            var currentOrgRegProgramId = int.Parse(s: _httpContext.GetClaimValue(claimType: CacheKey.OrganizationRegulatoryProgramId));
            var authorityOrganization = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId);
            var authorityOrganizationId = authorityOrganization.OrganizationId;
            var authorityOrgRegProgramId = authorityOrganization.OrganizationRegulatoryProgramId;

            var shouldSaveChanges = false;
            using (_dbContext.BeginTransactionScope(from:MethodBase.GetCurrentMethod()))
            {
                var invalidDataSourceMonitoringPoints = (from t in _dbContext.DataSourceMonitoringPoints
                                                         join d in _dbContext.DataSources on t.DataSourceId equals d.DataSourceId
                                                         join s in _dbContext.MonitoringPoints on t.MonitoringPointId equals s.MonitoringPointId
                                                         where d.OrganizationRegulatoryProgramId == currentOrgRegProgramId
                                                               && s.OrganizationRegulatoryProgramId == currentOrgRegProgramId
                                                               && (s.IsRemoved || !s.IsEnabled)
                                                         select t).ToList();
                if (invalidDataSourceMonitoringPoints.Any())
                {
                    _logger.Info(message:"Found unavailable Monitoring Point Translation(s): {0}", 
                                 argument:string.Join(separator:",", values:invalidDataSourceMonitoringPoints.Select(x => x.DataSourceTerm)));
                    _dbContext.DataSourceMonitoringPoints.RemoveRange(entities:invalidDataSourceMonitoringPoints);
                    shouldSaveChanges = true;
                }

                var invalidDataSourceCollectionMethods = (from t in _dbContext.DataSourceCollectionMethods
                                                          join d in _dbContext.DataSources on t.DataSourceId equals d.DataSourceId
                                                          join s in _dbContext.CollectionMethods on t.CollectionMethodId equals s.CollectionMethodId
                                                          where d.OrganizationRegulatoryProgramId == currentOrgRegProgramId
                                                                && s.OrganizationId == authorityOrganizationId
                                                                && (s.IsRemoved || !s.IsEnabled)
                                                          select t).ToList();
                if (invalidDataSourceCollectionMethods.Any())
                {
                    _logger.Info(message: "Found unavailable Collection Method Translation(s): {0}",
                                 argument: string.Join(separator: ",", values: invalidDataSourceCollectionMethods.Select(x => x.DataSourceTerm)));
                    _dbContext.DataSourceCollectionMethods.RemoveRange(entities:invalidDataSourceCollectionMethods);
                    shouldSaveChanges = true;
                }

                var invalidDataSourceSampleTypes = (from t in _dbContext.DataSourceCtsEventTypes
                                                    join d in _dbContext.DataSources on t.DataSourceId equals d.DataSourceId
                                                    join s in _dbContext.CtsEventTypes on t.CtsEventTypeId equals s.CtsEventTypeId
                                                    where d.OrganizationRegulatoryProgramId == currentOrgRegProgramId 
                                                          && s.OrganizationRegulatoryProgramId == authorityOrgRegProgramId
                                                          && s.CtsEventCategoryName == "SAMPLE"
                                                          && (s.IsRemoved || !s.IsEnabled)
                                                    select t).ToList();
                if (invalidDataSourceSampleTypes.Any())
                {
                    _logger.Info(message: "Found unavailable Sample Type Translation(s): {0}",
                                 argument: string.Join(separator: ",", values: invalidDataSourceSampleTypes.Select(x => x.DataSourceTerm)));
                    _dbContext.DataSourceCtsEventTypes.RemoveRange(entities:invalidDataSourceSampleTypes);
                    shouldSaveChanges = true;
                }

                var invalidDataSourceParameters = (from t in _dbContext.DataSourceParameters
                                                   join d in _dbContext.DataSources on t.DataSourceId equals d.DataSourceId
                                                   join s in _dbContext.Parameters on t.ParameterId equals s.ParameterId
                                                   where d.OrganizationRegulatoryProgramId == currentOrgRegProgramId
                                                          && s.OrganizationRegulatoryProgramId == authorityOrgRegProgramId
                                                          && s.IsRemoved
                                                    select t).ToList();
                if (invalidDataSourceParameters.Any())
                {
                    _logger.Info(message: "Found unavailable Parameter Translation(s): {0}",
                                 argument: string.Join(separator: ",", values: invalidDataSourceParameters.Select(x => x.DataSourceTerm)));
                    _dbContext.DataSourceParameters.RemoveRange(entities:invalidDataSourceParameters);
                    shouldSaveChanges = true;
                }

                var invalidDataSourceUnits = (from t in _dbContext.DataSourceUnits
                                              join d in _dbContext.DataSources on t.DataSourceId equals d.DataSourceId
                                              join s in _dbContext.Units on t.UnitId equals s.UnitId
                                              where d.OrganizationRegulatoryProgramId == currentOrgRegProgramId
                                                    && s.OrganizationId == authorityOrganizationId
                                                    && (s.IsRemoved || !s.IsAvailableToRegulatee)
                                              select t).ToList();
                if (invalidDataSourceUnits.Any())
                {
                    _logger.Info(message: "Found unavailable Unit Translation(s): {0}",
                                 argument: string.Join(separator: ",", values: invalidDataSourceUnits.Select(x => x.DataSourceTerm)));
                    _dbContext.DataSourceUnits.RemoveRange(entities:invalidDataSourceUnits);
                    shouldSaveChanges = true;
                }

                if (shouldSaveChanges)
                {
                    _dbContext.SaveChanges();
                }
            }
        }
        #endregion

        private List<DataSourceTranslationDto> GetDataSourceMonitoringPointTranslations(Core.Domain.DataSource dataSource)
        {
            return _dbContext.DataSourceMonitoringPoints.Where(d => d.DataSourceId == dataSource.DataSourceId)
                             .OrderBy(d => d.DataSourceTerm).ToList()
                             .Select(x => _mapHelper.ToDataSourceMonitoringPointDto(from:x))
                             .ToList();
        }

        private List<DataSourceTranslationDto> GetDataSourceSampleTypeTranslations(Core.Domain.DataSource dataSource)
        {
            return _dbContext.DataSourceCtsEventTypes.Where(d => d.DataSourceId == dataSource.DataSourceId)
                             .OrderBy(d => d.DataSourceTerm).ToList()
                             .Select(x => _mapHelper.ToDataSourceSampleTypeDto(from:x))
                             .ToList();
        }

        private List<DataSourceTranslationDto> GetDataSourceCollectionMethodTranslations(Core.Domain.DataSource dataSource)
        {
            return _dbContext.DataSourceCollectionMethods.Where(d => d.DataSourceId == dataSource.DataSourceId)
                             .OrderBy(d => d.DataSourceTerm).ToList()
                             .Select(x => _mapHelper.ToDataSourceCollectionMethodDto(from:x))
                             .ToList();
        }

        private List<DataSourceTranslationDto> GetDataSourceParameterTranslations(Core.Domain.DataSource dataSource)
        {
            return _dbContext.DataSourceParameters.Where(d => d.DataSourceId == dataSource.DataSourceId)
                             .OrderBy(d => d.DataSourceTerm).ToList()
                             .Select(x => _mapHelper.ToDataSourceParameterDto(from:x))
                             .ToList();
        }

        private List<DataSourceTranslationDto> GetDataSourceUnitTranslations(Core.Domain.DataSource dataSource)
        {
            return _dbContext.DataSourceUnits.Where(d => d.DataSourceId == dataSource.DataSourceId)
                             .OrderBy(d => d.DataSourceTerm).ToList()
                             .Select(x => _mapHelper.ToDataSourceUnitDto(from:x))
                             .ToList();
        }

        private void ThrowRuleViolationErrorIfDataSourceNotExist(DataSourceTranslationDto dataSourceTranslation, DataSourceTranslationType translationType)
        {
            var doesDataSourceExist = _dbContext.DataSources.Any(x => x.DataSourceId == dataSourceTranslation.DataSourceId);
            if (!doesDataSourceExist)
            {
                throw CreateRuleViolationExceptionForValidationError(errorMessage: ErrorConstants.SampleImport.DataProviderDoesNotExist);
            }
        }

        private void ThrowRuleViolationErrorIfDataSourceTermIsAlreadyExist(DataSourceTranslationDto dataSourceTranslation, DataSourceTranslationType translationType)
        {
            bool doesExist;
            var dataSourceTerm = dataSourceTranslation.DataSourceTerm.ToLower();
            switch (translationType)
            {
                case DataSourceTranslationType.MonitoringPoint:
                    doesExist = dataSourceTranslation.Id.HasValue
                                    ? _dbContext.DataSourceMonitoringPoints.Count(x => x.DataSourceTerm.ToLower().Equals(dataSourceTerm)
                                                                                       && x.DataSourceId == dataSourceTranslation.DataSourceId
                                                                                       && x.DataSourceMonitoringPointId != dataSourceTranslation.Id)
                                      > 0
                                    : _dbContext.DataSourceMonitoringPoints.Count(x => x.DataSourceTerm.ToLower().Equals(dataSourceTerm)
                                                                                       && x.DataSourceId == dataSourceTranslation.DataSourceId)
                                      > 0;
                    break;
                case DataSourceTranslationType.SampleType:
                    doesExist = dataSourceTranslation.Id.HasValue
                                    ? _dbContext.DataSourceCtsEventTypes.Count(x => x.DataSourceTerm.ToLower().Equals(dataSourceTerm)
                                                                                    && x.DataSourceId == dataSourceTranslation.DataSourceId
                                                                                    && x.DataSourceCtsEventTypeId != dataSourceTranslation.Id)
                                      > 0
                                    : _dbContext.DataSourceCtsEventTypes.Count(x => x.DataSourceTerm.ToLower().Equals(dataSourceTerm)
                                                                                    && x.DataSourceId == dataSourceTranslation.DataSourceId)
                                      > 0;
                    break;
                case DataSourceTranslationType.CollectionMethod:
                    doesExist = dataSourceTranslation.Id.HasValue
                                    ? _dbContext.DataSourceCollectionMethods.Count(x => x.DataSourceTerm.ToLower().Equals(dataSourceTerm)
                                                                                        && x.DataSourceId == dataSourceTranslation.DataSourceId
                                                                                        && x.DataSourceCollectionMethodId != dataSourceTranslation.Id)
                                      > 0
                                    : _dbContext.DataSourceCollectionMethods.Count(x => x.DataSourceTerm.ToLower().Equals(dataSourceTerm)
                                                                                        && x.DataSourceId == dataSourceTranslation.DataSourceId)
                                      > 0;
                    break;
                case DataSourceTranslationType.Parameter:
                    doesExist = dataSourceTranslation.Id.HasValue
                                    ? _dbContext.DataSourceParameters.Count(x => x.DataSourceTerm.ToLower().Equals(dataSourceTerm)
                                                                                 && x.DataSourceId == dataSourceTranslation.DataSourceId
                                                                                 && x.DataSourceParameterId != dataSourceTranslation.Id)
                                      > 0
                                    : _dbContext.DataSourceParameters.Count(x => x.DataSourceTerm.ToLower().Equals(dataSourceTerm)
                                                                                 && x.DataSourceId == dataSourceTranslation.DataSourceId)
                                      > 0;
                    break;
                case DataSourceTranslationType.Unit:
                    doesExist = dataSourceTranslation.Id.HasValue
                                    ? _dbContext.DataSourceUnits.Count(x => x.DataSourceTerm.ToLower().Equals(dataSourceTerm)
                                                                            && x.DataSourceId == dataSourceTranslation.DataSourceId
                                                                            && x.DataSourceUnitId != dataSourceTranslation.Id)
                                      > 0
                                    : _dbContext.DataSourceUnits.Count(x => x.DataSourceTerm.ToLower().Equals(dataSourceTerm)
                                                                            && x.DataSourceId == dataSourceTranslation.DataSourceId)
                                      > 0;
                    break;
                default: throw CreateRuleViolationExceptionForValidationError(errorMessage:$"DataSourceTranslationType {translationType} is unsupported");
            }
            if (doesExist)
            {
                throw CreateRuleViolationExceptionForValidationError(errorMessage:$"An Import File Term \"{dataSourceTranslation.DataSourceTerm}\" already exists. Please select another name.");
            }
        }

        private void ThrowRuleViolationErrorIfTranslationItemIsInvalid(DataSourceTranslationItemDto translationItem, DataSourceTranslationType translationType)
        {
            var isInvalid = false;
            switch (translationType)
            {
                case DataSourceTranslationType.MonitoringPoint:
                    var monitoringPoint = _dbContext.MonitoringPoints.FirstOrDefault(x => x.MonitoringPointId == translationItem.TranslationId);
                    isInvalid = monitoringPoint == null || !monitoringPoint.IsEnabled || monitoringPoint.IsRemoved;
                    break;
                case DataSourceTranslationType.SampleType:
                    var sampleType = _dbContext.CtsEventTypes.FirstOrDefault(x => x.CtsEventTypeId == translationItem.TranslationId);
                    isInvalid = sampleType == null || !sampleType.IsEnabled || sampleType.IsRemoved;
                    break;
                case DataSourceTranslationType.CollectionMethod:
                    var collectionMethod = _dbContext.CollectionMethods.FirstOrDefault(x => x.CollectionMethodId == translationItem.TranslationId);
                    isInvalid = collectionMethod == null || !collectionMethod.IsEnabled || collectionMethod.IsRemoved;
                    break;
                case DataSourceTranslationType.Parameter:
                    var parameter = _dbContext.Parameters.FirstOrDefault(x => x.ParameterId == translationItem.TranslationId);
                    isInvalid = parameter == null || parameter.IsRemoved;
                    break;
                case DataSourceTranslationType.Unit:
                    var unit = _dbContext.Units.FirstOrDefault(x => x.UnitId == translationItem.TranslationId);
                    isInvalid = unit == null || unit.IsRemoved || !unit.IsAvailableToRegulatee;
                    break;
                default: throw CreateRuleViolationExceptionForValidationError(errorMessage: $"DataSourceTranslationType {translationType} is unsupported");
            }

            if (!isInvalid)
            {
                return;
            }

            var errorMessage = string.Format(format: ErrorConstants.SampleImport.LinkoExchangeTermNoLongerAvailable,
                                             arg0: DataSourceHelper.GetTranslatedTypeDomainName(translationType: translationType),
                                             arg1: translationItem.TranslationName);
            throw CreateRuleViolationExceptionForValidationError(errorMessage: errorMessage);
        }

        private Core.Domain.DataSource GetExistingDataSourceByName(int organizationRegulatoryProgramId, string name)
        {
            return _dbContext.DataSources.FirstOrDefault(ds => string.Equals(ds.Name.Trim(), name.Trim())
                                                               && ds.OrganizationRegulatoryProgramId == organizationRegulatoryProgramId);
        }

        private Core.Domain.DataSource GetExistingDataSourceById(int? dataSourceId)
        {
            return dataSourceId.HasValue ? _dbContext.DataSources.FirstOrDefault(param => param.DataSourceId == dataSourceId) : null;
        }

        
    }

    public class DataSourceHelper
    {
        public static string GetTranslatedTypeDomainName(DataSourceTranslationType translationType, bool plural = false)
        {
            switch (translationType)
            {
                case DataSourceTranslationType.MonitoringPoint: return plural ? "Monitoring Points" : "Monitoring Point";
                case DataSourceTranslationType.SampleType: return plural ? "Sample Types" : "Sample Type";
                case DataSourceTranslationType.CollectionMethod: return plural ? "Collection Methods" : "Collection Method";
                case DataSourceTranslationType.Parameter: return plural ? "Parameters" : "Parameter";
                case DataSourceTranslationType.Unit: return plural ? "Units" : "Unit";
                default: throw new NotImplementedException(message: $"DataSourceTranslationType {translationType} is unsupported");
            }
        }
    }
}