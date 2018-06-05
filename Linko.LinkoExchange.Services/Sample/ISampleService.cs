using System;
using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Sample
{
    public interface ISampleService
    {
        /// <summary>
        ///     The sample statistic count for different sample status
        /// </summary>
        /// <param name="startDate">
        ///     Nullable localized date/time time period range.
        ///     Sample start dates must on or after this date/time. Null parameters are ignored and not part of the filter.
        /// </param>
        /// <param name="endDate">
        ///     Nullable localized date/time time period range.
        ///     Sample end dates must on or before this date/time. Null parameters are ignored and not part of the filter.
        /// </param>
        /// <returns> A list of different sample status </returns>
        List<SampleCount> GetSampleCounts(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        ///     Gets Samples from the database for displaying in a grid
        /// </summary>
        /// <param name="status"> SampletStatus type to filter by </param>
        /// <param name="startDate">
        ///     Nullable localized date/time time period range.
        ///     Sample start dates must on or after this date/time. Null parameters are ignored and not part of the filter.
        /// </param>
        /// <param name="endDate">
        ///     Nullable localized date/time time period range.
        ///     Sample end dates must on or before this date/time. Null parameters are ignored and not part of the filter.
        /// </param>
        /// <param name="isIncludeChildObjects"> Switch to load result list or not (for display in grid) </param>
        /// <returns> Collection of filtered Sample Dto's </returns>
        IEnumerable<SampleDto> GetSamples(SampleStatusName status, DateTime? startDate = null, DateTime? endDate = null, bool isIncludeChildObjects = false);

	    /// <summary>
	    ///     Saves a Sample to the database after validating. Throw a list of RuleViolation exceptions
	    ///     for failed validation issues. If SampleDto.IsReadyToReport is true, validation is more strict.
	    /// </summary>
	    /// <param name="sample"> Sample Dto </param>
	    /// <returns> Existing Sample Id or newly created Sample Id </returns>
	    int SaveSample(SampleDto sample);

        /// <summary>
        ///     Deletes a sample from the database
        /// </summary>
        /// <param name="sampleId"> SampleId associated with the object in the tSample table </param>
        /// <returns> The sample object deleted </returns>
        SampleDto DeleteSample(int sampleId);

        /// <summary>
        ///     Gets the complete details of a single Sample
        /// </summary>
        /// <param name="sampleId"> SampleId associated with the object in the tSample table </param>
        /// <returns> Sample Dto associated with the passed in Id </returns>
        SampleDto GetSampleDetails(int sampleId);

        /// <summary>
        ///     Converts a Sample POCO into the complete details of a single Sample (Dto)
        /// </summary>
        /// <param name="sample"> POCO </param>
        /// <param name="isIncludeChildObjects"> Switch to load result list or not (for display in grid) </param>
        /// <param name="isLoggingEnabled"> </param>
        /// <returns> </returns>
        SampleDto GetSampleDetails(Core.Domain.Sample sample, bool isIncludeChildObjects = true, bool isLoggingEnabled = true);

        /// <summary>
        ///     Test to see if a Sample is included in at least 1 report package
        /// </summary>
        /// <param name="sampleId"> SampleId associated with the object in the tSample table </param>
        /// <returns> Boolean indicating if the Sample is or isn't included in at least 1 report package </returns>
        bool IsSampleIncludedInReportPackage(int sampleId);

        /// <summary>
        ///     Tests validation of a passed in Sample in either Draft Mode (sampleDto.IsReadyToReport = false)
        ///     or ReadyToReport Mode (sampleDto.IsReadyToReport = true)
        /// </summary>
        /// <param name="sampleDto"> Sample to validate </param>
        /// <param name="isSuppressExceptions"> False = throws RuleViolation exception, True = does not throw RuleViolation exceptions </param>
        /// <returns> Boolean indicating if Sample passed all validation (Draft or ReadyToReport mode) </returns>
        bool IsValidSample(SampleDto sampleDto, bool isSuppressExceptions = false);

        /// <summary>
        ///     Returns a list of active collection methods associated with the current user's authority org reg program,
        ///     or the current org reg program if the user belongs to the authority.
        /// </summary>
        /// <returns> </returns>
        IEnumerable<CollectionMethodDto> GetCollectionMethods();

        /// <summary>
        /// Returns a list of sample requirement dto objects for a given org reg program that match up with a
        /// specified "schedule" (matching start and end dates -- ignore time component).
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="orgRegProgramId"></param>
        /// <returns></returns>
        IEnumerable<SampleRequirementDto> GetSampleRequirements(DateTime startDate, DateTime endDate, int orgRegProgramId);

		/// <summary>
		/// Calculate the product of array of double numbers, with specified decimals. 
		/// </summary>
		/// <param name="numbers">The array of numbers to calculate</param>
		/// <param name="decimals">The decimals</param>
		/// <returns></returns>
	    FloatNumbersProductDto CalculateFlowNumbersProduct(double[] numbers, int decimals);
    }
}