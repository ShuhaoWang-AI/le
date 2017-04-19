using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Unit
{
    public interface IUnitService
    {
        List<UnitDto> GetFlowUnits();

        /// <summary>
        /// Always ppd as per client's requirements
        /// </summary>
        /// <returns></returns>
        UnitDto GetUnitForMassLoadingCalculations();
    }
}
