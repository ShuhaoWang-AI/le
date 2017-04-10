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

        public int SaveSample(SampleDto sampleDto, bool isSavingAsReadyToSubmit = false)
        {
            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var authOrgRegProgramId = _orgService.GetAuthority(currentOrgRegProgramId).OrganizationRegulatoryProgramId;
            var currentUserId = int.Parse(_httpContext.GetClaimValue(CacheKey.UserProfileId));
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));
            var sampleIdToReturn = -1;
            bool isFlowValuesExist = false;
            List<RuleViolation> validationIssues = new List<RuleViolation>();

            //Check required field (UC-15-1.2.1): "Sample Type"
            if (sampleDto.CtsEventTypeId < 1)
            {
                ThrowSimpleException("Sample Type is required.");
            }
            //Check required field (UC-15-1.2.1): "Collection Method"
            if (sampleDto.CollectionMethodId < 1)
            {
                ThrowSimpleException("Collection Method is required.");
            }
            //Check required field (UC-15-1.2.1): "Start Date/Time"
            if (sampleDto.StartDateTimeLocal == null)
            {
                ThrowSimpleException("Start Date/Time is required.");
            }
            //Check required field (UC-15-1.2.1): "End Date/Time"
            if (sampleDto.EndDateTimeLocal == null)
            {
                ThrowSimpleException("End Date/Time is required.");
            }
            
            //Check sample start/end dates are not in the future (UC-15-1.2.9.1.b)

            var sampleStartDateTimeUtc = _timeZoneService.GetUTCDateTimeUsingThisTimeZoneId(sampleDto.StartDateTimeLocal, timeZoneId);
            var sampleEndDateTimeUtc = _timeZoneService.GetUTCDateTimeUsingThisTimeZoneId(sampleDto.EndDateTimeLocal, timeZoneId);

            if (sampleStartDateTimeUtc > DateTime.UtcNow || sampleEndDateTimeUtc > DateTime.UtcNow)
            {
                ThrowSimpleException("Sample dates cannot be future dates.");
            }

            if (sampleDto.FlowUnitId != null || sampleDto.FlowValue != null)
            {
                //All flow values must exist!
                if (sampleDto.FlowUnitId == null || sampleDto.FlowValue == null)
                {
                    ThrowSimpleException("A flow value must be accompanied by a flow unit.");
                }

                isFlowValuesExist = true;
            }

            using (var transaction = _dbContext.BeginTransaction())
            {
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

                    if (isFlowValuesExist)
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

                        if (isSavingAsReadyToSubmit)
                        {
                            if ((resultDto.Qualifier == ">" || resultDto.Qualifier == "<" || string.IsNullOrEmpty(resultDto.Qualifier))
                                && resultDto.Value == null)
                            {
                                ThrowSimpleException("Every numeric qualifier must be accompanied by a numeric value.");
                            }

                            if (resultDto.Qualifier == "ND" || resultDto.Qualifier == "NF" && resultDto.Value != null)
                            {
                                ThrowSimpleException("Values cannot be associated with non-numeric qualifiers");
                            }

                            if (resultDto.IsCalcMassLoading && !isFlowValuesExist)
                            {
                                ThrowSimpleException("Flow values must be provided if including mass loading results.");
                            }

                            if (resultDto.IsCalcMassLoading && (resultDto.MassLoadingUnitId < 0 || resultDto.MassLoadingValue == null))
                            {
                                ThrowSimpleException("Missing mass loading values.");
                            }
                        }

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

                    transaction.Commit();
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
            return sampleIdToReturn;
        }

        private void ThrowSimpleException(string message)
        {
            List<RuleViolation> validationIssues = new List<RuleViolation>();
            validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
            throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
        }

        public bool IsReadyToSubmit(int sampleId, bool isSuppressExceptions = false)
        {
            bool isReadyToSubmit = true;
            List<RuleViolation> validationIssues = new List<RuleViolation>();

            var sample = _dbContext.Samples
                .Include(s => s.SampleResults)
                .Include(s => s.SampleResults.Select(r => r.LimitBasis))
                .Include(s => s.SampleResults.Select(r => r.LimitType))
                .Single(s => s.SampleId == sampleId);

            //Check required field (UC-15-1.2.1): "Sample Type"
            if (sample.CtsEventTypeId < 1)
            {
                isReadyToSubmit = false;
                if (!isSuppressExceptions)
                {
                    string message = "Sample Type is required.";
                    validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                    throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                }
            }
            //Check required field (UC-15-1.2.1): "Collection Method"
            if (sample.CollectionMethodId < 1)
            {
                isReadyToSubmit = false;
                if (!isSuppressExceptions)
                {
                    string message = "Collection Method is required.";
                    validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                    throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                }
            }
            //Check required field (UC-15-1.2.1): "Start Date/Time"
            if (sample.StartDateTimeUtc == null)
            {
                isReadyToSubmit = false;
                if (!isSuppressExceptions)
                {
                    string message = "Start Date/Time is required.";
                    validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                    throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                }
            }
            //Check required field (UC-15-1.2.1): "End Date/Time"
            if (sample.EndDateTimeUtc == null)
            {
                isReadyToSubmit = false;
                if (!isSuppressExceptions)
                {
                    string message = "End Date/Time is required.";
                    validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                    throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                }
            }

            //Check sample start/end dates are not in the future (UC-15-1.2.9.1.b)
            if (sample.StartDateTimeUtc > DateTime.UtcNow || sample.EndDateTimeUtc > DateTime.UtcNow)
            {
                isReadyToSubmit = false;
                if (!isSuppressExceptions)
                {
                    string message = "Sample dates cannot be future dates.";
                    validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                    throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                }
            }

            //Check results
            //

            //Step 1: load all results into dtos
            //Step 2: iterate through all the dtos to make sure
            //          - qualifier exists and if required, concentration values and units exist
            //          - if calc mass loading, we have flow value(s)
            //          - if calc mass loading, we have mass value(s)
            var flowResult = sample.SampleResults
                .SingleOrDefault(sr => sr.IsFlowForMassLoadingCalculation && sr.LimitBasisId == null && sr.LimitTypeId == null);
            var sampleResultDtos = new Dictionary<int, SampleResultDto>();
            foreach (var result in sample.SampleResults)
            {
                if (result.LimitBasisId == null && result.LimitTypeId == null)
                {
                    //ignore
                }
                else
                {
                    SampleResultDto thisSampleResult;
                    if (sampleResultDtos.ContainsKey(result.ParameterId))
                    {
                        thisSampleResult = sampleResultDtos[result.ParameterId];
                    }
                    else
                    {
                        thisSampleResult = new SampleResultDto();
                        sampleResultDtos.Add(result.ParameterId, thisSampleResult);
                    }

                    if (result.LimitType.Name == LimitTypeName.Daily.ToString()
                        && result.LimitBasis.Name == LimitBasisName.Concentration.ToString())
                    {
                        thisSampleResult.IsCalcMassLoading = result.IsMassLoadingCalculationRequired;
                        thisSampleResult.Qualifier = result.Qualifier;
                        thisSampleResult.Value = result.Value;
                        thisSampleResult.UnitId = result.UnitId;
                        thisSampleResult.UnitName = result.UnitName;
                        thisSampleResult.DecimalPlaces = result.DecimalPlaces;

                    }
                    else if (result.LimitType.Name == LimitTypeName.Daily.ToString()
                        && result.LimitBasis.Name == LimitBasisName.MassLoading.ToString())
                    {
                        thisSampleResult.MassLoadingQualifier = result.Qualifier;
                        thisSampleResult.MassLoadingValue = result.Value;
                        thisSampleResult.MassLoadingUnitId = result.UnitId;
                        thisSampleResult.MassLoadingUnitName = result.UnitName;
                        thisSampleResult.MassLoadingDecimalPlaces = result.DecimalPlaces;

                    }
                    else
                    {
                        throw new Exception($"Unknown sample result type encountered. SampleResultId={result.SampleResultId}");
                    }

                }


            }

            //Step 2: iterate through all the dtos
            foreach (var resultDto in sampleResultDtos.Values)
            {
                if ((resultDto.Qualifier == ">" || resultDto.Qualifier == "<" || string.IsNullOrEmpty(resultDto.Qualifier))
                    && resultDto.Value == null)
                {
                    isReadyToSubmit = false;
                    if (!isSuppressExceptions)
                    {
                        string message = "All numeric values must be accompanied by a numeric qualifier.";
                        validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                        throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                    }
                }
                if (resultDto.Qualifier == "ND" || resultDto.Qualifier == "NF" && resultDto.Value != null)
                {
                    isReadyToSubmit = false;
                    if (!isSuppressExceptions)
                    {
                        string message = "Only null values can be associated with non-numeric qualifiers.";
                        validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                        throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                    }
                }

                if (resultDto.IsCalcMassLoading && (flowResult == null || flowResult.UnitId < 0 || flowResult.Value == null))
                {
                    isReadyToSubmit = false;
                    if (!isSuppressExceptions)
                    {
                        string message = "You must provide valid a flow value to calculate mass loading results";
                        validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                        throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                    }
                }

                if (resultDto.IsCalcMassLoading && (resultDto.MassLoadingUnitId < 0 || resultDto.MassLoadingValue == null))
                {
                    isReadyToSubmit = false;
                    if (!isSuppressExceptions)
                    {
                        string message = "You must provide valid mass loading unit/value if electing to calculate mass loading results";
                        validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                        throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                    }
                }
            }

            return isReadyToSubmit;
        }

        public void DeleteSample(int sampleId)
        {
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

        private SampleDto GetSampleDetails(Core.Domain.Sample sample)
        {
            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));

            var dto = _mapHelper.GetSampleDtoFromSample(sample);

            //Set Sample Start Local Timestamp
            dto.StartDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(sample.StartDateTimeUtc.DateTime, timeZoneId);

            //Set Sample End Local Timestamp
            dto.StartDateTimeLocal = _timeZoneService
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
                    var errorString = $"Encountered Sample Result with SampleResultId={sampleResult.SampleResultId} with unknown " + 
                        $"IsFlowForMassLoadingCalculation / Limit Type / Limit Basis combination";
                    throw new Exception(errorString);
                }

              
            }

            //Check that all results have at least concentration fields.
            foreach (var resultDtoValue in resultDtoList.Values)
            {
                if (string.IsNullOrEmpty(resultDtoValue.Qualifier) ||
                    resultDtoValue.Value == null || resultDtoValue.DecimalPlaces < 1 ||
                    resultDtoValue.UnitId < 1 || string.IsNullOrEmpty(resultDtoValue.UnitName))
                {
                    var errorString = $"Sample Result DTO for Sample Id = {resultDtoValue.SampleId}, " +
                        $"Parameter Id = {resultDtoValue.ParameterId} could not be correctly constructed due to missing concentration fields";
                    throw new Exception(errorString);
                }
            }

            dto.SampleResults = resultDtoList.Values.ToList();

            return dto;
        }

        private void SetSampleResultDatesAndLastModified(SampleResult sampleResult, ref SampleResultDto resultDto, int timeZoneId)
        {
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
        }

        public SampleDto GetSampleDetails(int sampleId)
        {
            var sample = _dbContext.Samples
                .Include(s => s.SampleStatus)
                .Include(s => s.SampleResults)
                .Single(s => s.SampleId == sampleId);

            var dto = this.GetSampleDetails(sample);

            return dto;
        }

        public bool IsSampleIncludedInReportPackage(int sampleId)
        {
            var isExists = _dbContext.ReportSamples.Any(rs => rs.SampleId == sampleId);

            return isExists;
        }

        public IEnumerable<SampleDto> GetSamples(SampleStatusName status, DateTime? startDate = null, DateTime? endDate = null)
        {
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

            return dtos;
        }
    }
}
