using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Unit
{
    public interface IUnitService
    {
        /// <summary>
        /// Gets all available flow units for an Organization where IsFlowUnit = true in tUnit table
        /// </summary>
        /// <returns></returns>
        IEnumerable<UnitDto> GetFlowUnits();

        /// <summary>
        /// Reads unit labels from the Org Reg Program Setting "FlowUnitValidValues"
        /// </summary>
        /// <returns>Collection of unit dto's corresponding to the labels read from the setting</returns>
        IEnumerable<UnitDto> GetFlowUnitValidValues();

        /// <summary>
        /// Reads unit labels from passed in comma delimited string
        /// </summary>
        /// <param name="commaDelimitedString"></param>
        /// <returns>Collection of unit dto's corresponding to the labels read from passed in string</returns>
        IEnumerable<UnitDto> GetFlowUnitsFromCommaDelimitedString(string commaDelimitedString, bool isLoggingEnabled = true);

        /// <summary>
        /// Always return unit information for "ppd" = Pounds per Day (as per client's requirements)
        /// from the tUnit table.
        /// </summary>
        /// <returns></returns>
        UnitDto GetUnitForMassLoadingCalculations();
    }
}
