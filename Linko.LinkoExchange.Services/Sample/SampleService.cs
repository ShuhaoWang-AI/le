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

        public int SaveSample(SampleDto sampleDto)
        {
            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var authOrgRegProgramId = _orgService.GetAuthority(currentOrgRegProgramId).OrganizationRegulatoryProgramId;
            var currentUserId = int.Parse(_httpContext.GetClaimValue(CacheKey.UserProfileId));
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));
            var sampleIdToReturn = -1;
            List<RuleViolation> validationIssues = new List<RuleViolation>();

            //Check required fields
            if (string.IsNullOrEmpty(sampleDto.Name))
            {
                string message = "Name is required.";
                validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
            }
            //Check for any mass results exist, if so check for flow value provided?
            // OR HANDLED AT UI LAYER

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    //Find existing Sample with same Name
                    string proposedSampleName = sampleDto.Name.Trim().ToLower();
                    var samplesWithMatchingName = _dbContext.Samples
                        .Where(s => s.Name.Trim().ToLower() == proposedSampleName
                                && s.OrganizationRegulatoryProgramId == authOrgRegProgramId);

                    Core.Domain.Sample sampleToPersist = null;

                    if (sampleDto.SampleId.HasValue && sampleDto.SampleId.Value > 0)
                    {
                        //Ensure there are no other samples with same name
                        foreach (var sampleWithMatchingName in samplesWithMatchingName)
                        {
                            if (sampleWithMatchingName.SampleId != sampleDto.SampleId.Value)
                            {
                                string message = "A Sample with that name already exists. Please select another name.";
                                validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                                throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                            }
                        }

                        //Update existing
                        sampleToPersist = _dbContext.Samples.Single(c => c.SampleId == sampleDto.SampleId);
                        sampleToPersist = _mapHelper.GetSampleFromSampleDto(sampleDto, sampleToPersist);
                        sampleToPersist.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                        sampleToPersist.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;
                        sampleToPersist.LastModifierUserId = currentUserId;

                        //Delete existing results
                        var existingSampleResults = _dbContext.SampleResults
                            .Where(sr => sr.SampleId == sampleDto.SampleId);
                        _dbContext.SampleResults.RemoveRange(existingSampleResults);
                    }
                    else
                    {
                        //Ensure there are no other element types with same name
                        if (samplesWithMatchingName.Count() > 0)
                        {
                            string message = "A Sample with that name already exists.  Please select another name.";
                            validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                            throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                        }

                        //Get new
                        sampleToPersist = _mapHelper.GetSampleFromSampleDto(sampleDto);
                        sampleToPersist.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                        sampleToPersist.StartDateTimeUtc = _timeZoneService.GetUTCDateTimeUsingThisTimeZoneId(sampleDto.StartDateTimeLocal, timeZoneId);
                        sampleToPersist.EndDateTimeUtc = _timeZoneService.GetUTCDateTimeUsingThisTimeZoneId(sampleDto.EndDateTimeLocal, timeZoneId);
                        sampleToPersist.CreationDateTimeUtc = DateTimeOffset.UtcNow;
                        sampleToPersist.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;
                        sampleToPersist.LastModifierUserId = currentUserId;
                        _dbContext.Samples.Add(sampleToPersist);
                    }


                    _dbContext.SaveChanges(); //Needed here?

                    sampleIdToReturn = sampleToPersist.SampleId;

                    //Add results
                    sampleToPersist.SampleResults = new Collection<SampleResult>();
                    //
                    //Add flow result first
                    var flowParameter = _dbContext.Parameters
                        .First(p => p.IsFlowForMassLoadingCalculation == true); //Chris: "Should be one but just get first".

                    var flowResult = new SampleResult() {
                        SampleId = sampleIdToReturn
                        ,ParameterId = flowParameter.ParameterId
                        ,ParameterName = flowParameter.Name
                        ,Qualifier = ""
                        ,Value = sampleDto.FlowValue
                        ,DecimalPlaces = sampleDto.FlowValueDecimalPlaces
                        ,UnitId = sampleDto.FlowUnitId
                        ,UnitName = sampleDto.FlowUnitName
                        ,IsFlowForMassLoadingCalculation = true
                        ,LimitTypeId = null
                        ,LimitBasisId = null
                        ,IsCalculated = false
                    };
                    sampleToPersist.SampleResults.Add(flowResult);

                    //Add "regular" sample results
                    var massLimitBasisId = _dbContext.LimitBases.Single(lb => lb.Name == LimitBasisName.Mass.ToString()).LimitBasisId;
                    var concentrationLimitBasisId = _dbContext.LimitBases.Single(lb => lb.Name == LimitBasisName.Mass.ToString()).LimitBasisId;
                    var dailyLimitTypeId = _dbContext.LimitTypes.Single(lt => lt.Name == LimitTypeName.DailyLimit.ToString()).LimitTypeId;
                    foreach (var resultDto in sampleDto.SampleResults)
                    {
                        //Concentration result
                        var sampleResult = _mapHelper.GetConcentrationSampleResultFromSampleResultDto(resultDto);
                        sampleResult.AnalysisDateTimeUtc = _timeZoneService.GetUTCDateTimeUsingThisTimeZoneId(resultDto.AnalysisDateTimeLocal.Value, timeZoneId);
                        sampleResult.LimitBasisId = concentrationLimitBasisId;
                        sampleResult.LimitTypeId = dailyLimitTypeId;
                        sampleResult.CreationDateTimeUtc = DateTimeOffset.UtcNow;
                        sampleResult.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;
                        sampleResult.LastModifierUserId = currentUserId;

                        sampleToPersist.SampleResults.Add(sampleResult);

                        //Mass result (if calculated)
                        if (resultDto.IsCalcMassLoading)
                        {
                            var sampleMassResult = _mapHelper.GetMassSampleResultFromSampleResultDto(resultDto);
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

            var resultDtoList = new List<SampleResultDto>();
            foreach (var sampleResult in sample.SampleResults)
            {
                var resultDto = _mapHelper.GetSampleResultDtoFromSampleResult(sampleResult);
                
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

                resultDtoList.Add(resultDto);
            }
            dto.SampleResults = resultDtoList;

            return dto;
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
