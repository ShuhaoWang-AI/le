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
        /// <summary>
        /// Gets all parameters associated with this Authority with optional parameters to filter the returned collection
        /// </summary>
        /// <param name="startsWith">Optional parameter to filter the Parameter name using "Starts With" condition</param>
        /// <param name="monitoringPointId">Optional Monitoring Point parameter must be combined with the other
        /// optional parameter "sampleEndDateTimeUtc"</param>
        /// <param name="sampleEndDateTimeUtc">If monitoring point and sample end date/time are passed in,
        ///default unit gets overidden with monitoring point specific unit and default "Calc Mass" boolean is set
        ///for each child parameter that is associated with the monitoring point and effective date range.</param>
        /// <returns>A parameter group with children parameters some with potentially overidden default units</returns>
        IEnumerable<ParameterDto> GetGlobalParameters(string startsWith = null, int? monitoringPointId = null, DateTimeOffset? sampleEndDateTimeUtc = null);

        /// <summary>
        /// Used to obtain a collection of Parameter Groups from the database that match optionally passed in criteria
        /// </summary>
        /// <param name="monitoringPointId">Optional Monitoring Point parameter must be combined with the other
        /// optional parameter "sampleEndDateTimeUtc"</param>
        /// <param name="sampleEndDateTimeUtc">If monitoring point and sample end date/time are passed in,
        ///default unit gets overidden with monitoring point specific unit and default "Calc Mass" boolean is set
        ///for each child parameter that is associated with the monitoring point and effective date range.</param>
        /// <returns>Collection of parameter groups with children parameters some with potentially overidden default units</returns>
        IEnumerable<ParameterGroupDto> GetStaticParameterGroups(int? monitoringPointId = null, DateTimeOffset? sampleEndDateTimeUtc = null);

        /// <summary>
        /// Used to read the details of a static ParameterGroup from the database along with
        /// Parameter children contained within.
        /// </summary>
        /// <param name="parameterGroupId">Id from tParameterGroup associated with Parameter Group to read</param>
        /// <returns></returns>
        ParameterGroupDto GetParameterGroup(int parameterGroupId);

        /// <summary>
        /// Creates a new Parameter group or updates and existing one in the database.
        /// </summary>
        /// <param name="parameterGroup">Parameter group to create new or update if and Id is included</param>
        /// <returns>Existing Id or newly created Id from tParameterGroup</returns>
        int SaveParameterGroup(ParameterGroupDto parameterGroup);

        /// <summary>
        /// Removes a Parameter Group from the database
        /// </summary>
        /// <param name="parameterGroupId">ParameterGroupId from tParameterGroup of the Parameter Group to delete.</param>
        void DeleteParameterGroup(int parameterGroupId);

        /// <summary>
        /// Gets a collection of both static and dynamic Parameter Groups associated with a Monitoring Point and
        /// a Sample End Date/time (Local will get converted to UTC for comparison against database items)
        /// </summary>
        /// <param name="monitoringPointId">Monitoring point that must be associated with a Sample</param>
        /// <param name="sampleEndDateTimeLocal">Sample end date/time, once converted to UTC will be used to get monitoring point
        /// specific parameter information if it falls between effective and retirement date/time values.</param>
        /// <returns>Static and Dynamic Parameter Groups</returns>
        IEnumerable<ParameterGroupDto> GetAllParameterGroups(int monitoringPointId, DateTime sampleEndDateTimeLocal);

    }
}
