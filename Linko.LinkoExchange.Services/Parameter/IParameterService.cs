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
        IEnumerable<ParameterDto> GetGlobalParameters(string startsWith = null);
        IEnumerable<ParameterGroupDto> GetStaticParameterGroups();
        ParameterGroupDto GetParameterGroup(int parameterGroupId);
        void SaveParameterGroup(ParameterGroupDto parameterGroup);
        void DeleteParameterGroup(int parameterGroupId);

    }
}
