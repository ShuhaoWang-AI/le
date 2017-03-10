using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Parameter
{
    public interface IParameterService
    {
        ICollection<ParameterDto> GetGlobalParameters();
        ICollection<ParameterGroupDto> GetParameterGroups();
        ParameterGroupDto GetParameterGroup(int parameterGroupId);
        void SaveParameterGroup(ParameterGroupDto parameterGroup);
        void DeleteParameterGroup(int parameterGroupId);

    }
}
