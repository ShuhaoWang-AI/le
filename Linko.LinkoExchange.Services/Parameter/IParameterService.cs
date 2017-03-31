using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Parameter
{
    public interface IParameterService
    {
        IEnumerable<ParameterDto> GetGlobalParameters(string startsWith = null, int? monitoringPointId = null, DateTimeOffset? sampleEndDateTimeUtc = null);
        IEnumerable<ParameterGroupDto> GetStaticParameterGroups(int? monitoringPointId = null, DateTimeOffset? sampleEndDateTimeUtc = null);
        ParameterGroupDto GetParameterGroup(int parameterGroupId);
        int SaveParameterGroup(ParameterGroupDto parameterGroup);
        void DeleteParameterGroup(int parameterGroupId);

        /// <summary>
        /// Returns a collection of Static and Dynamic Parameter Groups
        /// </summary>
        /// <returns>Static and Dynamic Parameter Groups</returns>
        IEnumerable<ParameterGroupDto> GetAllParameterGroups(int monitoringPointId, DateTime sampleEndDateTimeLocal);

    }
}
