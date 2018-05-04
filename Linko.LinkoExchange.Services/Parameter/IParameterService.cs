using System;
using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Parameter
{
    public interface IParameterService
    {
        /// <summary>
        ///     Gets all parameters associated with this Authority with optional parameters to filter the returned collection
        /// </summary>
        /// <param name="startsWith"> Optional parameter to filter the Parameter name using "Starts With" condition </param>
        /// <param name="monitoringPointId">
        ///     Optional Monitoring Point parameter must be combined with the other
        ///     optional parameter "sampleEndDateTimeUtc"
        /// </param>
        /// <param name="sampleDateTimeLocal">
        ///     If monitoring point and sample date/time are passed in,
        ///     default unit gets overridden with monitoring point specific unit and default "Calc Mass" boolean is set
        ///     for each child parameter that is associated with the monitoring point and effective date range.
        /// </param>
        /// <returns> A parameter group with children parameters some with potentially overridden default units </returns>
        IEnumerable<ParameterDto> GetGlobalParameters(string startsWith = null, int? monitoringPointId = null, DateTime? sampleDateTimeLocal = null);

        /// <summary>
        ///     Used to obtain a collection of Parameter Groups from the database that matches optionally passed in criteria
        /// </summary>
        /// <param name="monitoringPointId">
        ///     Optional Monitoring Point parameter must be combined with the other
        ///     optional parameter "sampleEndDateTimeUtc"
        /// </param>
        /// <param name="sampleDateTimeLocal">
        ///     If monitoring point and sample date/time are passed in,
        ///     default unit gets overridden with monitoring point specific unit and default "Calc Mass" boolean is set
        ///     for each child parameter that is associated with the monitoring point and effective date range.
        /// </param>
        /// <param name="isGetActiveOnly"> True when being called by IU for use in Sample creation. False when being called by Authority </param>
        /// <returns> Collection of parameter groups with children parameters some with potentially overridden default units </returns>
        IEnumerable<ParameterGroupDto> GetStaticParameterGroups(int? monitoringPointId = null, DateTime? sampleDateTimeLocal = null, bool? isGetActiveOnly = null);

        /// <summary>
        ///     Used to read the details of a static ParameterGroup from the database along with
        ///     Parameter children contained within.
        /// </summary>
        /// <param name="parameterGroupId"> Id from tParameterGroup associated with Parameter Group to read </param>
        /// <param name="isAuthorizationRequired"> </param>
        /// <returns> </returns>
        ParameterGroupDto GetParameterGroup(int parameterGroupId, bool isAuthorizationRequired = false);

        /// <summary>
        ///     Creates a new Parameter group or updates and existing one in the database.
        /// </summary>
        /// <param name="parameterGroup"> Parameter group to create new or update if and Id is included </param>
        /// <returns> Existing Id or newly created Id from tParameterGroup </returns>
        int SaveParameterGroup(ParameterGroupDto parameterGroup);

        /// <summary>
        ///     Removes a Parameter Group from the database
        /// </summary>
        /// <param name="parameterGroupId"> ParameterGroupId from tParameterGroup of the Parameter Group to delete. </param>
        void DeleteParameterGroup(int parameterGroupId);

        /// <summary>
        ///     Gets a collection of both static and dynamic Parameter Groups associated with a Monitoring Point,
        ///     a Sample End Date/time (Local will get converted to UTC for comparison against database items),
        ///     and collection method.
        /// </summary>
        /// <param name="monitoringPointId"> Monitoring point that must be associated with a Sample </param>
        /// <param name="sampleDateTimeLocal">
        ///     Sample date/time, once converted to UTC will be used to get monitoring point
        ///     specific parameter information if it falls between effective and retirement date/time values.
        /// </param>
        /// <param name="collectionMethodId"> Used to further filter and obtain parameter groups related to a given collection method only.</param>
        /// <returns> Static and Dynamic Parameter Groups </returns>
        IEnumerable<ParameterGroupDto> GetAllParameterGroups(int monitoringPointId, DateTime sampleDateTimeLocal, int collectionMethodId);

		/// <summary>
		///    Gets the discharge limit report file data. 
		/// </summary>
		/// <returns>The binary discharge limit report data</returns>
	    byte[] GetIndustryDischargeLimitReport();
    }
}