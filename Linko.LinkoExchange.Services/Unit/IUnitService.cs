using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Unit
{
    public interface IUnitService
    {
        /// <summary>
        /// Gets all available units for the current authority in tUnit table
        /// </summary>
        /// <returns> </returns>
        IEnumerable<UnitDto> GetUnits();

        /// <summary>
        /// Gets unit
        /// </summary>
        /// <returns> </returns>
        UnitDto GetUnit(int unitId);

        /// <summary>
        /// Gets all available system units in tSystemUnit table
        /// </summary>
        /// <returns> </returns>
        IEnumerable<SystemUnitDto> GetSystemUnits();

        /// <summary>
        /// Gets SystemUnitDto
        /// </summary>
        /// <returns> </returns>
        SystemUnitDto GetSystemUnit(int systemUnitId);

        /// <summary>
        /// Gets all available flow units for an Organization where IsFlowUnit = true in tUnit table
        /// </summary>
        /// <returns> </returns>
        IEnumerable<UnitDto> GetFlowUnits();

        /// <summary>
        /// Reads unit labels from the Org Reg Program Setting "FlowUnitValidValues"
        /// </summary>
        /// <returns> Collection of unit dto's corresponding to the labels read from the setting </returns>
        IEnumerable<UnitDto> GetFlowUnitValidValues();

        /// <summary>
        /// Reads unit labels from passed in comma delimited string
        /// </summary>
        /// <param name="commaDelimitedString"> The comma delimited string. </param>
        /// <param name="isLoggingEnabled"> if set to <c> true </c> [is logging enabled]. </param>
        /// <returns>
        /// Collection of unit dto's corresponding to the labels read from passed in string
        /// </returns>
        IEnumerable<UnitDto> GetFlowUnitsFromCommaDelimitedString(string commaDelimitedString, bool isLoggingEnabled = true);

        /// <summary>
        /// Always return unit information for "ppd" = Pounds per Day (as per client's requirements)
        /// from the tUnit table.
        /// </summary>
        /// <returns> </returns>
        UnitDto GetUnitForMassLoadingCalculations();

        /// <summary>
        /// Returns how many current authority units didn't translate to system unit
        /// </summary>
        /// <returns></returns>
        int GetMissingAuthorityUnitToSystemUnitTranslationCount();

        /// <summary>
        /// Update unit for the current authority in tUnit table
        /// </summary>
        /// <param name="unitDto"></param>
        void UpdateAuthorityUnit(UnitDto unitDto);

        double? ConvertResultToTargetUnit(double? result, Core.Domain.Unit currentAuthorityUnit, Core.Domain.Unit targetAuthorityUnit);
        double? ConvertResultToTargetUnit(double? result, UnitDto currentAuthorityUnit, UnitDto targetAuthorityUnit);
    }
}