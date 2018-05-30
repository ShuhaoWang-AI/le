using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Organization;
using NLog;

namespace Linko.LinkoExchange.Services.SelectList
{
    public class SelectListService : ISelectListService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContextService;
        private readonly ILogger _logger;
        private readonly IOrganizationService _orgService;

        #endregion

        #region constructors and destructor

        public SelectListService(
            LinkoExchangeContext dbContext,
            ILogger logger,
            IHttpContextService httpContextService,
            IOrganizationService orgService)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(paramName:nameof(dbContext));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(paramName:nameof(logger));
            }

            if (httpContextService == null)
            {
                throw new ArgumentNullException(paramName:nameof(httpContextService));
            }

            if (orgService == null)
            {
                throw new ArgumentNullException(paramName:nameof(orgService));
            }

            _dbContext = dbContext;
            _logger = logger;
            _httpContextService = httpContextService;
            _orgService = orgService;
        }

        #endregion

        private int GetIndustryOrganizationRegulatoryProgramId()
        {
            return int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
        }

        private int GetAuthorityOrganizationId()
        {
            return _orgService.GetAuthority(orgRegProgramId:GetIndustryOrganizationRegulatoryProgramId()).OrganizationId;
        }

        private int GetAuthorityOrganizationRegulatoryProgramId()
        {
            return _orgService.GetAuthority(orgRegProgramId:GetIndustryOrganizationRegulatoryProgramId()).OrganizationRegulatoryProgramId;
        }

        #region Implementation of ISelectListService

        /// <inheritdoc />
        public IEnumerable<ListItemDto> GetIndustryMonitoringPointSelectList()
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod()))
            {
                var industryOrganizationRegulatoryProgramId = GetIndustryOrganizationRegulatoryProgramId();

                var monitoringPoints = _dbContext.MonitoringPoints
                                                 .Where(x => x.OrganizationRegulatoryProgramId == industryOrganizationRegulatoryProgramId
                                                             && x.IsRemoved == false)
                                                 .Select(x => new {x.MonitoringPointId, x.Name})
                                                 .OrderBy(x => x.Name)
                                                 .ToList();

                return monitoringPoints.Select(x => new ListItemDto
                {
                                                        Id = x.MonitoringPointId,
                                                        DisplayValue = x.Name
                                                    });
            }
        }

        /// <inheritdoc />
        public IEnumerable<ListItemDto> GetAuthoritySampleTypeSelectList()
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod()))
            {
                var authorityOrganizationRegulatoryProgramId = GetAuthorityOrganizationRegulatoryProgramId();

                var sampleTypes = _dbContext.CtsEventTypes
                                            .Where(x => x.OrganizationRegulatoryProgramId == authorityOrganizationRegulatoryProgramId
                                                        && x.CtsEventCategoryName == "SAMPLE"
                                                        && x.IsRemoved == false)
                                            .Select(x => new {x.CtsEventTypeId, x.Name})
                                            .OrderBy(x => x.Name)
                                            .ToList();

                return sampleTypes.Select(x => new ListItemDto
                {
                                                   Id = x.CtsEventTypeId,
                                                   DisplayValue = x.Name
                                               });
            }
        }

        /// <inheritdoc />
        public IEnumerable<ListItemDto> GetAuthorityCollectionMethodSelectList()
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod()))
            {
                var authrotyOrganizationId = GetAuthorityOrganizationId();

                var collectionMethods = _dbContext.CollectionMethods
                                                  .Where(x => x.OrganizationId == authrotyOrganizationId 
                                                              && x.IsRemoved == false)
                                                  .Select(x => new {x.CollectionMethodId, x.Name})
                                                  .OrderBy(x => x.Name)
                                                  .ToList();

                return collectionMethods.Select(x => new ListItemDto
                {
                                                         Id = x.CollectionMethodId,
                                                         DisplayValue = x.Name
                                                     });
            }
        }

        /// <inheritdoc />
        public IEnumerable<ListItemDto> GetAuthorityParameterSelectList()
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod()))
            {
                var authorityOrganizationRegulatoryProgramId = GetAuthorityOrganizationRegulatoryProgramId();

                var parameters = _dbContext.Parameters
                                           .Where(x => x.OrganizationRegulatoryProgramId == authorityOrganizationRegulatoryProgramId 
                                                       && x.IsRemoved == false)
                                           .Select(x => new {x.ParameterId, x.Name})
                                           .OrderBy(x => x.Name)
                                           .ToList();

                return parameters.Select(x => new ListItemDto
                {
                                                  Id = x.ParameterId,
                                                  DisplayValue = x.Name
                                              });
            }
        }

        /// <inheritdoc />
        public IEnumerable<ListItemDto> GetAuthorityUnitSelectList()
        {
            using (new MethodLogger(logger:_logger, methodBase:MethodBase.GetCurrentMethod()))
            {
                var authorityOrganizationId = GetAuthorityOrganizationId();

                var units = _dbContext.Units
                                      .Where(x => x.OrganizationId == authorityOrganizationId 
                                                  && x.IsRemoved == false 
                                                  && x.IsAvailableToRegulatee)
                                      .Select(x => new {x.UnitId, x.Name, x.Description})
                                      .OrderBy(x => x.Name)
                                      .ToList();

                return units.Select(x => new ListItemDto
                {
                    Id = x.UnitId,
                    DisplayValue = x.Name,
                    Description = x.Description
                });
            }
        }

        #endregion
    }
}