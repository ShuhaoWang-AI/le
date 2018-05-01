using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using NLog;

namespace Linko.LinkoExchange.Services.DataSource
{
    public class DataSourceService : IDataSourceService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContext;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;

        #endregion

        #region constructors and destructor

        public DataSourceService(LinkoExchangeContext dbContext,
                                 IHttpContextService httpContext,
                                 ILogger logger,
                                 IMapHelper mapHelper)
        {
            _dbContext = dbContext;
            _httpContext = httpContext;
            _logger = logger;
            _mapHelper = mapHelper;
        }

        #endregion

        #region interface implementations

        public int SaveDataSource(DataSourceDto dataSourceDto)
        {
            var dataSourceIdString = dataSourceDto.DataSourceId?.ToString() ?? "null";
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod(), descripition:$"dataSourceId={dataSourceIdString}"))
            {
                var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                var currentUserProfileId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.UserProfileId));

                if (string.IsNullOrWhiteSpace(value: dataSourceDto.Name))
                {
                    throw CreateRuleViolationExceptionForValidationError(errorMessage: "Name is required.");
                }

                int parameterGroupIdToReturn;
                using (_dbContext.CreateAutoCommitScope())
                {
                    var existingDataSource = GetExistingDataSource(organziationRegulatoryProgramId:currentOrgRegProgramId, name:dataSourceDto.Name);
                    Core.Domain.DataSource dataSourceToPersist;
                    if (existingDataSource != null)
                    {
                        if (existingDataSource.DataSourceId != dataSourceDto.DataSourceId)
                        {
                            throw CreateRuleViolationExceptionForValidationError(errorMessage:"A DataSource with the name already exists. Please select another name.");
                        }
                        else
                        {
                            dataSourceToPersist = _mapHelper.GetDataSourceFroDataSourceDto(dto:dataSourceDto, existingDataSource:existingDataSource);
                            dataSourceToPersist.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                            dataSourceToPersist.LastModificationDateTimeUtc = DateTimeOffset.Now;
                            dataSourceToPersist.LastModifierUserId = currentUserProfileId;
                        }
                    }
                    else
                    {
                        dataSourceToPersist = _mapHelper.GetDataSourceFroDataSourceDto(dto:dataSourceDto, 
                                                                                       existingDataSource:new Core.Domain.DataSource
                                                                                                      {
                                                                                                          OrganizationRegulatoryProgramId = currentOrgRegProgramId,
                                                                                                          CreationDateTimeUtc = DateTimeOffset.Now,
                                                                                                          LastModificationDateTimeUtc = DateTimeOffset.Now,
                                                                                                          LastModifierUserId = currentUserProfileId
                                                                                                      });

                        _dbContext.DataSources.Add(entity:dataSourceToPersist);
                    }

                    _dbContext.SaveChanges();

                    parameterGroupIdToReturn = dataSourceToPersist.DataSourceId;
                }

                return parameterGroupIdToReturn;
            }
        }

        private static RuleViolationException CreateRuleViolationExceptionForValidationError(string errorMessage)
        {
            return new RuleViolationException("Validation errors", new RuleViolation(propertyName:string.Empty,
                                                                                     propertyValue:null,
                                                                                     errorMessage:errorMessage));
        }

        public void DeleteDataSource(int dataSourceId)
        {
            using (new MethodLogger(logger: _logger, methodBase: MethodBase.GetCurrentMethod(), descripition: $"dataSourceId={dataSourceId}"))
            using(_dbContext.CreateAutoCommitScope())
            {
                var foundDataSource = _dbContext.DataSources.Single(pg => pg.DataSourceId == dataSourceId);

                _dbContext.DataSources.Remove(entity:foundDataSource);

                _dbContext.SaveChanges();
            }
        }

        public List<DataSourceDto> GetDataSources(int organziationRegulatoryProgramId)
        {
            var dataSources = _dbContext.DataSources.Where(d => d.OrganizationRegulatoryProgramId == organziationRegulatoryProgramId).ToList();
            return dataSources.ConvertAll(s => _mapHelper.GetDataSourceDtoFroDataSource(dataSource:s));
        }

        public DataSourceDto GetDataSource(int organziationRegulatoryProgramId, string name)
        {
            var existingDataSource = GetExistingDataSource(organziationRegulatoryProgramId:organziationRegulatoryProgramId, name:name);
            return existingDataSource == null ? null : _mapHelper.GetDataSourceDtoFroDataSource(dataSource:existingDataSource);
        }

        /// <inheritdoc />
        public DataSourceDto GetDataSourceById(int dataSourceId)
        {
            var existingDataSource = _dbContext.DataSources.FirstOrDefault(param => param.DataSourceId == dataSourceId);
            return existingDataSource == null ? null : _mapHelper.GetDataSourceDtoFroDataSource(dataSource:existingDataSource);
        }

        private Core.Domain.DataSource GetExistingDataSource(int organziationRegulatoryProgramId, string name)
        {
            return _dbContext.DataSources.FirstOrDefault(ds => string.Equals(ds.Name.Trim(), name.Trim())
                                                               && ds.OrganizationRegulatoryProgramId == organziationRegulatoryProgramId);
        }

        #endregion
    }
}