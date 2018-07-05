using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Resources;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.Unit;
using NLog;

namespace Linko.LinkoExchange.Services.Sample
{
    public class SampleService : BaseService, ISampleService
    {
        #region fields

        private readonly IApplicationCache _cache;
        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContext;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;
        private readonly IOrganizationService _orgService;
        private readonly ISettingService _settings;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IUnitService _unitService;

        #endregion

        #region constructors and destructor

        public SampleService(LinkoExchangeContext dbContext,
                             IHttpContextService httpContext,
                             IOrganizationService orgService,
                             IMapHelper mapHelper,
                             ILogger logger,
                             ITimeZoneService timeZoneService,
                             ISettingService settings,
                             IUnitService unitService,
                             IApplicationCache cache)
        {
            _dbContext = dbContext;
            _httpContext = httpContext;
            _orgService = orgService;
            _mapHelper = mapHelper;
            _logger = logger;
            _timeZoneService = timeZoneService;
            _settings = settings;
            _unitService = unitService;
            _cache = cache;
        }

        #endregion

        #region interface implementations

        public override bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id)
        {
            bool retVal;

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var currentPortalName = _httpContext.GetClaimValue(claimType:CacheKey.PortalName);
            currentPortalName = string.IsNullOrWhiteSpace(value:currentPortalName) ? "" : currentPortalName.Trim().ToLower();

            switch (apiName)
            {
                case "GetSampleDetails":
                {
                    var sampleId = id[0];
                    if (currentPortalName.Equals(value:OrganizationTypeName.Authority.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                    {
                        //currentOrgRegProgramId must match the authority of the ForOrganizationRegulatoryProgram of the sample
                        var authorityOrgRegProgramId = _orgService.GetAuthority(orgRegProgramId:_dbContext.Samples
                                                                                                          .Single(s => s.SampleId == sampleId).ForOrganizationRegulatoryProgramId)
                                                                  .OrganizationRegulatoryProgramId;

                        retVal = currentOrgRegProgramId == authorityOrgRegProgramId;
                    }
                    else
                    {
                        //currentOrgRegProgramId must match the ForOrganizationRegulatoryProgramId of the sample
                        //(this also handles unknown sampleId's)
                        var isSampleWithThisOwnerExist = _dbContext.Samples.Any(s => s.SampleId == sampleId && s.ForOrganizationRegulatoryProgramId == currentOrgRegProgramId);

                        retVal = isSampleWithThisOwnerExist;
                    }
                }

                    break;

                default: throw new Exception(message:$"ERROR: Unhandled API authorization attempt using name = '{apiName}'");
            }

            return retVal;
        }

	    /// <summary>
	    ///     Saves a Sample to the database after validating. Throw a list of RuleViolation exceptions
	    ///     for failed validation issues. If SampleDto.IsReadyToReport is true, validation is more strict.
	    /// </summary>
	    /// <param name="sampleDto"> Sample Dto </param>
	    /// <returns> Existing Sample Id or newly created Sample Id </returns>
	    public int SaveSample(SampleDto sampleDto)
	    {
		    var sampleIdString = sampleDto.SampleId?.ToString() ?? "null";
		    _logger.Info(message:$"Start: SampleService.SaveSample. SampleId={sampleIdString}, isSavingAsReadyToReport={sampleDto.IsReadyToReport}");

		    var sampleId = -1;
		    
			using (_dbContext.BeginTransactionScope(MethodBase.GetCurrentMethod()))
			{
				//Cannot save if included in a report
				//      (UC-15-1.2(*.a.) - System identifies Sample is in use in a Report Package (draft or otherwise) an displays the "REPORTED" Status.  
				//      Actor cannot perform any actions of any kind except view all details.)
				if (sampleDto.SampleId.HasValue && IsSampleIncludedInReportPackage(sampleId: sampleDto.SampleId.Value))
				{
					ThrowSimpleException(message:"Sample is in use in a Report Package and is therefore READ-ONLY.");
				}

				if (IsValidSample(sampleDto:sampleDto, isSuppressExceptions:false))
				{
					sampleId = SimplePersist(sampleDto:sampleDto);
				}


				_logger.Info(message:"End: SampleService.SaveSample.");
			} 

		    return sampleId;
	    }

	    /// <summary>
        ///     Tests validation of a passed in Sample in either Draft Mode (sampleDto.IsReadyToReport = false)
        ///     or ReadyToReport Mode (sampleDto.IsReadyToReport = true)
        /// </summary>
        /// <param name="sampleDto"> Sample to validate </param>
        /// <param name="isSuppressExceptions"> False = throws RuleViolation exception, True = does not throw RuleViolation exceptions </param>
        /// <returns> Boolean indicating if Sample passed all validation (Draft or ReadyToReport mode) </returns>
        public bool IsValidSample(SampleDto sampleDto, bool isSuppressExceptions = false)
        {
            var sampleIdString = sampleDto.SampleId?.ToString() ?? "null";

            _logger.Info(message:$"Start: SampleService.IsValidSample. SampleId={sampleIdString}");

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var authOrgRegProgramId = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationRegulatoryProgramId;

            var timeZoneId = Convert.ToInt32(value:_settings.GetOrganizationSettingValue(orgRegProgramId:authOrgRegProgramId, settingType:SettingType.TimeZone));
            var isValid = true;

            //Check required field (UC-15-1.2.1): "Sample Type"
            if (sampleDto.CtsEventTypeId < 1)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    ThrowSimpleException(message:"Sample Type is required.");
                }
            }

            //Check required field (UC-15-1.2.1): "Collection Method"
            if (sampleDto.CollectionMethodId < 1)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    ThrowSimpleException(message:"Collection Method is required.");
                }
            }

            //Check required field (UC-15-1.2.1): "Start Date/Time"
            if (sampleDto.StartDateTimeLocal == null || sampleDto.StartDateTimeLocal == DateTime.MinValue)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    ThrowSimpleException(message:"Start Date/Time is required.");
                }
            }

            //Check required field (UC-15-1.2.1): "End Date/Time"
            if (sampleDto.EndDateTimeLocal == null || sampleDto.EndDateTimeLocal == DateTime.MinValue)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    ThrowSimpleException(message:"End Date/Time is required.");
                }
            }

            //Check sample start/end dates are not in the future (UC-15-1.2.9.1.b)

            if (_timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(localDateTime:sampleDto.StartDateTimeLocal, timeZoneId:timeZoneId) > DateTimeOffset.Now
                || _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(localDateTime:sampleDto.EndDateTimeLocal, timeZoneId:timeZoneId) > DateTimeOffset.Now)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    ThrowSimpleException(message:"Sample dates cannot be future dates.");
                }
            }

            //Check end date is not before start date
            if (sampleDto.EndDateTimeLocal < sampleDto.StartDateTimeLocal)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    ThrowSimpleException(message:"End date must be after Start date");
                }
            }

            //Check Flow Value exists and is complete if provided (both value and unit)

            if (!string.IsNullOrEmpty(value:sampleDto.FlowEnteredValue)
                && sampleDto.FlowUnitId.HasValue
                && sampleDto.FlowUnitId.Value > 0
                && !string.IsNullOrEmpty(value:sampleDto.FlowUnitName))
            {
                double flowValueAsDouble;
                if (!double.TryParse(s:sampleDto.FlowEnteredValue, result:out flowValueAsDouble))
                {
                    //Could not convert -- throw exception
                    ThrowSimpleException(message:$"Could not convert provided flow value '{sampleDto.FlowEnteredValue}' to double.");
                }
            }

            //
            //Validation for Sample Results
            //

            if (sampleDto.SampleResults != null)
            {
                foreach (var resultDto in sampleDto.SampleResults)
                {
                    //Check if concentration entered value is numeric and can be converted to double.
                    if (!string.IsNullOrEmpty(value:resultDto.EnteredValue))
                    {
                        double concentrationValueAsDouble;
                        if (!double.TryParse(s:resultDto.EnteredValue, result:out concentrationValueAsDouble))
                        {
                            //Could not convert -- throw exception
                            ThrowSimpleException(message:$"Could not convert provided concentration value '{resultDto.EnteredValue}' to double.");
                        }
                    }

                    //Check if mass loading entered value is numeric and can be converted to double.
                    if (!string.IsNullOrEmpty(value:resultDto.MassLoadingValue))
                    {
                        double massLoadingValueAsDouble;
                        if (!double.TryParse(s:resultDto.MassLoadingValue, result:out massLoadingValueAsDouble))
                        {
                            //Could not convert -- throw exception
                            ThrowSimpleException(message:$"Could not convert provided mass loading value '{resultDto.MassLoadingValue}' to double.");
                        }
                    }

                    //All results must have a unit if provided (applied to both Draft or ReadyToReport)
                    if (resultDto.UnitId < 1)
                    {
                        isValid = false;
                        if (!isSuppressExceptions)
                        {
                            ThrowSimpleException(message:"All results must be associated with a valid unit.");
                        }
                    }

                    //if (resultDto.IsCalcMassLoading && resultDto.MassLoadingUnitId < 1)
                    //{
                    //    isValid = false;
                    //    if (!isSuppressExceptions)
                    //    {
                    //        this.ThrowSimpleException("All mass loading calculations must be associated with a valid unit.");
                    //    }
                    //}

                    if (sampleDto.IsReadyToReport)
                    {
                        //
                        //ReadyToReport Validation For Concentration Results
                        //

                        if ((resultDto.Qualifier == ">" || resultDto.Qualifier == "<" || string.IsNullOrEmpty(value:resultDto.Qualifier))
                            && string.IsNullOrEmpty(value:resultDto.EnteredValue))
                        {
                            isValid = false;
                            if (!isSuppressExceptions)
                            {
                                ThrowSimpleException(message:"Result is required.");
                            }
                        }

                        if ((resultDto.Qualifier == "ND" || resultDto.Qualifier == "NF")
                            && !string.IsNullOrEmpty(value:resultDto.EnteredValue))
                        {
                            isValid = false;
                            if (!isSuppressExceptions)
                            {
                                ThrowSimpleException(message:"ND or NF qualifiers cannot be followed by a value.");
                            }
                        }
                    }
                }
            }

            _logger.Info(message:$"End: SampleService.IsValidSample. isValid={isValid}");

            return isValid;
        }

        /// <summary>
        ///     Deletes a sample from the database
        /// </summary>
        /// <param name="sampleId"> SampleId associated with the object in the tSample table </param>
        /// <returns> The sample object deleted </returns>
        public SampleDto DeleteSample(int sampleId)
        {
            _logger.Info(message:$"Start: SampleService.DeleteSample. sampleId={sampleId}");
            var validationIssues = new List<RuleViolation>();
            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    if (IsSampleIncludedInReportPackage(sampleId:sampleId))
                    {
                        var message = "Attempting to delete a sample that is included in a report.";
                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                        throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                    }
                    else
                    {
                        var sampleToRemove = _dbContext.Samples
                                                       .Include(s => s.SampleResults)
                                                       .Single(s => s.SampleId == sampleId);

                        var sampleDto = GetSampleDetails(sample:sampleToRemove, isIncludeChildObjects:false, isLoggingEnabled:false);

                        //First remove results (if applicable)
                        if (sampleToRemove.SampleResults != null)
                        {
                            _dbContext.SampleResults.RemoveRange(entities:sampleToRemove.SampleResults);
                        }
                        _dbContext.Samples.Remove(entity:sampleToRemove);
                        _dbContext.SaveChanges();
                        transaction.Commit();
                        _logger.Info(message:"End: SampleService.DeleteSample.");

                        return sampleDto;
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        ///     Converts a Sample POCO into the complete details of a single Sample (Dto)
        /// </summary>
        /// <param name="sample"> POCO </param>
        /// <param name="isIncludeChildObjects"> Switch to load result list or not (for display in grid) </param>
        /// <param name="isLoggingEnabled"> </param>
        /// <returns> </returns>
        public SampleDto GetSampleDetails(Core.Domain.Sample sample, bool isIncludeChildObjects = true, bool isLoggingEnabled = true)
        {
            if (isLoggingEnabled)
            {
                _logger.Info(message:$"Start: SampleService.GetSampleDetails. SampleId={sample.SampleId}");
            }

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(value:_settings.GetOrganizationSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.TimeZone));
            var flowLimitBasisId = GetVolumeFlowRateLimitBasisId();

            var dto = _mapHelper.GetSampleDtoFromSample(sample:sample);

            //Handle FlowUnitValidValues
            dto.FlowUnitValidValues = _unitService.GetFlowUnitsFromCommaDelimitedString(commaDelimitedString:sample.FlowUnitValidValues, isLoggingEnabled:isLoggingEnabled);

            //Set Sample Start Local Timestamp
            dto.StartDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:sample.StartDateTimeUtc.UtcDateTime, timeZoneId:timeZoneId);

            //Set Sample End Local Timestamp
            dto.EndDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:sample.EndDateTimeUtc.UtcDateTime, timeZoneId:timeZoneId);

            //Set LastModificationDateTimeLocal
            dto.LastModificationDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:sample.LastModificationDateTimeUtc.HasValue
                                                                         ? sample.LastModificationDateTimeUtc.Value.UtcDateTime
                                                                         : sample.CreationDateTimeUtc.UtcDateTime, timeZoneId:timeZoneId);

            if (sample.LastModifierUserId.HasValue)
            {
                var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == sample.LastModifierUserId.Value);
                dto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
            }
            else
            {
                dto.LastModifierFullName = "N/A";
            }

            if (sample.IsReadyToReport)
            {
                dto.SampleStatusName = SampleStatusName.ReadyToReport;
            }
            else
            {
                dto.SampleStatusName = SampleStatusName.Draft;
            }

            if (sample.ReportSamples != null && sample.ReportSamples.Any())
            {
                dto.SampleStatusName = SampleStatusName.Reported;
            }

            var resultDtoList = new Dictionary<int, SampleResultDto>();

            if (isIncludeChildObjects)
            {
                DateTime sampleDateTimeLocal;

                //Check which date to use for compliance checking
                var complianceDeterminationDate = _settings.GetOrgRegProgramSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.ComplianceDeterminationDate);
                if (complianceDeterminationDate == ComplianceDeterminationDate.StartDateSampled.ToString())
                {
                    sampleDateTimeLocal = _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(sample.StartDateTimeUtc.UtcDateTime, sample.ByOrganizationRegulatoryProgramId);
                }
                else
                {
                    sampleDateTimeLocal = _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(sample.EndDateTimeUtc.UtcDateTime, sample.ByOrganizationRegulatoryProgramId);
                }

                foreach (var sampleResult in sample.SampleResults)
                {
                    //Handle "special case" Sample Results. These do not get mapped to their own
                    //SampleResult dtos.
                    //1. Flow - gets mapped to properties of the parent Sample Dto
                    //2. Mass - gets mapped to corresponding Concentration result Dto
                    //
                    //Remember that the items in this collection are unordered.

	                var resultDto = new SampleResultDto();

	                if (sampleResult.LimitBasisId == flowLimitBasisId)
                    {
                        dto.FlowParameterName = sampleResult.ParameterName;
                        dto.FlowEnteredValue = sampleResult.EnteredValue;
                        dto.FlowValue = sampleResult.Value;
                        dto.FlowUnitId = sampleResult.UnitId;
                        dto.FlowUnitName = sampleResult.UnitName;
                    }
                    else if (sampleResult.LimitType.Name == LimitTypeName.Daily.ToString()
                             && (sampleResult.LimitBasis.Name == LimitBasisName.MassLoading.ToString()
                                 || sampleResult.LimitBasis.Name == LimitBasisName.Concentration.ToString()))
                    {
                        if (resultDtoList.ContainsKey(key:sampleResult.ParameterId))
                        {
                            //There was already a result dto added for this parameter
                            //and we are now handling the corresponding concentration (or mass) result
                            //and must attach these fields to that dto
                            resultDto = resultDtoList[key:sampleResult.ParameterId];
                        }
                        else
                        {
                            //There may be a corresponding concentration (or mass) result
                            //later in the collection that needs to be attached to this result dto
                            //so we need to save this for looking up later. 
                            resultDtoList.Add(key:sampleResult.ParameterId, value:resultDto);
                        }

                        if (sampleResult.LimitBasis.Name == LimitBasisName.Concentration.ToString())
                        {
                            resultDto.ConcentrationSampleResultId = sampleResult.SampleResultId;
                            resultDto.ParameterId = sampleResult.ParameterId;
                            resultDto.ParameterName = sampleResult.ParameterName;
                            resultDto.EnteredMethodDetectionLimit = sampleResult.EnteredMethodDetectionLimit;
                            resultDto.MethodDetectionLimit = sampleResult.MethodDetectionLimit;
                            resultDto.AnalysisMethod = sampleResult.AnalysisMethod;
                            resultDto.IsApprovedEPAMethod = sampleResult.IsApprovedEPAMethod;
                            resultDto.ParameterId = sampleResult.ParameterId;
                            resultDto.IsCalcMassLoading = sampleResult.IsMassLoadingCalculationRequired;
                            resultDto.Qualifier = sampleResult.Qualifier;
                            resultDto.EnteredValue = sampleResult.EnteredValue;
                            resultDto.Value = sampleResult.Value;
                            resultDto.UnitId = sampleResult.UnitId;
                            resultDto.UnitName = sampleResult.UnitName;

                            SetSampleResultDatesAndLastModified(sampleResult:sampleResult, resultDto:ref resultDto, timeZoneId:timeZoneId);

                        }
                        else
                        {
                            //Mass Result -- must be included with a resultDto where LimitBasisName = LimitBasisName.Concentration
                            resultDto.MassLoadingSampleResultId = sampleResult.SampleResultId;
                            resultDto.MassLoadingQualifier = sampleResult.Qualifier;
                            resultDto.MassLoadingValue = sampleResult.EnteredValue;
                            resultDto.MassLoadingUnitId = sampleResult.UnitId;
                            resultDto.MassLoadingUnitName = sampleResult.UnitName;

                        }
                    }
                    else
                    {
                        //  "Any introduction of a new Limit Type or new Limit Basis at the data level 
                        //  will be ignored until we change the code..." - mj
                    }
                }
				
                dto.SampleResults = resultDtoList.Values.ToList(); 
            }


            if (isLoggingEnabled)
            {
                _logger.Info(message:"End: SampleService.GetSampleDetails.");
            }

			SampleComplianceCheck(new List<SampleDto> {dto});
			return dto;
        }

	    public void SampleComplianceCheck(List<SampleDto> sampleDtos)
	    {
		    var monitoringPointParameters = _dbContext.MonitoringPointParameters
		                                              .Include(mppl => mppl.DefaultUnit)
		                                              .Include(a => a.MonitoringPoint)
													  .ToList();

		    var monitoringPointParameterLimits = _dbContext.MonitoringPointParameterLimits
		                                                   .Include(mppl => mppl.LimitBasis)
		                                                   .Include(mppl => mppl.LimitType)
		                                                   .ToList();
			
		    foreach (var sampleDto in sampleDtos)
		    {
			    if (sampleDto.SampleResults == null)
			    {
				    continue;
			    }

			    //Check compliance on all of sample results
			    foreach (var sampleResult in sampleDto.SampleResults)
			    {
				    CheckResultComplianceInner(sampleResult, sampleDto.MonitoringPointId, monitoringPointParameters, monitoringPointParameterLimits, sampleDto.StartDateTimeLocal,
				                               LimitBasisName.Concentration);
				    if (sampleDto.FlowValue.HasValue)
				    {
					    CheckResultComplianceInner(sampleResult, sampleDto.MonitoringPointId, monitoringPointParameters, monitoringPointParameterLimits, sampleDto.StartDateTimeLocal,
					                               LimitBasisName.MassLoading);
				    }
				    else
				    {
					    sampleResult.MassResultCompliance = ResultComplianceType.Unknown;
				    }
			    }
		    }
	    }
		
        /// <summary>
        ///     Gets the complete details of a single Sample
        /// </summary>
        /// <param name="sampleId"> SampleId associated with the object in the tSample table </param>
        /// <returns> Sample Dto associated with the passed in Id </returns>
        public SampleDto GetSampleDetails(int sampleId)
        {
            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            _logger.Info(message:$"Start: SampleService.GetSampleDetails. sampleId={sampleId}, currentOrgRegProgramId={currentOrgRegProgramId}");

            if (!CanUserExecuteApi(id:sampleId))
            {
                throw new UnauthorizedAccessException();
            }

            var sample = _dbContext.Samples
                                   .Include(s => s.ReportSamples)
                                   .Include(s => s.SampleResults)
                                   .Include(s => s.ByOrganizationRegulatoryProgram)
                                   .SingleOrDefault(s => s.SampleId == sampleId);

            if (sample == null)
            {
                throw new Exception(message:$"ERROR: Could not find Sample associated with sampleId={sampleId}");
            }

            var dto = GetSampleDetails(sample:sample, isIncludeChildObjects: true, isLoggingEnabled: false);

            _logger.Info(message:"End: SampleService.GetSampleDetails.");

            return dto;
        }

        /// <summary>
        ///     Test to see if a Sample is included in at least 1 report package
        /// </summary>
        /// <param name="sampleId"> SampleId associated with the object in the tSample table </param>
        /// <returns> Boolean indicating if the Sample is or isn't included in at least 1 report package </returns>
        public bool IsSampleIncludedInReportPackage(int sampleId)
        {
            _logger.Info(message:$"Start: SampleService.IsSampleIncludedInReportPackage. sampleId={sampleId}");

            var isIncluded = _dbContext.ReportSamples.Any(rs => rs.SampleId == sampleId);

            _logger.Info(message:$"End: SampleService.IsSampleIncludedInReportPackage. isIncluded={isIncluded}");

            return isIncluded;
        }

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
        public List<SampleCount> GetSampleCounts(DateTime? startDate = null, DateTime? endDate = null)
        {
            var startDateString = startDate?.ToString() ?? "null";
            var endDateString = endDate?.ToString() ?? "null";

            _logger.Info(message:$"Start: SampleService.GetSampleCounts. startDate={startDateString}, endDate={endDateString}");

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(value:_settings.GetOrganizationSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.TimeZone));

            var foundSamples = _dbContext.Samples
                                         .Include(s => s.ReportSamples)
                                         .Include(s => s.SampleResults)
                                         .Include(s => s.ByOrganizationRegulatoryProgram)
                                         .Where(s => s.ForOrganizationRegulatoryProgramId == currentOrgRegProgramId);

            if (startDate.HasValue)
            {
                var startDateUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(localDateTime:startDate.Value, timeZoneId:timeZoneId);
                foundSamples = foundSamples.Where(s => s.StartDateTimeUtc >= startDateUtc);
            }
            if (endDate.HasValue)
            {
                var endDateUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(localDateTime:endDate.Value, timeZoneId:timeZoneId);
                foundSamples = foundSamples.Where(s => s.EndDateTimeUtc <= endDateUtc);
            }

            var sampleCounts = new List<SampleCount>();
            var draftSamples = new SampleCount
                               {
                                   Status = SampleStatusName.Draft,
                                   Count = foundSamples.Count(i => !i.IsReadyToReport)
                               };
            sampleCounts.Add(item:draftSamples);

            var readyToSubmitSamples = new SampleCount
                                       {
                                           Status = SampleStatusName.ReadyToReport,
                                           Count = foundSamples.Count(i => i.IsReadyToReport && !i.ReportSamples.Any())
                                       };

            sampleCounts.Add(item:readyToSubmitSamples);

            _logger.Info(message:"End: SampleService.GetSampleCounts.");

            return sampleCounts;
        }

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
        /// <returns> Collection of filtered Sample Dto's </returns>
        /// <param name="isIncludeChildObjects"> Switch to load result list or not (for display in grid) </param>
        public IEnumerable<SampleDto> GetSamples(SampleStatusName status, DateTime? startDate = null, DateTime? endDate = null, bool isIncludeChildObjects = false)
        {
            var startDateString = startDate?.ToString() ?? "null";
            var endDateString = endDate?.ToString() ?? "null";
            _logger.Info(message:$"Start: SampleService.GetSamples. status={status}, startDate={startDateString}, endDate={endDateString}");

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(value:_settings.GetOrganizationSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.TimeZone));
            var dtos = new List<SampleDto>();
            var foundSamples = _dbContext.Samples
                                         .Include(s => s.ReportSamples)
                                         .Include(s => s.SampleResults)
                                         .Include(s => s.ByOrganizationRegulatoryProgram)
                                         .Where(s => s.ForOrganizationRegulatoryProgramId == currentOrgRegProgramId);

            if (startDate.HasValue)
            {
                var startDateUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(localDateTime:startDate.Value, timeZoneId:timeZoneId);
                foundSamples = foundSamples.Where(s => s.StartDateTimeUtc >= startDateUtc);
            }
            if (endDate.HasValue)
            {
                var endDateUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(localDateTime:endDate.Value, timeZoneId:timeZoneId);
                foundSamples = foundSamples.Where(s => s.EndDateTimeUtc <= endDateUtc);
            }

            switch (status)
            {
                case SampleStatusName.All:

                    //don't filter any further
                    break;
                case SampleStatusName.Draft:
                    foundSamples = foundSamples.Where(s => !s.IsReadyToReport);
                    break;
                case SampleStatusName.ReadyToReport:
                    foundSamples = foundSamples.Where(s => s.IsReadyToReport && !s.ReportSamples.Any());
                    break;
                case SampleStatusName.Reported:
                    foundSamples = foundSamples.Where(s => s.ReportSamples.Any());
                    break;
                case SampleStatusName.DraftOrReadyToReport:
                    foundSamples = foundSamples.Where(s => !s.ReportSamples.Any());
                    break;
                default: throw new Exception(message:$"Unhandled SampleStatusName = {status}");
            }

            foreach (var sample in foundSamples.ToList())
            {
                var dto = GetSampleDetails(sample:sample, isIncludeChildObjects:isIncludeChildObjects);
                dtos.Add(item:dto);
            }

            _logger.Info(message:$"End: SampleService.GetSamples. Count={dtos.Count}");

            return dtos;
        }

        public IEnumerable<CollectionMethodDto> GetCollectionMethods()
        {
            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

            _logger.Info(message:$"Start: SampleService.GetCollectionMethods. currentOrgRegProgramId={currentOrgRegProgramId}");

            var collectionMethodList = new List<CollectionMethodDto>();
            var authOrganizationId = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationId;
            var collectionMethods = _dbContext.CollectionMethods
                                              .Where(cm => cm.IsEnabled
                                                           && !cm.IsRemoved
                                                           && cm.OrganizationId == authOrganizationId);

            foreach (var cm in collectionMethods)
            {
                collectionMethodList.Add(item:new CollectionMethodDto {CollectionMethodId = cm.CollectionMethodId, Name = cm.Name});
            }

            _logger.Info(message:"End: SampleService.GetCollectionMethods.");

            return collectionMethodList;
        }

        public IEnumerable<SampleRequirementDto> GetSampleRequirements(DateTime startDate, DateTime endDate, int orgRegProgramId)
        {
            _logger.Info(message: $"Start: SampleService.GetSampleRequirements. startDate={startDate}, endDate={endDate}, orgRegProgramId={orgRegProgramId}");

            var sampleRequirementDtos = _dbContext.SampleRequirements
                .Include(sr => sr.MonitoringPointParameter)
                .Include(sr => sr.MonitoringPointParameter.MonitoringPoint)
                .Include(sr => sr.MonitoringPointParameter.Parameter)
                .Where(sr => sr.ByOrganizationRegulatoryProgramId == orgRegProgramId &&
                            ((DbFunctions.TruncateTime(sr.PeriodStartDateTime) >= DbFunctions.TruncateTime(startDate) && (DbFunctions.TruncateTime(sr.PeriodStartDateTime) <= DbFunctions.TruncateTime(endDate)))
                            ||
                            (DbFunctions.TruncateTime(sr.PeriodEndDateTime) >= DbFunctions.TruncateTime(startDate) && (DbFunctions.TruncateTime(sr.PeriodEndDateTime) <= DbFunctions.TruncateTime(endDate)))))
                .GroupBy(sr => new
                {
                    sr.MonitoringPointParameter.MonitoringPointId,
                    MonitoringPointName = sr.MonitoringPointParameter.MonitoringPoint.Name,
                    sr.MonitoringPointParameter.ParameterId,
                    ParameterName = sr.MonitoringPointParameter.Parameter.Name
                })
                .Select(consolidatedRequirement => new SampleRequirementDto()
                {
                    MonitoringPointId = consolidatedRequirement.Key.MonitoringPointId,
                    MonitoringPointName = consolidatedRequirement.Key.MonitoringPointName,
                    ParameterId = consolidatedRequirement.Key.ParameterId,
                    ParameterName = consolidatedRequirement.Key.ParameterName,
                    TotalSamplesRequiredCount = consolidatedRequirement.Sum(g => g.SamplesRequired)
                }).ToList();

            _logger.Info(message: $"End: SampleService.GetSampleRequirements. Count={sampleRequirementDtos.Count}");

            return sampleRequirementDtos;
        }


	    public FloatNumbersProductDto CalculateFlowNumbersProduct(double[] numbers, int decimals)
	    {
		    var result = new FloatNumbersProductDto();
		    if (numbers == null || !numbers.Any())
		    {
			    return result;
		    }

		    var product = 0.0;
		    for (var i = 0; i < numbers.Length; i++)
		    {
			    if (i == 0)
			    {
				    product = numbers[i];
			    }

			    else
			    {
				    product *= numbers[i];
			    }
		    }

		    var formatStr = "0.";
		    for (var j = 0; j < decimals; j++)
		    {
			    formatStr += "0";
		    }

		    var value = Math.Round(value:product, digits:decimals, mode:MidpointRounding.AwayFromZero); 

		    result.Product = value;
			result.ProductStr = value.ToString(format:formatStr);

		    return result;
	    }

	    #endregion

		/// <summary>
		///     Maps Sample Dto to Sample and saves to database. No validation.
		/// </summary>
		/// <param name="sampleDto"> Sample Dto to map and save </param>
		/// <returns> Existing Id or newly created Id of Sample row in tSample table </returns>
		private int SimplePersist(SampleDto sampleDto)
        {
            var sampleIdString = sampleDto.SampleId?.ToString() ?? "null";

            _logger.Info(message:$"Start: SampleService.SimplePersist. SampleId. Value={sampleIdString}");

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

            var authOrgRegProgramId = _orgService.GetAuthority(currentOrgRegProgramId).OrganizationRegulatoryProgramId;
            var currentUserId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.UserProfileId));
            var timeZoneId = Convert.ToInt32(value:_settings.GetOrganizationSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.TimeZone));
            var sampleStartDateTimeUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(localDateTime:sampleDto.StartDateTimeLocal, timeZoneId:timeZoneId);
            var sampleEndDateTimeUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(localDateTime:sampleDto.EndDateTimeLocal, timeZoneId:timeZoneId);
            var massLimitBasisId = _dbContext.LimitBases.Single(lb => lb.Name == LimitBasisName.MassLoading.ToString()).LimitBasisId;
            var concentrationLimitBasisId = _dbContext.LimitBases.Single(lb => lb.Name == LimitBasisName.Concentration.ToString()).LimitBasisId;
            var dailyLimitTypeId = _dbContext.LimitTypes.Single(lt => lt.Name == LimitTypeName.Daily.ToString()).LimitTypeId;

            Core.Domain.Sample sampleToPersist;
            if (sampleDto.SampleId.HasValue && sampleDto.SampleId.Value > 0)
            {
                //Update existing
                sampleToPersist = _dbContext.Samples
                                            .Include(c => c.SampleResults)
                                            .Include(c => c.SampleResults.Select(sr => sr.LimitBasis))
                                            .Single(c => c.SampleId == sampleDto.SampleId);
                sampleToPersist = _mapHelper.GetSampleFromSampleDto(sampleDto:sampleDto, existingSample:sampleToPersist);

                //Handle Sample Result "Deletions"
                //(Remove items from the database that cannot be found in the dto collection)
                //
                var existingSampleResults = sampleToPersist.SampleResults
                                                           .Where(sr => sr.LimitBasis.Name == LimitBasisName.Concentration.ToString()
                                                                        || sr.LimitBasis.Name == LimitBasisName.MassLoading.ToString())
                                                           .ToArray();

                foreach (var existingSampleResult in existingSampleResults) {
                    //Find match in sample dto results
                    var matchedSampleResult = sampleDto.SampleResults.SingleOrDefault(sr =>
                                                                                          sr.ConcentrationSampleResultId.HasValue
                                                                                          && sr.ConcentrationSampleResultId.Value == existingSampleResult.SampleResultId
                                                                                          || sr.MassLoadingSampleResultId.HasValue
                                                                                          && sr.MassLoadingSampleResultId.Value == existingSampleResult.SampleResultId);

                    if (matchedSampleResult == null)
                    {
                        //existing sample result must have been deleted -- remove
                        _dbContext.SampleResults.Remove(entity:existingSampleResult);
                    }
                }
            }
            else
            {
                //Get new
                sampleToPersist = _mapHelper.GetSampleFromSampleDto(sampleDto:sampleDto);
                sampleToPersist.CreationDateTimeUtc = DateTimeOffset.Now;
                sampleToPersist.SampleResults = new List<SampleResult>();
                _dbContext.Samples.Add(entity:sampleToPersist);
            }

            //Set Name auto-generated using settings format
            string sampleName;
            var nameCreationRule = _settings.GetOrgRegProgramSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.SampleNameCreationRule);
            if (nameCreationRule == SampleNameCreationRuleOption.SampleEventType.ToString())
            {
                sampleName = sampleToPersist.CtsEventTypeName;
            }
            else if (nameCreationRule == SampleNameCreationRuleOption.SampleEventTypeCollectionMethod.ToString())
            {
                sampleName = $"{sampleToPersist.CtsEventTypeName} {sampleToPersist.CollectionMethodName}";
            }
            else
            {
                throw new Exception(message:$"ERROR: Unknown SampleNameCreationRuleOption={nameCreationRule}, currentOrgRegProgramId={currentOrgRegProgramId}");
            }

            sampleToPersist.Name = sampleName;
            sampleToPersist.ByOrganizationRegulatoryProgramId = currentOrgRegProgramId; //these are the same only per current workflow
            sampleToPersist.ForOrganizationRegulatoryProgramId = currentOrgRegProgramId; //these are the same only per current workflow
            sampleToPersist.StartDateTimeUtc = sampleStartDateTimeUtc;
            sampleToPersist.EndDateTimeUtc = sampleEndDateTimeUtc;
            sampleToPersist.LastModificationDateTimeUtc = DateTimeOffset.Now;
            sampleToPersist.LastModifierUserId = currentUserId;

            //Handle FlowUnitValidValues
            if (sampleDto.FlowUnitValidValues != null)
            {
                var flowUnitValidValues = "";
                var commaString = "";
                foreach (var unitDto in sampleDto.FlowUnitValidValues)
                {
                    flowUnitValidValues += commaString + unitDto.Name;
                    commaString = ",";
                }

                sampleToPersist.FlowUnitValidValues = flowUnitValidValues;
            }

            //Need to Save to get new SampleId and to load LimitBasis navigation property for next query (only LimitBasisId was set up to this point for new Results!)
            _dbContext.SaveChanges();

            //
            //Handle Flow Result
            //
            var existingFlowResultRow = sampleToPersist.SampleResults
                                                       .SingleOrDefault(sr => sr.LimitBasis.Name == LimitBasisName.VolumeFlowRate.ToString());

            if (!string.IsNullOrEmpty(value:sampleDto.FlowEnteredValue) && sampleDto.FlowUnitId != null && !string.IsNullOrEmpty(value:sampleDto.FlowUnitName))
            {
                //Check flow unit id is within valid values
                if (sampleDto.FlowUnitValidValues == null || !sampleDto.FlowUnitValidValues.Select(fu => fu.UnitId).Contains(value:sampleDto.FlowUnitId.Value))
                {
                    ThrowSimpleException(message:
                                         $"ERROR: Selected flow unit (id={sampleDto.FlowUnitId.Value}, name={sampleDto.FlowUnitName}) could not be found within passed in FlowUnitValidValues collection.");
                }

                if (existingFlowResultRow == null)
                {
                    existingFlowResultRow = new SampleResult {CreationDateTimeUtc = DateTimeOffset.UtcNow, SampleId = sampleToPersist.SampleId};
                    sampleToPersist.SampleResults.Add(item:existingFlowResultRow);
                }

                var flowParameter = _dbContext.Parameters
                                              .First(p => p.IsFlowForMassLoadingCalculation
                                                     && p.OrganizationRegulatoryProgramId == authOrgRegProgramId); 
                //Chris: "Should be one but just get first".

                var flowLimitBasisId = _dbContext.LimitBases.Single(lb => lb.Name == LimitBasisName.VolumeFlowRate.ToString()).LimitBasisId;

                existingFlowResultRow.SampleId = sampleToPersist.SampleId;
                existingFlowResultRow.ParameterId = flowParameter.ParameterId;
                existingFlowResultRow.ParameterName = flowParameter.Name;
                existingFlowResultRow.Qualifier = "";
                existingFlowResultRow.EnteredValue = sampleDto.FlowEnteredValue;

                //existingFlowResultRow.Value = Convert.ToDouble(sampleDto.FlowValue); //handle this below
                existingFlowResultRow.UnitId = sampleDto.FlowUnitId.Value;
                existingFlowResultRow.UnitName = sampleDto.FlowUnitName;
                existingFlowResultRow.EnteredMethodDetectionLimit = "";
                existingFlowResultRow.LimitTypeId = null;
                existingFlowResultRow.LimitBasisId = flowLimitBasisId;
                existingFlowResultRow.LastModificationDateTimeUtc = DateTimeOffset.Now;
                existingFlowResultRow.LastModifierUserId = currentUserId;
                existingFlowResultRow.IsApprovedEPAMethod = true;

                double valueAsDouble;
                if (!double.TryParse(s:existingFlowResultRow.EnteredValue, result:out valueAsDouble))
                {
                    //Could not convert
                    return -1; // throw exception than -1 as that will wrongly treat as sample ID
                }

                existingFlowResultRow.Value = valueAsDouble;
            }
            else
            {
                if (existingFlowResultRow != null)
                {
                    //Flow value must have been deleted -- must remove
                    _dbContext.SampleResults.Remove(entity:existingFlowResultRow);
                }
            }

            //Handle Sample Results "Updates and/or Additions"
            foreach (var sampleResultDto in sampleDto.SampleResults)
            {
                //Each SampleResultDto has at least a Concentration component
                SampleResult concentrationResultRowToUpdate;
                if (!sampleResultDto.ConcentrationSampleResultId.HasValue)
                {
                    concentrationResultRowToUpdate = new SampleResult {CreationDateTimeUtc = DateTimeOffset.UtcNow, SampleId = sampleToPersist.SampleId};
                    _dbContext.SampleResults.Add(entity:concentrationResultRowToUpdate);
                }
                else
                {
                    concentrationResultRowToUpdate = sampleToPersist.SampleResults.Single(sr => sr.SampleResultId == sampleResultDto.ConcentrationSampleResultId.Value);
                }

                //Update Concentration Result
                concentrationResultRowToUpdate =
                    _mapHelper.GetConcentrationSampleResultFromSampleResultDto(dto:sampleResultDto, existingSampleResult:concentrationResultRowToUpdate);

                if (!string.IsNullOrEmpty(value:concentrationResultRowToUpdate.EnteredValue))
                {
                    double valueAsDouble, mdlAsDouble;
                    if (!double.TryParse(s:concentrationResultRowToUpdate.EnteredValue, result:out valueAsDouble))
                    {
                        //Could not convert
                        return -1; // throw exception than -1 as that will wrongly treat as sample ID
                    }

                    concentrationResultRowToUpdate.Value = valueAsDouble;

                    if (double.TryParse(s:concentrationResultRowToUpdate.EnteredMethodDetectionLimit, result:out mdlAsDouble))
                    {
                        concentrationResultRowToUpdate.MethodDetectionLimit = mdlAsDouble;
                    }
                }

                if (sampleResultDto.AnalysisDateTimeLocal.HasValue)
                {
                    concentrationResultRowToUpdate.AnalysisDateTimeUtc = _timeZoneService
                        .GetDateTimeOffsetFromLocalUsingThisTimeZoneId(localDateTime:sampleResultDto.AnalysisDateTimeLocal.Value, timeZoneId:timeZoneId);
                }
                concentrationResultRowToUpdate.LimitBasisId = concentrationLimitBasisId;
                concentrationResultRowToUpdate.LimitTypeId = dailyLimitTypeId;
                concentrationResultRowToUpdate.LastModificationDateTimeUtc = DateTimeOffset.Now;
                concentrationResultRowToUpdate.LastModifierUserId = currentUserId;
                concentrationResultRowToUpdate.IsApprovedEPAMethod = true;

                //this is always persisted with the concentration result NOT the mass loading result
                concentrationResultRowToUpdate.IsMassLoadingCalculationRequired = sampleResultDto.IsCalcMassLoading;

                //... the SampleResultDto MAY ALSO have a Mass Loading component
                if (!string.IsNullOrEmpty(value:sampleResultDto.MassLoadingValue))
                {
                    SampleResult massResultRowToUpdate;
                    if (!sampleResultDto.MassLoadingSampleResultId.HasValue)
                    {
                        massResultRowToUpdate = new SampleResult {CreationDateTimeUtc = DateTimeOffset.UtcNow, SampleId = sampleToPersist.SampleId};
                        _dbContext.SampleResults.Add(entity:massResultRowToUpdate);
                    }
                    else
                    {
                        massResultRowToUpdate = sampleToPersist.SampleResults.Single(sr => sr.SampleResultId == sampleResultDto.MassLoadingSampleResultId.Value);
                    }

                    //Update Mass Loading Result
                    massResultRowToUpdate = _mapHelper.GetMassSampleResultFromSampleResultDto(dto:sampleResultDto, existingSampleResult:massResultRowToUpdate);
                    massResultRowToUpdate.IsMassLoadingCalculationRequired = false; //always FALSE for mass loading result
                    if (!string.IsNullOrEmpty(value:massResultRowToUpdate.EnteredValue))
                    {
                        double massValueAsDouble;
                        if (!double.TryParse(s:massResultRowToUpdate.EnteredValue, result:out massValueAsDouble))
                        {
                            //Could not convert
                            return -1; // throw exception than -1 as that will wrongly treat as sample ID
                        }

                        massResultRowToUpdate.Value = massValueAsDouble;
                    }

                    massResultRowToUpdate.LimitBasisId = massLimitBasisId;
                    massResultRowToUpdate.LimitTypeId = dailyLimitTypeId;
                    massResultRowToUpdate.LastModificationDateTimeUtc = DateTimeOffset.Now;
                    massResultRowToUpdate.LastModifierUserId = currentUserId;
                    massResultRowToUpdate.IsApprovedEPAMethod = true;
                }
            }

            _dbContext.SaveChanges();

            _logger.Info(message:$"End: SampleService.SimplePersist. sampleIdToReturn={sampleToPersist.SampleId}");

            return sampleToPersist.SampleId;
        }

        /// <summary>
        ///     Used to simplify and clean up methods where there are multiple validation tests.
        /// </summary>
        /// <param name="message"> Rule violation message to use when throwing the exception. </param>
        private void ThrowSimpleException(string message)
        {
            _logger.Info(message:$"Start: SampleService.ThrowSimpleException. message={message}");

            var validationIssues = new List<RuleViolation>
                                   {
                                       new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message)
                                   };

            _logger.Info(message:"End: SampleService.ThrowSimpleException.");

            throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
        }

        private int GetVolumeFlowRateLimitBasisId()
        {
            if (_cache.Get(key:"VolumeFlowRateLimitBasisId") == null)
            {
                var flowlimitBasisId = _dbContext.LimitBases.Single(lb => lb.Name == LimitBasisName.VolumeFlowRate.ToString()).LimitBasisId;

                var cacheDurationHours = int.Parse(s:ConfigurationManager.AppSettings[name:"VolumeFlowRateLimitBasisCacheDurationHours"]);
                _cache.Insert(key:"VolumeFlowRateLimitBasisId", item:flowlimitBasisId, hours:cacheDurationHours);
            }

            return Convert.ToInt32(value:_cache.Get(key:"VolumeFlowRateLimitBasisId"));
        }

        /// <summary>
        ///     Helper function to localize the date/times found in a Sample Result. Also sets Last Modifier.
        /// </summary>
        /// <param name="sampleResult"> Sample Result containing UTC date/times </param>
        /// <param name="resultDto"> Output Sample Result dto that needs localized date/times </param>
        /// <param name="timeZoneId"> The time zone id used to localize the date/times </param>
        private void SetSampleResultDatesAndLastModified(SampleResult sampleResult, ref SampleResultDto resultDto, int timeZoneId)
        {
            //Need to set localized time stamps for SampleResults
            if (sampleResult.AnalysisDateTimeUtc.HasValue)
            {
                resultDto.AnalysisDateTimeLocal =
                    _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:sampleResult.AnalysisDateTimeUtc.Value.UtcDateTime, timeZoneId:timeZoneId);
            }

            //Set LastModificationDateTimeLocal
            resultDto.LastModificationDateTimeLocal =
                _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:sampleResult.LastModificationDateTimeUtc?.UtcDateTime ?? sampleResult.CreationDateTimeUtc.UtcDateTime,
                                                             timeZoneId:timeZoneId);

            if (sampleResult.LastModifierUserId.HasValue)
            {
                var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == sampleResult.LastModifierUserId.Value);
                resultDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
            }
            else
            {
                resultDto.LastModifierFullName = "N/A";
            }
        }

		private void CheckResultComplianceInner(SampleResultDto sampleResultDto, int monitoringPointId, List<MonitoringPointParameter> monitoringPointParameters,
										 List<MonitoringPointParameterLimit> monitoringPointParameterLimits, DateTime sampleDateTime, LimitBasisName limitBasisName)
		{
			//Set compliance as unknown initially by default
			if (limitBasisName == LimitBasisName.Concentration)
			{
				sampleResultDto.ConcentrationResultCompliance = ResultComplianceType.Unknown;
				sampleResultDto.ConcentrationResultComplianceComment = string.Format(Message.ResultComplianceUnknown, sampleResultDto.ParameterName);
			}
			else if (limitBasisName == LimitBasisName.MassLoading)
			{
				sampleResultDto.MassResultCompliance = ResultComplianceType.Unknown;
				sampleResultDto.MassResultComplianceComment = string.Format(Message.ResultComplianceUnknown, sampleResultDto.ParameterName);
			}

			var parameterId = sampleResultDto.ParameterId;

			//Check MonitoringPointParameter
			var foundMonitoringPointParameter = monitoringPointParameters.FirstOrDefault(mppl => mppl.MonitoringPointId == monitoringPointId
																								 && mppl.ParameterId == parameterId
																								 && mppl.EffectiveDateTime <= sampleDateTime
																								 && mppl.RetirementDateTime >= sampleDateTime);

			if (foundMonitoringPointParameter == null)
			{
				return;
			}

			int foundMonitoringPointParameterId = foundMonitoringPointParameter.MonitoringPointParameterId;

			var foundLimit = monitoringPointParameterLimits.FirstOrDefault(mppl => mppl.MonitoringPointParameterId == foundMonitoringPointParameterId
																				   && mppl.LimitBasis.Name == limitBasisName.ToString()
																				   && mppl.LimitType.Name == LimitTypeName.Daily.ToString()
																				   && !mppl.IsAlertOnly);

			if (foundLimit != null)
			{
				var floor = foundLimit.MinimumValue;
				var ceiling = foundLimit.MaximumValue;

				//If qualifier is "ND" or "NF" both concentration and mass is in compliance
				if (sampleResultDto.Qualifier == "ND" || sampleResultDto.Qualifier == "NF")
				{
					//Compliance met
					if (limitBasisName == LimitBasisName.Concentration)
					{
						sampleResultDto.ConcentrationResultCompliance = ResultComplianceType.Good;
						if (floor.HasValue)
						{
							sampleResultDto.ConcentrationResultComplianceComment =
								string.Format(Message.ResultComplianceGoodWithinRange, sampleResultDto.ParameterName, sampleResultDto.Qualifier, floor, ceiling);
						}
						else
						{
							sampleResultDto.ConcentrationResultComplianceComment =
								string.Format(Message.ResultComplianceGoodWithinLimit, sampleResultDto.ParameterName, sampleResultDto.Qualifier, ceiling);
						}

					}
					else if (limitBasisName == LimitBasisName.MassLoading)
					{
						sampleResultDto.MassResultCompliance = ResultComplianceType.Good;
						if (floor.HasValue)
						{
							sampleResultDto.MassResultComplianceComment =
								string.Format(Message.ResultComplianceGoodWithinRange, sampleResultDto.ParameterName, sampleResultDto.Qualifier, floor, ceiling);
						}
						else
						{
							sampleResultDto.MassResultComplianceComment =
								string.Format(Message.ResultComplianceGoodWithinLimit, sampleResultDto.ParameterName, sampleResultDto.Qualifier, ceiling);
						}

					}

					return;
				}

				double valueToCheck; //Concentration value or mass loading value
				string qualifierToCheck;
				if (limitBasisName == LimitBasisName.Concentration)
				{
					if (sampleResultDto.Value != null)
					{
						valueToCheck = sampleResultDto.Value.Value;
						qualifierToCheck = sampleResultDto.Qualifier;
					}
					else
					{
						return;
					}
				}
				else if (limitBasisName == LimitBasisName.MassLoading)
				{
					if (!string.IsNullOrWhiteSpace(sampleResultDto.MassLoadingValue)
						&& Double.TryParse(sampleResultDto.MassLoadingValue, out valueToCheck))
					{
						//valueToCheck is set via "out" parameter
						qualifierToCheck = sampleResultDto.MassLoadingQualifier;
					}
					else
					{
						return;
					}
				}
				else
				{
					throw new Exception($"CheckResultCompliance. ERROR: cannot check compliance. Limit Basis = {limitBasisName}.");
				}


				if (floor.HasValue)
				{
					//Range limit
					if ((qualifierToCheck == ">" && valueToCheck >= ceiling)
						|| (qualifierToCheck == "<" && valueToCheck <= floor)
						|| (valueToCheck > ceiling || valueToCheck < floor))
					{
						//Outside the range
						if (limitBasisName == LimitBasisName.Concentration)
						{
							sampleResultDto.ConcentrationResultCompliance = ResultComplianceType.Bad;
							sampleResultDto.ConcentrationResultComplianceComment =
								string.Format(Message.ResultComplianceBadOutsideRange, sampleResultDto.ParameterName, sampleResultDto.Qualifier + sampleResultDto.EnteredValue, floor, ceiling);
						}
						else if (limitBasisName == LimitBasisName.MassLoading)
						{
							sampleResultDto.MassResultCompliance = ResultComplianceType.Bad;
							sampleResultDto.MassResultComplianceComment =
								string.Format(Message.ResultComplianceBadOutsideRange, sampleResultDto.ParameterName, sampleResultDto.MassLoadingQualifier + sampleResultDto.MassLoadingValue, floor,
											  ceiling);
						}
					}
					else
					{
						//Compliance met
						if (limitBasisName == LimitBasisName.Concentration)
						{
							sampleResultDto.ConcentrationResultCompliance = ResultComplianceType.Good;
							sampleResultDto.ConcentrationResultComplianceComment =
								string.Format(Message.ResultComplianceGoodWithinRange, sampleResultDto.ParameterName, sampleResultDto.Qualifier + sampleResultDto.EnteredValue, floor, ceiling);
						}
						else if (limitBasisName == LimitBasisName.MassLoading)
						{
							sampleResultDto.MassResultCompliance = ResultComplianceType.Good;
							sampleResultDto.MassResultComplianceComment =
								string.Format(Message.ResultComplianceGoodWithinRange, sampleResultDto.ParameterName, sampleResultDto.MassLoadingQualifier + sampleResultDto.MassLoadingValue, floor,
											  ceiling);
						}
					}
				}
				else
				{
					//Max-only limit
					if ((qualifierToCheck == ">" && valueToCheck >= ceiling)
						|| (valueToCheck > ceiling))
					{
						//Over the max
						if (limitBasisName == LimitBasisName.Concentration)
						{
							sampleResultDto.ConcentrationResultCompliance = ResultComplianceType.Bad;
							sampleResultDto.ConcentrationResultComplianceComment =
								string.Format(Message.ResultComplianceBadAboveMax, sampleResultDto.ParameterName, sampleResultDto.Qualifier + sampleResultDto.EnteredValue, ceiling);
						}
						else if (limitBasisName == LimitBasisName.MassLoading)
						{
							sampleResultDto.MassResultCompliance = ResultComplianceType.Bad;
							sampleResultDto.MassResultComplianceComment =
								string.Format(Message.ResultComplianceBadAboveMax, sampleResultDto.ParameterName, sampleResultDto.MassLoadingQualifier + sampleResultDto.MassLoadingValue, ceiling);

						}
					}
					else
					{
						//Compliance met
						if (limitBasisName == LimitBasisName.Concentration)
						{
							sampleResultDto.ConcentrationResultCompliance = ResultComplianceType.Good;
							sampleResultDto.ConcentrationResultComplianceComment =
								string.Format(Message.ResultComplianceGoodWithinLimit, sampleResultDto.ParameterName, sampleResultDto.Qualifier + sampleResultDto.EnteredValue, ceiling);
						}
						else if (limitBasisName == LimitBasisName.MassLoading)
						{
							sampleResultDto.MassResultCompliance = ResultComplianceType.Good;
							sampleResultDto.MassResultComplianceComment =
								string.Format(Message.ResultComplianceGoodWithinLimit, sampleResultDto.ParameterName, sampleResultDto.MassLoadingQualifier + sampleResultDto.MassLoadingValue, ceiling);
						}
					}
				}
			}
		}

	}
}