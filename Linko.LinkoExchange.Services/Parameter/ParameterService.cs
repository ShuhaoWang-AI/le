using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Services.Parameter
{
    public class ParameterService : IParameterService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;

        private int _orgRegProgramId;

        public ParameterService(LinkoExchangeContext dbContext,
            IHttpContextService httpContext,
            IMapHelper mapHelper,
            ILogger logger)
        {
            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _orgRegProgramId = int.Parse(httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            _logger = logger;
        }

        /// <summary>
        /// Returns a complete list of parameters associated with this Organization Regulatory Program
        /// </summary>
        /// <returns></returns>
        public ICollection<ParameterDto> GetGlobalParameters()
        {
            var parameterDtos = new List<ParameterDto>();
            var foundParams = _dbContext.Parameters
                .Where(param => param.OrganizationRegulatoryProgramId == _orgRegProgramId);
            foreach (var parameter in foundParams)
            {
                var dto = _mapHelper.GetParameterDtoFromParameter(parameter);
                parameterDtos.Add(dto);
            }
            return parameterDtos;
        }

        /// <summary>
        /// Returns all Parameter Groups associated with this Organization Regulatory Program
        /// including children parameters
        /// </summary>
        /// <returns></returns>
        public ICollection<ParameterGroupDto> GetParameterGroups()
        {
            var parameterGroupDtos = new List<ParameterGroupDto>();
            var foundParamGroups = _dbContext.ParameterGroups
                .Include(param => param.ParameterGroupParameters)
                .Where(param => param.OrganizationRegulatoryProgramId == _orgRegProgramId)
                .ToList();
            foreach (var paramGroup in foundParamGroups)
            {
                var dto = _mapHelper.GetParameterGroupDtoFromParameterGroup(paramGroup);
                parameterGroupDtos.Add(dto);
            }
            return parameterGroupDtos;
        }

        /// <summary>
        /// Returns single Paramater Group associated with Id
        /// including children parameters
        /// </summary>
        /// <param name="parameterGroupId">Id</param>
        /// <returns></returns>
        public ParameterGroupDto GetParameterGroup(int parameterGroupId)
        {
            var foundParamGroup = _dbContext.ParameterGroups
                .Include(param => param.ParameterGroupParameters)
                .Single(param => param.ParameterGroupId == parameterGroupId);

            var parameterGroupDto = _mapHelper.GetParameterGroupDtoFromParameterGroup(foundParamGroup);
            return parameterGroupDto;
        }

        /// <summary>
        /// If ParameterGroupId exists in passed in Dto, finds existing ParameterGroup to update 
        /// OR creates new object to persist.
        /// </summary>
        /// <param name="parameterGroup"></param>
        public void SaveParameterGroup(ParameterGroupDto parameterGroup)
        {
            ParameterGroup paramGroupToPersist = null;
            if (parameterGroup.ParameterGroupId.HasValue && parameterGroup.ParameterGroupId.Value > 0)
            {
                //Update existing
                paramGroupToPersist = _dbContext.ParameterGroups.Single(param => param.ParameterGroupId == parameterGroup.ParameterGroupId);
                paramGroupToPersist = _mapHelper.GetParameterGroupFromParameterGroupDto(parameterGroup, paramGroupToPersist);
            }
            else
            {
                //Get new
                paramGroupToPersist = _mapHelper.GetParameterGroupFromParameterGroupDto(parameterGroup);
                _dbContext.ParameterGroups.Add(paramGroupToPersist);
            }
            _dbContext.SaveChanges();

        }

        /// <summary>
        /// Deletes associated child rows from tParameterGroupParameter before deleting 
        /// from tParamaterGroup
        /// </summary>
        /// <param name="parameterGroupId">Id</param>
        public void DeleteParameterGroup(int parameterGroupId)
        {
            using (var transaction = _dbContext.BeginTransaction())
            {
                try {

                    var childAssociations = _dbContext.ParameterGroupParameters
                        .Where(child => child.ParameterGroupId == parameterGroupId);

                    if (childAssociations.Count() > 0)
                    {
                        _dbContext.ParameterGroupParameters.RemoveRange(childAssociations);
                    }

                    var foundParameterGroup = _dbContext.ParameterGroups
                        .Single(pg => pg.ParameterGroupId == parameterGroupId);

                    _dbContext.ParameterGroups.Remove(foundParameterGroup);

                    _dbContext.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    var errors = new List<string>() { ex.Message };

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(ex.Message);
                    }

                    _logger.Error("Error occurred {0} ", String.Join("," + Environment.NewLine, errors));

                    transaction.Rollback();
                    throw;
                }
            }

        }

    }
}
