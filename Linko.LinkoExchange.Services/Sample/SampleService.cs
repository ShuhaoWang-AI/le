using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Linko.LinkoExchange.Services.Cache;
using System.Collections.ObjectModel;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Services.Sample
{
    public class SampleService : ISampleService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContext;
        private readonly IOrganizationService _orgService;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ISettingService _settings;

        public SampleService(LinkoExchangeContext dbContext,
            IHttpContextService httpContext,
            IOrganizationService orgService,
            IMapHelper mapHelper,
            ILogger logger,
            ITimeZoneService timeZoneService,
            ISettingService settings)
        {
            _dbContext = dbContext;
            _httpContext = httpContext;
            _orgService = orgService;
            _mapHelper = mapHelper;
            _logger = logger;
            _timeZoneService = timeZoneService;
            _settings = settings;
        }

        /// <summary>
        /// Maps Sample Dto to Sample and saves to database. No validation.
        /// </summary>
        /// <param name="sampleDto">Sample Dto to map and save</param>
        /// <returns>Existing Id or newly created Id of Sample row in tSample table</returns>
        private int SimplePersist(SampleDto sampleDto)
        {
            string sampleIdString = string.Empty;
            if (sampleDto.SampleId.HasValue)
            {
                sampleIdString = sampleDto.SampleId.Value.ToString();
            }
            else {
                sampleIdString = "null";
            }

            _logger.Info($"Enter SampleService.SimplePersist. sampleDto.SampleId.Value={sampleIdString}");

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var authOrgRegProgramId = _orgService.GetAuthority(currentOrgRegProgramId).OrganizationRegulatoryProgramId;
            var currentUserId = int.Parse(_httpContext.GetClaimValue(CacheKey.UserProfileId));
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));
            var sampleIdToReturn = -1;
            var sampleStartDateTimeUtc = _timeZoneService.GetUTCDateTimeUsingThisTimeZoneId(sampleDto.StartDateTimeLocal, timeZoneId);
            var sampleEndDateTimeUtc = _timeZoneService.GetUTCDateTimeUsingThisTimeZoneId(sampleDto.EndDateTimeLocal, timeZoneId);

            try
            {
                Core.Domain.Sample sampleToPersist = null;

                if (sampleDto.SampleId.HasValue && sampleDto.SampleId.Value > 0)
                {

                    if (IsSampleIncludedInReportPackage(sampleDto.SampleId.Value))
                    {
                        //Sample is in use in a Report Package (draft or otherwise)...  
                        //Actor can not perform any actions of any kind except view all details.
                        return sampleDto.SampleId.Value;
                    }

                    //Update existing
                    sampleToPersist = _dbContext.Samples.Single(c => c.SampleId == sampleDto.SampleId);
                    sampleToPersist = _mapHelper.GetSampleFromSampleDto(sampleDto, sampleToPersist);

                    //Delete existing results
                    var existingSampleResults = _dbContext.SampleResults
                        .Where(sr => sr.SampleId == sampleDto.SampleId);
                    _dbContext.SampleResults.RemoveRange(existingSampleResults);
                }
                else
                {
                    //Get new
                    sampleToPersist = _mapHelper.GetSampleFromSampleDto(sampleDto);
                    sampleToPersist.CreationDateTimeUtc = DateTimeOffset.UtcNow;
                    _dbContext.Samples.Add(sampleToPersist);
                }

                sampleToPersist.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                sampleToPersist.StartDateTimeUtc = sampleStartDateTimeUtc;
                sampleToPersist.EndDateTimeUtc = sampleEndDateTimeUtc;
                sampleToPersist.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;
                sampleToPersist.LastModifierUserId = currentUserId;

                _dbContext.SaveChanges(); //Needed here?

                sampleIdToReturn = sampleToPersist.SampleId;

                //Add results
                sampleToPersist.SampleResults = new Collection<SampleResult>();
                //
                //Add flow result first (if exists)
                //  - this is only required if there is at least 1 mass loading result

                if (sampleDto.FlowValue != null && sampleDto.FlowUnitId != null && !string.IsNullOrEmpty(sampleDto.FlowUnitName))
                {
                    var flowParameter = _dbContext.Parameters
                        .First(p => p.IsFlowForMassLoadingCalculation == true); //Chris: "Should be one but just get first".

                    var flowResult = new SampleResult()
                    {
                        SampleId = sampleIdToReturn,
                        ParameterId = flowParameter.ParameterId,
                        ParameterName = flowParameter.Name,
                        Qualifier = "",
                        Value = sampleDto.FlowValue,
                        DecimalPlaces = sampleDto.FlowValueDecimalPlaces,
                        UnitId = sampleDto.FlowUnitId.Value,
                        UnitName = sampleDto.FlowUnitName,
                        MethodDetectionLimit = "",
                        IsFlowForMassLoadingCalculation = true,
                        LimitTypeId = null,
                        LimitBasisId = null,
                        IsCalculated = false
                    };
                    sampleToPersist.SampleResults.Add(flowResult);

                }

                //Add "regular" sample results
                var massLimitBasisId = _dbContext.LimitBases.Single(lb => lb.Name == LimitBasisName.MassLoading.ToString()).LimitBasisId;
                var concentrationLimitBasisId = _dbContext.LimitBases.Single(lb => lb.Name == LimitBasisName.Concentration.ToString()).LimitBasisId;
                var dailyLimitTypeId = _dbContext.LimitTypes.Single(lt => lt.Name == LimitTypeName.Daily.ToString()).LimitTypeId;
                foreach (var resultDto in sampleDto.SampleResults)
                {

                    //Concentration result
                    var sampleResult = _mapHelper.GetConcentrationSampleResultFromSampleResultDto(resultDto);
                    sampleResult.AnalysisDateTimeUtc = _timeZoneService
                        .GetUTCDateTimeUsingThisTimeZoneId(resultDto.AnalysisDateTimeLocal.Value, timeZoneId);
                    sampleResult.LimitBasisId = concentrationLimitBasisId;
                    sampleResult.LimitTypeId = dailyLimitTypeId;
                    sampleResult.CreationDateTimeUtc = DateTimeOffset.UtcNow;
                    sampleResult.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;
                    sampleResult.LastModifierUserId = currentUserId;

                    sampleToPersist.SampleResults.Add(sampleResult);

                    //Mass result (if calculated)
                    if (resultDto.IsCalcMassLoading)
                    {
                        sampleResult.IsMassLoadingCalculationRequired = true;

                        var sampleMassResult = _mapHelper.GetMassSampleResultFromSampleResultDto(resultDto);
                        sampleMassResult.AnalysisDateTimeUtc = _timeZoneService
                            .GetUTCDateTimeUsingThisTimeZoneId(resultDto.AnalysisDateTimeLocal.Value, timeZoneId);
                        sampleMassResult.LimitBasisId = massLimitBasisId;
                        sampleMassResult.LimitTypeId = dailyLimitTypeId;
                        sampleMassResult.CreationDateTimeUtc = DateTimeOffset.UtcNow;
                        sampleMassResult.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;
                        sampleMassResult.LastModifierUserId = currentUserId;

                        sampleToPersist.SampleResults.Add(sampleMassResult);
                    }
                }

                _dbContext.SaveChanges();

            }
            catch (RuleViolationException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                var errors = new List<string>() { ex.Message };

                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    errors.Add(ex.Message);
                }

                _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                throw;
            }

            _logger.Info($"Leaving SampleService.SimplePersist. sampleIdToReturn={sampleIdToReturn}");

            return sampleIdToReturn;

        }

        /// <summary>
        /// Saves a Sample to the database after validating. Throw a list of RuleViolation exceptions
        /// for failed validation issues.
        /// </summary>
        /// <param name="sample">Sample Dto</param>
        /// <param name="isSavingAsReadyToSubmit">True to perform stricter validation</param>
        /// <returns>Existing Sample Id or newly created Sample Id</returns>
        public int SaveSample(SampleDto sampleDto, bool isSavingAsReadyToReport = false)
        {
            string sampleIdString = string.Empty;
            if (sampleDto.SampleId.HasValue)
            {
                sampleIdString = sampleDto.SampleId.Value.ToString();
            }
            else
            {
                sampleIdString = "null";
            }
            _logger.Info($"Enter SampleService.SaveSample. sampleDto.SampleId.Value={sampleIdString}, isSavingAsReadyToReport={isSavingAsReadyToReport}");

            var sampleId = -1;
            using (var transaction = _dbContext.BeginTransaction())
            {
                try {

                    if (this.IsValidSample(sampleDto, isSavingAsReadyToReport, isSuppressExceptions: false))
                    {
                        if (isSavingAsReadyToReport)
                        {
                            //Update the sample status to "Ready to Report"
                            var sampleStatusReadyToReport = _dbContext.SampleStatuses
                                .Single(ss => ss.Name == SampleStatusName.ReadyToReport.ToString());

                            sampleDto.SampleStatusId = sampleStatusReadyToReport.SampleStatusId;

                        }
                        sampleId = this.SimplePersist(sampleDto);
                        transaction.Commit();
                    }
                }
                catch (RuleViolationException ex)
                {
                    transaction.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    var errors = new List<string>() { ex.Message };

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(ex.Message);
                    }

                    _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                    throw;
                }

            }
               
            _logger.Info($"Leaving SampleService.SaveSample. sampleId={sampleId}, isSavingAsReadyToReport={isSavingAsReadyToReport}");
            return sampleId;
        }

        /// <summary>
        /// Used to simplify and clean up methods where there are multiple validation tests.
        /// </summary>
        /// <param name="message">Rule violation message to use when throwing the exception.</param>
        private void ThrowSimpleException(string message)
        {
            _logger.Info($"Enter SampleService.ThrowSimpleException. message={message}");

            List<RuleViolation> validationIssues = new List<RuleViolation>();
            validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));

            _logger.Info($"Leaving SampleService.ThrowSimpleException. message={message}");

            throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
        }

        /// <summary>
        /// Tests validation of a passed in Sample in either Draft or ReadyToReport Mode
        /// </summary>
        /// <param name="sampleDto">Sample to validate</param>
        /// <param name="isReadyToSubmit">False = Draft Mode, True = ReadyToReport Mode</param>
        /// <param name="isSuppressExceptions">False = throws RuleViolation exception, True = does not throw RuleViolation exceptions</param>
        /// <returns>Boolean indicating if Sample passed all validation (Draft or ReadyToReport mode)</returns>
        public bool IsValidSample(SampleDto sampleDto, bool isReadyToReport, bool isSuppressExceptions = false)
        {
            string sampleIdString = string.Empty;
            if (sampleDto.SampleId.HasValue)
            {
                sampleIdString = sampleDto.SampleId.Value.ToString();
            }
            else
            {
                sampleIdString = "null";
            }
            _logger.Info($"Enter SampleService.IsValidSample. sampleDto.SampleId.Value={sampleIdString}");

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var authOrgRegProgramId = _orgService.GetAuthority(currentOrgRegProgramId).OrganizationRegulatoryProgramId;
            var currentUserId = int.Parse(_httpContext.GetClaimValue(CacheKey.UserProfileId));
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(authOrgRegProgramId, SettingType.TimeZone));
            bool isValid = true;

            //Check required field (UC-15-1.2.1): "Sample Type"
            if (sampleDto.CtsEventTypeId < 1)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    this.ThrowSimpleException("Sample Type is required.");
                }
            }
            //Check required field (UC-15-1.2.1): "Collection Method"
            if (sampleDto.CollectionMethodId < 1)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    this.ThrowSimpleException("Collection Method is required.");
                }
            }
            //Check required field (UC-15-1.2.1): "Start Date/Time"
            if (sampleDto.StartDateTimeLocal == null)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    this.ThrowSimpleException("Start Date/Time is required.");
                }
            }
            //Check required field (UC-15-1.2.1): "End Date/Time"
            if (sampleDto.EndDateTimeLocal == null)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    this.ThrowSimpleException("End Date/Time is required.");
                }
            }

            //Check sample start/end dates are not in the future (UC-15-1.2.9.1.b)
            
            if (_timeZoneService.GetUTCDateTimeUsingThisTimeZoneId(sampleDto.StartDateTimeLocal, timeZoneId) > DateTime.UtcNow ||
                _timeZoneService.GetUTCDateTimeUsingThisTimeZoneId(sampleDto.EndDateTimeLocal, timeZoneId) > DateTime.UtcNow)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    this.ThrowSimpleException("Sample dates cannot be future dates.");
                }
            }

            //Check results
            //

            //
            //Iterate through all the sample result dtos to make sure
            //  - qualifier exists and if required, concentration values and units exist
            //  - if calc mass loading, we have flow value(s)
            //  - if calc mass loading, we have mass value(s)

            bool isValidFlowValueExists = false;
            if (sampleDto.FlowValue != null && sampleDto.FlowUnitId != null && !string.IsNullOrEmpty(sampleDto.FlowUnitName))
            {
                isValidFlowValueExists = true;
            }

            foreach (var resultDto in sampleDto.SampleResults)
            {
                if (isReadyToReport &&
                    ((resultDto.Qualifier == ">" || resultDto.Qualifier == "<" || string.IsNullOrEmpty(resultDto.Qualifier))
                    && resultDto.Value == null))
                {
                    isValid = false;
                    if (!isSuppressExceptions)
                    {
                        this.ThrowSimpleException("All numeric values must be accompanied by a numeric qualifier.");
                    }
                }

                if (isReadyToReport &&
                    (resultDto.Qualifier == "ND" || resultDto.Qualifier == "NF" && resultDto.Value != null))
                {
                    isValid = false;
                    if (!isSuppressExceptions)
                    {
                        this.ThrowSimpleException("Only null values can be associated with non-numeric qualifiers.");
                    }
                }

                if (isReadyToReport &&
                    (resultDto.IsCalcMassLoading && !isValidFlowValueExists))
                {
                    isValid = false;
                    if (!isSuppressExceptions)
                    {
                        this.ThrowSimpleException("You must provide valid a flow value to calculate mass loading results");
                    }
                }

                if (isReadyToReport &&
                    (resultDto.IsCalcMassLoading && 
                    (resultDto.MassLoadingUnitId < 0 || resultDto.MassLoadingValue == null)))
                {
                    isValid = false;
                    if (!isSuppressExceptions)
                    {
                        this.ThrowSimpleException("You must provide valid mass loading unit/value if electing to calculate mass loading results");
                    }
                }
            }

            _logger.Info($"Leaving SampleService.IsValidSample. sampleDto.SampleId.Value={sampleIdString}, isValid={isValid}");

            return isValid;
        }

        /// <summary>
        /// Deletes a sample from the database
        /// </summary>
        /// <param name="sampleId">SampleId associated with the object in the tSample table</param>
        public void DeleteSample(int sampleId)
        {
            _logger.Info($"Enter SampleService.DeleteSample. sampleId={sampleId}");
            List<RuleViolation> validationIssues = new List<RuleViolation>();
            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {

                    if (this.IsSampleIncludedInReportPackage(sampleId))
                    {
                        string message = "Attempting to delete a sample that is included in a report.";
                        validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                        throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                    }
                    else
                    {
                        var sampleToRemove = _dbContext.Samples
                            .Include(s => s.SampleResults)
                            .Single(s => s.SampleId == sampleId);

                        //First remove results (if applicable)
                        if (sampleToRemove.SampleResults != null)
                        {
                            _dbContext.SampleResults.RemoveRange(sampleToRemove.SampleResults);
                        }
                        _dbContext.Samples.Remove(sampleToRemove);
                        _dbContext.SaveChanges();
                        transaction.Commit();
                    }

                    _logger.Info($"Leaving SampleService.DeleteSample. sampleId={sampleId}");

                }
                catch (RuleViolationException ex)
                {
                    transaction.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    var errors = new List<string>() { ex.Message };

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(ex.Message);
                    }

                    _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                    throw;
                }

            }
        }

        /// <summary>
        /// Helper method to map a passed in Sample object to a Sample Dto.
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        private SampleDto GetSampleDetails(Core.Domain.Sample sample)
        {
            _logger.Info($"Enter SampleService.GetSampleDetails. sample.SampleId={sample.SampleId}");
            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));

            var dto = _mapHelper.GetSampleDtoFromSample(sample);

            //Set Sample Start Local Timestamp
            dto.StartDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(sample.StartDateTimeUtc.DateTime, timeZoneId);

            //Set Sample End Local Timestamp
            dto.EndDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(sample.EndDateTimeUtc.DateTime, timeZoneId);

            //Set LastModificationDateTimeLocal
            dto.LastModificationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId((sample.LastModificationDateTimeUtc.HasValue ? sample.LastModificationDateTimeUtc.Value.DateTime
                        : sample.CreationDateTimeUtc.DateTime), timeZoneId);

            if (sample.LastModifierUserId.HasValue)
            {
                var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == sample.LastModifierUserId.Value);
                dto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
            }
            else
            {
                dto.LastModifierFullName = "N/A";
            }

            var resultDtoList = new Dictionary<int, SampleResultDto>();
            foreach (var sampleResult in sample.SampleResults)
            {
                //Handle "special case" Sample Results. These do not get mapped to their own
                //SampleResult dtos.
                //1. Flow - gets mapped to properties of the parent Sample Dto
                //2. Mass - gets mapped to corresponding Concentration result Dto
                //
                //Remember that the items in this collection are unordered.

                var resultDto = new SampleResultDto();
                
                if (sampleResult.IsFlowForMassLoadingCalculation &&
                    (sampleResult.LimitBasisId == null && sampleResult.LimitTypeId == null))
                {
                    dto.FlowValue = sampleResult.Value;
                    dto.FlowUnitId = sampleResult.UnitId;
                    dto.FlowUnitName = sampleResult.UnitName;
                    dto.FlowValueDecimalPlaces = sampleResult.DecimalPlaces;

                }
                else if (sampleResult.IsFlowForMassLoadingCalculation == false &&
                    sampleResult.LimitType.Name == LimitTypeName.Daily.ToString() &&
                    (sampleResult.LimitBasis.Name == LimitBasisName.MassLoading.ToString() 
                    || sampleResult.LimitBasis.Name == LimitBasisName.Concentration.ToString()))
                {

                    if (resultDtoList.ContainsKey(sampleResult.ParameterId))
                    {
                        //There was already a result dto added for this parameter
                        //and we are now handling the corresponding concentration (or mass) result
                        //and must attach these fields to that dto
                        resultDto = resultDtoList[sampleResult.ParameterId];
                    }
                    else
                    {
                        //There may be a corresponding concentation (or mass) result
                        //later in the collection that needs to be attached to this result dto
                        //so we need to save this for looking up later. 
                        resultDtoList.Add(sampleResult.ParameterId, resultDto);
                    }

                    if (sampleResult.LimitBasis.Name == LimitBasisName.Concentration.ToString())
                    {
                        resultDto.SampleId = sampleResult.SampleId;
                        resultDto.ParameterId = sampleResult.ParameterId;
                        resultDto.ParameterName = sampleResult.ParameterName;
                        resultDto.MethodDetectionLimit = sampleResult.MethodDetectionLimit;
                        resultDto.AnalysisMethod = sampleResult.AnalysisMethod;
                        resultDto.IsApprovedEPAMethod = sampleResult.IsApprovedEPAMethod;
                        resultDto.ParameterId = sampleResult.ParameterId;
                        //resultDto.IsCalcMassLoading = sampleResult.IsMassLoadingCalculationRequired;
                        resultDto.Qualifier = sampleResult.Qualifier;
                        resultDto.Value = sampleResult.Value;
                        resultDto.DecimalPlaces = sampleResult.DecimalPlaces;
                        resultDto.UnitId = sampleResult.UnitId;
                        resultDto.UnitName = sampleResult.UnitName;

                        SetSampleResultDatesAndLastModified(sampleResult, ref resultDto, timeZoneId);

                    }
                    else {
                        //Mass Result
                        resultDto.MassLoadingQualifier = sampleResult.Qualifier;
                        resultDto.MassLoadingValue = sampleResult.Value;
                        resultDto.MassLoadingDecimalPlaces = sampleResult.DecimalPlaces;
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
            _logger.Info($"Leaving SampleService.GetSampleDetails. sample.SampleId={sample.SampleId}");
            return dto;
        }

        /// <summary>
        /// Helper function to localize the date/times found in a Sample Result. Also sets Last Modifier.
        /// </summary>
        /// <param name="sampleResult">Sample Result containing UTC date/times</param>
        /// <param name="resultDto">Output Sample Result dto that needs localized date/times</param>
        /// <param name="timeZoneId">The time zone id used to localize the date/times</param>
        private void SetSampleResultDatesAndLastModified(SampleResult sampleResult, ref SampleResultDto resultDto, int timeZoneId)
        {
            _logger.Info($"Enter SampleService.SetSampleResultDatesAndLastModified. sampleResult.SampleId={sampleResult.SampleId}");

            //Need to set localized time stamps for SampleResults
            if (sampleResult.AnalysisDateTimeUtc.HasValue)
            {
                resultDto.AnalysisDateTimeLocal = _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(sampleResult.AnalysisDateTimeUtc.Value.DateTime, timeZoneId);
            }

            //Set LastModificationDateTimeLocal
            resultDto.LastModificationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId((sampleResult.LastModificationDateTimeUtc.HasValue ? sampleResult.LastModificationDateTimeUtc.Value.DateTime
                        : sampleResult.CreationDateTimeUtc.DateTime), timeZoneId);

            if (sampleResult.LastModifierUserId.HasValue)
            {
                var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == sampleResult.LastModifierUserId.Value);
                resultDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
            }
            else
            {
                resultDto.LastModifierFullName = "N/A";
            }

            _logger.Info($"Leaving SampleService.SetSampleResultDatesAndLastModified. sampleResult.SampleId={sampleResult.SampleId}");
        }

        /// <summary>
        /// Gets the complete details of a single Sample
        /// </summary>
        /// <param name="sampleId">SampleId associated with the object in the tSample table</param>
        /// <returns>Sample Dto associated with the passed in Id</returns>
        public SampleDto GetSampleDetails(int sampleId)
        {
            _logger.Info($"Enter SampleService.GetSampleDetails. sampleId={sampleId}");

            var sample = _dbContext.Samples
                .Include(s => s.SampleStatus)
                .Include(s => s.SampleResults)
                .Single(s => s.SampleId == sampleId);

            var dto = this.GetSampleDetails(sample);

            _logger.Info($"Leaving SampleService.GetSampleDetails. sampleId={sampleId}");

            return dto;
        }

        /// <summary>
        /// Test to see if a Sample is included in at least 1 report package
        /// </summary>
        /// <param name="sampleId">SampleId associated with the object in the tSample table</param>
        /// <returns>Boolean indicating if the Sample is or isn't included in at least 1 report package</returns>
        public bool IsSampleIncludedInReportPackage(int sampleId)
        {
            _logger.Info($"Enter SampleService.IsSampleIncludedInReportPackage. sampleId={sampleId}");

            var isExists = _dbContext.ReportSamples.Any(rs => rs.SampleId == sampleId);

            _logger.Info($"Leaving SampleService.IsSampleIncludedInReportPackage. sampleId={sampleId}");

            return isExists;
        }

        /// <summary>
        /// Gets Samples from the database for displaying in a grid
        /// </summary>
        /// <param name="status">SampletStatus type to filter by</param>
        /// <param name="startDate">Nullable localized date/time time period range. 
        /// Sample start dates must on or after this date/time. Null parameters are ignored and not part of the filter.</param>
        /// <param name="endDate">Nullable localized date/time time period range. 
        /// Sample end dates must on or before this date/time. Null parameters are ignored and not part of the filter.</param>
        /// <returns>Collection of filtered Sample Dto's</returns>
        public IEnumerable<SampleDto> GetSamples(SampleStatusName status, DateTime? startDate = null, DateTime? endDate = null)
        {
            string startDateString = startDate.HasValue ? startDate.Value.ToString() : "null";
            string endDateString = endDate.HasValue ? endDate.Value.ToString() : "null";
            _logger.Info($"Enter SampleService.GetSamples. status={status}, startDate={startDateString}, endDate={endDateString}");

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));
            var dtos = new List<SampleDto>();
            var foundSamples = _dbContext.Samples
                .Include(s => s.SampleStatus)
                .Include(s => s.SampleResults)
                .Where(s => s.OrganizationRegulatoryProgramId == currentOrgRegProgramId);

            if (startDate.HasValue)
            {
                var startDateUtc = _timeZoneService.GetUTCDateTimeUsingThisTimeZoneId(startDate.Value, timeZoneId);
                foundSamples = foundSamples.Where(s => s.StartDateTimeUtc >= startDateUtc);
            }
            if (endDate.HasValue)
            {
                var endDateUtc = _timeZoneService.GetUTCDateTimeUsingThisTimeZoneId(startDate.Value, timeZoneId);
                foundSamples = foundSamples.Where(s => s.EndDateTimeUtc <= endDateUtc);
            }


            switch (status)
            {
                case SampleStatusName.All:
                    //don't filter any further
                    break;
                case SampleStatusName.Draft:
                case SampleStatusName.ReadyToReport:
                case SampleStatusName.Reported:
                    foundSamples = foundSamples.Where(s => s.SampleStatus.Name == status.ToString());
                    break;
                case SampleStatusName.DraftOrReadyToReport:
                    foundSamples = foundSamples.Where(s => s.SampleStatus.Name == SampleStatusName.Draft.ToString()
                            || s.SampleStatus.Name == SampleStatusName.ReadyToReport.ToString());
                    break;
                case SampleStatusName.DraftOrReported:
                    foundSamples = foundSamples.Where(s => s.SampleStatus.Name == SampleStatusName.Draft.ToString()
                            || s.SampleStatus.Name == SampleStatusName.Reported.ToString());
                    break;
                case SampleStatusName.ReadyToReportOrReported:
                    foundSamples = foundSamples.Where(s => s.SampleStatus.Name == SampleStatusName.ReadyToReport.ToString()
                            || s.SampleStatus.Name == SampleStatusName.Reported.ToString());
                    break;
                default:
                    throw new Exception($"Unknown SampleStatusName = {status}");
            }

            foreach (var sample in foundSamples.ToList())
            {
                var dto = this.GetSampleDetails(sample);
                dtos.Add(dto);
            }

            _logger.Info($"Leaving SampleService.GetSamples. status={status}, startDate={startDateString}, endDate={endDateString}, dtos.Count={dtos.Count()}");

            return dtos;
        }
    }
}
