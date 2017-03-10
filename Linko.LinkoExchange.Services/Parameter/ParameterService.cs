using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Parameter
{
    public class UnitDto
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int UnitId { get; set; }

        /// <summary>
        /// Unique within a particular OrganizationRegulatoryProgramId.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsFlow { get; set; }

        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgram { get; set; }

        public bool IsRemoved { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }

    public class ParameterDto
    {
        public int ParameterId { get; set; }

        /// <summary>
        /// Unique within a particular OrganizationRegulatoryProgramId.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public UnitDto DefaultUnit { get; set; }

        public double? TrcFactor { get; set; }

        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgram { get; set; }

        public bool IsRemoved { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }

    public class ParameterGroupDto
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int ParameterGroupId { get; set; }

        /// <summary>
        /// Unique within a particular OrganizationRegulatoryProgramId.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgram { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        public virtual ICollection<ParameterDto> Parameters { get; set; }
    }


    public class ParameterService : IParameterService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper;

        private int _orgRegProgramId;

        public ParameterService(LinkoExchangeContext dbContext,
            IHttpContextService httpContext,
            IMapHelper mapHelper)
        {
            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _orgRegProgramId = int.Parse(httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
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

        public ICollection<ParameterGroupDto> GetParameterGroups()
        {
            //TO-DO
            return new List<ParameterGroupDto>();
        }

        public ParameterGroupDto GetParameterGroup(int parameterGroupId)
        {
            //TO-DO
            return new ParameterGroupDto();
        }

        public void SaveParameterGroup(ParameterGroupDto parameterGroup)
        {
            //TO-DO
        }

        /// <summary>
        /// Deletes associated child rows from tParameterGroupParameter before deleting 
        /// from tParamaterGroup
        /// </summary>
        /// <param name="parameterGroupId">Id</param>
        public void DeleteParameterGroup(int parameterGroupId)
        {
            //TO-DO
            //using (var transaction = _dbContext.BeginTransaction())
            //{

            //}

            var foundParameterGroup = _dbContext.ParameterGroups
                .Single(pg => pg.ParameterGroupId == parameterGroupId);

            _dbContext.ParameterGroups.Remove(foundParameterGroup);
            _dbContext.SaveChanges();
        }

    }
}
