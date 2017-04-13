﻿using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Sample
{
    public interface ISampleService
    {
        /// <summary>
        /// Gets Samples from the database for displaying in a grid
        /// </summary>
        /// <param name="status">SampletStatus type to filter by</param>
        /// <param name="startDate">Nullable localized date/time time period range. 
        /// Sample start dates must on or after this date/time. Null parameters are ignored and not part of the filter.</param>
        /// <param name="endDate">Nullable localized date/time time period range. 
        /// Sample end dates must on or before this date/time. Null parameters are ignored and not part of the filter.</param>
        /// <returns>Collection of filtered Sample Dto's</returns>
        IEnumerable<SampleDto> GetSamples(SampleStatusName status, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Saves a Sample to the database after validating. Throw a list of RuleViolation exceptions
        /// for failed validation issues. If SampleDto.IsReadyToReport is true, validation is more strict.
        /// </summary>
        /// <param name="sample">Sample Dto</param>
        /// <returns>Existing Sample Id or newly created Sample Id</returns>
        int SaveSample(SampleDto sample);

        /// <summary>
        /// Deletes a sample from the database
        /// </summary>
        /// <param name="sampleId">SampleId associated with the object in the tSample table</param>
        void DeleteSample(int sampleId);

        /// <summary>
        /// Gets the complete details of a single Sample
        /// </summary>
        /// <param name="sampleId">SampleId associated with the object in the tSample table</param>
        /// <returns>Sample Dto associated with the passed in Id</returns>
        SampleDto GetSampleDetails(int sampleId);

        /// <summary>
        /// Test to see if a Sample is included in at least 1 report package
        /// </summary>
        /// <param name="sampleId">SampleId associated with the object in the tSample table</param>
        /// <returns>Boolean indicating if the Sample is or isn't included in at least 1 report package</returns>
        bool IsSampleIncludedInReportPackage(int sampleId);

        /// <summary>
        /// Tests validation of a passed in Sample in either Draft Mode (sampleDto.IsReadyToReport = false)
        /// or ReadyToReport Mode (sampleDto.IsReadyToReport = true)
        /// </summary>
        /// <param name="sampleDto">Sample to validate</param>
        /// <param name="isSuppressExceptions">False = throws RuleViolation exception, True = does not throw RuleViolation exceptions</param>
        /// <returns>Boolean indicating if Sample passed all validation (Draft or ReadyToReport mode)</returns>
        bool IsValidSample(SampleDto sampleDto, bool isSuppressExceptions = false);
    }
}
